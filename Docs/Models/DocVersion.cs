namespace BlazingStory.Docs.Models;

/// <summary>
/// A single entry of the documentation version drop-down list.
/// </summary>
public sealed class DocVersion
{
    public string Version { get; set; } = "";

    public Dictionary<string, string> DisplayText { get; set; } = [];
}
