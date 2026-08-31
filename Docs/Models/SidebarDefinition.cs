using System.Text.Json.Serialization;

namespace BlazingStory.Docs.Models;

/// <summary>
/// The sidebar layout of a single documentation version, deserialized from "Docs/{version}/sidebar.json".
/// </summary>
public sealed class SidebarDefinition
{
    public string DefaultSlug { get; set; } = "";

    public List<SidebarSection> Sections { get; set; } = [];

    [JsonIgnore]
    public IEnumerable<SidebarItem> AllItems => this.Sections.SelectMany(section => section.Items);

    public SidebarItem? FindItem(string slug) => this.AllItems.FirstOrDefault(item => item.Slug == slug);

    public SidebarSection? FindSection(string slug) => this.Sections.FirstOrDefault(section => section.Items.Any(item => item.Slug == slug));

    public (SidebarItem? Previous, SidebarItem? Next) FindNeighbors(string slug)
    {
        var items = this.AllItems.ToList();
        var index = items.FindIndex(item => item.Slug == slug);
        if (index < 0) return (null, null);
        return (
            index > 0 ? items[index - 1] : null,
            index < items.Count - 1 ? items[index + 1] : null);
    }
}
