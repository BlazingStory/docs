namespace BlazingStory.Docs.Models;

/// <summary>
/// A single document link in the sidebar. The slug is the path of the Markdown file
/// under "Docs/{version}/{language}/", without the ".md" extension.
/// </summary>
public sealed class SidebarItem
{
    public string Slug { get; set; } = "";

    public Dictionary<string, string> DisplayText { get; set; } = [];
}
