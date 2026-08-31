using System.Net.Http.Json;
using BlazingStory.Docs.Models;

namespace BlazingStory.Docs.Services;

/// <summary>
/// Loads and caches the version catalog and the per version sidebar definitions.
/// </summary>
public sealed class DocsCatalogService(HttpClient httpClient)
{
    private readonly Dictionary<string, SidebarDefinition?> _sidebars = [];

    private VersionCatalog? _versionCatalog;

    public async ValueTask<VersionCatalog> GetVersionCatalogAsync()
    {
        this._versionCatalog ??= await httpClient.GetFromJsonAsync("Docs/versions.json", DocsJsonContext.Default.VersionCatalog)
            ?? throw new InvalidOperationException("\"Docs/versions.json\" could not be loaded.");
        return this._versionCatalog;
    }

    public async ValueTask<SidebarDefinition?> GetSidebarAsync(string version)
    {
        if (this._sidebars.TryGetValue(version, out var cached)) return cached;

        SidebarDefinition? sidebar = null;
        using var response = await httpClient.GetAsync($"Docs/{version}/sidebar.json");
        if (response.IsSuccessStatusCode)
        {
            sidebar = await response.Content.ReadFromJsonAsync(DocsJsonContext.Default.SidebarDefinition);
        }

        this._sidebars[version] = sidebar;
        return sidebar;
    }
}
