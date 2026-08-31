namespace BlazingStory.Docs.Models;

/// <summary>
/// The catalog of published documentation versions, deserialized from "Docs/versions.json".
/// </summary>
public sealed class VersionCatalog
{
    public string DefaultVersion { get; set; } = "";

    public List<DocVersion> Versions { get; set; } = [];

    public bool Contains(string version) => this.Versions.Any(candidate => candidate.Version == version);
}
