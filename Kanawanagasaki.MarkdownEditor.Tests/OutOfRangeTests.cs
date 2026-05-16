namespace Kanawanagasaki.MarkdownEditor.Tests;

public class OutOfRangeTests
{
    [Fact]
    public void ApplyBold_NegativeOffset_Throws()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.ApplyBold(new TextOffset(-1), new TextOffset(5)));
    }

    [Fact]
    public void ApplyBold_OffsetBeyondLength_Throws()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.ApplyBold(new TextOffset(0), new TextOffset(100)));
    }

    [Fact]
    public void ApplyItalic_NegativeOffset_Throws()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.ApplyItalic(new TextOffset(-3), new TextOffset(5)));
    }

    [Fact]
    public void ApplyCode_OffsetBeyondLength_Throws()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.ApplyCode(new TextOffset(0), new TextOffset(50)));
    }

    [Fact]
    public void ApplyStrikethrough_NegativeOffset_Throws()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.ApplyStrikethrough(new TextOffset(-1), new TextOffset(5)));
    }

    [Fact]
    public void MakeLink_NegativeOffset_Throws()
    {
        var doc = new MarkdownDocument();
        doc.Write("Click here");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.MakeLink(new TextOffset(-1), new TextOffset(5), "https://example.com"));
    }

    [Fact]
    public void MakeImage_OffsetBeyondLength_Throws()
    {
        var doc = new MarkdownDocument();
        doc.Write("See logo");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.MakeImage(new TextOffset(0), new TextOffset(100), "https://example.com/img.png"));
    }

    [Fact]
    public void RemoveText_NegativeOffset_Throws()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.RemoveText(new TextOffset(-5), new TextOffset(2)));
    }

    [Fact]
    public void RemoveText_OffsetBeyondLength_Throws()
    {
        var doc = new MarkdownDocument();
        doc.Write("abc");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.RemoveText(new TextOffset(0), new TextOffset(100)));
    }

    [Fact]
    public void ConvertToHeading_BlockIndexBeyondCount_Throws()
    {
        var doc = new MarkdownDocument();
        doc.Write("Title");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.ConvertToHeading(new BlockIndex(5), 1));
    }

    [Fact]
    public void ConvertToHeading_NegativeBlockIndex_Throws()
    {
        var doc = new MarkdownDocument();
        doc.Write("Title");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.ConvertToHeading(new BlockIndex(-1), 1));
    }

    [Fact]
    public void ConvertToOrderedList_NegativeBlockIndex_Throws()
    {
        var doc = new MarkdownDocument();
        doc.Write("Item");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.ConvertToOrderedList(new BlockIndex(-1)));
    }

    [Fact]
    public void ConvertToUnorderedList_BlockIndexBeyondCount_Throws()
    {
        var doc = new MarkdownDocument();
        doc.Write("Item");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.ConvertToUnorderedList(new BlockIndex(10)));
    }

    [Fact]
    public void ConvertToBlockquote_NegativeBlockIndex_Throws()
    {
        var doc = new MarkdownDocument();
        doc.Write("Quote");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.ConvertToBlockquote(new BlockIndex(-1)));
    }

    [Fact]
    public void ConvertToHeading_LevelAbove6_Throws()
    {
        var doc = new MarkdownDocument();
        doc.Write("Title");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.ConvertToHeading(new BlockIndex(0), 99));
    }

    [Fact]
    public void InsertLine_NegativeBlockIndex_Throws()
    {
        var doc = new MarkdownDocument();
        doc.Write("Text");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.InsertLine(new BlockIndex(-5), 0, "Inserted"));
    }

    [Fact]
    public void InsertParagraph_BlockIndexBeyondCount_Throws()
    {
        var doc = new MarkdownDocument();
        doc.Write("Text");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.InsertParagraph(new BlockIndex(100), "Inserted"));
    }

    [Fact]
    public void ClearStylesForRange_NegativeOffset_Throws()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bold text");
        doc.ApplyBold(new TextOffset(0), new TextOffset(4));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.ClearStylesForRange(new TextOffset(-1), new TextOffset(4)));
    }

    [Fact]
    public void ClearStylesForLine_NegativeBlockIndex_Throws()
    {
        var doc = new MarkdownDocument();
        doc.Write("Text");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.ClearStylesForLine(new BlockIndex(-1)));
    }

    [Fact]
    public void InsertHorizontalRule_NegativeBlockIndex_Throws()
    {
        var doc = new MarkdownDocument();
        doc.Write("Text");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.InsertHorizontalRule(new BlockIndex(-1)));
    }

    [Fact]
    public void BatchConversion_StartBlockBeyondCount_Throws()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("A");
        doc.WriteParagraph("B");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.ConvertToOrderedList(new BlockIndex(10)));
    }

    [Fact]
    public void BatchConversion_EndBlockBeforeStartBlock_Throws()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("A");
        doc.WriteParagraph("B");
        doc.WriteParagraph("C");
        Assert.Throws<ArgumentException>(() =>
            doc.ConvertToOrderedList(new BlockIndex(-1)));
    }

    [Fact]
    public void LineWithinBlock_BeyondLineCount_Throws()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("Line one");
        doc.WriteLine("Line two");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.ConvertToHeading(new BlockIndex(0), 1, lineWithinBlock: 10));
    }

    [Fact]
    public void LineWithinBlock_Negative_Throws()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("Line one");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.ConvertToHeading(new BlockIndex(0), 1, lineWithinBlock: -1));
    }
}
