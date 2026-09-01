namespace BlazingStory.Docs.Services;

/// <summary>
/// Builds base-relative URLs for documentation pages. All URLs are resolved by the browser
/// against the &lt;base href&gt; of the document, so the site can be hosted under a sub path.
/// </summary>
public static class DocRoutes
{
    public const string Home = "./";

    /// <summary>The language folder that holds the Markdown files. Only English exists for now.</summary>
    public const string Language = "EN";

    public const string VersionCatalogFile = "Docs/versions.json";

    public static string Doc(string version, string defaultVersion, string slug)
        => version == defaultVersion ? slug : $"{version}/{slug}";

    public static string SidebarFile(string version) => $"Docs/{version}/sidebar.json";

    public static string MarkdownFile(string version, string slug) => $"Docs/{version}/{Language}/{slug}.md";

    public static string ContentFile(string version, string relativePath) => $"Docs/{version}/{Language}/{relativePath}";

    /// <summary>The metadata half of the vector search index of one version, generated at build time.</summary>
    public static string SearchIndexJsonFile(string version) => $"Docs/{version}/search-index.json";

    /// <summary>The vectors of the search index of one version, in the same order as the metadata.</summary>
    public static string SearchIndexVecFile(string version) => $"Docs/{version}/search-index.vec";
}
