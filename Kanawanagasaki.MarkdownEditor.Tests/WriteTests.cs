namespace Kanawanagasaki.MarkdownEditor.Tests;

public class WriteTests
{
    [Fact]
    public void Write_SingleLine()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello world");
        Assert.Equal("Hello world", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void Write_AppendsToSameLine()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello ");
        doc.Write("world");
        Assert.Equal("Hello world", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void Write_EmptyString_NoOp()
    {
        var doc = new MarkdownDocument();
        doc.Write("");
        Assert.Equal("", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void WriteLine_CreatesNewLine()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello");
        doc.WriteLine();
        doc.Write("World");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Hello\nWorld", md);
    }

    [Fact]
    public void WriteLine_WithText_AppendsTextThenBreaks()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello");
        doc.WriteLine("!");
        doc.Write("World");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Hello!\nWorld", md);
    }

    [Fact]
    public void WriteParagraph_CreatesNewParagraph()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello");
        doc.WriteParagraph();
        doc.Write("World");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Hello\n\nWorld", md);
    }

    [Fact]
    public void WriteLine_WithText_WritesTextThenBreaks()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("First line");
        doc.WriteLine("Second line");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("First line\nSecond line\n", md);
    }

    [Fact]
    public void WriteParagraph_WithText_WritesTextThenBreaks()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("First line");
        doc.WriteParagraph("Second line");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("First line\n\nSecond line", md);
    }

    [Fact]
    public void InsertLine_AtBlockBeginning()
    {
        var doc = new MarkdownDocument();
        doc.Write("Original");
        doc.InsertLine(new BlockIndex(0), lineWithinBlock: 0, "Inserted first");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Inserted first\nOriginal", md);
    }

    [Fact]
    public void InsertParagraph_AtBlockBeginning()
    {
        var doc = new MarkdownDocument();
        doc.Write("Original");
        doc.InsertParagraph(new BlockIndex(0), "Inserted first");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Inserted first\n\nOriginal", md);
    }

    [Fact]
    public void InsertLine_AfterLastBlock()
    {
        var doc = new MarkdownDocument();
        doc.Write("Original");
        doc.InsertLine(new BlockIndex(0), lineWithinBlock: 1, "Appended");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Original\nAppended\n", md);
    }

    [Fact]
    public void InsertLine_InMiddle()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("First");
        doc.WriteLine("Third");
        doc.InsertLine(new BlockIndex(0), lineWithinBlock: 1, "Second");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("First\nSecond\nThird\n", md);
    }

    [Fact]
    public void InsertParagraph_InMiddle()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("First");
        doc.WriteParagraph("Third");
        doc.InsertParagraph(new BlockIndex(1), "Second");

        Assert.Equal("First\n\nSecond\n\nThird", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void InsertLine_WithEmptyText()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello");
        doc.InsertLine(new BlockIndex(0), lineWithinBlock: 1, "");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Hello\n\n", md);
    }

    [Fact]
    public void InsertParagraph_WithEmptyText()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello");
        doc.InsertParagraph(new BlockIndex(1), "");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Hello\n\n", md);
    }

    [Fact]
    public void Write_ReturnsTextRangeWithCorrectOffsets()
    {
        var doc = new MarkdownDocument();
        var range = doc.Write("Hello");
        Assert.Equal(new TextOffset(0), range.StartOffset);
        Assert.Equal(new TextOffset(5), range.EndOffset);
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
    public void WriteLine_WithText_ReturnsRangeOfWrittenText()
    {
        var doc = new MarkdownDocument();
        var range = doc.WriteLine("Hello");

        Assert.Equal(new TextOffset(0), range.StartOffset);
        Assert.Equal(new TextOffset(6), range.EndOffset);
    }

    [Fact]
    public void WriteParagraph_WithText_ReturnsRangeOfWrittenText()
    {
        var doc = new MarkdownDocument();
        var range = doc.WriteParagraph("Hello");

        Assert.Equal(new TextOffset(0), range.StartOffset);
        Assert.Equal(new TextOffset(5), range.EndOffset);
    }

    [Fact]
    public void PlainTextLength_AfterWrites()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello");
        Assert.Equal(5, doc.PlainTextLength);

        doc.Write(" world");
        Assert.Equal(11, doc.PlainTextLength);
    }

    [Fact]
    public void Length_WithStyledText_IncludesMarkers()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bold");
        doc.ApplyBold(new TextOffset(0), new TextOffset(4));
        Assert.Equal(4, doc.PlainTextLength);
        Assert.Equal(8, doc.Length);
    }
}
