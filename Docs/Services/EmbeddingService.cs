using Microsoft.JSInterop;

namespace BlazingStory.Docs.Services;

/// <summary>
/// Computes the embedding vector of a search text, by handing it to a JavaScript module that runs
/// a feature extraction model in the browser.
/// </summary>
public sealed class EmbeddingService(IJSRuntime jsRuntime) : IAsyncDisposable
{
    /// <summary>
    /// The model to embed the search text with. It has to stay the same as the one the build time
    /// index generator uses, otherwise the vectors do not match and the search results become
    /// meaningless without any error.
    /// </summary>
    private const string ModelId = "Xenova/all-MiniLM-L6-v2";

    private readonly Lazy<Task<IJSObjectReference>> _moduleTask =
        new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/search-embeddings.js").AsTask());

    private Task? _initializeTask;

    /// <summary>
    /// Loads the embedding model. The first call takes a while, since it downloads about 23 MB, but
    /// the browser caches that, so later visits are quick. Calling this more than once is safe:
    /// every call after the first one waits for the same work.
    /// </summary>
    public Task InitAsync() => this._initializeTask ??= this.InitCoreAsync();

    private async Task InitCoreAsync()
    {
        var module = await this._moduleTask.Value;
        await module.InvokeVoidAsync("initModel", ModelId);
    }

    /// <summary>
    /// Computes a normalized embedding vector for the given text.
    /// </summary>
    public async ValueTask<float[]> EmbedAsync(string text)
    {
        await this.InitAsync();
        var module = await this._moduleTask.Value;
        return await module.InvokeAsync<float[]>("embed", text);
    }

    /// <summary>
    /// Releases the JavaScript module reference used by this service.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (!this._moduleTask.IsValueCreated) return;

        var module = await this._moduleTask.Value;
        try { await module.DisposeAsync(); }
        catch (JSDisconnectedException) { }
    }
}
