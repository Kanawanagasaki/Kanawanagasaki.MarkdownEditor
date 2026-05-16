namespace Kanawanagasaki.MarkdownEditor.Tests;

public class InlineTests
{
    [Fact]
    public void ApplyBold_WithTextOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three");
        doc.ApplyBold(new TextOffset(4), new TextOffset(7));

        Assert.Equal("One **two** three", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ApplyBold_WithTextRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("One ");
        var range = doc.Write("two");
        doc.Write(" three");
        doc.ApplyBold(range);

        Assert.Equal("One **two** three", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ApplyBold_CrossLine()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("First line");
        doc.WriteLine("Second line");

        if (doc.Find("Second line") is { } range)
            doc.ApplyBold(range);

        Assert.Equal("First line\n**Second line**\n", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ApplyBold_MultipleLines()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("First line");
        doc.WriteLine("Second line");
        doc.WriteLine("Third line");
        doc.WriteLine("Fourth line");

        if (doc.Find("Second line") is { } startRange && doc.Find("Third line") is { } endRange)
            doc.ApplyBold(startRange.StartOffset, endRange.EndOffset);

        Assert.Equal("First line\n**Second line**\n**Third line**\nFourth line\n", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ApplyItalic_WithTextOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three");
        doc.ApplyItalic(new TextOffset(4), new TextOffset(7));

        Assert.Equal("One *two* three", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ApplyItalic_WithTextRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("One ");
        var range = doc.Write("two");
        doc.Write(" three");
        doc.ApplyItalic(range);

        Assert.Equal("One *two* three", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ApplyItalic_ThirdParagraph()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("First paragraph");
        doc.WriteParagraph("Second paragraph");
        doc.WriteParagraph("Third paragraph");

        if (doc.Find("Third paragraph") is { } range)
            doc.ApplyItalic(range);

        Assert.Equal("First paragraph\n\nSecond paragraph\n\n*Third paragraph*", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ApplyStrikethrough_WithTextOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three");
        doc.ApplyStrikethrough(new TextOffset(4), new TextOffset(7));

        Assert.Equal("One ~~two~~ three", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ApplyStrikethrough_WithTextRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("Old ");
        var range = doc.Write("text");
        doc.Write(" new text");
        doc.ApplyStrikethrough(range);

        Assert.Equal("Old ~~text~~ new text", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ApplyCode_WithTextOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three");
        doc.ApplyCode(new TextOffset(4), new TextOffset(7));

        Assert.Equal("One `two` three", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ApplyCode_WithTextRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("Use ");
        var range = doc.Write("var");
        doc.Write(" keyword");
        doc.ApplyCode(range);

        Assert.Equal("Use `var` keyword", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void BoldItalic_Combined()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three");
        doc.ApplyBold(new TextOffset(4), new TextOffset(7));
        doc.ApplyItalic(new TextOffset(4), new TextOffset(7));

        Assert.Equal("One ***two*** three", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void BoldItalicStrikethrough_Combined()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three");
        doc.ApplyBold(new TextOffset(4), new TextOffset(7));
        doc.ApplyItalic(new TextOffset(4), new TextOffset(7));
        doc.ApplyStrikethrough(new TextOffset(4), new TextOffset(7));

        Assert.Equal("One ***~~two~~*** three", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void AllFourStyles_Combined()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three");
        doc.ApplyBold(new TextOffset(4), new TextOffset(7));
        doc.ApplyItalic(new TextOffset(4), new TextOffset(7));
        doc.ApplyStrikethrough(new TextOffset(4), new TextOffset(7));
        doc.ApplyCode(new TextOffset(4), new TextOffset(7));

        Assert.Equal("One ***~~`two`~~*** three", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void AllFourStyles_OrderIsImportant()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three");
        doc.ApplyCode(new TextOffset(4), new TextOffset(7));
        doc.ApplyItalic(new TextOffset(4), new TextOffset(7));
        doc.ApplyStrikethrough(new TextOffset(4), new TextOffset(7));
        doc.ApplyBold(new TextOffset(4), new TextOffset(7));

        Assert.Equal("One `*~~**two**~~*` three", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void DifferentStyleOrder_ProducesDifferentNesting()
    {
        var doc1 = new MarkdownDocument();
        doc1.Write("One two three");
        doc1.ApplyCode(new TextOffset(4), new TextOffset(7));
        doc1.ApplyStrikethrough(new TextOffset(4), new TextOffset(7));
        doc1.ApplyItalic(new TextOffset(4), new TextOffset(7));
        doc1.ApplyBold(new TextOffset(4), new TextOffset(7));

        var doc2 = new MarkdownDocument();
        doc2.Write("One two three");
        doc2.ApplyStrikethrough(new TextOffset(4), new TextOffset(7));
        doc2.ApplyBold(new TextOffset(4), new TextOffset(7));
        doc2.ApplyCode(new TextOffset(4), new TextOffset(7));
        doc2.ApplyItalic(new TextOffset(4), new TextOffset(7));

        Assert.NotEqual(doc1.ToMarkdown("\n"), doc2.ToMarkdown("\n"));
    }

    [Fact]
    public void ApplyBold_AcrossParagraphs()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("First paragraph");
        doc.WriteParagraph("Second paragraph");
        doc.WriteParagraph("Third paragraph");
        doc.WriteParagraph("Fourth paragraph");
        doc.WriteParagraph("Fifth paragraph");

        if (doc.Find("Third paragraph") is { } startRange && doc.Find("Fourth paragraph") is { } endRange)
            doc.ApplyBold(startRange.StartOffset, endRange.EndOffset);

        Assert.Equal("First paragraph\n\nSecond paragraph\n\n**Third paragraph**\n\n**Fourth paragraph**\n\nFifth paragraph", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ApplyStyle_ReturnsSameOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello world");
        var result = doc.ApplyBold(new TextOffset(0), new TextOffset(5));

        Assert.Equal(new TextOffset(0), result.StartOffset);
        Assert.Equal(new TextOffset(5), result.EndOffset);
    }

    [Fact]
    public void WriteThenApplyStyle_FullChain()
    {
        var doc = new MarkdownDocument();
        doc.Write("ABC ");
        var r1 = doc.Write("DEF");
        doc.Write(" GHI");
        doc.ApplyBold(r1);
        doc.ApplyCode(r1);
        doc.ApplyStrikethrough(r1);

        Assert.Equal("ABC **`~~DEF~~`** GHI", doc.ToMarkdown("\n"));
    }
}
