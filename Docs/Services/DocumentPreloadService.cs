using Microsoft.AspNetCore.Components;

namespace BlazingStory.Docs.Services;

/// <summary>
/// Fills the caches that <see cref="Pages.DocPage"/> reads from before the app starts, so that its
/// first render already holds the whole document instead of flashing blank over the prerendered
/// markup. The page shares these very service instances, which makes its awaits complete synchronously.
/// </summary>
public sealed class DocumentPreloadService(
    NavigationManager navigationManager,
    DocsCatalogService catalogService,
    MarkdownService markdownService)
{
    /// <summary>Loads the version catalog, the sidebar, and the Markdown of the document the browser asked for.</summary>
    public async ValueTask PreloadCurrentDocumentAsync()
    {
        try
        {
            var catalog = await catalogService.GetVersionCatalogAsync();
            var (version, slug) = DocRoutes.ResolveDocPath(this.GetRequestedPath(), catalog);

            var sidebar = await catalogService.GetSidebarAsync(version);
            if (sidebar is null) return;

            if (slug.Length == 0) slug = sidebar.DefaultSlug;
            if (sidebar.FindItem(slug) is null) return;

            await markdownService.GetDocumentAsync(version, slug, catalog.DefaultVersion);
        }
        catch (Exception)
        {
            // Only an optimization: whatever failed here fails again inside "DocPage", where it belongs.
        }
    }

    /// <summary>The path of the current URL, in the shape the router hands it to the "Path" parameter of "DocPage".</summary>
    private string GetRequestedPath()
    {
        var relativePath = navigationManager.ToBaseRelativePath(navigationManager.Uri);
        var endOfPath = relativePath.IndexOfAny(['?', '#']);
        if (endOfPath >= 0) relativePath = relativePath[..endOfPath];
        return Uri.UnescapeDataString(relativePath);
    }
}
