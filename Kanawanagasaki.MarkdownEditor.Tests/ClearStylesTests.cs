namespace Kanawanagasaki.MarkdownEditor.Tests;

public class ClearStylesTests
{
    [Fact]
    public void ClearAllStyles_ClearsBold()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bold text");
        doc.ApplyBold(new TextOffset(0), new TextOffset(4));
        Assert.Equal("**Bold** text", doc.ToMarkdown("\n"));

        doc.ClearAllStyles();
        Assert.Equal("Bold text", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ClearAllStyles_ClearsMultipleStyles()
    {
        var doc = new MarkdownDocument();
        doc.Write("Styled text");
        doc.ApplyBold(new TextOffset(0), new TextOffset(6));
        doc.ApplyItalic(new TextOffset(0), new TextOffset(6));
        Assert.Equal("***Styled*** text", doc.ToMarkdown("\n"));

        doc.ClearAllStyles();
        Assert.Equal("Styled text", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ClearAllStyles_PreservesPlainText()
    {
        var doc = new MarkdownDocument();
        doc.Write("No styles here");
        doc.ClearAllStyles();
        Assert.Equal("No styles here", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ClearAllStyles_ClearsAcrossMultipleBlocks()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("AAA BBB");
        doc.ApplyBold(new TextOffset(0), new TextOffset(3));
        doc.WriteParagraph("CCC DDD");
        doc.ApplyBold(new TextOffset(9), new TextOffset(12));

        Assert.Equal("**AAA** BBB\n\n**CCC** DDD", doc.ToMarkdown("\n"));

        doc.ClearAllStyles();
        
        Assert.Equal("AAA BBB\n\nCCC DDD", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ClearStylesForLine_ByBlockIndex()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("Bold here");
        doc.WriteLine("Plain here");
        doc.ApplyBold(new TextOffset(0), new TextOffset(4));

        Assert.Equal("**Bold** here\nPlain here\n", doc.ToMarkdown("\n"));

        doc.ClearStylesForLine(new BlockIndex(0));

        Assert.Equal("Bold here\nPlain here\n", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ClearStylesForLine_WithLineWithinBlock()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("Bold one");
        doc.WriteLine("Bold two");
        doc.ApplyBold(new TextOffset(0), new TextOffset(8));
        doc.ApplyBold(new TextOffset(9), new TextOffset(17));

        Assert.Equal("**Bold one**\n**Bold two**\n", doc.ToMarkdown("\n"));

        doc.ClearStylesForLine(new BlockIndex(0), lineWithinBlock: 1);

        Assert.Equal("**Bold one**\nBold two\n", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ClearStylesForRange_WithTextOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("AAA BBB CCC");

        doc.ApplyBold(new TextOffset(4), new TextOffset(7));

        Assert.Equal("AAA **BBB** CCC", doc.ToMarkdown("\n"));

        doc.ClearStylesForRange(new TextOffset(4), new TextOffset(7));

        Assert.Equal("AAA BBB CCC", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ClearStylesForRange_WithTextRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("One ");
        var range = doc.Write("two");
        doc.Write(" three");

        doc.ApplyBold(range);

        Assert.Equal("One **two** three", doc.ToMarkdown("\n"));

        var clearRange = doc.ClearStylesForRange(range);

        Assert.Equal("One two three", doc.ToMarkdown("\n"));

        doc.ApplyBold(clearRange.StartOffset, clearRange.EndOffset);

        Assert.Equal("One **two** three", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ClearStylesForRange_LeavesUnaffectedStyles()
    {
        var doc = new MarkdownDocument();
        doc.Write("AAA BBB CCC");
        doc.ApplyBold(new TextOffset(0), new TextOffset(3));
        doc.ApplyBold(new TextOffset(8), new TextOffset(11));

        Assert.Equal("**AAA** BBB **CCC**", doc.ToMarkdown("\n"));

        doc.ClearStylesForRange(new TextOffset(0), new TextOffset(5));

        Assert.Equal("AAA BBB **CCC**", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ClearStylesForRange_PartialClear_SplitsStyle()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three");
        doc.ApplyBold(new TextOffset(0), new TextOffset(13));

        Assert.Equal("**One two three**", doc.ToMarkdown("\n"));

        doc.ClearStylesForRange(new TextOffset(3), new TextOffset(7));

        Assert.Equal("**One** two** three**", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ClearStylesForRange_Crossborder()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three");
        doc.ApplyBold(new TextOffset(0), new TextOffset(7));

        Assert.Equal("**One two** three", doc.ToMarkdown("\n"));

        doc.ClearStylesForRange(new TextOffset(4), new TextOffset(13));

        Assert.Equal("**One **two three", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ClearStylesForBlock_ByBlockIndex()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("Bold here");
        doc.WriteParagraph("Plain here");

        doc.ApplyBold(new TextOffset(0), new TextOffset(4));

        Assert.Equal("**Bold** here\n\nPlain here", doc.ToMarkdown("\n"));

        doc.ClearStylesForBlock(new BlockIndex(0));

        Assert.Equal("Bold here\n\nPlain here", doc.ToMarkdown("\n"));
    }
}
