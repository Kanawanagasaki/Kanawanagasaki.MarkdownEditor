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
    public void InsertLine_AtBeginning()
    {
        var doc = new MarkdownDocument();
        doc.Write("Original");
        doc.InsertLine(0, "Inserted first");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Inserted first\nOriginal", md);
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
    public void InsertLine_AtEnd()
    {
        var doc = new MarkdownDocument();
        doc.Write("Original");
        doc.InsertLine(1, "Appended");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Original\nAppended\n", md);
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
    public void InsertLine_InMiddle()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("First");
        doc.WriteLine("Third");
        doc.InsertLine(1, "Second");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("First\nSecond\nThird\n", md);
    }

    [Fact]
    public void InsertParagraph_InMiddle()
    {
        var doc1 = new MarkdownDocument();
        doc1.WriteParagraph("First");
        doc1.WriteParagraph("Third");
        doc1.InsertParagraph(1, "Second");

        Assert.Equal("First\n\nSecond\n\nThird", doc1.ToMarkdown("\n"));
        
        var doc2 = new MarkdownDocument();
        doc2.WriteParagraph("First");
        doc2.WriteParagraph("Third");
        doc2.InsertParagraph(2, "Second");

        Assert.Equal("First\n\nSecond\n\nThird", doc2.ToMarkdown("\n"));
    }

    [Fact]
    public void InsertLine_WithEmptyText()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello");
        doc.InsertLine(1, "");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Hello\n\n", md);
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
    public void InsertLine_NegativeOffset_InsertsAtStart()
    {
        var doc = new MarkdownDocument();
        doc.Write("Original");
        doc.InsertLine(-5, "Clamped to start");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Clamped to start\nOriginal", md);
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
    public void InsertLine_OutOfRangeOffset_InsertsAtEnd()
    {
        var doc = new MarkdownDocument();
        doc.Write("Original");
        doc.InsertLine(100, "Clamped to end");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Original\nClamped to end\n", md);
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
