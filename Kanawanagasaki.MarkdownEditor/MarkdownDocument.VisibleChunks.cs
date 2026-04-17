using System.Text;
using Kanawanagasaki.MarkdownEditor.Ast;

namespace Kanawanagasaki.MarkdownEditor;

public partial class MarkdownDocument
{
    public List<VisibleTextChunk> GetVisibleChunks()
    {
        var chunks = new List<VisibleTextChunk>();
        int runningOffset = 0;
        CollectBlockChunks(Children, chunks, [], true, ref runningOffset);
        return chunks;
    }

    public string GetPlainText()
    {
        var sb = new StringBuilder();
        foreach (var chunk in GetVisibleChunks())
            sb.Append(chunk.Text);
        return sb.ToString();
    }

    private static bool LastLeafEndsWithLineBreak(Block block)
    {
        if (block is ParagraphBlock para)
            return para.HasTrailingLineBreak;
        if (block is LeafBlock leaf)
            return leaf.Inline?.LastChild is LineInline;
        if (block is ContainerBlock container && container.Children.Count > 0)
            return LastLeafEndsWithLineBreak(container.Children[^1]);
        return false;
    }

    private void CollectBlockChunks(
        IReadOnlyList<Block> blocks,
        List<VisibleTextChunk> chunks,
        List<TextChunkStyle> blockStyles,
        bool isTopLevel,
        ref int runningOffset)
    {
        Block? lastRendered = null;
        bool hadEmptyParagraphBefore = false;

        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i] is ParagraphBlock p && IsEmptyParagraph(p))
            {
                hadEmptyParagraphBefore = true;
                continue;
            }

            if (lastRendered is not null)
            {
                bool prevEndsWithLB = LastLeafEndsWithLineBreak(lastRendered);
                if (!prevEndsWithLB)
                {
                    string sep = hadEmptyParagraphBefore
                        ? (isTopLevel ? "\n\n" : "\n")
                        : "\n";
                    chunks.Add(new VisibleTextChunk(sep, runningOffset, []));
                    runningOffset += sep.Length;
                }
            }

            lastRendered = blocks[i];
            hadEmptyParagraphBefore = false;

            switch (blocks[i])
            {
                case HeadingBlock heading:
                {
                    var styles = new List<TextChunkStyle>(blockStyles)
                    {
                        TextChunkStyle.Heading(heading.Level, heading)
                    };
                    CollectInlineChunks(heading.Inline, chunks, styles, ref runningOffset);
                    break;
                }

                case ParagraphBlock para:
                    CollectInlineChunks(para.Inline, chunks, blockStyles, ref runningOffset);
                    break;

                case ListBlock list:
                    CollectListChunks(list, chunks, blockStyles, ref runningOffset);
                    break;

                case QuoteBlock quote:
                {
                    var styles = new List<TextChunkStyle>(blockStyles)
                    {
                        TextChunkStyle.Blockquote(quote.NestingLevel, quote)
                    };
                    CollectBlockChunks(quote.Children, chunks, styles, false, ref runningOffset);
                    break;
                }

                case ThematicBreakBlock:
                    break;
            }
        }
    }

    private void CollectListChunks(
        ListBlock list,
        List<VisibleTextChunk> chunks,
        List<TextChunkStyle> parentStyles,
        ref int runningOffset)
    {
        for (int i = 0; i < list.Children.Count; i++)
        {
            if (i > 0)
            {
                bool prevEndsWithLB = false;
                if (list.Children[i - 1] is ListItemBlock prevItem
                    && prevItem.Children.Count > 0
                    && prevItem.Children[0] is ParagraphBlock prevPara)
                {
                    prevEndsWithLB = prevPara.HasTrailingLineBreak;
                }
                if (!prevEndsWithLB)
                {
                    chunks.Add(new VisibleTextChunk("\n", runningOffset, []));
                    runningOffset += 1;
                }
            }

            if (list.Children[i] is not ListItemBlock listItem)
                continue;

            var itemStyles = new List<TextChunkStyle>(parentStyles);
            if (list.ListType == ListType.Ordered)
                itemStyles.Add(TextChunkStyle.OrderedList(list.StartNumber + i, list));
            else
                itemStyles.Add(TextChunkStyle.UnorderedList(list));

            CollectBlockChunks(listItem.Children, chunks, itemStyles, false, ref runningOffset);
        }
    }

    private void CollectInlineChunks(
        ContainerInline? container,
        List<VisibleTextChunk> chunks,
        List<TextChunkStyle> blockStyles,
        ref int runningOffset)
    {
        if (container is null) return;
        CollectInlineChunksRecursive(container.FirstChild, chunks, blockStyles, [], ref runningOffset);
    }

    private void CollectInlineChunksRecursive(
        Inline? inline,
        List<VisibleTextChunk> chunks,
        List<TextChunkStyle> blockStyles,
        List<TextChunkStyle> inlineStyles,
        ref int runningOffset)
    {
        while (inline is not null)
        {
            switch (inline)
            {
                case LiteralInline lit:
                    if (lit.Content.Length > 0)
                    {
                        chunks.Add(new VisibleTextChunk(lit.Content, runningOffset, [..blockStyles, ..inlineStyles]));
                        runningOffset += lit.Content.Length;
                    }
                    break;

                case CodeInline code:
                {
                    var styles = new List<TextChunkStyle>(inlineStyles)
                    {
                        TextChunkStyle.Code(code)
                    };
                    if (code.Content.Length > 0)
                    {
                        chunks.Add(new VisibleTextChunk(code.Content, runningOffset, [..blockStyles, ..styles]));
                        runningOffset += code.Content.Length;
                    }
                    break;
                }

                case EmphasisInline emph:
                {
                    var styles = new List<TextChunkStyle>(inlineStyles);
                    if (emph.DelimiterChar is '*' or '_')
                    {
                        if (emph.DelimiterCount == 3)
                        {
                            styles.Add(TextChunkStyle.Bold(emph));
                            styles.Add(TextChunkStyle.Italic(emph));
                        }
                        else if (emph.DelimiterCount == 2)
                        {
                            styles.Add(TextChunkStyle.Bold(emph));
                        }
                        else if (emph.DelimiterCount == 1)
                        {
                            styles.Add(TextChunkStyle.Italic(emph));
                        }
                    }
                    else if (emph.DelimiterChar == '~')
                    {
                        styles.Add(TextChunkStyle.Strikethrough(emph));
                    }

                    CollectInlineChunksRecursive(emph.FirstChild, chunks, blockStyles, styles, ref runningOffset);
                    break;
                }

                case LinkInline link:
                {
                    var styles = new List<TextChunkStyle>(inlineStyles)
                    {
                        link.IsImage
                            ? TextChunkStyle.Image(link.Url, link.Title, link)
                            : TextChunkStyle.Link(link.Url, link.Title, link)
                    };
                    CollectInlineChunksRecursive(link.FirstChild, chunks, blockStyles, styles, ref runningOffset);
                    break;
                }

                case LineInline lineInline:
                    CollectInlineChunksRecursive(lineInline.FirstChild, chunks, blockStyles, inlineStyles, ref runningOffset);
                    if (lineInline.NextSibling is LineInline)
                    {
                        chunks.Add(new VisibleTextChunk("\n", runningOffset, [..blockStyles, ..inlineStyles]));
                        runningOffset += 1;
                    }
                    break;

                case HtmlEntityInline entity:
                    if (entity.Transcoded.Length > 0)
                    {
                        chunks.Add(new VisibleTextChunk(entity.Transcoded, runningOffset, [..blockStyles, ..inlineStyles]));
                        runningOffset += entity.Transcoded.Length;
                    }
                    break;

                case HtmlInline html:
                    if (html.Content.Length > 0)
                    {
                        chunks.Add(new VisibleTextChunk(html.Content, runningOffset, [..blockStyles, ..inlineStyles]));
                        runningOffset += html.Content.Length;
                    }
                    break;

                case AutolinkInline auto:
                    if (auto.Url.Length > 0)
                    {
                        chunks.Add(new VisibleTextChunk(auto.Url, runningOffset, [..blockStyles, ..inlineStyles]));
                        runningOffset += auto.Url.Length;
                    }
                    break;
            }

            inline = inline.NextSibling;
        }
    }
}
