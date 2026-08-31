using Microsoft.AspNetCore.Components;

namespace BlazingStory.Docs.Models;

/// <summary>
/// A Markdown document that has been fetched, converted to HTML, and analyzed for its outline.
/// </summary>
public sealed class DocContent
{
    public required string Title { get; init; }

    public required MarkupString Html { get; init; }

    public required IReadOnlyList<TocEntry> TableOfContents { get; init; }
}
