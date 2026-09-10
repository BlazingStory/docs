using System.Reflection;
using BlazingStory.Docs.Models;

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

    // Appended here and never at the call sites, or "DocPage"'s preload hints and the "HttpClient"
    // fetches end up with different URLs and every file is downloaded twice.
    private static readonly string _CacheBuster = BuildCacheBuster();

    /// <summary>The catalog of every published documentation version.</summary>
    public static readonly string VersionCatalogFile = $"Docs/versions.json{_CacheBuster}";

    public static string Doc(string version, string defaultVersion, string slug)
        => version == defaultVersion ? slug : $"{version}/{slug}";

    public static string SidebarFile(string version) => $"Docs/{version}/sidebar.json{_CacheBuster}";

    public static string MarkdownFile(string version, string slug) => $"Docs/{version}/{Language}/{slug}.md{_CacheBuster}";

    public static string ContentFile(string version, string relativePath) => $"Docs/{version}/{Language}/{relativePath}{_CacheBuster}";

    /// <summary>The metadata half of the vector search index of one version, generated at build time.</summary>
    public static string SearchIndexJsonFile(string version) => $"Docs/{version}/search-index.json{_CacheBuster}";

    /// <summary>The vectors of the search index of one version, in the same order as the metadata.</summary>
    public static string SearchIndexVecFile(string version) => $"Docs/{version}/search-index.vec{_CacheBuster}";

    /// <summary>The inverse of <see cref="Doc"/>: splits a documentation URL path into its version and slug.</summary>
    public static (string Version, string Slug) ResolveDocPath(string? path, VersionCatalog catalog)
    {
        var trimmed = (path ?? "").Trim('/');
        var separator = trimmed.IndexOf('/');
        var firstSegment = separator < 0 ? trimmed : trimmed[..separator];

        return catalog.Contains(firstSegment)
            ? (firstSegment, separator < 0 ? "" : trimmed[(separator + 1)..])
            : (catalog.DefaultVersion, trimmed);
    }

    // The SDK writes the Git commit of the build into the informational version as
    // "{version}+{commit hash}", and 8 digits of the hash are enough to tell two builds apart.
    private static string BuildCacheBuster()
    {
        var informationalVersion = typeof(DocRoutes).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";

        var separator = informationalVersion.IndexOf('+');
        var token = separator < 0 ? informationalVersion : informationalVersion[(separator + 1)..];
        if (token.Length > 8) token = token[..8];

        return token == "" ? "" : "?v=" + token;
    }
}
