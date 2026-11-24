using Toolbelt.Blazor.Prismazor;

namespace BlazingStory.Docs.MarkdownRendering;

/// <summary>
/// Maps the language name of a code fence to the Prismazor grammar that highlights it.
/// </summary>
public static class CodeLanguages
{
    // Listing a grammar here keeps it from being trimmed, so list only the languages the documents use.
    private static readonly Dictionary<string, Grammar> _Grammars = new(StringComparer.OrdinalIgnoreCase)
    {
        ["csharp"] = PrismazorLanguages.CSharp,
        ["cs"] = PrismazorLanguages.CSharp,
        ["cshtml"] = PrismazorLanguages.Cshtml,
        ["razor"] = PrismazorLanguages.Cshtml,
        ["markup"] = PrismazorLanguages.Markup,
        ["html"] = PrismazorLanguages.Markup,
        ["xml"] = PrismazorLanguages.Markup,
        ["css"] = PrismazorLanguages.Css,
        ["javascript"] = PrismazorLanguages.JavaScript,
        ["js"] = PrismazorLanguages.JavaScript,
        ["typescript"] = PrismazorLanguages.Typescript,
        ["ts"] = PrismazorLanguages.Typescript,
        ["json"] = PrismazorLanguages.Json,
        ["bash"] = PrismazorLanguages.Bash,
        ["sh"] = PrismazorLanguages.Bash,
        ["shell"] = PrismazorLanguages.Bash,
        ["nginx"] = PrismazorLanguages.Nginx,
        ["markdown"] = PrismazorLanguages.Markdown,
        ["md"] = PrismazorLanguages.Markdown,
    };

    private static readonly Grammar _Fallback = PrismazorLanguages.Plain;

    static CodeLanguages()
    {
        // Lets a code fence inside a Markdown code block find its language, as Prism.languages does.
        foreach (var (name, grammar) in _Grammars)
        {
            PrismazorLanguageRegistry.Register(grammar, name);
        }
    }

    /// <summary>Returns the grammar for the given fence language, or the fallback grammar for an unknown or empty one.</summary>
    public static Grammar Resolve(string? language)
        => language is { Length: > 0 } && _Grammars.TryGetValue(language, out var grammar) ? grammar : _Fallback;
}
