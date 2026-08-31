namespace BlazingStory.Docs.Models;

public static class DisplayTextExtensions
{
    /// <summary>The language whose texts are always present and used as the fallback.</summary>
    public const string DefaultLanguage = "EN";

    public static string Text(this IReadOnlyDictionary<string, string> displayText, string fallback = "", string language = DefaultLanguage)
    {
        if (displayText.TryGetValue(language, out var localized)) return localized;
        if (displayText.TryGetValue(DefaultLanguage, out var english)) return english;
        return fallback;
    }
}
