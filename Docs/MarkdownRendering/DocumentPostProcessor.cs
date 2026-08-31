using System.Text;
using BlazingStory.Docs.Models;
using BlazingStory.Docs.Services;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace BlazingStory.Docs.MarkdownRendering;

/// <summary>
/// Adjusts a parsed Markdown document so that it can be rendered inside the single page
/// application: code fence languages are normalized and every relative link and image path
/// is rewritten to an application URL.
/// </summary>
public static class DocumentPostProcessor
{
    private static readonly Dictionary<string, string> _LanguageAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["cs"] = "csharp",
        ["sh"] = "bash",
        ["shell"] = "bash",
        ["razor"] = "cshtml",
        ["xml"] = "markup",
        ["html"] = "markup",
    };

    private static readonly string[] _AbsoluteUrlPrefixes = ["http://", "https://", "//", "mailto:", "tel:", "data:", "/"];

    public static void NormalizeCodeBlockLanguages(MarkdownDocument document)
    {
        foreach (var codeBlock in document.Descendants<FencedCodeBlock>())
        {
            if (codeBlock.Info is { Length: > 0 } info && _LanguageAliases.TryGetValue(info, out var normalized))
            {
                codeBlock.Info = normalized;
            }
        }
    }

    public static void RewriteLinks(MarkdownDocument document, string version, string defaultVersion, string slug)
    {
        var documentDirectory = slug.LastIndexOf('/') is var separator && separator >= 0 ? slug[..separator] : "";
        var currentUrl = DocRoutes.Doc(version, defaultVersion, slug);

        foreach (var link in document.Descendants<LinkInline>())
        {
            var url = link.Url;
            if (string.IsNullOrEmpty(url)) continue;

            // A fragment only link must keep the current document in the URL, otherwise the
            // <base href> of the page would turn it into a link to the site root.
            if (url.StartsWith('#'))
            {
                link.Url = currentUrl + url;
                continue;
            }

            if (IsAbsolute(url))
            {
                if (!link.IsImage)
                {
                    var attributes = link.GetAttributes();
                    attributes.AddProperty("target", "_blank");
                    attributes.AddProperty("rel", "noopener noreferrer");
                }
                continue;
            }

            var (path, fragment) = SplitFragment(url);
            var resolved = ResolveRelative(documentDirectory, path);

            link.Url = link.IsImage
                ? DocRoutes.ContentFile(version, resolved)
                : DocRoutes.Doc(version, defaultVersion, TrimMarkdownExtension(resolved)) + fragment;
        }
    }

    public static string GetTitle(MarkdownDocument document, string fallback)
    {
        var heading = document.Descendants<HeadingBlock>().FirstOrDefault(heading => heading.Level == 1);
        var title = heading is null ? "" : GetPlainText(heading.Inline);
        return string.IsNullOrWhiteSpace(title) ? fallback : title;
    }

    public static IReadOnlyList<TocEntry> BuildTableOfContents(MarkdownDocument document)
    {
        var entries = new List<TocEntry>();
        foreach (var heading in document.Descendants<HeadingBlock>())
        {
            if (heading.Level is not (2 or 3)) continue;

            var id = heading.GetAttributes().Id;
            if (string.IsNullOrEmpty(id)) continue;

            entries.Add(new TocEntry(id, GetPlainText(heading.Inline), heading.Level));
        }
        return entries;
    }

    private static bool IsAbsolute(string url) => _AbsoluteUrlPrefixes.Any(prefix => url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

    private static (string Path, string Fragment) SplitFragment(string url)
    {
        var index = url.IndexOf('#');
        return index < 0 ? (url, "") : (url[..index], url[index..]);
    }

    private static string ResolveRelative(string documentDirectory, string relativePath)
    {
        var baseUri = new Uri("https://docs.invalid/" + (documentDirectory.Length > 0 ? documentDirectory + "/" : ""));
        return new Uri(baseUri, relativePath).AbsolutePath.TrimStart('/');
    }

    private static string TrimMarkdownExtension(string path)
        => path.EndsWith(".md", StringComparison.OrdinalIgnoreCase) ? path[..^3] : path;

    private static string GetPlainText(ContainerInline? container)
    {
        var builder = new StringBuilder();
        AppendPlainText(container?.FirstChild, builder);
        return builder.ToString().Trim();
    }

    private static void AppendPlainText(Inline? inline, StringBuilder builder)
    {
        for (var node = inline; node is not null; node = node.NextSibling)
        {
            switch (node)
            {
                case LinkInline { IsImage: true }:
                    break;
                case LiteralInline literal:
                    builder.Append(literal.Content.AsSpan());
                    break;
                case CodeInline code:
                    builder.Append(code.Content);
                    break;
                case LineBreakInline:
                    builder.Append(' ');
                    break;
                case ContainerInline nested:
                    AppendPlainText(nested.FirstChild, builder);
                    break;
            }
        }
    }
}
