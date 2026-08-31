using System.Text.RegularExpressions;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace BlazingStory.Docs.MarkdownRendering;

/// <summary>
/// Renders the <c>title="..."</c> argument of a fenced code block as a title bar above the code,
/// the same way Docusaurus does.
/// </summary>
public sealed class CodeBlockTitleExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is not HtmlRenderer htmlRenderer) return;
        if (htmlRenderer.ObjectRenderers.FindExact<TitledCodeBlockRenderer>() is not null) return;

        htmlRenderer.ObjectRenderers.TryRemove<CodeBlockRenderer>();
        htmlRenderer.ObjectRenderers.Add(new TitledCodeBlockRenderer());
    }
}

public static class CodeBlockTitleExtensionMethods
{
    public static MarkdownPipelineBuilder UseCodeBlockTitles(this MarkdownPipelineBuilder pipeline)
    {
        pipeline.Extensions.AddIfNotAlready<CodeBlockTitleExtension>();
        return pipeline;
    }
}

internal sealed partial class TitledCodeBlockRenderer : CodeBlockRenderer
{
    [GeneratedRegex("title\\s*=\\s*\"(?<title>[^\"]*)\"")]
    private static partial Regex TitleArgument();

    protected override void Write(HtmlRenderer renderer, CodeBlock obj)
    {
        var title = GetTitle(obj);

        renderer.EnsureLine();
        renderer.Write("<div class=\"code-block\">");

        if (title is not null)
        {
            renderer.Write("<div class=\"code-block-title\">");
            renderer.WriteEscape(title);
            renderer.Write("</div>");
        }

        base.Write(renderer, obj);
        renderer.WriteLine("</div>");
    }

    private static string? GetTitle(CodeBlock codeBlock)
    {
        if (codeBlock is not FencedCodeBlock { Arguments: { Length: > 0 } arguments }) return null;

        var match = TitleArgument().Match(arguments);
        return match.Success ? match.Groups["title"].Value : null;
    }
}
