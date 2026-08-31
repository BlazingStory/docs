using BlazingStory.Docs.MarkdownRendering;
using BlazingStory.Docs.Models;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.EmphasisExtras;
using Markdig.Renderers;
using Markdig.Syntax;
using Microsoft.AspNetCore.Components;

namespace BlazingStory.Docs.Services;

/// <summary>
/// Fetches Markdown files at runtime and converts them into HTML with GitHub Flavored Markdown
/// support plus the two Docusaurus conventions the content relies on: GitHub alerts and code
/// fence titles.
/// </summary>
public sealed class MarkdownService(HttpClient httpClient)
{
    private static readonly MarkdownPipeline _Pipeline = new MarkdownPipelineBuilder()
        .UsePipeTables()
        .UseTaskLists()
        .UseEmphasisExtras(EmphasisExtraOptions.Strikethrough)
        .UseAutoLinks()
        .UseFootnotes()
        .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
        .UseAlertBlocks()
        .UseCodeBlockTitles()
        .Build();

    private readonly Dictionary<string, DocContent> _cache = [];

    public async ValueTask<DocContent?> GetDocumentAsync(string version, string slug, string defaultVersion)
    {
        var cacheKey = $"{version}/{slug}";
        if (this._cache.TryGetValue(cacheKey, out var cached)) return cached;

        using var response = await httpClient.GetAsync(DocRoutes.MarkdownFile(version, slug));
        if (!response.IsSuccessStatusCode) return null;

        var markdown = StripFrontMatter(await response.Content.ReadAsStringAsync());
        var document = Markdig.Markdown.Parse(markdown, _Pipeline);

        DocumentPostProcessor.NormalizeCodeBlockLanguages(document);
        DocumentPostProcessor.RewriteLinks(document, version, defaultVersion, slug);

        var content = new DocContent
        {
            Title = DocumentPostProcessor.GetTitle(document, slug),
            Html = new MarkupString(Render(document)),
            TableOfContents = DocumentPostProcessor.BuildTableOfContents(document),
        };

        this._cache[cacheKey] = content;
        return content;
    }

    private static string Render(MarkdownDocument document)
    {
        using var writer = new StringWriter();
        var renderer = new HtmlRenderer(writer);
        _Pipeline.Setup(renderer);
        renderer.Render(document);
        writer.Flush();
        return writer.ToString();
    }

    /// <summary>Removes the YAML front matter block, but only when it is at the very top of the file.</summary>
    private static string StripFrontMatter(string markdown)
    {
        var normalized = markdown.Replace("\r\n", "\n");
        if (!normalized.StartsWith("---\n")) return normalized;

        var closing = normalized.IndexOf("\n---", 3, StringComparison.Ordinal);
        if (closing < 0) return normalized;

        var endOfLine = normalized.IndexOf('\n', closing + 1);
        return endOfLine < 0 ? "" : normalized[(endOfLine + 1)..];
    }
}
