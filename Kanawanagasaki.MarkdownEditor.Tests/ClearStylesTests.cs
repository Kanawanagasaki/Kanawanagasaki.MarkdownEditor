namespace Kanawanagasaki.MarkdownEditor.Tests;

public class ClearStylesTests
{
    [Fact]
    public void ClearAllStyles_ClearsBold()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bold text");
        doc.ApplyBold(0, 4);
        Assert.Equal("**Bold** text", doc.ToMarkdown("\n"));

        doc.ClearAllStyles();
        Assert.Equal("Bold text", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ClearAllStyles_ClearsMultipleStyles()
    {
        var doc = new MarkdownDocument();
        doc.Write("Styled text");
        doc.ApplyBold(0, 6);
        doc.ApplyItalic(0, 6);
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
    public void ClearStylesForLine_ClearsSpecificLine()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("Bold here");
        doc.WriteLine("Plain here");

        doc.ApplyBold(0, 4);

        doc.ClearStylesForLine(0);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Bold here\nPlain here\n", md);
    }


    [Fact]
    public void ClearStylesForRange_PartialClear()
    {
        var doc = new MarkdownDocument();
        doc.Write("AAA BBB CCC");
        doc.ApplyBold(4, 7);

        doc.ClearStylesForRange(4, 7);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("AAA BBB CCC", md);
    }

    [Fact]
    public void ClearStylesForRange_LeavesUnaffectedStyles()
    {
        var doc = new MarkdownDocument();
        doc.Write("AAA BBB CCC");
        doc.ApplyBold(0, 3);
        doc.ApplyBold(8, 11);

        doc.ClearStylesForRange(0, 3);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("AAA BBB **CCC**", md);
    }

    [Fact]
    public void ClearStylesForRange_EmptyRange_NoOp()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bold text");
        doc.ApplyBold(0, 4);
        doc.ClearStylesForRange(5, 9);

        Assert.Equal("**Bold** text", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ClearStylesForRange_SmallChunk()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three");
        doc.ApplyBold(0, "One two three".Length);
        int twoStart = "One".Length;
        int twoEnd = twoStart + " two".Length;
        doc.ClearStylesForRange(twoStart, twoEnd);

        Assert.Equal("**One** two** three**", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ClearStylesForRange_Crossborder()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three");
        doc.ApplyBold(0, "One two".Length);
        int twoStart = "One ".Length;
        int threeEnd = twoStart + "two three".Length;
        doc.ClearStylesForRange(twoStart, threeEnd);

        Assert.Equal("**One **two three", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ClearStylesForRange_OneLine()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("One");
        doc.WriteLine("Two");
        doc.WriteLine("Three");
        var endOffset = doc.GetPlainText().Length;

        doc.ApplyBold(0, endOffset);
        Assert.Equal("**One**\n**Two**\n**Three**\n", doc.ToMarkdown("\n"));

        doc.ClearStylesForLine(1);

        Assert.Equal("**One**\nTwo\n**Three**\n", doc.ToMarkdown("\n"));
    }
}
