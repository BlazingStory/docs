using Toolbelt.Blazor.Prismizor;

namespace BlazingStory.Docs.MarkdownRendering;

/// <summary>
/// Maps the language name of a code fence to the Prismizor grammar that highlights it.
/// </summary>
public static class CodeLanguages
{
    // Listing a grammar here keeps it from being trimmed, so list only the languages the documents use.
    private static readonly Dictionary<string, Grammar> _Grammars = new(StringComparer.OrdinalIgnoreCase)
    {
        ["csharp"] = PrismizorLanguages.CSharp,
        ["cs"] = PrismizorLanguages.CSharp,
        ["cshtml"] = PrismizorLanguages.Cshtml,
        ["razor"] = PrismizorLanguages.Cshtml,
        ["markup"] = PrismizorLanguages.Markup,
        ["html"] = PrismizorLanguages.Markup,
        ["xml"] = PrismizorLanguages.Markup,
        ["css"] = PrismizorLanguages.Css,
        ["javascript"] = PrismizorLanguages.JavaScript,
        ["js"] = PrismizorLanguages.JavaScript,
        ["typescript"] = PrismizorLanguages.Typescript,
        ["ts"] = PrismizorLanguages.Typescript,
        ["json"] = PrismizorLanguages.Json,
        ["bash"] = PrismizorLanguages.Bash,
        ["sh"] = PrismizorLanguages.Bash,
        ["shell"] = PrismizorLanguages.Bash,
        ["nginx"] = PrismizorLanguages.Nginx,
        ["markdown"] = PrismizorLanguages.Markdown,
        ["md"] = PrismizorLanguages.Markdown,
    };

    private static readonly Grammar _Fallback = PrismizorLanguages.Plain;

    static CodeLanguages()
    {
        // Lets a code fence inside a Markdown code block find its language, as Prism.languages does.
        foreach (var (name, grammar) in _Grammars)
        {
            PrismizorLanguageRegistry.Register(grammar, name);
        }
    }

    /// <summary>Returns the grammar for the given fence language, or the fallback grammar for an unknown or empty one.</summary>
    public static Grammar Resolve(string? language)
        => language is { Length: > 0 } && _Grammars.TryGetValue(language, out var grammar) ? grammar : _Fallback;
}
