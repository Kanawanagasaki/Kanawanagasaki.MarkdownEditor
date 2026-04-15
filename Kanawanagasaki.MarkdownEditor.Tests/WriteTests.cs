namespace Kanawanagasaki.MarkdownEditor.Tests;

public class WriteTests
{
    [Fact]
    public void Write_SingleParagraph()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello world");
        Assert.Equal("Hello world", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void Write_AppendsToSameParagraph()
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
    public void WriteParagraph_WithText_WritesTextThenBreaks()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("First line");
        doc.WriteParagraph("Second line");

        var md = doc.ToMarkdown("\n");
        
        Assert.Equal("First line\n\nSecond line", md);
    }

    [Fact]
    public void WriteParagraph_NullText_OnlyInsertsBreak()
    {
        var doc = new MarkdownDocument();
        doc.Write("First");
        doc.WriteParagraph();
        doc.Write("Second");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("First\n\nSecond", md);
    }

    [Fact]
    public void InsertParagraph_AtBeginning()
    {
        var doc = new MarkdownDocument();
        doc.Write("Original");
        doc.InsertParagraph(0, "Inserted first");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Inserted first\n\nOriginal", md);
    }

    [Fact]
    public void InsertParagraph_AtEnd()
    {
        var doc = new MarkdownDocument();
        doc.Write("Original");
        doc.InsertParagraph(1, "Appended");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Original\n\nAppended", md);
    }

    [Fact]
    public void InsertParagraph_InMiddle()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("First");
        doc.WriteParagraph("Third");
        doc.InsertParagraph(1, "Second");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("First\n\nSecond\n\nThird", md);
    }

    [Fact]
    public void InsertParagraph_WithEmptyText()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello");
        doc.InsertParagraph(1, "");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Hello\n\n", md);
    }

    [Fact]
    public void InsertParagraph_NegativeOffset_InsertsAtStart()
    {
        var doc = new MarkdownDocument();
        doc.Write("Original");
        doc.InsertParagraph(-5, "Clamped to start");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Clamped to start\n\nOriginal", md);
    }

    [Fact]
    public void InsertParagraph_OutOfRangeOffset_InsertsAtEnd()
    {
        var doc = new MarkdownDocument();
        doc.Write("Original");
        doc.InsertParagraph(100, "Clamped to end");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Original\n\nClamped to end", md);
    }
}
