namespace Kanawanagasaki.MarkdownEditor.Tests;

public class InsertHorizontalRuleTests
{
    [Fact]
    public void AtEnd()
    {
        var doc = new MarkdownDocument();
        doc.Write("Before");
        doc.InsertHorizontalRule();

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Before\n\n---", md);
    }

    [Fact]
    public void AtBlockIndex0()
    {
        var doc = new MarkdownDocument();
        doc.Write("After");
        doc.InsertHorizontalRule(new BlockIndex(0));

        var md = doc.ToMarkdown("\n");
        Assert.Equal("---\n\nAfter", md);
    }

    [Fact]
    public void BetweenParagraphs()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("Before");
        doc.WriteParagraph("After");
        doc.InsertHorizontalRule(new BlockIndex(1));

        Assert.Equal("Before\n\n---\n\nAfter", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void InsertThenWrite()
    {
        var doc = new MarkdownDocument();
        doc.InsertHorizontalRule();
        doc.Write("Hello, world!");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("---\n\nHello, world!", md);
    }

    [Fact]
    public void WriteInsertWrite()
    {
        var doc = new MarkdownDocument();
        doc.Write("One");
        doc.InsertHorizontalRule();
        doc.Write("Two");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("One\n\n---\n\nTwo", md);
    }

    [Fact]
    public void AtEndOfDocument_WithBlockIndex()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("First");
        doc.WriteParagraph("Second");
        doc.InsertHorizontalRule(new BlockIndex(2));

        var md = doc.ToMarkdown("\n");
        Assert.Equal("First\n\nSecond\n\n---", md);
    }
}
