using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.EmphasisExtras;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace BlazingStory.Docs.IndexGenerator;

/// <summary>
/// One chunk of a documentation page, the unit that gets its own embedding vector and its own
/// entry in the search results.
/// </summary>
/// <param name="Slug">The slug of the page, the same value as in "sidebar.json".</param>
/// <param name="Anchor">The id of the heading this chunk belongs to, or an empty string for the lead of a page.</param>
/// <param name="Heading">The text of that heading, or the title of the page for the lead chunk.</param>
/// <param name="Text">The plain text of the chunk.</param>
/// <param name="EmbedText">The text to compute the embedding vector from, the heading path followed by <paramref name="Text"/>.</param>
public sealed record class ChunkSource(string Slug, string Anchor, string Heading, string Text, string EmbedText);

/// <summary>
/// Splits a Markdown document into the chunks that get indexed for the vector search.
/// </summary>
/// <remarks>
/// The heading anchors this class produces are what the search results link to, so they have to
/// match the ids the app generates when it renders the same document at runtime. That is why the
/// pipeline below has to keep using <c>UseAutoIdentifiers(AutoIdentifierOptions.GitHub)</c>, and
/// why <see cref="StripFrontMatter"/> has to keep behaving like its counterpart in "MarkdownService"
/// of the app: leaving the front matter in place would turn it into a thematic break plus a setext
/// heading, which would shift every heading id of the document.
/// </remarks>
public static class MarkdownChunker
{
    /// <summary>The marker of a GitHub alert block, which the app renders with its own extension.</summary>
    private static readonly Regex _AlertMarker = new(@"^\[!(NOTE|TIP|IMPORTANT|WARNING|CAUTION)\]\s*", RegexOptions.Compiled);

    private static readonly Regex _Whitespace = new(@"\s+", RegexOptions.Compiled);

    /// <summary>Every character of the Unicode symbol categories (Sm, Sc, Sk and So).</summary>
    private static readonly Regex _Symbols = new(@"\p{S}", RegexOptions.Compiled);

    private static readonly MarkdownPipeline _Pipeline = new MarkdownPipelineBuilder()
        .UsePipeTables()
        .UseTaskLists()
        .UseEmphasisExtras(EmphasisExtraOptions.Strikethrough)
        .UseAutoLinks()
        .UseFootnotes()
        .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
        .Build();

    /// <summary>
    /// Splits the given Markdown into chunks, one per heading section, further divided so that no
    /// chunk goes over the token limit of the embedding model.
    /// </summary>
    /// <param name="slug">The slug of the page the Markdown belongs to.</param>
    /// <param name="markdown">The content of the Markdown file, with its front matter still in place.</param>
    /// <param name="countTokens">Counts how many tokens a text takes, as the embedding model sees it.</param>
    /// <param name="maxTokens">The maximum number of tokens a single chunk may take.</param>
    public static IReadOnlyList<ChunkSource> Split(string slug, string markdown, Func<string, int> countTokens, int maxTokens = 200)
    {
        var document = Markdown.Parse(StripFrontMatter(markdown), _Pipeline);
        var title = GetTitle(document, slug);

        // Budgets are worked out on the text the model really sees, which is the normalized one.
        var countNormalizedTokens = (string text) => countTokens(NormalizeForEmbedding(text));

        var chunks = new List<ChunkSource>();
        foreach (var section in EnumerateSections(document, title))
        {
            var prefix = section.Heading == title ? title : $"{title} > {section.Heading}";
            foreach (var text in PackIntoChunks(section.Blocks, prefix, countNormalizedTokens, maxTokens))
            {
                chunks.Add(new ChunkSource(slug, section.Anchor, section.Heading, text, NormalizeForEmbedding($"{prefix} {text}")));
            }
        }
        return chunks;
    }

    /// <summary>
    /// Removes the YAML front matter block, but only when it is at the very top of the file.
    /// This has to stay identical to "MarkdownService.StripFrontMatter" of the app.
    /// </summary>
    public static string StripFrontMatter(string markdown)
    {
        var normalized = markdown.Replace("\r\n", "\n");
        if (!normalized.StartsWith("---\n")) return normalized;

        var closing = normalized.IndexOf("\n---", 3, StringComparison.Ordinal);
        if (closing < 0) return normalized;

        var endOfLine = normalized.IndexOf('\n', closing + 1);
        return endOfLine < 0 ? "" : normalized[(endOfLine + 1)..];
    }

    /// <summary>
    /// Walks the document from top to bottom and groups its blocks into sections, starting a new
    /// one at every level 2 heading. Deeper headings stay inside the section they belong to, as
    /// another piece of its text.
    /// </summary>
    private static IEnumerable<Section> EnumerateSections(MarkdownDocument document, string title)
    {
        var current = new Section("", title);

        foreach (var block in document)
        {
            if (block is HeadingBlock { Level: 1 }) continue;   // the title, already carried by every section

            if (block is HeadingBlock { Level: 2 } heading)
            {
                if (current.Blocks.Count > 0) yield return current;

                var headingText = GetPlainText(heading.Inline);
                current = new Section(heading.GetAttributes().Id ?? "", headingText.Length > 0 ? headingText : title);
                continue;
            }

            var text = GetBlockText(block);
            if (text.Length > 0) current.Blocks.Add(text);
        }

        if (current.Blocks.Count > 0) yield return current;
    }

    /// <summary>
    /// Fills chunks with as many blocks as fit under the token limit, and splits any single block
    /// that does not fit on its own into sentences.
    /// </summary>
    private static IEnumerable<string> PackIntoChunks(List<string> blocks, string prefix, Func<string, int> countTokens, int maxTokens)
    {
        var builder = new StringBuilder();

        foreach (var block in blocks)
        {
            foreach (var piece in Fit(block, prefix, countTokens, maxTokens))
            {
                var candidate = builder.Length == 0 ? piece : builder + " " + piece;
                if (builder.Length > 0 && countTokens($"{prefix} {candidate}") > maxTokens)
                {
                    yield return builder.ToString();
                    builder.Clear();
                    builder.Append(piece);
                }
                else
                {
                    builder.Clear();
                    builder.Append(candidate);
                }
            }
        }

        if (builder.Length > 0) yield return builder.ToString();
    }

    /// <summary>
    /// Breaks a block that is too long on its own into sentences, so that every piece fits under
    /// the token limit. A single sentence that still does not fit is cut short by words.
    /// </summary>
    private static IEnumerable<string> Fit(string block, string prefix, Func<string, int> countTokens, int maxTokens)
    {
        if (countTokens($"{prefix} {block}") <= maxTokens) { yield return block; yield break; }

        var builder = new StringBuilder();
        foreach (var sentence in SplitIntoSentences(block))
        {
            var piece = countTokens($"{prefix} {sentence}") <= maxTokens ? sentence : TrimToTokenLimit(sentence, prefix, countTokens, maxTokens);
            var candidate = builder.Length == 0 ? piece : builder + " " + piece;

            if (builder.Length > 0 && countTokens($"{prefix} {candidate}") > maxTokens)
            {
                yield return builder.ToString();
                builder.Clear();
                builder.Append(piece);
            }
            else
            {
                builder.Clear();
                builder.Append(candidate);
            }
        }

        if (builder.Length > 0) yield return builder.ToString();
    }

    private static IEnumerable<string> SplitIntoSentences(string text)
    {
        var sentences = Regex.Split(text, @"(?<=[.!?])\s+");
        return sentences.Select(sentence => sentence.Trim()).Where(sentence => sentence.Length > 0);
    }

    private static string TrimToTokenLimit(string sentence, string prefix, Func<string, int> countTokens, int maxTokens)
    {
        var words = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var builder = new StringBuilder();
        foreach (var word in words)
        {
            var candidate = builder.Length == 0 ? word : builder + " " + word;
            if (countTokens($"{prefix} {candidate}") > maxTokens) break;
            builder.Clear();
            builder.Append(candidate);
        }
        return builder.Length > 0 ? builder.ToString() : sentence;
    }

    /// <summary>
    /// Turns one block into plain text. Fenced and indented code blocks are left out on purpose:
    /// the embedding model is trained on natural language, and averaging a mass of code tokens into
    /// the vector pulls every code heavy chunk toward the same spot of the embedding space. Inline
    /// code stays, since it is part of a sentence.
    /// </summary>
    private static string GetBlockText(Block block)
    {
        var builder = new StringBuilder();
        AppendBlockText(block, builder);
        return _AlertMarker.Replace(NormalizeWhitespace(builder.ToString()), "");
    }

    /// <summary>
    /// Collapses every run of white space into a single space.
    /// </summary>
    /// <remarks>
    /// This is not only about tidiness. "Microsoft.ML.Tokenizers" does not treat a line feed as a
    /// word boundary, while the tokenizer of transformers.js does, so a text with a line feed in it
    /// gets a different vector on each side: "Documentation\nBy" becomes "documentation" + "##by"
    /// here but "documentation" + "by" in the browser. Every chunk joins a heading and its blocks
    /// together, so leaving the line feeds in would put that mismatch into every single vector.
    /// </remarks>
    private static string NormalizeWhitespace(string text) => _Whitespace.Replace(text, " ").Trim();

    /// <summary>
    /// Prepares a text for the embedding model by dropping every symbol character and tidying the
    /// white space that leaves behind.
    /// </summary>
    /// <remarks>
    /// The two tokenizers disagree on symbols: "Microsoft.ML.Tokenizers" drops every character of
    /// the Unicode symbol categories, while the tokenizer of transformers.js keeps each of them as
    /// its own token. Characters such as "&lt;", "&gt;", "=" and "→" are common in this content, so
    /// that difference would show up in most vectors. Removing them on both sides is what keeps the
    /// index and the search queries in the same embedding space.
    /// <para>
    /// The counterpart of this method lives in "wwwroot/js/search-embeddings.js" of the app, and the
    /// two have to keep doing the same thing.
    /// </para>
    /// </remarks>
    public static string NormalizeForEmbedding(string text) => NormalizeWhitespace(_Symbols.Replace(text, " "));

    private static void AppendBlockText(Block block, StringBuilder builder)
    {
        switch (block)
        {
            case CodeBlock:
            case ThematicBreakBlock:
            case LinkReferenceDefinitionGroup:
                return;

            case LeafBlock leaf:
                var text = GetPlainText(leaf.Inline);
                if (text.Length == 0) return;
                if (builder.Length > 0) builder.Append(' ');
                builder.Append(text);
                return;

            case ContainerBlock container:
                foreach (var child in container) AppendBlockText(child, builder);
                return;
        }
    }

    private static string GetTitle(MarkdownDocument document, string fallback)
    {
        var heading = document.Descendants<HeadingBlock>().FirstOrDefault(heading => heading.Level == 1);
        var title = heading is null ? "" : GetPlainText(heading.Inline);
        return string.IsNullOrWhiteSpace(title) ? fallback : title;
    }

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

    private sealed record class Section(string Anchor, string Heading)
    {
        public List<string> Blocks { get; } = [];
    }
}
