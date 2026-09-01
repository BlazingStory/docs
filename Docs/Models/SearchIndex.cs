namespace BlazingStory.Docs.Models;

/// <summary>
/// The content of a "Docs/{version}/search-index.json" file, the metadata half of the vector search
/// index of one documentation version. The vectors live in the "search-index.vec" file next to it,
/// in the same order as <see cref="Chunks"/>.
/// </summary>
public sealed class SearchIndexFile
{
    /// <summary>The identifier of the model the vectors were computed with.</summary>
    public string Model { get; set; } = "";

    /// <summary>The number of dimensions of a single vector.</summary>
    public int Dimensions { get; set; }

    /// <summary>The number of chunks, which is also the number of vectors in the ".vec" file.</summary>
    public int Count { get; set; }

    public List<SearchChunk> Chunks { get; set; } = [];
}

/// <summary>
/// One searchable chunk of a documentation page.
/// </summary>
public sealed class SearchChunk
{
    /// <summary>The slug of the page, the same value as in "sidebar.json".</summary>
    public string Slug { get; set; } = "";

    /// <summary>The id of the heading this chunk belongs to, or an empty string for the lead of a page.</summary>
    public string Anchor { get; set; } = "";

    /// <summary>The text of that heading, or the title of the page for the lead chunk.</summary>
    public string Heading { get; set; } = "";

    /// <summary>A short excerpt shown in the search results.</summary>
    public string Text { get; set; } = "";
}

/// <summary>
/// One hit of a vector search, ready to be rendered as a link in the result list.
/// </summary>
/// <param name="Title">The name of the page, as the sidebar shows it.</param>
/// <param name="Heading">The heading of the part of the page that matched.</param>
/// <param name="Text">The excerpt of that part.</param>
/// <param name="Url">The application URL of the page, with the heading anchor when there is one.</param>
/// <param name="Score">The cosine similarity to the search text, from -1 to 1.</param>
public sealed record class SearchResult(string Title, string Heading, string Text, string Url, float Score);
