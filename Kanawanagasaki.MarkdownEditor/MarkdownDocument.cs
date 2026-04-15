using System.Text;
using Kanawanagasaki.MarkdownEditor.Ast;
using Kanawanagasaki.MarkdownEditor.Editing;

namespace Kanawanagasaki.MarkdownEditor;

public partial class MarkdownDocument : Ast.MarkdownDocument
{
    public void Write(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        var paragraph = GetOrCreateLastParagraph();
        AppendLiteralToParagraph(paragraph, text);
    }

    public void WriteLine(string? text = null)
    {
        if (!string.IsNullOrEmpty(text))
        {
            if (Children.Count > 0 && Children[^1] is LeafBlock lastLeaf
                && lastLeaf.Inline is not null
                && lastLeaf.Inline.LastChild is not LineBreakInline)
            {
                lastLeaf.Inline.AppendChild(new LineBreakInline());
            }

            var paragraph = new ParagraphBlock();
            paragraph.Inline = new InlineRoot();
            paragraph.Inline.AppendChild(new LiteralInline(text));
            paragraph.Inline.AppendChild(new LineBreakInline());
            AddChild(paragraph);
            _nextWriteCreatesNewParagraph = false;
        }
        else
        {
            var paragraph = GetOrCreateLastParagraph();
            paragraph.Inline ??= new InlineRoot();
            paragraph.Inline.AppendChild(new LineBreakInline());
        }
    }

    public void WriteParagraph(string? text = null)
    {
        if (text is not null)
            Write(text);
        _nextWriteCreatesNewParagraph = true;
    }

    public void InsertLine(int lineOffset, string text)
    {
        if (lineOffset <= 0)
        {
            ParagraphBlock paragraph;
            if (Children.Count > 0 && Children[0] is ParagraphBlock firstPara)
            {
                paragraph = firstPara;
            }
            else
            {
                paragraph = new ParagraphBlock();
                InsertChild(0, paragraph);
            }

            paragraph.Inline ??= new InlineRoot();
            paragraph.Inline.PrependChild(new LineBreakInline());
            if (!string.IsNullOrEmpty(text))
                paragraph.Inline.PrependChild(new LiteralInline(text));
            return;
        }

        int cumulativeLBs = 0;
        for (int blockIndex = 0; blockIndex < Children.Count; blockIndex++)
        {
            if (Children[blockIndex] is LeafBlock leaf && leaf.Inline is not null)
            {
                Inline? targetLineBreak = null;
                int localLBs = 0;
                var child = leaf.Inline.FirstChild;
                while (child is not null)
                {
                    if (child is LineBreakInline)
                    {
                        if (cumulativeLBs + localLBs == lineOffset - 1)
                            targetLineBreak = child;
                        localLBs++;
                    }
                    child = child.NextSibling;
                }

                if (targetLineBreak is not null)
                {
                    Inline lastInserted = targetLineBreak;
                    if (!string.IsNullOrEmpty(text))
                    {
                        var lit = new LiteralInline(text);
                        InlineSplitter.InsertAfterInline(lastInserted, lit);
                        lastInserted = lit;
                    }
                    InlineSplitter.InsertAfterInline(lastInserted, new LineBreakInline());
                    return;
                }

                cumulativeLBs += localLBs;
            }
        }

        ParagraphBlock lastParagraph;
        if (Children.Count > 0 && Children[^1] is ParagraphBlock lp)
        {
            lastParagraph = lp;
        }
        else
        {
            lastParagraph = new ParagraphBlock();
            AddChild(lastParagraph);
        }

        lastParagraph.Inline ??= new InlineRoot();
        lastParagraph.Inline.AppendChild(new LineBreakInline());
        if (!string.IsNullOrEmpty(text))
        {
            lastParagraph.Inline.AppendChild(new LiteralInline(text));
        }
        lastParagraph.Inline.AppendChild(new LineBreakInline());
    }

    public void InsertParagraph(int lineOffset, string text)
    {
        var paragraph = new ParagraphBlock();
        if (!string.IsNullOrEmpty(text))
        {
            paragraph.Inline ??= new InlineRoot();
            paragraph.Inline.AppendChild(new LiteralInline(text));
        }

        if (lineOffset <= 0)
        {
            InsertChild(0, paragraph);
        }
        else if (lineOffset >= Children.Count)
        {
            if (Children.Count > 1)
                InsertChild(Children.Count - 1, paragraph);
            else
                AddChild(paragraph);
        }
        else
        {
            InsertChild(lineOffset, paragraph);
        }
    }

    public void RemoveText(LeafBlock block, int start, int end)
    {
        ArgumentNullException.ThrowIfNull(block);

        if (block.Inline is null)
            return;

        var map = InlineOffsetMap.Build(block);
        if (start < 0) start = 0;
        if (end > map.TotalLength) end = map.TotalLength;
        if (start >= end || map.TotalLength == 0)
            return;

        var inclusiveEnd = end - 1;
        var entries = map.GetEntriesInRange(start, inclusiveEnd);
        foreach (var entry in entries)
        {
            var relStart = Math.Max(0, start - entry.Start);
            var relEnd = Math.Min(entry.Length - 1, inclusiveEnd - entry.Start);

            if (entry.Inline is LiteralInline lit)
            {
                RemoveFromLiteral(lit, relStart, relEnd, entry.ContentOffset);
            }
            else if (entry.Inline is CodeInline code)
            {
                RemoveFromCodeInline(code, relStart, relEnd);
            }
        }

        if (block.Inline?.FirstChild is null)
        {
            if (block.Parent is ContainerBlock container)
                container.RemoveChild(block);
        }
    }

    public void RemoveText(int start, int end)
    {
        var block = GetLastParagraph() ?? throw new InvalidOperationException("Document has no paragraph to remove text from.");
        RemoveText(block, start, end);
    }

    public void ConvertToHeading(LeafBlock block, int level)
    {
        ArgumentNullException.ThrowIfNull(block);

        if (level <= 0)
        {
            if (block is HeadingBlock existingHeading)
            {
                var paragraph = new ParagraphBlock
                {
                    Inline = existingHeading.Inline,
                    Span = existingHeading.Span,
                    Line = existingHeading.Line,
                    Column = existingHeading.Column
                };
                if (paragraph.Inline is not null)
                    paragraph.Inline.Parent = paragraph;
                ReplaceBlockInParent(block, paragraph);
            }
            return;
        }

        if (level > 6) level = 6;

        if (block is HeadingBlock existingHeadingBlock)
        {
            existingHeadingBlock.Level = level;
            return;
        }

        var newHeading = new HeadingBlock(level)
        {
            Inline = block.Inline,
            Span = block.Span,
            Line = block.Line,
            Column = block.Column
        };

        if (newHeading.Inline is not null)
            newHeading.Inline.Parent = newHeading;

        ReplaceBlockInParent(block, newHeading);
    }

    public void ConvertToHeading(int lineIndex, int level)
    {
        var block = GetBlockAtLine(lineIndex);
        if (block is LeafBlock leaf)
            ConvertToHeading(leaf, level);
    }

    public void ConvertToBlockquote(Block block)
    {
        ArgumentNullException.ThrowIfNull(block);

        var parent = block.Parent as ContainerBlock;
        if (parent is null)
            throw new InvalidOperationException("Block has no parent container.");

        var index = parent.IndexOfChild(block);
        if (index < 0) return;

        if (index > 0 && parent.Children[index - 1] is QuoteBlock prevQuote)
        {
            parent.RemoveChildAt(index);
            AddBlockToQuote(prevQuote, block);
            return;
        }

        if (index < parent.Children.Count - 1 && parent.Children[index + 1] is QuoteBlock nextQuote)
        {
            parent.RemoveChildAt(index);
            InsertBlockIntoQuote(nextQuote, block, 0);
            return;
        }

        var quote = new QuoteBlock { NestingLevel = 1 };
        parent.RemoveChildAt(index);
        AddBlockToQuote(quote, block);
        parent.InsertChild(index, quote);
    }

    private static void AddBlockToQuote(QuoteBlock quote, Block block)
    {
        if (block is LeafBlock leaf)
        {
            quote.AddChild(leaf);
        }
        else if (block is ListBlock listBlock)
        {
            quote.AddChild(listBlock);
        }
        else if (block is ContainerBlock container)
        {
            foreach (var child in container.Children.ToList())
            {
                container.RemoveChild(child);
                quote.AddChild(child);
            }
        }
    }

    private static void InsertBlockIntoQuote(QuoteBlock quote, Block block, int insertIndex)
    {
        if (block is LeafBlock leaf)
        {
            quote.InsertChild(insertIndex, leaf);
        }
        else if (block is ListBlock listBlock)
        {
            quote.InsertChild(insertIndex, listBlock);
        }
        else if (block is ContainerBlock container)
        {
            foreach (var child in container.Children.ToList())
            {
                container.RemoveChild(child);
                quote.InsertChild(insertIndex, child);
                insertIndex++;
            }
        }
    }

    public void ConvertToBlockquote(int lineIndex)
    {
        var block = GetBlockAtLine(lineIndex);
        ConvertToBlockquote(block);
    }

    public void ConvertToBlockquote(int lineIndex, int nestedLevel)
    {
        var block = GetBlockAtLine(lineIndex);

        if (nestedLevel <= 0)
        {
            if (block.Parent is QuoteBlock quote)
            {
                var parent = quote.Parent as ContainerBlock;
                if (parent is not null)
                {
                    var index = parent.IndexOfChild(quote);
                    if (index >= 0)
                    {
                        parent.RemoveChildAt(index);
                        var insertIdx = index;
                        foreach (var child in quote.Children.ToList())
                        {
                            quote.RemoveChild(child);
                            parent.InsertChild(insertIdx, child);
                            insertIdx++;
                        }
                    }
                }
            }
            return;
        }

        if (block.Parent is QuoteBlock existingQuote)
        {
            existingQuote.NestingLevel = nestedLevel;
            return;
        }

        ConvertToBlockquote(block);
        if (block.Parent is QuoteBlock newQuote)
        {
            newQuote.NestingLevel = nestedLevel;
        }
    }

    public void ConvertToOrderedList(Block block)
    {
        ConvertToList(block, ListType.Ordered);
    }

    public void ConvertToOrderedList(int lineIndex)
    {
        var block = GetBlockAtLine(lineIndex);
        ConvertToOrderedList(block);
    }

    public void ConvertToUnorderedList(Block block)
    {
        ConvertToList(block, ListType.Unordered);
    }

    public void ConvertToUnorderedList(int lineIndex)
    {
        var block = GetBlockAtLine(lineIndex);
        ConvertToUnorderedList(block);
    }

    public void InsertHorizontalRule()
    {
        var hr = new ThematicBreakBlock();
        AddChild(hr);
        _nextWriteCreatesNewParagraph = true;
    }

    public void InsertHorizontalRule(int lineIndex)
    {
        var hr = new ThematicBreakBlock();
        if (lineIndex <= 0)
            InsertChild(0, hr);
        else if (lineIndex >= Children.Count)
            AddChild(hr);
        else
            InsertChild(lineIndex, hr);
    }

    public void ApplyBold(LeafBlock block, int start, int end)
    {
        ApplyEmphasisStyle(block, start, end, '*', 2);
    }

    public void ApplyBold(int start, int end)
    {
        var block = FindLeafBlockForRange(start, end) ?? throw new InvalidOperationException("Document has no paragraph.");
        ApplyBold(block, start, end);
    }

    public void ApplyItalic(LeafBlock block, int start, int end)
    {
        ApplyEmphasisStyle(block, start, end, '*', 1);
    }

    public void ApplyItalic(int start, int end)
    {
        var block = GetLastParagraph() ?? throw new InvalidOperationException("Document has no paragraph.");
        ApplyItalic(block, start, end);
    }

    public void ApplyCode(LeafBlock block, int start, int end)
    {
        ArgumentNullException.ThrowIfNull(block);
        if (block.Inline is null) return;

        var map = InlineOffsetMap.Build(block);
        if (start < 0) start = 0;
        if (end > map.TotalLength) end = map.TotalLength;
        if (start >= end) return;

        var inclusiveEnd = end - 1;

        SplitAtOffset(block, start);
        map = InlineOffsetMap.Build(block);

        if (end < map.TotalLength)
        {
            SplitAtOffset(block, end);
            map = InlineOffsetMap.Build(block);
        }

        var entries = map.GetEntriesInRange(start, inclusiveEnd);
        if (entries.Count == 0) return;

        var inlinesToWrap = CollectInlinesToWrap(entries, start, inclusiveEnd, map);
        if (inlinesToWrap.Count == 0) return;

        var sb = new StringBuilder();
        foreach (var entry in entries)
        {
            if (entry.Inline is LiteralInline lit)
                sb.Append(lit.Content);
            else if (entry.Inline is CodeInline code)
                sb.Append(code.Content);
        }

        var codeContent = sb.ToString();

        var codeInline = new CodeInline(codeContent);
        if (inlinesToWrap.Count == 1)
        {
            InlineSplitter.ReplaceInline(inlinesToWrap[0], codeInline);
        }
        else
        {
            var firstInline = inlinesToWrap[0];
            var parentInline = firstInline.ParentInline ?? block.Inline!;

            var insertBefore = firstInline.PreviousSibling;
            var insertAsFirstChild = insertBefore is null;

            foreach (var inline in inlinesToWrap)
            {
                inline.Remove();
            }

            if (insertAsFirstChild)
            {
                if (parentInline.FirstChild is not null)
                    InlineSplitter.InsertBeforeInline(parentInline.FirstChild, codeInline);
                else
                    parentInline.AppendChild(codeInline);
            }
            else
            {
                InlineSplitter.InsertAfterInline(insertBefore!, codeInline);
            }
        }
    }

    public void ApplyCode(int start, int end)
    {
        var block = GetLastParagraph() ?? throw new InvalidOperationException("Document has no paragraph.");
        ApplyCode(block, start, end);
    }

    public void ApplyStrikethrough(LeafBlock block, int start, int end)
    {
        ApplyEmphasisStyle(block, start, end, '~', 2);
    }

    public void ApplyStrikethrough(int start, int end)
    {
        var block = GetLastParagraph() ?? throw new InvalidOperationException("Document has no paragraph.");
        ApplyStrikethrough(block, start, end);
    }

    public void MakeImage(LeafBlock block, int start, int end, string url, string? title = null)
    {
        MakeLinkOrImage(block, start, end, url, title, isImage: true);
    }

    public void MakeImage(int start, int end, string url, string? title = null)
    {
        var block = GetLastParagraph() ?? throw new InvalidOperationException("Document has no paragraph.");
        MakeImage(block, start, end, url, title);
    }

    public void MakeLink(LeafBlock block, int start, int end, string url, string? title = null)
    {
        MakeLinkOrImage(block, start, end, url, title, isImage: false);
    }

    public void MakeLink(int start, int end, string url, string? title = null)
    {
        var block = GetLastParagraph() ?? throw new InvalidOperationException("Document has no paragraph.");
        MakeLink(block, start, end, url, title);
    }

    public void ClearAllStyles()
    {
        foreach (var child in Children.ToList())
        {
            ClearBlockStyles(child);
        }
    }

    public void ClearStylesForLine(int lineIndex)
    {
        var block = GetBlockAtLine(lineIndex);
        ClearBlockStyles(block);
    }

    public void ClearStylesForBlock(Block block)
    {
        ClearBlockStyles(block);
    }

    public void ClearStylesForRange(LeafBlock block, int start, int end)
    {
        ArgumentNullException.ThrowIfNull(block);
        if (block.Inline is null) return;

        var map = InlineOffsetMap.Build(block);
        if (start < 0) start = 0;
        if (end > map.TotalLength) end = map.TotalLength;
        if (start >= end) return;

        var inclusiveEnd = end - 1;

        SplitAtOffset(block, start);
        map = InlineOffsetMap.Build(block);
        if (end < map.TotalLength)
        {
            SplitAtOffset(block, end);
            map = InlineOffsetMap.Build(block);
        }

        UnwrapContainersInRange(block.Inline, start, inclusiveEnd, map);
    }

    public void ClearStylesForRange(int start, int end)
    {
        var block = GetLastParagraph() ?? throw new InvalidOperationException("Document has no paragraph.");
        ClearStylesForRange(block, start, end);
    }

    public string ToMarkdown(string? newLine = null)
    {
        var sb = new StringBuilder();
        RenderBlocks(Children, sb, 0, newLine ?? Environment.NewLine);
        return sb.ToString();
    }

    private bool _nextWriteCreatesNewParagraph;

    private ParagraphBlock GetOrCreateLastParagraph()
    {
        if (!_nextWriteCreatesNewParagraph && Children.Count > 0 && Children[^1] is ParagraphBlock lastPara)
            return lastPara;

        _nextWriteCreatesNewParagraph = false;
        var paragraph = new ParagraphBlock();
        AddChild(paragraph);
        return paragraph;
    }

    private ParagraphBlock? GetLastParagraph()
    {
        if (Children.Count > 0 && Children[^1] is ParagraphBlock para)
            return para;
        return null;
    }

    private LeafBlock? FindLeafBlockForRange(int start, int end)
    {
        foreach (var child in Children)
        {
            if (child is LeafBlock leaf && leaf.Inline is not null)
            {
                var map = InlineOffsetMap.Build(leaf);
                if (start < map.TotalLength)
                    return leaf;
            }
        }
        return GetLastParagraph();
    }

    private static void AppendLiteralToParagraph(ParagraphBlock paragraph, string text)
    {
        paragraph.Inline ??= new InlineRoot();

        var lastChild = paragraph.Inline.LastChild;
        if (lastChild is LiteralInline lastLit)
        {
            lastLit.Content += text;
        }
        else
        {
            paragraph.Inline.AppendChild(new LiteralInline(text));
        }
    }

    private Block GetBlockAtLine(int lineIndex)
    {
        if (lineIndex < 0) lineIndex = 0;
        var blockIndex = Math.Min(lineIndex, Children.Count - 1);
        return Children[blockIndex];
    }

    private void ReplaceBlockInParent(Block oldBlock, Block newBlock)
    {
        if (oldBlock.Parent is ContainerBlock container)
        {
            var index = container.IndexOfChild(oldBlock);
            if (index >= 0)
                container.ReplaceChild(oldBlock, newBlock);
        }
    }

    private void ConvertToList(Block block, ListType listType)
    {
        ArgumentNullException.ThrowIfNull(block);

        var parent = block.Parent as ContainerBlock;
        if (parent is null)
            throw new InvalidOperationException("Block has no parent container.");

        var index = parent.IndexOfChild(block);
        if (index < 0) return;

        if (index > 0 && parent.Children[index - 1] is ListBlock prevList && prevList.ListType == listType)
        {
            parent.RemoveChildAt(index);
            var listItem = new ListItemBlock();
            if (block is LeafBlock leaf)
                listItem.AddChild(leaf);
            else if (block is ContainerBlock containerBlock)
            {
                foreach (var child in containerBlock.Children.ToList())
                {
                    containerBlock.RemoveChild(child);
                    listItem.AddChild(child);
                }
            }
            prevList.AddChild(listItem);
            return;
        }

        var list = new ListBlock(listType);
        var listItemBlock = new ListItemBlock();

        parent.RemoveChildAt(index);

        if (block is LeafBlock leafBlock)
            listItemBlock.AddChild(leafBlock);
        else if (block is ContainerBlock containerBlock)
        {
            foreach (var child in containerBlock.Children.ToList())
            {
                containerBlock.RemoveChild(child);
                listItemBlock.AddChild(child);
            }
        }

        list.AddChild(listItemBlock);
        parent.InsertChild(index, list);
    }

    private void ApplyEmphasisStyle(LeafBlock block, int start, int end, char delimiterChar, int delimiterCount)
    {
        ArgumentNullException.ThrowIfNull(block);
        if (block.Inline is null) return;

        var map = InlineOffsetMap.Build(block);
        if (start < 0) start = 0;
        if (end > map.TotalLength) end = map.TotalLength;
        if (start >= end) return;

        var inclusiveEnd = end - 1;

        SplitAtOffset(block, start);
        map = InlineOffsetMap.Build(block);

        if (end < map.TotalLength)
        {
            SplitAtOffset(block, end);
            map = InlineOffsetMap.Build(block);
        }

        var entries = map.GetEntriesInRange(start, inclusiveEnd);
        if (entries.Count == 0) return;

        if (entries.Count == 1 && entries[0].Inline is CodeInline code)
        {
            var delim = new string(delimiterChar, delimiterCount);
            code.Prefix += delim;
            code.Suffix = delim + code.Suffix;
            return;
        }

        var inlinesToWrap = CollectInlinesToWrap(entries, start, inclusiveEnd, map, delimiterChar);
        if (inlinesToWrap.Count == 0) return;

        WrapInlinesInContainer(block, inlinesToWrap, new EmphasisInline(delimiterChar, delimiterCount));
    }

    private void SplitAtOffset(LeafBlock block, int offset)
    {
        if (offset <= 0 || block.Inline is null) return;

        var map = InlineOffsetMap.Build(block);
        if (offset >= map.TotalLength) return;

        var entry = map.FindEntryAt(offset);
        if (entry is null) return;

        if (entry.Inline is LiteralInline lit)
        {
            var relOffset = offset - entry.Start;
            if (relOffset > 0 && relOffset < lit.Content.Length)
                InlineSplitter.Split(lit, relOffset);
        }
    }

    private void WrapInlinesInContainer(LeafBlock block, List<Inline> inlinesToWrap, ContainerInline container)
    {
        var firstInline = inlinesToWrap[0];
        var parentInline = firstInline.ParentInline ?? block.Inline!;

        var insertBefore = firstInline.PreviousSibling;
        var insertAsFirstChild = insertBefore is null;

        foreach (var inline in inlinesToWrap)
        {
            inline.Remove();
            container.AppendChild(inline);
        }

        if (insertAsFirstChild)
        {
            if (parentInline.FirstChild is not null)
            {
                InlineSplitter.InsertBeforeInline(parentInline.FirstChild, container);
            }
            else
            {
                parentInline.AppendChild(container);
            }
        }
        else
        {
            InlineSplitter.InsertAfterInline(insertBefore!, container);
        }
    }

    private List<Inline> CollectInlinesToWrap(List<InlineOffsetMap.Entry> entries, int start, int end, InlineOffsetMap map, char? delimiterChar = null)
    {
        var entryInlines = new HashSet<Inline>(entries.Select(e => e.Inline));

        var toWrap = new List<Inline>();
        var seen = new HashSet<Inline>();

        foreach (var entry in entries)
        {
            var inline = entry.Inline;

            while (inline.ParentInline is not null and not InlineRoot)
            {
                var parent = inline.ParentInline;

                var parentStart = int.MaxValue;
                var parentEnd = int.MinValue;
                foreach (var pe in map.Entries)
                {
                    if (pe.Inline == parent || IsDescendantOf(pe.Inline, parent))
                    {
                        parentStart = Math.Min(parentStart, pe.Start);
                        parentEnd = Math.Max(parentEnd, pe.End);
                    }
                }

                if (parentStart <= parentEnd && parentStart >= start && parentEnd <= end
                    && delimiterChar is not null
                    && parent is EmphasisInline parentEmph && parentEmph.DelimiterChar == delimiterChar)
                {
                    inline = parent;
                }
                else
                {
                    break;
                }
            }

            if (seen.Add(inline))
                toWrap.Add(inline);
        }

        if (toWrap.Count > 1)
        {
            var groups = toWrap.GroupBy(i => i.ParentInline).ToList();
            if (groups.Count > 1)
            {
                toWrap = groups.OrderByDescending(g => g.Count()).First().ToList();
            }
        }

        toWrap.Sort((a, b) =>
        {
            if (a.ParentInline == b.ParentInline && a.ParentInline is not null)
            {
                var current = a.ParentInline.FirstChild;
                var posA = 0;
                var posB = 0;
                var foundA = false;
                var foundB = false;
                var idx = 0;
                while (current is not null)
                {
                    if (ReferenceEquals(current, a)) { posA = idx; foundA = true; }
                    if (ReferenceEquals(current, b)) { posB = idx; foundB = true; }
                    if (foundA && foundB) break;
                    current = current.NextSibling;
                    idx++;
                }
                return posA.CompareTo(posB);
            }
            return 0;
        });

        return toWrap;
    }

    private static bool IsDescendantOf(Inline inline, Inline ancestor)
    {
        var current = inline.ParentInline;
        while (current is not null)
        {
            if (ReferenceEquals(current, ancestor))
                return true;
            current = current.ParentInline;
        }
        return false;
    }

    private static bool LastLeafEndsWithSoftBreak(Block block)
    {
        if (block is LeafBlock leaf)
            return leaf.Inline?.LastChild is LineBreakInline;
        if (block is ContainerBlock container && container.Children.Count > 0)
            return LastLeafEndsWithSoftBreak(container.Children[^1]);
        return false;
    }

    private void MakeLinkOrImage(LeafBlock block, int start, int end, string url, string? title, bool isImage)
    {
        ArgumentNullException.ThrowIfNull(block);
        if (block.Inline is null) return;

        var map = InlineOffsetMap.Build(block);
        if (start < 0) start = 0;
        if (end > map.TotalLength) end = map.TotalLength;
        if (start >= end) return;

        var inclusiveEnd = end - 1;

        SplitAtOffset(block, start);
        map = InlineOffsetMap.Build(block);

        if (end < map.TotalLength)
        {
            SplitAtOffset(block, end);
            map = InlineOffsetMap.Build(block);
        }

        var entries = map.GetEntriesInRange(start, inclusiveEnd);
        var inlinesToWrap = CollectInlinesToWrap(entries, start, inclusiveEnd, map);
        if (inlinesToWrap.Count == 0) return;

        var link = new LinkInline
        {
            Url = url ?? string.Empty,
            Title = title,
            IsImage = isImage
        };

        WrapInlinesInContainer(block, inlinesToWrap, link);
    }

    private void RemoveFromLiteral(LiteralInline lit, int relStart, int relEnd, int contentOffset)
    {
        var content = lit.Content;
        var absStart = contentOffset + relStart;
        var absEnd = contentOffset + relEnd;

        if (absStart <= 0 && absEnd >= content.Length - 1)
        {
            lit.Remove();
        }
        else if (absStart <= 0)
        {
            lit.Content = content[(absEnd + 1)..];
        }
        else if (absEnd >= content.Length - 1)
        {
            lit.Content = content[..absStart];
        }
        else
        {
            var leftContent = content[..absStart];
            var rightContent = content[(absEnd + 1)..];

            lit.Content = leftContent;
            if (rightContent.Length > 0)
            {
                var rightLit = new LiteralInline(rightContent);
                InlineSplitter.InsertAfterInline(lit, rightLit);
            }
        }
    }

    private void RemoveFromCodeInline(CodeInline code, int relStart, int relEnd)
    {
        var content = code.Content;

        if (relStart <= 0 && relEnd >= content.Length - 1)
        {
            code.Remove();
        }
        else if (relStart <= 0)
        {
            code.Content = content[(relEnd + 1)..];
        }
        else if (relEnd >= content.Length - 1)
        {
            code.Content = content[..relStart];
        }
        else
        {
            var leftContent = content[..relStart];
            var rightContent = content[(relEnd + 1)..];

            if (leftContent.Length > 0)
            {
                code.Content = leftContent;
                if (rightContent.Length > 0)
                {
                    var rightLit = new LiteralInline(rightContent);
                    InlineSplitter.InsertAfterInline(code, rightLit);
                }
            }
            else if (rightContent.Length > 0)
            {
                var rightLit = new LiteralInline(rightContent);
                InlineSplitter.ReplaceInline(code, rightLit);
            }
        }
    }

    private void RemoveTextInRange(LeafBlock block, int start, int end, InlineOffsetMap map, List<InlineOffsetMap.Entry> entries)
    {
        foreach (var entry in entries)
        {
            var relStart = Math.Max(0, start - entry.Start);
            var relEnd = Math.Min(entry.Length - 1, end - entry.Start);

            if (entry.Inline is LiteralInline lit)
                RemoveFromLiteral(lit, relStart, relEnd, entry.ContentOffset);
            else if (entry.Inline is CodeInline code)
                RemoveFromCodeInline(code, relStart, relEnd);
        }
    }

    private void InsertInlineAtOffset(LeafBlock block, int offset, Inline newInline)
    {
        if (block.Inline is null)
        {
            block.Inline = new InlineRoot();
        }

        var map = InlineOffsetMap.Build(block);

        if (map.TotalLength == 0 || offset <= 0)
        {
            block.Inline.PrependChild(newInline);
            return;
        }

        if (offset >= map.TotalLength)
        {
            block.Inline.AppendChild(newInline);
            return;
        }

        var entry = map.FindEntryAt(offset);
        if (entry is not null)
        {
            InlineSplitter.InsertBeforeInline(entry.Inline, newInline);
        }
        else
        {
            block.Inline.AppendChild(newInline);
        }
    }

    private void ClearBlockStyles(Block block)
    {
        switch (block)
        {
            case LeafBlock leaf:
                if (leaf.Inline is not null)
                    UnwrapAllContainers(leaf.Inline);
                break;
            case ContainerBlock container:
                foreach (var child in container.Children.ToList())
                    ClearBlockStyles(child);
                break;
        }
    }

    private void UnwrapAllContainers(ContainerInline container)
    {
        var child = container.FirstChild;
        while (child is not null)
        {
            var next = child.NextSibling;

            if (child is ContainerInline childContainer)
            {
                UnwrapAllContainers(childContainer);

                var grandChild = childContainer.FirstChild;
                var lastInserted = child;

                while (grandChild is not null)
                {
                    var nextGrand = grandChild.NextSibling;
                    grandChild.Remove();
                    InlineSplitter.InsertAfterInline(lastInserted, grandChild);
                    lastInserted = grandChild;
                    grandChild = nextGrand;
                }

                child.Remove();
            }

            child = next;
        }
    }

    private void UnwrapContainersInRange(ContainerInline container, int start, int end, InlineOffsetMap map)
    {
        var child = container.FirstChild;
        while (child is not null)
        {
            var next = child.NextSibling;

            if (child is ContainerInline childContainer)
            {
                var containerEntries = map.Entries.Where(e =>
                    e.Inline == childContainer || IsDescendantOf(e.Inline, childContainer)).ToList();

                if (containerEntries.Count > 0)
                {
                    var containerStart = containerEntries.Min(e => e.Start);
                    var containerEnd = containerEntries.Max(e => e.End);

                    bool fullyInRange = containerStart >= start && containerEnd <= end;

                    if (fullyInRange)
                    {
                        UnwrapAllContainers(childContainer);

                        var grandChild = childContainer.FirstChild;
                        var lastInserted = child;

                        while (grandChild is not null)
                        {
                            var nextGrand = grandChild.NextSibling;
                            grandChild.Remove();
                            InlineSplitter.InsertAfterInline(lastInserted, grandChild);
                            lastInserted = grandChild;
                            grandChild = nextGrand;
                        }

                        childContainer.Remove();
                    }
                    else
                    {
                        SplitContainerPartially(childContainer, start, end, map);
                    }
                }
            }

            child = next;
        }
    }

    private void SplitContainerPartially(ContainerInline container, int start, int end, InlineOffsetMap map)
    {
        var before = new List<Inline>();
        var during = new List<Inline>();
        var after = new List<Inline>();

        var child = container.FirstChild;
        while (child is not null)
        {
            var entries = map.Entries.Where(e =>
                ReferenceEquals(e.Inline, child) || IsDescendantOf(e.Inline, child)).ToList();

            if (entries.Count == 0)
            {
                during.Add(child);
            }
            else
            {
                var childStart = entries.Min(e => e.Start);
                var childEnd = entries.Max(e => e.End);

                if (childEnd < start)
                    before.Add(child);
                else if (childStart > end)
                    after.Add(child);
                else
                    during.Add(child);
            }

            child = child.NextSibling;
        }

        if (during.Count == 0)
            return;

        foreach (var c in during) c.Remove();
        foreach (var c in after) c.Remove();

        Inline? insertAfter;
        ContainerInline? insertParent;

        if (container.FirstChild is null)
        {
            insertAfter = container.PreviousSibling;
            insertParent = container.ParentInline;
            container.Remove();
        }
        else
        {
            insertAfter = container;
            insertParent = container.ParentInline;
        }

        ContainerInline? newContainer = null;
        if (after.Count > 0)
        {
            newContainer = CreateContainerLike(container);
            foreach (var c in after)
                newContainer.AppendChild(c);
        }

        Inline? lastInserted = null;
        if (during.Count > 0)
        {
            if (insertAfter is not null)
                InlineSplitter.InsertAfterInline(insertAfter, during[0]);
            else if (insertParent is not null)
            {
                if (insertParent.FirstChild is not null)
                    InlineSplitter.InsertBeforeInline(insertParent.FirstChild, during[0]);
                else
                    insertParent.AppendChild(during[0]);
            }
            lastInserted = during[0];

            for (int i = 1; i < during.Count; i++)
            {
                InlineSplitter.InsertAfterInline(lastInserted!, during[i]);
                lastInserted = during[i];
            }
        }

        if (newContainer is not null)
        {
            if (lastInserted is not null)
                InlineSplitter.InsertAfterInline(lastInserted, newContainer);
            else if (insertAfter is not null)
                InlineSplitter.InsertAfterInline(insertAfter, newContainer);
            else if (insertParent is not null)
            {
                if (insertParent.FirstChild is not null)
                    InlineSplitter.InsertBeforeInline(insertParent.FirstChild, newContainer);
                else
                    insertParent.AppendChild(newContainer);
            }
        }
    }

    private static ContainerInline CreateContainerLike(ContainerInline original)
    {
        return original switch
        {
            EmphasisInline emph => new EmphasisInline(emph.DelimiterChar, emph.DelimiterCount),
            LinkInline link => new LinkInline { Url = link.Url, Title = link.Title, IsImage = link.IsImage },
            _ => throw new InvalidOperationException($"Unknown container type: {original.GetType().Name}")
        };
    }

    private void RenderBlocks(IReadOnlyList<Block> blocks, StringBuilder sb, int indentLevel, string newLine)
    {
        for (var i = 0; i < blocks.Count; i++)
        {
            if (i > 0)
            {
                var prev = blocks[i - 1];
                bool prevEndsWithSoftBreak = prev is LeafBlock pl
                    && pl.Inline?.LastChild is LineBreakInline;
                if (!prevEndsWithSoftBreak)
                    sb.Append(newLine).Append(newLine);
            }
            else if (indentLevel > 0)
                sb.Append(newLine);

            var block = blocks[i];
            RenderBlock(block, sb, indentLevel, newLine);
        }
    }

    private void RenderBlock(Block block, StringBuilder sb, int indentLevel, string newLine)
    {
        var indent = new string(' ', indentLevel * 4);

        switch (block)
        {
            case ParagraphBlock para:
                sb.Append(indent);
                RenderInlines(para.Inline, sb);
                break;

            case HeadingBlock heading:
                sb.Append(indent);
                sb.Append(new string('#', heading.Level));
                sb.Append(' ');
                RenderInlines(heading.Inline, sb);
                break;

            case ListBlock list:
                RenderList(list, sb, indentLevel, newLine);
                break;

            case ListItemBlock listItem:
                RenderBlocks(listItem.Children, sb, indentLevel, newLine);
                break;

            case QuoteBlock quote:
                var quoteSb = new StringBuilder();
                RenderBlocks(quote.Children, quoteSb, 0, newLine);
                var quoteText = quoteSb.ToString();
                var prefix = new string('>', quote.NestingLevel);
                if (quote.NestingLevel > 0) prefix += " ";
                var lines = quoteText.Split('\n');

                bool quoteEndsWithLB = quote.Children.Count > 0
                    && LastLeafEndsWithSoftBreak(quote.Children[^1]);

                var lineCount = lines.Length;
                if (quoteEndsWithLB)
                    while (lineCount > 0 && string.IsNullOrEmpty(lines[lineCount - 1]))
                        lineCount--;

                for (var i = 0; i < lineCount; i++)
                {
                    if (i > 0) sb.Append(newLine);
                    sb.Append(indent);
                    sb.Append(prefix);
                    sb.Append(lines[i].TrimEnd('\r'));
                }

                if (quoteEndsWithLB && lineCount > 0)
                    sb.Append(newLine);
                break;

            case FencedCodeBlock fenced:
                sb.Append(indent);
                sb.Append(new string(fenced.FenceChar, fenced.FenceCount));
                if (!string.IsNullOrEmpty(fenced.Info))
                    sb.Append(fenced.Info);
                sb.Append(newLine);
                foreach (var line in fenced.Lines)
                {
                    sb.Append(indent);
                    sb.AppendLine(line);
                }
                sb.Append(indent);
                sb.Append(new string(fenced.FenceChar, fenced.FenceCount));
                break;

            case CodeBlock codeBlock:
                foreach (var line in codeBlock.Lines)
                {
                    sb.Append(indent);
                    sb.Append("    ");
                    sb.AppendLine(line);
                }
                break;

            case ThematicBreakBlock:
                sb.Append(indent);
                sb.Append(new string(block is ThematicBreakBlock hr ? hr.Character : '-', 3));
                break;

            case HtmlBlock html:
                foreach (var line in html.Lines)
                {
                    sb.Append(indent);
                    sb.AppendLine(line);
                }
                break;
        }
    }

    private void RenderList(ListBlock list, StringBuilder sb, int indentLevel, string newLine)
    {
        var indent = new string(' ', indentLevel * 4);
        for (var i = 0; i < list.Children.Count; i++)
        {
            if (i > 0)
            {
                var prevItem = list.Children[i - 1] as ListItemBlock;
                bool prevEndsWithLB = false;
                if (prevItem?.Children.Count > 0 && prevItem.Children[0] is ParagraphBlock prevPara)
                    prevEndsWithLB = prevPara.Inline?.LastChild is LineBreakInline;
                if (!prevEndsWithLB)
                    sb.Append(newLine);
            }

            var item = list.Children[i];
            sb.Append(indent);
            if (list.ListType == ListType.Ordered)
                sb.Append($"{list.StartNumber + i}. ");
            else
                sb.Append($"{list.BulletCharacter} ");

            if (item is ListItemBlock listItem && listItem.Children.Count > 0)
            {
                if (listItem.Children[0] is ParagraphBlock para)
                {
                    RenderInlines(para.Inline, sb);
                    for (var j = 1; j < listItem.Children.Count; j++)
                    {
                        sb.Append(newLine);
                        RenderBlock(listItem.Children[j], sb, indentLevel + 1, newLine);
                    }
                }
                else
                {
                    RenderBlocks(listItem.Children, sb, indentLevel + 1, newLine);
                }
            }
        }
    }

    private void RenderInlines(ContainerInline? container, StringBuilder sb)
    {
        if (container is null) return;

        var child = container.FirstChild;
        while (child is not null)
        {
            RenderInline(child, sb);
            child = child.NextSibling;
        }
    }

    private void RenderInline(Inline inline, StringBuilder sb)
    {
        switch (inline)
        {
            case LiteralInline lit:
                sb.Append(lit.Content);
                break;
            case EmphasisInline emphasis:
                var delim = new string(emphasis.DelimiterChar, emphasis.DelimiterCount);
                sb.Append(delim);
                RenderInlines(emphasis, sb);
                sb.Append(delim);
                break;
            case CodeInline code:
                var codeDelim = new string('`', code.DelimiterCount);
                sb.Append(codeDelim);
                sb.Append(code.Prefix);
                sb.Append(code.Content);
                sb.Append(code.Suffix);
                sb.Append(codeDelim);
                break;
            case LinkInline link:
                if (link.IsImage)
                    sb.Append('!');
                sb.Append('[');
                RenderInlines(link, sb);
                sb.Append("](");
                sb.Append(link.Url);
                if (!string.IsNullOrEmpty(link.Title))
                    sb.Append($" \"{link.Title}\"");
                sb.Append(')');
                break;
            case AutolinkInline auto:
                sb.Append('<');
                sb.Append(auto.Url);
                sb.Append('>');
                break;
            case LineBreakInline lineBreak:
                sb.Append(lineBreak.IsHard ? "  \n" : "\n");
                break;
            case HtmlInline html:
                sb.Append(html.Content);
                break;
            case HtmlEntityInline entity:
                sb.Append(entity.Original);
                break;
            case ContainerInline container:
                RenderInlines(container, sb);
                break;
        }
    }
}
