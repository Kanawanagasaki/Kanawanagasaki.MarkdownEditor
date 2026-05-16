namespace Kanawanagasaki.MarkdownEditor.Tests;

public class TextRangeTests
{
    [Fact]
    public void Write_ReturnsCorrectOffsets()
    {
        var doc = new MarkdownDocument();
        var range = doc.Write("One ");
        doc.Write("two");

        Assert.Equal(new TextOffset(0), range.StartOffset);
        Assert.Equal(new TextOffset(4), range.EndOffset);
        Assert.Equal(new BlockIndex(0), range.StartBlock);
        Assert.Equal(new BlockIndex(0), range.EndBlock);
    }

    [Fact]
    public void Write_ChainedReturnsCumulativeOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("One ");
        var range = doc.Write("two");
        doc.Write(" three");

        Assert.Equal(new TextOffset(4), range.StartOffset);
        Assert.Equal(new TextOffset(7), range.EndOffset);
    }

    [Fact]
    public void Write_EmptyString_ReturnsZeroLengthRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello");
        var range = doc.Write("");

        Assert.Equal(new TextOffset(5), range.StartOffset);
        Assert.Equal(new TextOffset(5), range.EndOffset);
    }

    [Fact]
    public void WriteLine_WithText_ReturnsRangeOfWrittenText()
    {
        var doc = new MarkdownDocument();
        var range = doc.WriteLine("Hello");

        Assert.Equal(new TextOffset(0), range.StartOffset);
        Assert.Equal(new TextOffset(5), range.EndOffset);
    }

    [Fact]
    public void WriteParagraph_WithText_ReturnsCorrectRange()
    {
        var doc = new MarkdownDocument();
        var range = doc.WriteParagraph("Hello");

        Assert.Equal(new TextOffset(0), range.StartOffset);
        Assert.Equal(new TextOffset(5), range.EndOffset);
    }

    [Fact]
    public void InsertLine_ReturnsCorrectRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("Original");
        var range = doc.InsertLine(new BlockIndex(0), lineWithinBlock: 0, "Inserted");

        Assert.Equal(new TextOffset(0), range.StartOffset);
        Assert.Equal(new TextOffset(9), range.EndOffset);
    }

    [Fact]
    public void InsertLine_InMiddle_ReturnsCorrectRange()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("First");
        doc.WriteLine("Third");
        var range = doc.InsertLine(new BlockIndex(0), lineWithinBlock: 1, "Second");

        Assert.Equal(new TextOffset(6), range.StartOffset);
        Assert.Equal(new TextOffset(13), range.EndOffset);
    }

    [Fact]
    public void InsertParagraph_ReturnsCorrectRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("Original");
        var range = doc.InsertParagraph(new BlockIndex(0), "Inserted");

        Assert.Equal(new TextOffset(0), range.StartOffset);
        Assert.Equal(new TextOffset(10), range.EndOffset);
    }

    [Fact]
    public void ApplyBold_RangeCanBeUsed()
    {
        var doc = new MarkdownDocument();
        doc.Write("One ");
        var range = doc.Write("two");
        doc.Write(" three");
        doc.ApplyBold(range);

        Assert.Equal("One **two** three", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ApplyBold_ReturnsSameOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello world");
        var result = doc.ApplyBold(new TextOffset(0), new TextOffset(5));

        Assert.Equal(new TextOffset(0), result.StartOffset);
        Assert.Equal(new TextOffset(5), result.EndOffset);
    }

    [Fact]
    public void ApplyItalic_ReturnsSameOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello world");
        var result = doc.ApplyItalic(new TextOffset(6), new TextOffset(11));

        Assert.Equal(new TextOffset(6), result.StartOffset);
        Assert.Equal(new TextOffset(11), result.EndOffset);
    }

    [Fact]
    public void ApplyCode_ReturnsSameOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("some code here");
        var result = doc.ApplyCode(new TextOffset(5), new TextOffset(9));

        Assert.Equal(new TextOffset(5), result.StartOffset);
        Assert.Equal(new TextOffset(9), result.EndOffset);
    }

    [Fact]
    public void ApplyStrikethrough_ReturnsSameOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("strike this");
        var result = doc.ApplyStrikethrough(new TextOffset(7), new TextOffset(11));

        Assert.Equal(new TextOffset(7), result.StartOffset);
        Assert.Equal(new TextOffset(11), result.EndOffset);
    }

    [Fact]
    public void MakeLink_RangeCanBeUsed()
    {
        var doc = new MarkdownDocument();
        doc.Write("Click ");
        var range = doc.Write("here");
        doc.Write(" for more");
        doc.MakeLink(range, "https://example.com");

        Assert.Equal("Click [here](https://example.com) for more", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void MakeLink_ReturnsSameOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("Visit example site");
        var result = doc.MakeLink(new TextOffset(6), new TextOffset(13), "https://example.com");

        Assert.Equal(new TextOffset(6), result.StartOffset);
        Assert.Equal(new TextOffset(13), result.EndOffset);
    }

    [Fact]
    public void MakeImage_RangeCanBeUsed()
    {
        var doc = new MarkdownDocument();
        doc.Write("See ");
        var range = doc.Write("photo");
        doc.Write(" here");
        doc.MakeImage(range, "https://example.com/img.png");

        Assert.Equal("See ![photo](https://example.com/img.png) here", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ConvertToHeading_ByBlockIndex_FromWriteRange()
    {
        var doc = new MarkdownDocument();
        var range = doc.Write("Title");
        doc.ConvertToHeading(range.StartBlock, 1);

        Assert.Equal("# Title", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ConvertToHeading_MultipleLines_ByWriteRanges()
    {
        var doc = new MarkdownDocument();
        var titleRange = doc.WriteLine("Title");
        var subtitleRange = doc.WriteLine("Subtitle");
        var thirdLineRange = doc.WriteLine("Just a third line");

        doc.ConvertToHeading(titleRange.StartBlock, 1);
        doc.ConvertToHeading(subtitleRange.StartBlock, 2);
        doc.ConvertToHeading(thirdLineRange.StartBlock, 3);

        Assert.Equal("# Title\n## Subtitle\n### Just a third line", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ConvertToHeading_SecondParagraph_ByWriteRange()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("One");
        var range = doc.WriteParagraph("Two");
        doc.WriteParagraph("Three");

        doc.ConvertToHeading(range.StartBlock, 2);

        Assert.Equal("One\n\n## Two\n\nThree", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ConvertToBlockquote_ReturnsCorrectRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("A quote");
        var range = doc.ConvertToBlockquote(new BlockIndex(0));

        Assert.Equal(new TextOffset(0), range.StartOffset);
        Assert.Equal(new TextOffset(7), range.EndOffset);
        Assert.Equal(new BlockIndex(0), range.StartBlock);
        Assert.Equal(new BlockIndex(0), range.EndBlock);
    }

    [Fact]
    public void ConvertToOrderedList_ReturnsCorrectRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("First item");
        var range = doc.ConvertToOrderedList(new BlockIndex(0));

        Assert.Equal(new TextOffset(0), range.StartOffset);
        Assert.Equal(new TextOffset(10), range.EndOffset);
        Assert.Equal(new BlockIndex(0), range.StartBlock);
        Assert.Equal(new BlockIndex(0), range.EndBlock);
        Assert.Equal("1. First item", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ConvertToUnorderedList_ReturnsCorrectRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bullet item");
        var range = doc.ConvertToUnorderedList(new BlockIndex(0));

        Assert.Equal(new TextOffset(0), range.StartOffset);
        Assert.Equal(new TextOffset(11), range.EndOffset);
        Assert.Equal("- Bullet item", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ClearAllStyles_ReturnsFullRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bold text");
        doc.ApplyBold(new TextOffset(0), new TextOffset(4));
        var range = doc.ClearAllStyles();

        Assert.Equal(new TextOffset(0), range.StartOffset);
        Assert.Equal(new TextOffset(9), range.EndOffset);
    }

    [Fact]
    public void ClearStylesForRange_ReturnsSameOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("AAA BBB CCC");
        doc.ApplyBold(new TextOffset(4), new TextOffset(7));
        var range = doc.ClearStylesForRange(new TextOffset(4), new TextOffset(7));

        Assert.Equal(new TextOffset(4), range.StartOffset);
        Assert.Equal(new TextOffset(7), range.EndOffset);
    }

    [Fact]
    public void ClearStylesForRange_PreservesUnaffectedStyles()
    {
        var doc = new MarkdownDocument();
        doc.Write("AAA BBB CCC");
        doc.ApplyBold(new TextOffset(0), new TextOffset(3));
        doc.ApplyBold(new TextOffset(8), new TextOffset(11));
        var range = doc.ClearStylesForRange(new TextOffset(1), new TextOffset(5));

        Assert.Equal(new TextOffset(1), range.StartOffset);
        Assert.Equal(new TextOffset(5), range.EndOffset);
        Assert.Equal("**A**AA BBB **CCC**", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void InsertHorizontalRule_ReturnsPointRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("Before");
        var range = doc.InsertHorizontalRule();

        Assert.Equal("""
            Before

            ---
            """, doc.ToMarkdown("\n"));

        Assert.Equal(new TextOffset(8), range.StartOffset);
        Assert.Equal(new TextOffset(11), range.EndOffset);
    }

    [Fact]
    public void InsertHorizontalRule_AtBlockIndex_ReturnsPointRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("After");
        var range = doc.InsertHorizontalRule(new BlockIndex(0));

        Assert.Equal(new TextOffset(0), range.StartOffset);
        Assert.Equal(new TextOffset(2), range.EndOffset);
    }

    [Fact]
    public void WriteThenApplyBoldThenMakeLink_FullChain()
    {
        var doc = new MarkdownDocument();
        doc.Write("Click ");
        var range = doc.Write("here");
        doc.Write(" for info");
        doc.ApplyBold(range);
        doc.MakeLink(range, "https://example.com");

        Assert.Equal("Click **[here](https://example.com)** for info", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void WriteThenMakeImageThenApplyBold_FullChain()
    {
        var doc = new MarkdownDocument();
        doc.Write("See ");
        var range = doc.Write("logo");
        doc.Write(" icon");
        doc.MakeImage(range, "https://example.com/logo.png");
        doc.ApplyBold(range);

        Assert.Equal("See ![**logo**](https://example.com/logo.png) icon", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void WriteThenApplyItalicThenConvertToBlockquote_Chain()
    {
        var doc = new MarkdownDocument();
        doc.Write("A ");
        var range = doc.Write("quote");
        doc.ApplyItalic(range);
        doc.ConvertToBlockquote(range.StartBlock);

        Assert.Equal("> A *quote*", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void WriteThenConvertToOrderedList_WithStyle()
    {
        var doc = new MarkdownDocument();
        var r1 = doc.WriteLine("Bold item");
        var r2 = doc.WriteLine("Plain item");
        var r3 = doc.Write("Styled");
        doc.ConvertToOrderedList(r1.StartBlock);
        doc.ConvertToOrderedList(r2.StartBlock);
        doc.ConvertToOrderedList(r3.StartBlock);
        doc.ApplyBold(r3);

        Assert.Equal("1. Bold item\n2. Plain item\n3. **Styled**", doc.ToMarkdown("\n"));
    }
}
