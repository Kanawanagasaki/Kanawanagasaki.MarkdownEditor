namespace Kanawanagasaki.MarkdownEditor.Tests;

public class InsertHorizontalRuleTests
{
    [Fact]
    public void InsertHorizontalRule_AtEnd()
    {
        var doc = new MarkdownDocument();
        doc.Write("Before");
        doc.InsertHorizontalRule();

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Before\n\n---", md);
    }

    [Fact]
    public void InsertHorizontalRule_AtBeginning()
    {
        var doc = new MarkdownDocument();
        doc.Write("After");
        doc.InsertHorizontalRule(0);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("---\n\nAfter", md);
    }

    [Fact]
    public void InsertHorizontalRule_BetweenParagraphs()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("Before");
        doc.Write("After");
        doc.InsertHorizontalRule(1);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Before\n\n---\n\nAfter", md);
    }

    [Fact]
    public void InsertHorizontalRule_InsertThenWrite()
    {
        var doc = new MarkdownDocument();
        doc.InsertHorizontalRule();
        doc.Write("Hello, world!");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("---\n\nHello, world!", md);
    }

    [Fact]
    public void InsertHorizontalRule_WriteInsertWrite()
    {
        var doc = new MarkdownDocument();
        doc.Write("One");
        doc.InsertHorizontalRule();
        doc.Write("Two");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("One\n\n---\n\nTwo", md);
    }
}
