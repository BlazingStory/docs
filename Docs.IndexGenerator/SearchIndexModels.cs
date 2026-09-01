using System.Text.Json.Serialization;

namespace BlazingStory.Docs.IndexGenerator;

/// <summary>
/// The content of a "search-index.json" file, the metadata half of one version's search index.
/// The vectors live in the "search-index.vec" file next to it, in the same order as <see cref="Chunks"/>.
/// </summary>
/// <param name="Model">The identifier of the model the vectors were computed with.</param>
/// <param name="Dimensions">The number of dimensions of a single vector.</param>
/// <param name="Count">The number of chunks, which is also the number of vectors in the ".vec" file.</param>
/// <param name="Chunks">The metadata of every chunk, in the same order as the vectors.</param>
public sealed record class SearchIndexFile(
    string Model,
    int Dimensions,
    int Count,
    IReadOnlyList<SearchChunk> Chunks);

/// <summary>
/// One searchable chunk of a documentation page.
/// </summary>
/// <param name="Slug">The slug of the page, the same value as in "sidebar.json".</param>
/// <param name="Anchor">The id of the heading this chunk belongs to, or an empty string for the lead of a page.</param>
/// <param name="Heading">The text of that heading, or the title of the page for the lead chunk.</param>
/// <param name="Text">A short excerpt shown in the search results.</param>
public sealed record class SearchChunk(
    string Slug,
    string Anchor,
    string Heading,
    string Text);

/// <summary>The source generation context for the JSON files this program writes and reads.</summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true, WriteIndented = false)]
[JsonSerializable(typeof(SearchIndexFile))]
[JsonSerializable(typeof(VersionCatalog))]
[JsonSerializable(typeof(SidebarDefinition))]
internal sealed partial class IndexJsonContext : JsonSerializerContext
{
}

/// <summary>The subset of "versions.json" this program needs.</summary>
public sealed class VersionCatalog
{
    public string DefaultVersion { get; set; } = "";

    public List<DocVersion> Versions { get; set; } = [];
}

/// <summary>One entry of the "versions" array of "versions.json".</summary>
public sealed class DocVersion
{
    public string Version { get; set; } = "";
}

/// <summary>The subset of "sidebar.json" this program needs, the list of the pages of one version.</summary>
public sealed class SidebarDefinition
{
    public List<SidebarSection> Sections { get; set; } = [];

    public IEnumerable<string> AllSlugs => this.Sections.SelectMany(section => section.Items).Select(item => item.Slug);
}

/// <summary>One group of pages in "sidebar.json".</summary>
public sealed class SidebarSection
{
    public List<SidebarItem> Items { get; set; } = [];
}

/// <summary>One page in "sidebar.json".</summary>
public sealed class SidebarItem
{
    public string Slug { get; set; } = "";
}
