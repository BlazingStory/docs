using Microsoft.JSInterop;

namespace BlazingStory.Docs.Services;

/// <summary>
/// Mirrors the color theme that "js/site.js" applies to the &lt;html data-theme&gt; attribute.
/// </summary>
public sealed class ThemeService(IJSRuntime jsRuntime) : IAsyncDisposable
{
    public const string Light = "light";

    public const string Dark = "dark";

    private DotNetObjectReference<ThemeService>? _selfReference;

    public string Theme { get; private set; } = Light;

    public event Action? Changed;

    private IJSObjectReference? _jsModule;

    private async ValueTask<IJSObjectReference> GetJsModuleAsync() => this._jsModule ??= await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/site.js");

    public async Task InitializeAsync()
    {
        var module = await this.GetJsModuleAsync();
        this._selfReference ??= DotNetObjectReference.Create(this);
        this.UpdateTheme(await module.InvokeAsync<string>("initializeTheme", this._selfReference));
    }

    public async Task ToggleAsync()
    {
        this.UpdateTheme(this.Theme == Dark ? Light : Dark);
        var module = await this.GetJsModuleAsync();
        await module.InvokeVoidAsync("setTheme", this.Theme);
    }

    [JSInvokable]
    public void OnSystemThemeChanged(string theme) => this.UpdateTheme(theme);

    private void UpdateTheme(string theme)
    {
        if (this.Theme == theme) return;

        this.Theme = theme;
        this.Changed?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        if (this._jsModule is not null)
        {
            try { await this._jsModule.DisposeAsync(); }
            catch (JSDisconnectedException) { } // Ignore if the JS runtime is already disconnected
            this._jsModule = null;
        }
        this._selfReference?.Dispose();
    }
}
