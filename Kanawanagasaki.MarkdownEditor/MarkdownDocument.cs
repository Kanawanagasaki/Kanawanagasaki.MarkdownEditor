using System.Text;
using Kanawanagasaki.MarkdownEditor.Ast;
using Kanawanagasaki.MarkdownEditor.Editing;

namespace Kanawanagasaki.MarkdownEditor;

/// <summary>
/// A markdown editor that maintains its state as an Abstract Syntax Tree (AST).
/// All content is represented as typed nodes — never raw markdown strings.
/// Provides methods for writing text, applying/removing styles, and converting
/// between block types.
/// </summary>
public partial class MarkdownDocument : Ast.MarkdownDocument
{
    /// <summary>
    /// Creates a new, empty MarkdownDocument.
    /// </summary>
    public MarkdownDocument() : base()
    {
    }

    #region Writing Operations

    /// <summary>
    /// Writes text at the end of the document. If the document is empty or the
    /// last block is not a paragraph, a new ParagraphBlock is created.
    /// The text is appended as a LiteralInline within the last paragraph.
    /// </summary>
    /// <param name="text">The text to write.</param>
    public void Write(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        var paragraph = GetOrCreateLastParagraph();
        AppendLiteralToParagraph(paragraph, text);
    }

    /// <summary>
    /// Writes text at the end of the document followed by a line break,
    /// effectively starting a new line. The next Write call will create a new paragraph.
    /// </summary>
    /// <param name="text">The text to write before the line break. If null, only a line break is written.</param>
    public void WriteLine(string? text = null)
    {
        if (text is not null)
            Write(text);

        // End the current paragraph by ensuring the next Write creates a new one
        _nextWriteCreatesNewParagraph = true;
    }

    /// <summary>
    /// Inserts a new line of text at the specified zero-based line offset.
    /// Line offset 0 inserts before the first block, 1 after the first block, etc.
    /// The inserted line becomes a new ParagraphBlock.
    /// </summary>
    /// <param name="lineOffset">The zero-based line offset at which to insert.</param>
    /// <param name="text">The text content of the new line.</param>
    public void InsertLine(int lineOffset, string text)
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
            AddChild(paragraph);
        }
        else
        {
            InsertChild(lineOffset, paragraph);
        }
    }

    /// <summary>
    /// Removes text in the given character range within the specified leaf block.
    /// The range is [start, end) (start inclusive, end exclusive), measured in the flattened inline text of that block.
    /// </summary>
    /// <param name="block">The leaf block containing the text to remove.</param>
    /// <param name="start">Start offset (inclusive) in the block's flattened text.</param>
    /// <param name="end">End offset (exclusive) in the block's flattened text.</param>
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

        // Convert to inclusive for internal processing
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

        // If the block now has no content, remove it from its parent
        if (block.Inline?.FirstChild is null)
        {
            if (block.Parent is ContainerBlock container)
                container.RemoveChild(block);
        }
    }

    /// <summary>
    /// Removes text in the given character range within the document's last (or only) paragraph.
    /// Convenience method for single-paragraph documents.
    /// </summary>
    /// <param name="start">Start offset (inclusive) in the paragraph's flattened text.</param>
    /// <param name="end">End offset (exclusive) in the paragraph's flattened text.</param>
    public void RemoveText(int start, int end)
    {
        var block = GetLastParagraph() ?? throw new InvalidOperationException("Document has no paragraph to remove text from.");
        RemoveText(block, start, end);
    }

    #endregion

    #region Block Conversion Operations

    /// <summary>
    /// Converts the specified block to a heading with the given level (1–6).
    /// If the block is already a HeadingBlock, its level is changed.
    /// If the block is a ParagraphBlock or other LeafBlock, it becomes a HeadingBlock
    /// with the same inline content.
    /// </summary>
    public void ConvertToHeading(LeafBlock block, int level)
    {
        ArgumentNullException.ThrowIfNull(block);
        if (level < 1) level = 1;
        if (level > 6) level = 6;

        if (block is HeadingBlock heading)
        {
            heading.Level = level;
            return;
        }

        var newHeading = new HeadingBlock(level)
        {
            Inline = block.Inline,
            Span = block.Span,
            Line = block.Line,
            Column = block.Column
        };

        // Re-parent the inline content
        if (newHeading.Inline is not null)
            newHeading.Inline.Parent = newHeading;

        ReplaceBlockInParent(block, newHeading);
    }

    /// <summary>
    /// Converts the block at the given line index to a heading with the specified level.
    /// </summary>
    public void ConvertToHeading(int lineIndex, int level)
    {
        var block = GetBlockAtLine(lineIndex);
        if (block is LeafBlock leaf)
            ConvertToHeading(leaf, level);
    }

    /// <summary>
    /// Converts the specified block to a blockquote.
    /// The block is wrapped in a QuoteBlock. If the block is already inside a QuoteBlock,
    /// the nesting level is increased.
    /// </summary>
    public void ConvertToBlockquote(Block block)
    {
        ArgumentNullException.ThrowIfNull(block);

        var parent = block.Parent as ContainerBlock;
        if (parent is null)
            throw new InvalidOperationException("Block has no parent container.");

        var index = parent.IndexOfChild(block);
        if (index < 0) return;

        var quote = new QuoteBlock { NestingLevel = 1 };

        // If the block is already a paragraph or leaf block, wrap it in the quote
        parent.RemoveChildAt(index);

        if (block is LeafBlock leaf)
        {
            quote.AddChild(leaf);
        }
        else if (block is ContainerBlock container)
        {
            // Move all children into the quote
            foreach (var child in container.Children.ToList())
            {
                container.RemoveChild(child);
                quote.AddChild(child);
            }
        }

        parent.InsertChild(index, quote);
    }

    /// <summary>
    /// Converts the block at the given line index to a blockquote.
    /// </summary>
    public void ConvertToBlockquote(int lineIndex)
    {
        var block = GetBlockAtLine(lineIndex);
        ConvertToBlockquote(block);
    }

    /// <summary>
    /// Wraps the specified block in an additional level of blockquote nesting.
    /// If the block is already inside a QuoteBlock, a new nested QuoteBlock is created.
    /// </summary>
    public void ConvertToNestedBlockquote(Block block)
    {
        ArgumentNullException.ThrowIfNull(block);

        var parent = block.Parent as ContainerBlock;
        if (parent is null)
            throw new InvalidOperationException("Block has no parent container.");

        var index = parent.IndexOfChild(block);
        if (index < 0) return;

        if (parent is QuoteBlock existingQuote)
        {
            // Create a nested quote
            var innerQuote = new QuoteBlock { NestingLevel = existingQuote.NestingLevel + 1 };
            parent.RemoveChildAt(index);
            innerQuote.AddChild(block);
            parent.InsertChild(index, innerQuote);
        }
        else
        {
            // Not in a quote yet, just wrap in a single quote (same as ConvertToBlockquote)
            ConvertToBlockquote(block);
        }
    }

    /// <summary>
    /// Converts the block at the given line index to a nested blockquote.
    /// </summary>
    public void ConvertToNestedBlockquote(int lineIndex)
    {
        var block = GetBlockAtLine(lineIndex);
        ConvertToNestedBlockquote(block);
    }

    /// <summary>
    /// Converts the specified block to an ordered list item.
    /// The block is wrapped in a ListBlock (ordered) containing a single ListItemBlock.
    /// </summary>
    public void ConvertToOrderedList(Block block)
    {
        ConvertToList(block, ListType.Ordered);
    }

    /// <summary>
    /// Converts the block at the given line index to an ordered list item.
    /// </summary>
    public void ConvertToOrderedList(int lineIndex)
    {
        var block = GetBlockAtLine(lineIndex);
        ConvertToOrderedList(block);
    }

    /// <summary>
    /// Converts the specified block to an unordered list item.
    /// The block is wrapped in a ListBlock (unordered) containing a single ListItemBlock.
    /// </summary>
    public void ConvertToUnorderedList(Block block)
    {
        ConvertToList(block, ListType.Unordered);
    }

    /// <summary>
    /// Converts the block at the given line index to an unordered list item.
    /// </summary>
    public void ConvertToUnorderedList(int lineIndex)
    {
        var block = GetBlockAtLine(lineIndex);
        ConvertToUnorderedList(block);
    }

    /// <summary>
    /// Inserts a horizontal rule (thematic break) at the end of the document.
    /// </summary>
    public void InsertHorizontalRule()
    {
        var hr = new ThematicBreakBlock();
        AddChild(hr);
        _nextWriteCreatesNewParagraph = true;
    }

    /// <summary>
    /// Inserts a horizontal rule (thematic break) at the specified line index.
    /// </summary>
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

    #endregion

    #region Inline Style Operations

    /// <summary>
    /// Applies bold styling to the text in the given character range [start, end) (start inclusive, end exclusive)
    /// within the specified leaf block's inline content.
    /// Internally wraps the affected inlines in an EmphasisInline with DelimiterCount=2.
    /// </summary>
    public void ApplyBold(LeafBlock block, int start, int end)
    {
        ApplyEmphasisStyle(block, start, end, '*', 2);
    }

    /// <summary>
    /// Applies bold styling to the text in the given character range [start, end) (start inclusive, end exclusive)
    /// within the document's last paragraph. Convenience method for single-paragraph documents.
    /// </summary>
    public void ApplyBold(int start, int end)
    {
        var block = GetLastParagraph() ?? throw new InvalidOperationException("Document has no paragraph.");
        ApplyBold(block, start, end);
    }

    /// <summary>
    /// Applies italic styling to the text in the given character range [start, end) (start inclusive, end exclusive)
    /// within the specified leaf block's inline content.
    /// </summary>
    public void ApplyItalic(LeafBlock block, int start, int end)
    {
        ApplyEmphasisStyle(block, start, end, '*', 1);
    }

    /// <summary>
    /// Applies italic styling to the text in the given character range [start, end) (start inclusive, end exclusive)
    /// within the document's last paragraph.
    /// </summary>
    public void ApplyItalic(int start, int end)
    {
        var block = GetLastParagraph() ?? throw new InvalidOperationException("Document has no paragraph.");
        ApplyItalic(block, start, end);
    }

    /// <summary>
    /// Applies inline code styling to the text in the given character range [start, end) (start inclusive, end exclusive)
    /// within the specified leaf block. The affected text is replaced by a CodeInline node.
    /// </summary>
    public void ApplyCode(LeafBlock block, int start, int end)
    {
        ArgumentNullException.ThrowIfNull(block);
        if (block.Inline is null) return;

        var map = InlineOffsetMap.Build(block);
        if (start < 0) start = 0;
        if (end > map.TotalLength) end = map.TotalLength;
        if (start >= end) return;

        // Convert to inclusive for internal processing
        var inclusiveEnd = end - 1;

        // Collect the text content in the range
        var sb = new StringBuilder();
        var entries = map.GetEntriesInRange(start, inclusiveEnd);
        foreach (var entry in entries)
        {
            var relStart = Math.Max(0, start - entry.Start);
            var relEnd = Math.Min(entry.Length - 1, inclusiveEnd - entry.Start);

            if (entry.Inline is LiteralInline lit)
                sb.Append(lit.Content.AsSpan(relStart, relEnd - relStart + 1));
            else if (entry.Inline is CodeInline code)
                sb.Append(code.Content.AsSpan(relStart, relEnd - relStart + 1));
        }

        var codeContent = sb.ToString();

        // Remove the range first, then insert CodeInline
        RemoveTextInRange(block, start, inclusiveEnd, map, entries);

        // Insert the CodeInline at the start position
        var codeInline = new CodeInline(codeContent);
        InsertInlineAtOffset(block, start, codeInline);
    }

    /// <summary>
    /// Applies inline code styling to the text in the given character range [start, end) (start inclusive, end exclusive)
    /// within the document's last paragraph.
    /// </summary>
    public void ApplyCode(int start, int end)
    {
        var block = GetLastParagraph() ?? throw new InvalidOperationException("Document has no paragraph.");
        ApplyCode(block, start, end);
    }

    /// <summary>
    /// Applies strikethrough styling to the text in the given character range [start, end) (start inclusive, end exclusive)
    /// within the specified leaf block's inline content.
    /// </summary>
    public void ApplyStrikethrough(LeafBlock block, int start, int end)
    {
        ApplyEmphasisStyle(block, start, end, '~', 2);
    }

    /// <summary>
    /// Applies strikethrough styling to the text in the given character range [start, end) (start inclusive, end exclusive)
    /// within the document's last paragraph.
    /// </summary>
    public void ApplyStrikethrough(int start, int end)
    {
        var block = GetLastParagraph() ?? throw new InvalidOperationException("Document has no paragraph.");
        ApplyStrikethrough(block, start, end);
    }

    /// <summary>
    /// Converts the text in the given range to an image inline.
    /// The existing text becomes the alt text, and the URL is set to the provided value.
    /// </summary>
    public void MakeImage(LeafBlock block, int start, int end, string url, string? title = null)
    {
        MakeLinkOrImage(block, start, end, url, title, isImage: true);
    }

    /// <summary>
    /// Converts the text in the given range to an image inline in the document's last paragraph.
    /// </summary>
    public void MakeImage(int start, int end, string url, string? title = null)
    {
        var block = GetLastParagraph() ?? throw new InvalidOperationException("Document has no paragraph.");
        MakeImage(block, start, end, url, title);
    }

    /// <summary>
    /// Converts the text in the given range to a link inline.
    /// The existing text becomes the link text, and the URL is set to the provided value.
    /// </summary>
    public void MakeLink(LeafBlock block, int start, int end, string url, string? title = null)
    {
        MakeLinkOrImage(block, start, end, url, title, isImage: false);
    }

    /// <summary>
    /// Converts the text in the given range to a link inline in the document's last paragraph.
    /// </summary>
    public void MakeLink(int start, int end, string url, string? title = null)
    {
        var block = GetLastParagraph() ?? throw new InvalidOperationException("Document has no paragraph.");
        MakeLink(block, start, end, url, title);
    }

    #endregion

    #region Clear Style Operations

    /// <summary>
    /// Clears all styles from the entire document, converting all styled inlines
    /// back to plain LiteralInline nodes and all blocks to ParagraphBlocks.
    /// </summary>
    public void ClearAllStyles()
    {
        foreach (var child in Children.ToList())
        {
            ClearBlockStyles(child);
        }
    }

    /// <summary>
    /// Clears all styles from the block at the given line index.
    /// ContainerInline nodes (EmphasisInline, LinkInline) are unwrapped,
    /// leaving only LiteralInline and other LeafInline nodes.
    /// </summary>
    public void ClearStylesForLine(int lineIndex)
    {
        var block = GetBlockAtLine(lineIndex);
        ClearBlockStyles(block);
    }

    /// <summary>
    /// Clears all styles from the specified block.
    /// </summary>
    public void ClearStylesForBlock(Block block)
    {
        ClearBlockStyles(block);
    }

    /// <summary>
    /// Clears styles for a character range [start, end) (start inclusive, end exclusive) within the
    /// specified leaf block's inline content. Only ContainerInline nodes that
    /// overlap with the range are unwrapped; others are left intact.
    /// </summary>
    public void ClearStylesForRange(LeafBlock block, int start, int end)
    {
        ArgumentNullException.ThrowIfNull(block);
        if (block.Inline is null) return;

        var map = InlineOffsetMap.Build(block);
        if (start < 0) start = 0;
        if (end > map.TotalLength) end = map.TotalLength;
        if (start >= end) return;

        // Convert to inclusive for internal processing
        var inclusiveEnd = end - 1;

        // Find all ContainerInline nodes in the tree and check if they overlap the range
        UnwrapContainersInRange(block.Inline, start, inclusiveEnd, map);
    }

    /// <summary>
    /// Clears styles for a character range [start, end) (start inclusive, end exclusive) within the
    /// document's last paragraph.
    /// </summary>
    public void ClearStylesForRange(int start, int end)
    {
        var block = GetLastParagraph() ?? throw new InvalidOperationException("Document has no paragraph.");
        ClearStylesForRange(block, start, end);
    }

    #endregion

    #region Rendering

    /// <summary>
    /// Renders the AST back to markdown text. This is the serialization of the AST
    /// to standard markdown syntax.
    /// </summary>
    public string ToMarkdown()
    {
        var sb = new StringBuilder();
        RenderBlocks(Children, sb, 0);
        return sb.ToString();
    }

    #endregion

    #region Internal State

    private bool _nextWriteCreatesNewParagraph;

    #endregion

    #region Private Helpers - Writing

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

    private static void AppendLiteralToParagraph(ParagraphBlock paragraph, string text)
    {
        paragraph.Inline ??= new InlineRoot();

        // If the last child is a LiteralInline, append to it for efficiency
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

    #endregion

    #region Private Helpers - Block Lookup

    private Block GetBlockAtLine(int lineIndex)
    {
        if (lineIndex < 0) lineIndex = 0;
        if (lineIndex < Children.Count)
            return Children[lineIndex];
        throw new ArgumentOutOfRangeException(nameof(lineIndex), $"Line index {lineIndex} is out of range. Document has {Children.Count} top-level blocks.");
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

    #endregion

    #region Private Helpers - List Conversion

    private void ConvertToList(Block block, ListType listType)
    {
        ArgumentNullException.ThrowIfNull(block);

        var parent = block.Parent as ContainerBlock;
        if (parent is null)
            throw new InvalidOperationException("Block has no parent container.");

        var index = parent.IndexOfChild(block);
        if (index < 0) return;

        // Check if the previous sibling is already a ListBlock of the same type
        if (index > 0 && parent.Children[index - 1] is ListBlock prevList && prevList.ListType == listType)
        {
            // Add to existing list
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

        // Create a new list
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

    #endregion

    #region Private Helpers - Inline Style Application

    private void ApplyEmphasisStyle(LeafBlock block, int start, int end, char delimiterChar, int delimiterCount)
    {
        ArgumentNullException.ThrowIfNull(block);
        if (block.Inline is null) return;

        var map = InlineOffsetMap.Build(block);
        if (start < 0) start = 0;
        if (end > map.TotalLength) end = map.TotalLength;
        if (start >= end) return;

        // Convert to inclusive for internal processing
        var inclusiveEnd = end - 1;

        // Step 1: Split at the start boundary, then rebuild the map
        SplitAtOffset(block, start);
        map = InlineOffsetMap.Build(block);

        // Step 2: Split at end boundary (after the last character), then rebuild
        if (end < map.TotalLength)
        {
            SplitAtOffset(block, end);
            map = InlineOffsetMap.Build(block);
        }

        // Step 3: Collect entries in range
        var entries = map.GetEntriesInRange(start, inclusiveEnd);
        if (entries.Count == 0) return;

        // Step 4: Collect the inlines to wrap — find the innermost sibling group
        var inlinesToWrap = CollectInlinesToWrap(entries, start, inclusiveEnd, map);
        if (inlinesToWrap.Count == 0) return;

        // Step 5: Create the emphasis container and wrap the inlines
        WrapInlinesInContainer(block, inlinesToWrap, new EmphasisInline(delimiterChar, delimiterCount));
    }

    /// <summary>
    /// Splits a LiteralInline at the given offset within the block's flattened text.
    /// Rebuild-agnostic: just finds the literal at that offset and splits it.
    /// </summary>
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

    /// <summary>
    /// Wraps a list of sibling inlines into a new ContainerInline (e.g., EmphasisInline, LinkInline).
    /// The inlines are removed from their current position in the linked list, placed inside the
    /// container, and the container is inserted at the position of the first removed inline.
    /// </summary>
    private void WrapInlinesInContainer(LeafBlock block, List<Inline> inlinesToWrap, ContainerInline container)
    {
        var firstInline = inlinesToWrap[0];
        var parentInline = firstInline.ParentInline ?? block.Inline!;

        // Remember the position where the first inline was
        var insertBefore = firstInline.PreviousSibling;  // The inline before our range
        var insertAsFirstChild = insertBefore is null;     // If no previous sibling, we were the first child

        // Remove all inlines from their current positions and add to the container
        foreach (var inline in inlinesToWrap)
        {
            inline.Remove();
            container.AppendChild(inline);
        }

        // Insert the container at the saved position
        if (insertAsFirstChild)
        {
            // Was the first child — prepend or set as first child
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
            // Insert after the inline that was before our range
            InlineSplitter.InsertAfterInline(insertBefore!, container);
        }
    }

    private List<Inline> CollectInlinesToWrap(List<InlineOffsetMap.Entry> entries, int start, int end, InlineOffsetMap map)
    {
        // After boundary splitting, the entries in [start, end] should correspond
        // to complete inline nodes that are direct siblings at some nesting level.
        // We collect these inlines, taking care to:
        //  1) Never wrap the InlineRoot — it's the block's root container
        //  2) If a ContainerInline is fully within the range, wrap it as a whole
        //     rather than wrapping its children individually
        //  3) Only wrap inlines that are siblings at the same level

        // Step 1: Collect the set of inlines that correspond to entries in the range.
        // These should be "leaf" inlines (LiteralInline, CodeInline, etc.) since
        // entries are built from the flattened text.
        var entryInlines = new HashSet<Inline>(entries.Select(e => e.Inline));

        // Step 2: For each entry inline, determine what to wrap.
        // Climb up the tree while the parent ContainerInline is fully within the range,
        // but stop before reaching InlineRoot.
        var toWrap = new List<Inline>();
        var seen = new HashSet<Inline>();

        foreach (var entry in entries)
        {
            var inline = entry.Inline;

            // Climb up while the parent container is fully covered by the range.
            // "Fully covered" means ALL of the parent's descendant entries fall within [start, end].
            while (inline.ParentInline is not null and not InlineRoot)
            {
                var parent = inline.ParentInline;

                // Compute the full extent of this parent across ALL entries (not just range entries)
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

                // If the parent's full extent is within [start, end], climb up
                if (parentStart <= parentEnd && parentStart >= start && parentEnd <= end)
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

        // Step 3: If we have multiple inlines at different nesting levels,
        // keep only those at the same level (siblings with the same parent).
        if (toWrap.Count > 1)
        {
            var groups = toWrap.GroupBy(i => i.ParentInline).ToList();
            if (groups.Count > 1)
            {
                // Prefer the group with the most members — typically the direct children level
                toWrap = groups.OrderByDescending(g => g.Count()).First().ToList();
            }
        }

        // Step 4: Sort by position in the sibling linked list
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

    private void MakeLinkOrImage(LeafBlock block, int start, int end, string url, string? title, bool isImage)
    {
        ArgumentNullException.ThrowIfNull(block);
        if (block.Inline is null) return;

        var map = InlineOffsetMap.Build(block);
        if (start < 0) start = 0;
        if (end > map.TotalLength) end = map.TotalLength;
        if (start >= end) return;

        // Convert to inclusive for internal processing
        var inclusiveEnd = end - 1;

        // Step 1: Split at the start boundary, then rebuild the map
        SplitAtOffset(block, start);
        map = InlineOffsetMap.Build(block);

        // Step 2: Split at end boundary, then rebuild
        if (end < map.TotalLength)
        {
            SplitAtOffset(block, end);
            map = InlineOffsetMap.Build(block);
        }

        // Step 3: Collect entries and inlines to wrap
        var entries = map.GetEntriesInRange(start, inclusiveEnd);
        var inlinesToWrap = CollectInlinesToWrap(entries, start, inclusiveEnd, map);
        if (inlinesToWrap.Count == 0) return;

        // Step 4: Create the link/image and wrap the inlines
        var link = new LinkInline
        {
            Url = url ?? string.Empty,
            Title = title,
            IsImage = isImage
        };

        WrapInlinesInContainer(block, inlinesToWrap, link);
    }

    #endregion

    #region Private Helpers - Text Removal

    private void RemoveFromLiteral(LiteralInline lit, int relStart, int relEnd, int contentOffset)
    {
        var content = lit.Content;
        var absStart = contentOffset + relStart;
        var absEnd = contentOffset + relEnd;

        if (absStart <= 0 && absEnd >= content.Length - 1)
        {
            // Remove entire literal
            lit.Remove();
        }
        else if (absStart <= 0)
        {
            // Remove from start
            lit.Content = content[(absEnd + 1)..];
        }
        else if (absEnd >= content.Length - 1)
        {
            // Remove from end
            lit.Content = content[..absStart];
        }
        else
        {
            // Remove from middle — split into two literals
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
            // Remove entire code inline
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
            // Remove from middle — convert to literal + gap + literal
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
                // Replace code with literal
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

        // Find the position in the linked list where this offset falls
        var map = InlineOffsetMap.Build(block);

        if (map.TotalLength == 0 || offset <= 0)
        {
            // Insert at the beginning
            block.Inline.PrependChild(newInline);
            return;
        }

        if (offset >= map.TotalLength)
        {
            block.Inline.AppendChild(newInline);
            return;
        }

        // Find the entry at the offset
        var entry = map.FindEntryAt(offset);
        if (entry is not null)
        {
            // Insert before this entry's inline
            InlineSplitter.InsertBeforeInline(entry.Inline, newInline);
        }
        else
        {
            block.Inline.AppendChild(newInline);
        }
    }

    #endregion

    #region Private Helpers - Style Clearing

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
                // First recurse into the container's children
                UnwrapAllContainers(childContainer);

                // Then unwrap: move the container's children to the parent
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

                // Remove the now-empty container
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
                // Check if this container overlaps the range
                var containerEntries = map.Entries.Where(e =>
                    e.Inline == childContainer || IsDescendantOf(e.Inline, childContainer)).ToList();

                if (containerEntries.Count > 0)
                {
                    var containerStart = containerEntries.Min(e => e.Start);
                    var containerEnd = containerEntries.Max(e => e.End);

                    bool fullyInRange = containerStart >= start && containerEnd <= end;

                    if (fullyInRange)
                    {
                        // Unwrap: move children up to parent level
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
                        // Partially in range — recurse into children
                        UnwrapContainersInRange(childContainer, start, end, map);
                    }
                }
            }

            child = next;
        }
    }

    #endregion

    #region Private Helpers - Rendering

    private void RenderBlocks(IReadOnlyList<Block> blocks, StringBuilder sb, int indentLevel)
    {
        for (var i = 0; i < blocks.Count; i++)
        {
            if (i > 0 || indentLevel > 0)
                sb.AppendLine();

            var block = blocks[i];
            RenderBlock(block, sb, indentLevel);
        }
    }

    private void RenderBlock(Block block, StringBuilder sb, int indentLevel)
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
                RenderList(list, sb, indentLevel);
                break;

            case ListItemBlock listItem:
                RenderBlocks(listItem.Children, sb, indentLevel);
                break;

            case QuoteBlock quote:
                sb.Append(indent);
                sb.Append("> ");
                // Render children with quote prefix
                var quoteSb = new StringBuilder();
                RenderBlocks(quote.Children, quoteSb, 0);
                var quoteText = quoteSb.ToString();
                // Add "> " prefix to each line
                var lines = quoteText.Split('\n');
                for (var i = 0; i < lines.Length; i++)
                {
                    if (i > 0) sb.AppendLine();
                    sb.Append(indent);
                    sb.Append("> ");
                    sb.Append(lines[i].TrimEnd('\r'));
                }
                break;

            case FencedCodeBlock fenced:
                sb.Append(indent);
                sb.Append(new string(fenced.FenceChar, fenced.FenceCount));
                if (!string.IsNullOrEmpty(fenced.Info))
                    sb.Append(fenced.Info);
                sb.AppendLine();
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

    private void RenderList(ListBlock list, StringBuilder sb, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 4);
        for (var i = 0; i < list.Children.Count; i++)
        {
            if (i > 0)
                sb.AppendLine();

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
                    // Render remaining children as nested blocks
                    for (var j = 1; j < listItem.Children.Count; j++)
                    {
                        sb.AppendLine();
                        RenderBlock(listItem.Children[j], sb, indentLevel + 1);
                    }
                }
                else
                {
                    RenderBlocks(listItem.Children, sb, indentLevel + 1);
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
                sb.Append(code.Content);
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
                // Unknown container type, just render children
                RenderInlines(container, sb);
                break;
        }
    }

    #endregion
}
