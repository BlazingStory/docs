namespace BlazingStory.Docs.Models;

/// <summary>
/// A group of documents rendered as one block in the sidebar.
/// </summary>
public sealed class SidebarSection
{
    public Dictionary<string, string> DisplayText { get; set; } = [];

    public List<SidebarItem> Items { get; set; } = [];
}
