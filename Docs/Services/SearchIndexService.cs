using System.Net.Http.Json;
using System.Runtime.InteropServices;
using BlazingStory.Docs.Models;

namespace BlazingStory.Docs.Services;

/// <summary>
/// Loads the vector search index that was generated at build time, one per documentation version,
/// and ranks its chunks by how close they are to a search text.
/// </summary>
public sealed class SearchIndexService(HttpClient httpClient, DocsCatalogService catalog, EmbeddingService embeddingService)
{
    /// <summary>The number of dimensions of a single vector, which comes from the all-MiniLM-L6-v2 model.</summary>
    private const int Dimensions = 384;

    /// <summary>
    /// Scores below this are noise with this model, so they never show up as a result. Measured
    /// against this content, a query about something the documentation does not cover peaks at
    /// about 0.20, while a short but fair query such as "dark mode" reaches only 0.28, so the line
    /// belongs between the two.
    /// </summary>
    private const float MinimumScore = 0.25f;

    private readonly Dictionary<string, Task<VersionIndex?>> _indexes = [];

    /// <summary>
    /// Loads the embedding model and the index of the given version, so that the first search does
    /// not have to wait for both. Calling this more than once is safe.
    /// </summary>
    public async ValueTask WarmUpAsync(string version)
    {
        var loadIndex = this.GetIndexAsync(version);
        await embeddingService.InitAsync();
        await loadIndex;
    }

    /// <summary>
    /// Returns the parts of the documentation of the given version that are the closest in meaning
    /// to the search text, the best match first. A page never takes more than one place in the
    /// result, so that a long page cannot fill the whole list.
    /// </summary>
    /// <param name="version">The documentation version to search in.</param>
    /// <param name="text">The search text.</param>
    /// <param name="maxCount">The maximum number of results to return.</param>
    public async ValueTask<SearchResult[]> SearchAsync(string version, string text, int maxCount = 10)
    {
        var index = await this.GetIndexAsync(version);
        if (index is null || string.IsNullOrWhiteSpace(text)) return [];

        var queryVector = await embeddingService.EmbedAsync(text);
        if (queryVector.Length != Dimensions) return [];

        var defaultVersion = (await catalog.GetVersionCatalogAsync()).DefaultVersion;
        var sidebar = await catalog.GetSidebarAsync(version);

        return [.. index.Chunks
            .Select((chunk, i) => (Chunk: chunk, Score: DotProduct(queryVector, index.Vectors, i)))
            .Where(candidate => candidate.Score >= MinimumScore)
            .GroupBy(candidate => candidate.Chunk.Slug)
            .Select(group => group.MaxBy(candidate => candidate.Score))
            .OrderByDescending(candidate => candidate.Score)
            .Take(maxCount)
            .Select(candidate => new SearchResult(
                Title: sidebar?.FindItem(candidate.Chunk.Slug)?.DisplayText.Text(candidate.Chunk.Heading) ?? candidate.Chunk.Heading,
                Heading: candidate.Chunk.Heading,
                Text: candidate.Chunk.Text,
                Url: DocRoutes.Doc(version, defaultVersion, candidate.Chunk.Slug) + (candidate.Chunk.Anchor.Length > 0 ? "#" + candidate.Chunk.Anchor : ""),
                Score: candidate.Score))];
    }

    private Task<VersionIndex?> GetIndexAsync(string version)
    {
        if (this._indexes.TryGetValue(version, out var loading)) return loading;
        return this._indexes[version] = LoadIndexAsync(version);
    }

    private async Task<VersionIndex?> LoadIndexAsync(string version)
    {
        var index = await httpClient.GetFromJsonAsync(DocRoutes.SearchIndexJsonFile(version), DocsJsonContext.Default.SearchIndexFile);
        if (index is null || index.Chunks.Count == 0) return null;

        // "search-index.vec" is a flat array of float32 values with no header, so the vector of the
        // chunk at index i sits at [i * Dimensions, (i + 1) * Dimensions).
        var bytes = await httpClient.GetByteArrayAsync(DocRoutes.SearchIndexVecFile(version));
        var vectors = MemoryMarshal.Cast<byte, float>(bytes).ToArray();

        // A stale ".vec" file would not throw, it would just rank everything meaninglessly.
        // Turning the search off for this version is the honest thing to do instead.
        if (index.Dimensions != Dimensions || vectors.Length != index.Chunks.Count * Dimensions) return null;

        return new VersionIndex(index.Chunks, vectors);
    }

    /// <summary>
    /// Every vector is L2 normalized, both here and at build time, so the dot product of two of
    /// them is their cosine similarity.
    /// </summary>
    private static float DotProduct(float[] queryVector, float[] vectors, int chunkIndex)
    {
        var vector = vectors.AsSpan(chunkIndex * Dimensions, Dimensions);
        var score = 0f;
        for (var d = 0; d < Dimensions; d++) score += queryVector[d] * vector[d];
        return score;
    }

    /// <summary>The index of one version: the metadata of every chunk, and all their vectors end to end.</summary>
    private sealed record class VersionIndex(List<SearchChunk> Chunks, float[] Vectors);
}
