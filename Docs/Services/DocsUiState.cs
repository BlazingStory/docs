namespace BlazingStory.Docs.Services;

/// <summary>
/// Shares the currently displayed document and the mobile drawer state between the
/// page component and the chrome components rendered by the layout.
/// </summary>
public sealed class DocsUiState
{
    public string Version { get; private set; } = "";

    public string DefaultVersion { get; private set; } = "";

    public string Slug { get; private set; } = "";

    public bool IsSidebarOpen { get; private set; }

    public event Action? Changed;

    public void SetCurrentDocument(string version, string defaultVersion, string slug)
    {
        if (this.Version == version && this.DefaultVersion == defaultVersion && this.Slug == slug) return;

        this.Version = version;
        this.DefaultVersion = defaultVersion;
        this.Slug = slug;
        this.Changed?.Invoke();
    }

    public void ToggleSidebar()
    {
        this.IsSidebarOpen = !this.IsSidebarOpen;
        this.Changed?.Invoke();
    }

    public void CloseSidebar()
    {
        if (!this.IsSidebarOpen) return;

        this.IsSidebarOpen = false;
        this.Changed?.Invoke();
    }
}
