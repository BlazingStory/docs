using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using BlazingStory.Docs.IndexGenerator;
using CommandLineSwitchParser;

// Generates the vector search index of the Blazing Story documentation site: one
// "search-index.json" (chunk metadata) and one "search-index.vec" (the embedding vectors) per
// documentation version, written next to that version's "sidebar.json".
//
// The list of pages to index comes from each version's "sidebar.json", because a page that is not
// listed there cannot be reached from the site at all.

const string ModelId = "Xenova/all-MiniLM-L6-v2";
const string Language = "EN";

// Leaves room under the model's 256 token limit for the "{title} > {heading}" prefix of a chunk.
const int MaxChunkTokens = 200;

// The number of characters of a chunk kept as the excerpt shown in the search results.
const int ExcerptLength = 200;

var options = CommandLineSwitch.Parse<CommandLineOptions>(ref args);

var modelDir = Path.GetFullPath(options.Model ?? DefaultModelDir());
MiniLmEmbedder? embedder = null;

try
{
    // "--embed" checks that this program and the browser side compute the same vectors.
    if (options.Embed is { Length: > 0 } textToEmbed)
    {
        embedder = await CreateEmbedderAsync(modelDir);
        var vector = embedder.Embed(textToEmbed);
        Console.WriteLine($"Text  : {textToEmbed}");
        Console.WriteLine($"Tokens: {embedder.Tokenize(textToEmbed).Count} [{string.Join(", ", embedder.Tokenize(textToEmbed))}]");
        Console.WriteLine($"Vector: [{string.Join(",", vector.Select(value => value.ToString("R")))}]");
        return 0;
    }

    var docsDir = Path.GetFullPath(options.Docs ?? FindDefaultDocsDir());
    var versionsJsonPath = Path.Combine(docsDir, "versions.json");
    if (!File.Exists(versionsJsonPath))
    {
        Console.Error.WriteLine($"error: \"versions.json\" was not found in \"{docsDir}\".");
        return 1;
    }

    var catalog = JsonSerializer.Deserialize(await File.ReadAllTextAsync(versionsJsonPath), IndexJsonContext.Default.VersionCatalog);
    if (catalog is null || catalog.Versions.Count == 0)
    {
        Console.Error.WriteLine($"error: \"{versionsJsonPath}\" contains no version.");
        return 1;
    }

    // Work out what has to be rebuilt before touching the model, so that a build with nothing to do
    // does not pay for downloading and loading it.
    var targets = new List<IndexTarget>();
    foreach (var version in catalog.Versions.Select(version => version.Version))
    {
        var versionDir = Path.Combine(docsDir, version);
        var sidebarPath = Path.Combine(versionDir, "sidebar.json");
        if (!File.Exists(sidebarPath))
        {
            Console.Error.WriteLine($"error: \"sidebar.json\" was not found in \"{versionDir}\".");
            return 1;
        }

        var sidebar = JsonSerializer.Deserialize(await File.ReadAllTextAsync(sidebarPath), IndexJsonContext.Default.SidebarDefinition);
        if (sidebar is null)
        {
            Console.Error.WriteLine($"error: \"{sidebarPath}\" could not be read.");
            return 1;
        }

        var pages = new List<(string Slug, string Path)>();
        foreach (var slug in sidebar.AllSlugs)
        {
            var markdownPath = Path.Combine(versionDir, Language, slug.Replace('/', Path.DirectorySeparatorChar) + ".md");
            if (!File.Exists(markdownPath))
            {
                // An old version folder can list a page whose file is gone. That is not worth failing a build over.
                Console.WriteLine($"warning: \"{version}\" lists \"{slug}\" in its sidebar, but \"{markdownPath}\" does not exist. Skipped.");
                continue;
            }
            pages.Add((slug, markdownPath));
        }

        var jsonPath = Path.Combine(versionDir, "search-index.json");
        var vecPath = Path.Combine(versionDir, "search-index.vec");
        var inputs = new[] { versionsJsonPath, sidebarPath }.Concat(pages.Select(page => page.Path));

        if (!options.Force && !options.Dump && IsUpToDate([jsonPath, vecPath], inputs))
        {
            Console.WriteLine($"\"{version}\" is up to date. Skipped.");
            continue;
        }

        targets.Add(new IndexTarget(version, jsonPath, vecPath, pages));
    }

    if (targets.Count == 0)
    {
        Console.WriteLine("Every version is up to date. (Use --force to rebuild.)");
        return 0;
    }

    embedder = await CreateEmbedderAsync(modelDir);

    var totalChunks = 0;
    foreach (var target in targets)
    {
        var chunks = new List<ChunkSource>();
        foreach (var (slug, markdownPath) in target.Pages)
        {
            var markdown = await File.ReadAllTextAsync(markdownPath);
            chunks.AddRange(MarkdownChunker.Split(slug, markdown, embedder.CountTokens, MaxChunkTokens));
        }

        if (options.Dump)
        {
            DumpChunks(target.Version, chunks, embedder);
            continue;
        }

        var vectors = chunks.Select(chunk => embedder.Embed(chunk.EmbedText)).ToArray();

        var index = new SearchIndexFile(
            Model: ModelId,
            Dimensions: MiniLmEmbedder.Dimensions,
            Count: chunks.Count,
            Chunks: [.. chunks.Select(chunk => new SearchChunk(chunk.Slug, chunk.Anchor, chunk.Heading, Excerpt(chunk.Text)))]);

        await File.WriteAllTextAsync(target.JsonPath, JsonSerializer.Serialize(index, IndexJsonContext.Default.SearchIndexFile));

        // "search-index.vec" has no header at all: it is just float32[count * dimensions], little
        // endian, laid out end to end in the same order as the chunks of "search-index.json".
        await using (var stream = File.Create(target.VecPath))
        {
            foreach (var vector in vectors) stream.Write(MemoryMarshal.AsBytes(vector.AsSpan()));
        }

        totalChunks += chunks.Count;
        Console.WriteLine($"\"{target.Version}\": {target.Pages.Count} page(s), {chunks.Count} chunk(s), {new FileInfo(target.VecPath).Length:N0} bytes.");
    }

    if (!options.Dump) Console.WriteLine($"Generated the search index of {targets.Count} version(s), {totalChunks} chunk(s) in total.");
    return 0;
}
finally
{
    embedder?.Dispose();
}

static async Task<MiniLmEmbedder> CreateEmbedderAsync(string modelDir)
{
    // The quantized model file has to be the same one transformers.js v2 loads in the browser,
    // otherwise the vectors of the two sides do not match and the search results become meaningless.
    var onnxModelPath = Path.Combine(modelDir, "model_quantized.onnx");
    var vocabPath = Path.Combine(modelDir, "vocab.txt");

    Directory.CreateDirectory(modelDir);
    if (!File.Exists(onnxModelPath)) await DownloadFileAsync($"https://huggingface.co/{ModelId}/resolve/main/onnx/model_quantized.onnx", onnxModelPath);
    if (!File.Exists(vocabPath)) await DownloadFileAsync($"https://huggingface.co/{ModelId}/resolve/main/vocab.txt", vocabPath);

    return new MiniLmEmbedder(onnxModelPath, vocabPath);
}

static async Task DownloadFileAsync(string url, string destinationPath)
{
    Console.WriteLine($"Downloading {url}...");
    using var client = new HttpClient();
    using var stream = await client.GetStreamAsync(url);
    await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
    await stream.CopyToAsync(fileStream);
}

/// <summary>
/// Caches the model files under the project folder, so that the same cache is used no matter
/// where this program is started from, and so that deleting that one folder reclaims the space.
/// </summary>
static string DefaultModelDir([CallerFilePath] string sourceFilePath = "")
    => Path.Combine(Path.GetDirectoryName(sourceFilePath) ?? Directory.GetCurrentDirectory(), ".model-cache");

/// <summary>Walks up from the current directory looking for the content folder of the site.</summary>
static string FindDefaultDocsDir()
{
    for (var dir = new DirectoryInfo(Directory.GetCurrentDirectory()); dir != null; dir = dir.Parent)
    {
        var candidate = Path.Combine(dir.FullName, "Docs", "wwwroot", "Docs");
        if (File.Exists(Path.Combine(candidate, "versions.json"))) return candidate;
    }
    throw new DirectoryNotFoundException("Could not find the \"Docs/wwwroot/Docs\" folder. Specify it with the --docs option.");
}

static bool IsUpToDate(IEnumerable<string> outputPaths, IEnumerable<string> inputPaths)
{
    if (outputPaths.Any(path => !File.Exists(path))) return false;
    var oldestOutput = outputPaths.Min(File.GetLastWriteTimeUtc);
    return inputPaths.All(path => File.GetLastWriteTimeUtc(path) <= oldestOutput);
}

static string Excerpt(string text)
    => text.Length <= ExcerptLength ? text : text[..ExcerptLength].TrimEnd() + "…";

static void DumpChunks(string version, List<ChunkSource> chunks, MiniLmEmbedder embedder)
{
    Console.WriteLine($"===== {version}: {chunks.Count} chunk(s) =====");
    foreach (var group in chunks.GroupBy(chunk => chunk.Slug))
    {
        Console.WriteLine($"--- {group.Key} ({group.Count()} chunk(s))");
        foreach (var chunk in group)
        {
            Console.WriteLine($"  [{embedder.CountTokens(chunk.EmbedText),3} tokens] #{chunk.Anchor} \"{chunk.Heading}\"");
            Console.WriteLine($"      {Excerpt(chunk.Text)}");
        }
    }
}

internal sealed record class IndexTarget(string Version, string JsonPath, string VecPath, List<(string Slug, string Path)> Pages);
