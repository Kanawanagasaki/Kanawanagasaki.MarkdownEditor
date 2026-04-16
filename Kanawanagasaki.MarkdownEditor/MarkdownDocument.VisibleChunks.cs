using System.Text;
using Kanawanagasaki.MarkdownEditor.Ast;

namespace Kanawanagasaki.MarkdownEditor;

public partial class MarkdownDocument
{
    /// <summary>
    /// Returns a flattened list of visible text chunks with their associated styles.
    /// Block-level styles (heading, list item, blockquote) and inline styles
    /// (bold, italic, strikethrough, code, link) are tracked per chunk.
    /// </summary>
    public List<VisibleTextChunk> GetVisibleChunks()
    {
        var chunks = new List<VisibleTextChunk>();
        CollectBlockChunks(Children, chunks, [], true);
        return chunks;
    }

    /// <summary>
    /// Returns only the visible text content (without markdown markers) as a single string.
    /// </summary>
    public string GetPlainText()
    {
        var sb = new StringBuilder();
        foreach (var chunk in GetVisibleChunks())
            sb.Append(chunk.Text);
        return sb.ToString();
    }

    private void CollectBlockChunks(
        IReadOnlyList<Block> blocks,
        List<VisibleTextChunk> chunks,
        List<TextChunkStyle> blockStyles,
        bool isTopLevel)
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            // Add separator between sibling blocks
            if (i > 0)
                chunks.Add(new VisibleTextChunk(isTopLevel ? "\n\n" : "\n", []));

            var block = blocks[i];

            switch (block)
            {
                case HeadingBlock heading:
                {
                    var styles = new List<TextChunkStyle>(blockStyles)
                    {
                        TextChunkStyle.Heading(heading.Level)
                    };
                    CollectInlineChunks(heading.Inline, chunks, styles);
                    break;
                }

                case ParagraphBlock para:
                    CollectInlineChunks(para.Inline, chunks, blockStyles);
                    break;

                case ListBlock list:
                    CollectListChunks(list, chunks, blockStyles);
                    break;

                case QuoteBlock quote:
                {
                    var styles = new List<TextChunkStyle>(blockStyles)
                    {
                        TextChunkStyle.Blockquote(quote.NestingLevel)
                    };
                    CollectBlockChunks(quote.Children, chunks, styles, false);
                    break;
                }

                case ThematicBreakBlock:
                    // Thematic breaks produce no visible text
                    break;
            }
        }
    }

    private void CollectListChunks(
        ListBlock list,
        List<VisibleTextChunk> chunks,
        List<TextChunkStyle> parentStyles)
    {
        for (int i = 0; i < list.Children.Count; i++)
        {
            if (i > 0)
                chunks.Add(new VisibleTextChunk("\n", []));

            if (list.Children[i] is not ListItemBlock listItem)
                continue;

            var itemStyles = new List<TextChunkStyle>(parentStyles);
            if (list.ListType == ListType.Ordered)
                itemStyles.Add(TextChunkStyle.OrderedList(list.StartNumber + i));
            else
                itemStyles.Add(TextChunkStyle.UnorderedList());

            CollectBlockChunks(listItem.Children, chunks, itemStyles, false);
        }
    }

    private void CollectInlineChunks(
        ContainerInline? container,
        List<VisibleTextChunk> chunks,
        List<TextChunkStyle> blockStyles)
    {
        if (container is null) return;
        CollectInlineChunksRecursive(container.FirstChild, chunks, blockStyles, []);
    }

    private void CollectInlineChunksRecursive(
        Inline? inline,
        List<VisibleTextChunk> chunks,
        List<TextChunkStyle> blockStyles,
        List<TextChunkStyle> inlineStyles)
    {
        while (inline is not null)
        {
            switch (inline)
            {
                case LiteralInline lit:
                    if (lit.Content.Length > 0)
                        chunks.Add(new VisibleTextChunk(lit.Content, [..blockStyles, ..inlineStyles]));
                    break;

                case CodeInline code:
                {
                    var styles = new List<TextChunkStyle>(inlineStyles)
                    {
                        TextChunkStyle.Code()
                    };
                    if (code.Content.Length > 0)
                        chunks.Add(new VisibleTextChunk(code.Content, [..blockStyles, ..styles]));
                    break;
                }

                case EmphasisInline emph:
                {
                    var styles = new List<TextChunkStyle>(inlineStyles);
                    if (emph.DelimiterChar is '*' or '_')
                    {
                        if (emph.DelimiterCount == 3)
                        {
                            styles.Add(TextChunkStyle.Bold());
                            styles.Add(TextChunkStyle.Italic());
                        }
                        else if (emph.DelimiterCount == 2)
                        {
                            styles.Add(TextChunkStyle.Bold());
                        }
                        else if (emph.DelimiterCount == 1)
                        {
                            styles.Add(TextChunkStyle.Italic());
                        }
                    }
                    else if (emph.DelimiterChar == '~')
                    {
                        styles.Add(TextChunkStyle.Strikethrough());
                    }

                    CollectInlineChunksRecursive(emph.FirstChild, chunks, blockStyles, styles);
                    break;
                }

                case LinkInline link:
                {
                    var styles = new List<TextChunkStyle>(inlineStyles)
                    {
                        link.IsImage
                            ? TextChunkStyle.Image(link.Url, link.Title)
                            : TextChunkStyle.Link(link.Url, link.Title)
                    };
                    CollectInlineChunksRecursive(link.FirstChild, chunks, blockStyles, styles);
                    break;
                }

                case LineBreakInline:
                    chunks.Add(new VisibleTextChunk("\n", [..blockStyles, ..inlineStyles]));
                    break;

                case HtmlEntityInline entity:
                    if (entity.Transcoded.Length > 0)
                        chunks.Add(new VisibleTextChunk(entity.Transcoded, [..blockStyles, ..inlineStyles]));
                    break;

                case HtmlInline html:
                    if (html.Content.Length > 0)
                        chunks.Add(new VisibleTextChunk(html.Content, [..blockStyles, ..inlineStyles]));
                    break;

                case AutolinkInline auto:
                    if (auto.Url.Length > 0)
                        chunks.Add(new VisibleTextChunk(auto.Url, [..blockStyles, ..inlineStyles]));
                    break;
            }

            inline = inline.NextSibling;
        }
    }
}
