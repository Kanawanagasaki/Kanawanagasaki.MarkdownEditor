using System.Text;
using Kanawanagasaki.MarkdownEditor.Ast;
using Kanawanagasaki.MarkdownEditor.Editing;

namespace Kanawanagasaki.MarkdownEditor;

public partial class MarkdownDocument : Ast.MarkdownDocument
{
    public EditRangeResult Write(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            int offset = GetPlainText().Length;
            int lineIdx = GetLineIndexAtOffset(offset);
            return new EditRangeResult { StartOffset = offset, EndOffset = offset, LineStartIndex = lineIdx, LineEndIndex = lineIdx };
        }

        var paragraph = GetOrCreateLastParagraph();
        var line = paragraph.GetOrCreateLine();

        bool wasEmpty = line.IsEmpty;
        line.AppendLiteral(text);

        if (wasEmpty && paragraph.HasTrailingLineBreak)
        {
            paragraph.HasTrailingLineBreak = false;
        }

        paragraph.MarkInlineDirty();

        int endOffset = GetPlainText().Length;
        int startOffset = endOffset - text.Length;
        int lineStart = GetLineIndexAtOffset(startOffset);
        int lineEnd = endOffset > startOffset ? GetLineIndexAtOffset(endOffset - 1) : lineStart;

        return new EditRangeResult { StartOffset = startOffset, EndOffset = endOffset, LineStartIndex = lineStart, LineEndIndex = lineEnd };
    }

    public EditRangeResult WriteLine(string? text = null)
    {
        ParagraphBlock targetPara;
        if (Children.Count > 0 && Children[^1] is ParagraphBlock p)
            targetPara = p;
        else
        {
            targetPara = new ParagraphBlock();
            AddChild(targetPara);
        }

        var lastLine = targetPara.GetOrCreateLine();

        int startOffset = GetPlainText().Length;
        int lineStart = GetLineIndexAtOffset(startOffset);

        if (!string.IsNullOrEmpty(text) && lastLine.IsEmpty)
        {
            lastLine.AppendLiteral(text);
        }

        targetPara.Lines.Add(new LineInline());
        targetPara.HasTrailingLineBreak = true;
        targetPara.MarkInlineDirty();
        _nextWriteCreatesNewParagraph = false;

        int endOffset = !string.IsNullOrEmpty(text) ? startOffset + text.Length : startOffset;
        return new EditRangeResult { StartOffset = startOffset, EndOffset = endOffset, LineStartIndex = lineStart, LineEndIndex = lineStart };
    }

    public EditRangeResult WriteParagraph(string? text = null)
    {
        if (Children.Count > 0 && !IsEmptyParagraph(Children[^1]))
        {
            AddChild(new ParagraphBlock());
        }

        EditRangeResult result;
        if (text is not null)
            result = Write(text);
        else
        {
            int offset = GetPlainText().Length;
            int line = GetLineIndexAtOffset(offset);
            result = new EditRangeResult { StartOffset = offset, EndOffset = offset, LineStartIndex = line, LineEndIndex = line };
        }
        _nextWriteCreatesNewParagraph = true;
        return result;
    }

    public EditRangeResult InsertLine(int lineOffset, string text)
    {
        string beforeText = GetPlainText();
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

            paragraph.SyncFromMergedIfNeeded();

            var newLine = new LineInline();
            if (!string.IsNullOrEmpty(text))
                newLine.AppendLiteral(text);
            paragraph.Lines.Insert(0, newLine);
            paragraph.MarkInlineDirty();
            return ComputeInsertRange(beforeText, text);
        }

        int cumulative = 0;
        for (int blockIndex = 0; blockIndex < Children.Count; blockIndex++)
        {
            var block = Children[blockIndex];
            int linesInBlock = CountLinesInBlock(block);

            if (lineOffset < cumulative + linesInBlock)
            {
                if (block is ParagraphBlock para)
                {
                    para.SyncFromMergedIfNeeded();
                    int localLineIndex = lineOffset - cumulative;

                    if (para.HasTrailingLineBreak && localLineIndex >= para.Lines.Count - 1 && para.Lines.Count > 0 && para.Lines[^1].IsEmpty)
                    {
                        var newLine = new LineInline();
                        if (!string.IsNullOrEmpty(text))
                            newLine.AppendLiteral(text);
                        para.Lines.Insert(localLineIndex, newLine);
                        para.MarkInlineDirty();
                        return ComputeInsertRange(beforeText, text);
                    }

                    var insertLine = new LineInline();
                    if (!string.IsNullOrEmpty(text))
                        insertLine.AppendLiteral(text);
                    para.Lines.Insert(localLineIndex, insertLine);
                    para.MarkInlineDirty();
                    return ComputeInsertRange(beforeText, text);
                }
            }

            cumulative += linesInBlock;
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

        lastParagraph.SyncFromMergedIfNeeded();
        var endLine = new LineInline();
        if (!string.IsNullOrEmpty(text))
            endLine.AppendLiteral(text);
        lastParagraph.Lines.Add(endLine);
        lastParagraph.HasTrailingLineBreak = true;
        lastParagraph.MarkInlineDirty();

        lastParagraph.Lines.Add(new LineInline());
        return ComputeInsertRange(beforeText, text);
    }

    public EditRangeResult InsertParagraph(int lineOffset, string text)
    {
        string beforeText = GetPlainText();
        var paragraph = new ParagraphBlock();
        if (!string.IsNullOrEmpty(text))
        {
            paragraph.Inline ??= new InlineRoot();
            paragraph.Inline.AppendChild(new LiteralInline(text));
        }

        if (lineOffset <= 0)
        {
            InsertChild(0, paragraph);
            if (Children.Count > 1 && !IsEmptyParagraph(Children[1]))
            {
                InsertChild(1, new ParagraphBlock());
            }
        }
        else if (lineOffset >= Children.Count)
        {
            if (Children.Count > 0 && !IsEmptyParagraph(Children[^1]))
            {
                AddChild(new ParagraphBlock());
            }
            AddChild(paragraph);
        }
        else
        {
            if (lineOffset > 0 && !IsEmptyParagraph(Children[lineOffset - 1]))
            {
                InsertChild(lineOffset, new ParagraphBlock());
                lineOffset++;
            }
            InsertChild(lineOffset, paragraph);
            if (lineOffset + 1 < Children.Count && !IsEmptyParagraph(Children[lineOffset + 1]))
            {
                InsertChild(lineOffset + 1, new ParagraphBlock());
            }
        }
        return ComputeInsertRange(beforeText, text);
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
        else if (block.Inline is not null)
        {
            CleanupEmptyContainers(block.Inline);

            if (block.Inline.FirstChild is null)
            {
                if (block.Parent is ContainerBlock container)
                    container.RemoveChild(block);
            }
        }
    }

    public void RemoveText(int start, int end)
    {
        foreach (var (block, localStart, localEnd) in ResolveGlobalRange(start, end))
            RemoveText(block, localStart, localEnd);
    }

    public EditRangeResult ConvertToHeading(LeafBlock block, int level)
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
            return GetRangeForBlock(block);
        }

        if (level > 6) level = 6;

        if (block is HeadingBlock existingHeadingBlock)
        {
            existingHeadingBlock.Level = level;
            return GetRangeForBlock(block);
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
        return GetRangeForBlock(block);
    }

    public EditRangeResult ConvertToHeading(int lineIndex, int level)
    {
        var (block, localLineIndex) = GetBlockAndLineAt(lineIndex);
        if (block is ParagraphBlock para && CountLinesInBlock(para) > 1)
        {
            var extracted = ExtractLineFromParagraph(para, localLineIndex);
            ConvertToHeading(extracted, level);
            return GetRangeForBlock(extracted);
        }
        else if (block is LeafBlock leaf)
        {
            ConvertToHeading(leaf, level);
            return GetRangeForBlock(leaf);
        }
        return new EditRangeResult();
    }

    public EditRangeResult ConvertToBlockquote(Block block)
    {
        ArgumentNullException.ThrowIfNull(block);

        if (block is QuoteBlock)
            return GetRangeForBlock(block);

        var parent = block.Parent as ContainerBlock;
        if (parent is null)
            throw new InvalidOperationException("Block has no parent container.");

        var index = parent.IndexOfChild(block);
        if (index < 0) return GetRangeForBlock(block);

        if (index > 0 && parent.Children[index - 1] is QuoteBlock prevQuote)
        {
            parent.RemoveChildAt(index);
            AddBlockToQuote(prevQuote, block);
            return GetRangeForBlock(block);
        }

        if (index < parent.Children.Count - 1 && parent.Children[index + 1] is QuoteBlock nextQuote)
        {
            parent.RemoveChildAt(index);
            InsertBlockIntoQuote(nextQuote, block, 0);
            return GetRangeForBlock(block);
        }

        var quote = new QuoteBlock { NestingLevel = 1 };
        parent.RemoveChildAt(index);
        AddBlockToQuote(quote, block);
        parent.InsertChild(index, quote);
        return GetRangeForBlock(block);
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

    public EditRangeResult ConvertToBlockquote(int lineIndex)
    {
        var range = GetRangeForBlock(GetBlockAtLine(lineIndex));
        var (block, localLineIndex) = GetBlockAndLineAt(lineIndex);
        if (block is QuoteBlock)
        {
            return range;
        }
        else if (block is ParagraphBlock para && CountLinesInBlock(para) > 1)
        {
            var extracted = ExtractLineFromParagraph(para, localLineIndex);
            WrapInStandaloneBlockquote(extracted);
        }
        else if (block is ListBlock listBlock && localLineIndex < listBlock.Children.Count)
        {
            ExtractListItemForBlockquote(listBlock, localLineIndex);
        }
        else
        {
            WrapInStandaloneBlockquote(block);
        }
        return range;
    }

    private void WrapInStandaloneBlockquote(Block block)
    {
        var parent = block.Parent as ContainerBlock;
        if (parent is null) return;
        var index = parent.IndexOfChild(block);
        if (index < 0) return;

        var quote = new QuoteBlock { NestingLevel = 1 };
        parent.RemoveChildAt(index);
        AddBlockToQuote(quote, block);
        parent.InsertChild(index, quote);
    }

    private void ExtractListItemForBlockquote(ListBlock listBlock, int itemIndex)
    {
        var parent = listBlock.Parent as ContainerBlock;
        if (parent is null) return;
        var listIndex = parent.IndexOfChild(listBlock);
        if (listIndex < 0) return;

        var extractedItem = listBlock.Children[itemIndex];
        listBlock.RemoveChildAt(itemIndex);

        var extractedList = new ListBlock(listBlock.ListType);
        extractedList.StartNumber = listBlock.StartNumber;
        extractedList.AddChild(extractedItem);

        ListBlock? tailList = null;
        while (itemIndex < listBlock.Children.Count)
        {
            var child = listBlock.Children[itemIndex];
            listBlock.RemoveChildAt(itemIndex);
            if (tailList is null)
            {
                tailList = new ListBlock(listBlock.ListType);
                tailList.StartNumber = listBlock.StartNumber;
            }
            tailList.AddChild(child);
        }

        if (listBlock.Children.Count == 0)
        {
            parent.RemoveChildAt(listIndex);
        }

        var quote = new QuoteBlock { NestingLevel = 1 };
        AddBlockToQuote(quote, extractedList);

        bool merged = false;
        if (listIndex > 0 && parent.Children[listIndex - 1] is QuoteBlock prevQuote
            && prevQuote.Children.Count > 0 && prevQuote.Children[0] is ListBlock prevList
            && prevList.ListType == listBlock.ListType)
        {
            prevQuote.RemoveChild(prevList);
            foreach (var item in extractedList.Children.ToList())
            {
                extractedList.RemoveChild(item);
                prevList.AddChild(item);
            }
            prevQuote.AddChild(prevList);
            merged = true;
        }

        if (!merged)
        {
            parent.InsertChild(listIndex, quote);
        }

        if (tailList is not null)
        {
            var insertPos = merged
                ? parent.IndexOfChild(parent.Children.OfType<QuoteBlock>().Last()) + 1
                : parent.IndexOfChild(quote) + 1;
            parent.InsertChild(insertPos, tailList);
        }
    }

    public EditRangeResult ConvertToBlockquote(int lineIndex, int nestedLevel)
    {
        var block = GetBlockAtLine(lineIndex);
        var range = GetRangeForBlock(block);

        if (nestedLevel <= 0)
        {
            if (block is QuoteBlock quoteBlock && quoteBlock.Parent is ContainerBlock quoteParent)
            {
                var index = quoteParent.IndexOfChild(quoteBlock);
                if (index >= 0)
                {
                    quoteParent.RemoveChildAt(index);
                    var insertIdx = index;
                    foreach (var child in quoteBlock.Children.ToList())
                    {
                        quoteBlock.RemoveChild(child);
                        quoteParent.InsertChild(insertIdx, child);
                        insertIdx++;
                    }
                }
            }
            else if (block.Parent is QuoteBlock innerQuote)
            {
                var outerParent = innerQuote.Parent as ContainerBlock;
                if (outerParent is not null)
                {
                    var index = outerParent.IndexOfChild(innerQuote);
                    if (index >= 0)
                    {
                        outerParent.RemoveChildAt(index);
                        var insertIdx = index;
                        foreach (var child in innerQuote.Children.ToList())
                        {
                            innerQuote.RemoveChild(child);
                            outerParent.InsertChild(insertIdx, child);
                            insertIdx++;
                        }
                    }
                }
            }
            return range;
        }

        if (block is QuoteBlock existingQuote)
        {
            var parent = existingQuote.Parent as ContainerBlock;
            if (parent is not null)
            {
                var quoteIndex = parent.IndexOfChild(existingQuote);
                if (quoteIndex >= 0 && quoteIndex + 1 < parent.Children.Count)
                {
                    var nextSibling = parent.Children[quoteIndex + 1];
                    parent.RemoveChildAt(quoteIndex + 1);

                    var deepest = existingQuote;
                    while (deepest.Children.Count > 0 && deepest.Children[^1] is QuoteBlock inner)
                        deepest = inner;

                    var innerQuote = new QuoteBlock { NestingLevel = 1 };
                    AddBlockToQuote(innerQuote, nextSibling);
                    deepest.AddChild(innerQuote);
                    return range;
                }
            }

            existingQuote.NestingLevel = nestedLevel;
            return range;
        }

        if (block.Parent is QuoteBlock parentQuote)
        {
            var outerParent = parentQuote.Parent as ContainerBlock;
            if (outerParent is not null)
            {
                var idx = outerParent.IndexOfChild(parentQuote);
                if (idx >= 0)
                {
                    parentQuote.RemoveChild(block);
                    outerParent.InsertChild(idx + 1, block);
                    if (parentQuote.Children.Count == 0)
                        outerParent.RemoveChild(parentQuote);
                }
            }
        }

        var container = block.Parent as ContainerBlock;
        if (container is not null)
        {
            var index = container.IndexOfChild(block);
            if (index >= 0)
            {
                container.RemoveChildAt(index);
                var quote = new QuoteBlock { NestingLevel = nestedLevel };
                AddBlockToQuote(quote, block);
                container.InsertChild(index, quote);
            }
        }
        return range;
    }

    public EditRangeResult ConvertToOrderedList(Block block)
    {
        var range = GetRangeForBlock(block);
        ConvertToList(block, ListType.Ordered);
        return range;
    }

    public EditRangeResult ConvertToOrderedList(int lineIndex)
    {
        var range = GetRangeForBlock(GetBlockAtLine(lineIndex));
        var (block, localLineIndex) = GetBlockAndLineAt(lineIndex);
        if (block is ParagraphBlock para && CountLinesInBlock(para) > 1)
        {
            var extracted = ExtractLineFromParagraph(para, localLineIndex);
            ConvertToOrderedList(extracted);
        }
        else
        {
            ConvertToOrderedList(block);
        }
        return range;
    }

    public EditRangeResult ConvertToUnorderedList(Block block)
    {
        var range = GetRangeForBlock(block);
        ConvertToList(block, ListType.Unordered);
        return range;
    }

    public EditRangeResult ConvertToUnorderedList(int lineIndex)
    {
        var range = GetRangeForBlock(GetBlockAtLine(lineIndex));
        var (block, localLineIndex) = GetBlockAndLineAt(lineIndex);
        if (block is ParagraphBlock para && CountLinesInBlock(para) > 1)
        {
            var extracted = ExtractLineFromParagraph(para, localLineIndex);
            ConvertToUnorderedList(extracted);
        }
        else
        {
            ConvertToUnorderedList(block);
        }
        return range;
    }

    public EditRangeResult InsertHorizontalRule()
    {
        string beforeText = GetPlainText();
        var hr = new ThematicBreakBlock();
        if (Children.Count > 0 && !IsEmptyParagraph(Children[^1]))
        {
            AddChild(new ParagraphBlock());
        }
        AddChild(hr);
        _nextWriteCreatesNewParagraph = true;
        string afterText = GetPlainText();
        int commonPrefix = CommonPrefixLength(beforeText, afterText);
        return CreateRange(commonPrefix, commonPrefix);
    }

    public EditRangeResult InsertHorizontalRule(int lineIndex)
    {
        string beforeText = GetPlainText();
        var hr = new ThematicBreakBlock();

        if (lineIndex <= 0)
        {
            InsertChild(0, hr);
            if (Children.Count > 1 && !IsEmptyParagraph(Children[1]))
                InsertChild(1, new ParagraphBlock());
        }
        else
        {
            int targetBlockIndex = SplitBlockAtLine(lineIndex);
            lineIndex = targetBlockIndex;

            if (lineIndex >= Children.Count)
            {
                if (Children.Count > 0 && !IsEmptyParagraph(Children[^1]))
                    AddChild(new ParagraphBlock());
                AddChild(hr);
            }
            else
            {
                if (lineIndex > 0 && !IsEmptyParagraph(Children[lineIndex - 1]))
                {
                    InsertChild(lineIndex, new ParagraphBlock());
                    lineIndex++;
                }
                InsertChild(lineIndex, hr);
                if (lineIndex + 1 < Children.Count && !IsEmptyParagraph(Children[lineIndex + 1]))
                    InsertChild(lineIndex + 1, new ParagraphBlock());
            }
        }
        string afterText = GetPlainText();
        int commonPrefix = CommonPrefixLength(beforeText, afterText);
        return CreateRange(commonPrefix, commonPrefix);
    }

    private int SplitBlockAtLine(int lineOffset)
    {
        int cumulative = 0;
        for (int blockIndex = 0; blockIndex < Children.Count; blockIndex++)
        {
            var block = Children[blockIndex];
            int linesInBlock = CountLinesInBlock(block);

            if (lineOffset < cumulative + linesInBlock)
            {
                if (block is ParagraphBlock para)
                {
                    int localLineIndex = lineOffset - cumulative;

                    if (localLineIndex <= 0)
                        return blockIndex;

                    para.SyncFromMergedIfNeeded();

                    var newPara = new ParagraphBlock();
                    while (para.Lines.Count > localLineIndex)
                    {
                        var line = para.Lines[localLineIndex];
                        para.Lines.RemoveAt(localLineIndex);
                        newPara.Lines.Add(line);
                    }

                    if (para.HasTrailingLineBreak)
                    {
                        newPara.HasTrailingLineBreak = true;
                        if (!para.Lines.Any(l => !l.IsEmpty))
                            para.HasTrailingLineBreak = false;
                    }

                    para.MarkInlineDirty();
                    newPara.MarkInlineDirty();

                    bool paraNowEmpty = IsEmptyParagraph(para);
                    if (paraNowEmpty)
                    {
                        InsertChild(blockIndex + 1, newPara);
                        RemoveChild(para);
                        return blockIndex + 1;
                    }
                    else
                    {
                        InsertChild(blockIndex + 1, new ParagraphBlock());
                        InsertChild(blockIndex + 2, newPara);
                        return blockIndex + 2;
                    }
                }
                return blockIndex + 1;
            }

            cumulative += linesInBlock;
        }

        return Children.Count;
    }

    public EditRangeResult ApplyBold(LeafBlock block, int start, int end)
    {
        ApplyEmphasisStyle(block, start, end, '*', 2);
        return CreateRange(start, end);
    }

    public EditRangeResult ApplyBold(int start, int end)
    {
        foreach (var (block, localStart, localEnd) in ResolveGlobalRange(start, end))
            ApplyBold(block, localStart, localEnd);
        return CreateRange(start, end);
    }

    public EditRangeResult ApplyItalic(LeafBlock block, int start, int end)
    {
        ApplyEmphasisStyle(block, start, end, '*', 1);
        return CreateRange(start, end);
    }

    public EditRangeResult ApplyItalic(int start, int end)
    {
        foreach (var (block, localStart, localEnd) in ResolveGlobalRange(start, end))
            ApplyItalic(block, localStart, localEnd);
        return CreateRange(start, end);
    }

    public EditRangeResult ApplyCode(LeafBlock block, int start, int end)
    {
        ArgumentNullException.ThrowIfNull(block);
        if (block.Inline is null) return CreateRange(start, end);

        var map = InlineOffsetMap.Build(block);
        if (start < 0) start = 0;
        if (end > map.TotalLength) end = map.TotalLength;
        if (start >= end) return CreateRange(start, end);

        var inclusiveEnd = end - 1;

        SplitAtOffset(block, start);
        map = InlineOffsetMap.Build(block);

        if (end < map.TotalLength)
        {
            SplitAtOffset(block, end);
            map = InlineOffsetMap.Build(block);
        }

        var entries = map.GetEntriesInRange(start, inclusiveEnd);
        if (entries.Count == 0) return CreateRange(start, end);

        var inlinesToWrap = CollectInlinesToWrap(entries, start, inclusiveEnd, map);
        if (inlinesToWrap.Count == 0) return CreateRange(start, end);

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
        return CreateRange(start, end);
    }

    public EditRangeResult ApplyCode(int start, int end)
    {
        foreach (var (block, localStart, localEnd) in ResolveGlobalRange(start, end))
            ApplyCode(block, localStart, localEnd);
        return CreateRange(start, end);
    }

    public EditRangeResult ApplyStrikethrough(LeafBlock block, int start, int end)
    {
        ApplyEmphasisStyle(block, start, end, '~', 2);
        return CreateRange(start, end);
    }

    public EditRangeResult ApplyStrikethrough(int start, int end)
    {
        foreach (var (block, localStart, localEnd) in ResolveGlobalRange(start, end))
            ApplyStrikethrough(block, localStart, localEnd);
        return CreateRange(start, end);
    }

    public EditRangeResult MakeImage(LeafBlock block, int start, int end, string url, string? title = null)
    {
        MakeLinkOrImage(block, start, end, url, title, isImage: true);
        return CreateRange(start, end);
    }

    public EditRangeResult MakeImage(int start, int end, string url, string? title = null)
    {
        foreach (var (block, localStart, localEnd) in ResolveGlobalRange(start, end))
            MakeImage(block, localStart, localEnd, url, title);
        return CreateRange(start, end);
    }

    public EditRangeResult MakeLink(LeafBlock block, int start, int end, string url, string? title = null)
    {
        MakeLinkOrImage(block, start, end, url, title, isImage: false);
        return CreateRange(start, end);
    }

    public EditRangeResult MakeLink(int start, int end, string url, string? title = null)
    {
        foreach (var (block, localStart, localEnd) in ResolveGlobalRange(start, end))
            MakeLink(block, localStart, localEnd, url, title);
        return CreateRange(start, end);
    }

    public EditRangeResult ClearAllStyles()
    {
        string plainText = GetPlainText();
        foreach (var child in Children.ToList())
        {
            ClearBlockStyles(child);
        }
        return CreateRange(0, plainText.Length);
    }

    public EditRangeResult ClearStylesForLine(int lineIndex)
    {
        var (block, localLineIndex) = GetBlockAndLineAt(lineIndex);

        if (block is ParagraphBlock para && CountLinesInBlock(para) > 1)
        {
            para.SyncFromMergedIfNeeded();
            if (localLineIndex >= 0 && localLineIndex < para.Lines.Count)
            {
                UnwrapAllContainers(para.Lines[localLineIndex]);
                para.MarkInlineDirty();
            }
            return GetRangeForBlock(block);
        }

        ClearBlockStyles(block);
        return GetRangeForBlock(block);
    }

    public EditRangeResult ClearStylesForBlock(Block block)
    {
        ClearBlockStyles(block);
        return GetRangeForBlock(block);
    }

    public EditRangeResult ClearStylesForRange(LeafBlock block, int start, int end)
    {
        ArgumentNullException.ThrowIfNull(block);
        if (block.Inline is null) return CreateRange(start, end);

        var map = InlineOffsetMap.Build(block);
        if (start < 0) start = 0;
        if (end > map.TotalLength) end = map.TotalLength;
        if (start >= end) return CreateRange(start, end);

        var inclusiveEnd = end - 1;

        SplitAtOffset(block, start);
        map = InlineOffsetMap.Build(block);
        if (end < map.TotalLength)
        {
            SplitAtOffset(block, end);
            map = InlineOffsetMap.Build(block);
        }

        UnwrapContainersInRange(block.Inline, start, inclusiveEnd, map);
        return CreateRange(start, end);
    }

    public EditRangeResult ClearStylesForRange(int start, int end)
    {
        foreach (var (block, localStart, localEnd) in ResolveGlobalRange(start, end))
            ClearStylesForRange(block, localStart, localEnd);
        return CreateRange(start, end);
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

        if (Children.Count > 0 && !IsEmptyParagraph(Children[^1]))
            AddChild(new ParagraphBlock());

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

    private record struct BlockPlainTextRange(LeafBlock Block, int PlainTextStart, int PlainTextEnd);

    private List<(LeafBlock Block, int LocalStart, int LocalEnd)> ResolveGlobalRange(int globalStart, int globalEnd)
    {
        var result = new List<(LeafBlock Block, int LocalStart, int LocalEnd)>();
        var ranges = BuildBlockPlainTextRanges();

        foreach (var range in ranges)
        {
            if (range.PlainTextEnd <= globalStart) continue;
            if (range.PlainTextStart >= globalEnd) break;

            int overlapStart = Math.Max(globalStart, range.PlainTextStart);
            int overlapEnd = Math.Min(globalEnd, range.PlainTextEnd);

            int localPlainTextStart = overlapStart - range.PlainTextStart;
            int localPlainTextEnd = overlapEnd - range.PlainTextStart;

            int localInlineStart = PlainTextToInlineMapOffset(range.Block, localPlainTextStart);
            int localInlineEnd = PlainTextToInlineMapOffset(range.Block, localPlainTextEnd);

            var map = InlineOffsetMap.Build(range.Block);
            localInlineStart = Math.Max(0, Math.Min(localInlineStart, map.TotalLength));
            localInlineEnd = Math.Max(localInlineStart, Math.Min(localInlineEnd, map.TotalLength));

            if (localInlineStart < localInlineEnd)
                result.Add((range.Block, localInlineStart, localInlineEnd));
        }

        return result;
    }

    private List<BlockPlainTextRange> BuildBlockPlainTextRanges()
    {
        var ranges = new List<BlockPlainTextRange>();
        int offset = 0;
        CollectBlockPlainTextRanges(Children, ranges, ref offset, isTopLevel: true);
        return ranges;
    }

    private void CollectBlockPlainTextRanges(
        IReadOnlyList<Block> blocks,
        List<BlockPlainTextRange> ranges,
        ref int offset,
        bool isTopLevel)
    {
        Block? lastRendered = null;
        bool hadEmptyParagraphBefore = false;

        for (int i = 0; i < blocks.Count; i++)
        {
            if (IsEmptyParagraph(blocks[i]))
            {
                hadEmptyParagraphBefore = true;
                continue;
            }

            if (lastRendered is not null)
            {
                bool prevEndsWithLB = LastLeafEndsWithLineBreak(lastRendered);
                if (!prevEndsWithLB)
                {
                    offset += hadEmptyParagraphBefore
                        ? (isTopLevel ? 2 : 1)
                        : 1;
                }
            }

            lastRendered = blocks[i];
            hadEmptyParagraphBefore = false;

            switch (blocks[i])
            {
                case LeafBlock leaf when leaf.Inline is not null:
                {
                    int blockStart = offset;
                    offset += GetBlockPlainTextLength(leaf);
                    ranges.Add(new BlockPlainTextRange(leaf, blockStart, offset));
                    break;
                }
                case ContainerBlock container:
                    CollectBlockPlainTextRanges(container.Children, ranges, ref offset, false);
                    break;
            }
        }
    }

    private int PlainTextToInlineMapOffset(LeafBlock block, int localPlainTextOffset)
    {
        int plainTextPos = 0;
        int inlineMapPos = 0;
        WalkForOffsetTranslation(block.Inline, localPlainTextOffset, ref plainTextPos, ref inlineMapPos);
        return inlineMapPos;
    }

    private void WalkForOffsetTranslation(ContainerInline? container, int target, ref int ptPos, ref int imPos)
    {
        if (container is null) return;

        var child = container.FirstChild;
        while (child is not null)
        {
            int ptLen = GetInlinePlainTextLength(child);
            if (ptLen == 0)
            {
                child = child.NextSibling;
                continue;
            }

            if (ptPos + ptLen > target)
            {
                if (child is ContainerInline c)
                    WalkForOffsetTranslation(c, target, ref ptPos, ref imPos);
                else
                    imPos += target - ptPos;
                return;
            }

            ptPos += ptLen;
            imPos += GetInlineInlineMapLength(child);
            child = child.NextSibling;
        }
    }

    private int GetBlockPlainTextLength(LeafBlock block)
    {
        if (block.Inline is null) return 0;
        return GetContainerPlainTextLength(block.Inline);
    }

    private int GetContainerPlainTextLength(ContainerInline container)
    {
        int length = 0;
        var child = container.FirstChild;
        while (child is not null)
        {
            length += GetInlinePlainTextLength(child);
            child = child.NextSibling;
        }
        return length;
    }

    private int GetInlinePlainTextLength(Inline inline)
    {
        if (inline is LineInline line)
        {
            int len = GetContainerPlainTextLength(line);
            if (line.NextSibling is LineInline nextLine
                && !(nextLine.IsEmpty && nextLine.NextSibling is null))
                len += 1;
            return len;
        }
        return inline switch
        {
            LiteralInline lit => lit.Content.Length,
            CodeInline code => code.Content.Length,
            AutolinkInline auto => auto.Url.Length,
            HtmlInline html => html.Content.Length,
            HtmlEntityInline entity => entity.Transcoded.Length,
            ContainerInline c => GetContainerPlainTextLength(c),
            _ => 0
        };
    }

    private int GetContainerInlineMapLength(ContainerInline container)
    {
        int length = 0;
        var child = container.FirstChild;
        while (child is not null)
        {
            length += GetInlineInlineMapLength(child);
            child = child.NextSibling;
        }
        return length;
    }

    private int GetInlineInlineMapLength(Inline inline) => inline switch
    {
        LiteralInline lit => lit.Content.Length,
        CodeInline code => code.Content.Length,
        AutolinkInline auto => auto.Url.Length,
        HtmlInline html => html.Content.Length,
        HtmlEntityInline entity => entity.Transcoded.Length,
        ContainerInline c => GetContainerInlineMapLength(c),
        _ => 0
    };

    private (Block Block, int LineIndex) GetBlockAndLineAt(int lineIndex)
    {
        if (lineIndex < 0) lineIndex = 0;
        int cumulative = 0;
        for (int i = 0; i < Children.Count; i++)
        {
            var block = Children[i];
            int linesInBlock = CountLinesInBlock(block);
            if (lineIndex < cumulative + linesInBlock)
            {
                return (block, lineIndex - cumulative);
            }
            cumulative += linesInBlock;
        }
        var lastBlock = Children[Children.Count - 1];
        return (lastBlock, CountLinesInBlock(lastBlock) - 1);
    }

    private Block GetBlockAtLine(int lineIndex)
    {
        var (block, _) = GetBlockAndLineAt(lineIndex);
        return block;
    }

    private static int CountLinesInBlock(Block block)
    {
        if (block is ParagraphBlock para)
        {
            if (para.Lines.Count == 0) return 1;
            if (para.HasTrailingLineBreak && para.Lines.Count > 1 && para.Lines[^1].IsEmpty)
                return para.Lines.Count - 1;
            return para.Lines.Count;
        }
        return 1;
    }

    private ParagraphBlock ExtractLineFromParagraph(ParagraphBlock para, int lineIndex)
    {
        para.SyncFromMergedIfNeeded();

        if (lineIndex < 0 || lineIndex >= para.Lines.Count)
            lineIndex = Math.Clamp(lineIndex, 0, para.Lines.Count - 1);

        var newPara = new ParagraphBlock();
        var targetLine = para.Lines[lineIndex];
        
        var child = targetLine.FirstChild;
        while (child is not null)
        {
            var next = child.NextSibling;
            newPara.GetOrCreateLine().AppendChild(child);
            child = next;
        }

        bool wasFollowedByTrailingBreak = para.HasTrailingLineBreak 
            && lineIndex == para.Lines.Count;

        para.Lines.RemoveAt(lineIndex);

        if (para.Lines.Count == 0 || (para.Lines.Count == 1 && para.Lines[0].IsEmpty && !para.HasTrailingLineBreak))
        {
            para.HasTrailingLineBreak = false;
        }
        
        else if (para.HasTrailingLineBreak && para.Lines.Count > 0 && para.Lines[^1].IsEmpty)
        {
            para.Lines.RemoveAt(para.Lines.Count - 1);
        }

        if (wasFollowedByTrailingBreak)
        {
            newPara.HasTrailingLineBreak = true;
        }

        para.MarkInlineDirty();
        newPara.MarkInlineDirty();

        var parent = para.Parent as ContainerBlock;
        if (parent is not null)
        {
            var idx = parent.IndexOfChild(para);
            if (idx >= 0)
            {
                bool paraIsEmpty = IsEmptyParagraph(para);
                if (paraIsEmpty)
                {
                    parent.ReplaceChild(para, newPara);
                }
                else
                {
                    parent.InsertChild(idx, newPara);
                }
            }
        }

        return newPara;
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
            else if (block is QuoteBlock || block is ListBlock)
                listItem.AddChild(block);
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
        else if (block is QuoteBlock || block is ListBlock)
            listItemBlock.AddChild(block);
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

        var groups = GroupContiguousSiblings(inlinesToWrap);
        foreach (var group in groups)
        {
            WrapInlinesInContainer(block, group, new EmphasisInline(delimiterChar, delimiterCount));
        }
    }

    private static List<List<Inline>> GroupContiguousSiblings(List<Inline> inlines)
    {
        var groups = new List<List<Inline>>();
        if (inlines.Count == 0) return groups;

        var currentGroup = new List<Inline> { inlines[0] };
        groups.Add(currentGroup);

        for (int i = 1; i < inlines.Count; i++)
        {
            var prev = inlines[i - 1];
            var curr = inlines[i];

            bool adjacent = prev.ParentInline == curr.ParentInline
                && prev.NextSibling is not null
                && ReferenceEquals(prev.NextSibling, curr);

            if (adjacent)
            {
                currentGroup.Add(curr);
            }
            else
            {
                currentGroup = new List<Inline> { curr };
                groups.Add(currentGroup);
            }
        }

        return groups;
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

            while (inline.ParentInline is not null and not InlineRoot and not LineInline)
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
            var firstParent = toWrap[0].ParentInline;
            bool sameParent = toWrap.All(i => i.ParentInline == firstParent);
            bool allAdjacent = sameParent && AreAllAdjacentSiblings(toWrap);

            if (!sameParent || !allAdjacent)
            {
                var rewrapped = new List<Inline>();
                var reseen = new HashSet<Inline>();

                foreach (var entry in entries)
                {
                    var inline = entry.Inline;

                    while (inline.ParentInline is not null and not InlineRoot and not LineInline)
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
                        if (parentStart <= parentEnd && parentStart >= start && parentEnd <= end)
                            inline = parent;
                        else
                            break;
                    }

                    if (reseen.Add(inline))
                        rewrapped.Add(inline);
                }

                toWrap = rewrapped;
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

    private static bool AreAllAdjacentSiblings(List<Inline> inlines)
    {
        if (inlines.Count <= 1) return true;

        for (int i = 1; i < inlines.Count; i++)
        {
            var prev = inlines[i - 1];
            var curr = inlines[i];

            if (prev.ParentInline != curr.ParentInline || prev.ParentInline is null)
                return false;

            if (prev.NextSibling is not null && !ReferenceEquals(prev.NextSibling, curr))
                return false;
        }

        return true;
    }

    private static bool LastLeafEndsWithSoftBreak(Block block)
    {
        if (block is ParagraphBlock para)
            return para.HasTrailingLineBreak;
        if (block is LeafBlock leaf)
            return leaf.Inline?.LastChild is LineInline;
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

    private void CleanupEmptyContainers(ContainerInline container)
    {
        var child = container.FirstChild;
        while (child is not null)
        {
            var next = child.NextSibling;

            if (child is ContainerInline childContainer)
            {
                CleanupEmptyContainers(childContainer);

                if (childContainer.FirstChild is null && child is not LineInline)
                    childContainer.Remove();
            }

            child = next;
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

                if (child is not LineInline)
                {
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

            if (child is LineInline)
            {
                UnwrapContainersInRange((ContainerInline)child, start, end, map);
            }
            else if (child is ContainerInline childContainer)
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

    private bool IsEmptyParagraph(Block block)
    {
        if (block is not ParagraphBlock para) return false;
        if (para.Lines.Count > 0 && para.Lines.Any(l => !l.IsEmpty))
            return false;
        if (para.HasTrailingLineBreak)
            return false;
        return para.Inline?.FirstChild is null;
    }

    private void RenderBlocks(IReadOnlyList<Block> blocks, StringBuilder sb, int indentLevel, string newLine)
    {
        Block? lastRendered = null;
        bool hadEmptyParagraphBefore = false;

        for (var i = 0; i < blocks.Count; i++)
        {
            if (IsEmptyParagraph(blocks[i]))
            {
                hadEmptyParagraphBefore = true;
                continue;
            }

            if (lastRendered is not null)
            {
                bool prevEndsWithSoftBreak = lastRendered is ParagraphBlock pp
                    && pp.HasTrailingLineBreak;
                if (!prevEndsWithSoftBreak)
                {
                    sb.Append(hadEmptyParagraphBefore
                        ? newLine + newLine
                        : newLine);
                }
            }
            else if (indentLevel > 0)
            {
                sb.Append(newLine);
            }

            lastRendered = blocks[i];
            hadEmptyParagraphBefore = false;
            RenderBlock(blocks[i], sb, indentLevel, newLine);
        }

        if (lastRendered is not null && hadEmptyParagraphBefore)
        {
            bool lastEndsWithSoftBreak = lastRendered is ParagraphBlock pp
                && pp.HasTrailingLineBreak;
            if (!lastEndsWithSoftBreak)
                sb.Append(newLine + newLine);
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
                if (para.HasTrailingLineBreak)
                    sb.Append('\n');
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
                var prefix = string.Join(" ", Enumerable.Repeat(">", quote.NestingLevel)) + " ";
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
                    prevEndsWithLB = prevPara.HasTrailingLineBreak;
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
                    if (para.HasTrailingLineBreak)
                        sb.Append('\n');
                    for (var j = 1; j < listItem.Children.Count; j++)
                    {
                        sb.Append(newLine);
                        RenderBlock(listItem.Children[j], sb, indentLevel + 1, newLine);
                    }
                }
                else if (listItem.Children[0] is QuoteBlock)
                {
                    RenderBlocks(listItem.Children, sb, indentLevel, newLine);
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
            case LineInline lineInline:
                RenderInlines(lineInline, sb);
                if (lineInline.NextSibling is LineInline nextLine
                    && !(nextLine.IsEmpty && nextLine.NextSibling is null))
                    sb.Append('\n');
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

    private int GetLineIndexAtOffset(int offset)
    {
        if (offset <= 0) return 0;
        int lineIndex = 0;
        bool prevWasNewline = false;
        foreach (var chunk in GetVisibleChunks())
        {
            for (int i = 0; i < chunk.Text.Length; i++)
            {
                if (chunk.Start + i >= offset)
                    return lineIndex;
                if (chunk.Text[i] == '\n')
                {
                    if (prevWasNewline)
                        prevWasNewline = false;
                    else
                    {
                        lineIndex++;
                        prevWasNewline = true;
                    }
                }
                else
                {
                    prevWasNewline = false;
                }
            }
        }
        return lineIndex;
    }

    private EditRangeResult CreateRange(int startOffset, int endOffset)
    {
        int lineStart = GetLineIndexAtOffset(startOffset);
        int lineEnd = endOffset > startOffset ? GetLineIndexAtOffset(endOffset - 1) : lineStart;
        return new EditRangeResult { StartOffset = startOffset, EndOffset = endOffset, LineStartIndex = lineStart, LineEndIndex = lineEnd };
    }

    private (int Start, int End) GetOffsetRangeForBlock(Block block)
    {
        var ranges = BuildBlockPlainTextRanges();
        if (block is LeafBlock leaf)
        {
            foreach (var r in ranges)
            {
                if (ReferenceEquals(r.Block, leaf))
                    return (r.PlainTextStart, r.PlainTextEnd);
            }
        }
        int minStart = int.MaxValue, maxEnd = int.MinValue;
        bool found = false;
        foreach (var r in ranges)
        {
            if (ReferenceEquals(r.Block, block) || IsDescendantBlockOf(r.Block, block))
            {
                minStart = Math.Min(minStart, r.PlainTextStart);
                maxEnd = Math.Max(maxEnd, r.PlainTextEnd);
                found = true;
            }
        }
        return found ? (minStart, maxEnd) : (0, 0);
    }

    private EditRangeResult GetRangeForBlock(Block block)
    {
        var (startOffset, endOffset) = GetOffsetRangeForBlock(block);
        return CreateRange(startOffset, endOffset);
    }

    private static bool IsDescendantBlockOf(Block child, Block ancestor)
    {
        var current = child.Parent;
        while (current is not null)
        {
            if (ReferenceEquals(current, ancestor))
                return true;
            current = current.Parent;
        }
        return false;
    }

    private static int CommonPrefixLength(string a, string b)
    {
        int len = Math.Min(a.Length, b.Length);
        for (int i = 0; i < len; i++)
        {
            if (a[i] != b[i]) return i;
        }
        return len;
    }

    private EditRangeResult ComputeInsertRange(string beforeText, string insertedText)
    {
        string afterText = GetPlainText();
        int commonPrefix = CommonPrefixLength(beforeText, afterText);
        if (string.IsNullOrEmpty(insertedText))
            return CreateRange(commonPrefix, commonPrefix);
        return CreateRange(commonPrefix, commonPrefix + insertedText.Length);
    }
}
