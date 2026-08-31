using Microsoft.JSInterop;

namespace BlazingStory.Docs.Services;

/// <summary>
/// Mirrors the color theme that "js/site.js" applies to the &lt;html data-theme&gt; attribute.
/// </summary>
public sealed class ThemeService(IJSRuntime jsRuntime) : IDisposable
{
    public const string Light = "light";

    public const string Dark = "dark";

    private DotNetObjectReference<ThemeService>? _selfReference;

    public string Theme { get; private set; } = Light;

    public event Action? Changed;

    public async Task InitializeAsync()
    {
        this._selfReference ??= DotNetObjectReference.Create(this);
        this.UpdateTheme(await jsRuntime.InvokeAsync<string>("blazingStoryDocs.initializeTheme", this._selfReference));
    }

    public async Task ToggleAsync()
    {
        this.UpdateTheme(this.Theme == Dark ? Light : Dark);
        await jsRuntime.InvokeVoidAsync("blazingStoryDocs.setTheme", this.Theme);
    }

    [JSInvokable]
    public void OnSystemThemeChanged(string theme) => this.UpdateTheme(theme);

    private void UpdateTheme(string theme)
    {
        if (this.Theme == theme) return;

        this.Theme = theme;
        this.Changed?.Invoke();
    }

    public void Dispose() => this._selfReference?.Dispose();
}
