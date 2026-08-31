using System.Text.Json.Serialization;

namespace BlazingStory.Docs.Models;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(VersionCatalog))]
[JsonSerializable(typeof(SidebarDefinition))]
public sealed partial class DocsJsonContext : JsonSerializerContext
{
}
