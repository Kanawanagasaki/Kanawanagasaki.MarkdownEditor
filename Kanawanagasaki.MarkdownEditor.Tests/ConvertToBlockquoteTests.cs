namespace Kanawanagasaki.MarkdownEditor.Tests;

public class ConvertToBlockquoteTests
{
    [Fact]
    public void ConvertToBlockquote_BasicParagraph()
    {
        var doc = new MarkdownDocument();
        doc.Write("A quote");
        doc.ConvertToBlockquote(0);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("> A quote", md);
    }

    [Fact]
    public void ConvertToBlockquote_PreservesStyles()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bold quote");
        doc.ApplyBold(0, 4);
        doc.ConvertToBlockquote(0);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("> **Bold** quote", md);
    }

    [Fact]
    public void ConvertToBlockquote_MultipleLines()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("First quoted");
        doc.WriteLine("Second quoted");
        doc.ConvertToBlockquote(0);
        doc.ConvertToBlockquote(1);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("> First quoted\n> Second quoted\n", md);
    }

    [Fact]
    public void ConvertToBlockquote_MultipleParagraphs()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("First quoted");
        doc.WriteParagraph("Second quoted");
        doc.ConvertToBlockquote(0);
        doc.ConvertToBlockquote(1);
        doc.ConvertToBlockquote(2);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("> First quoted\n> \n> Second quoted", md);
    }

    [Fact]
    public void ConvertToBlockquote_NestedLevel()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello, world!");
        doc.ConvertToBlockquote(0, 1);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("> Hello, world!", md);

        doc.ConvertToBlockquote(0, 2);

        md = doc.ToMarkdown("\n");
        Assert.Equal("> > Hello, world!", md);

        doc.ConvertToBlockquote(0, 5);

        md = doc.ToMarkdown("\n");
        Assert.Equal("> > > > > Hello, world!", md);

        doc.ConvertToBlockquote(0, 0);

        md = doc.ToMarkdown("\n");
        Assert.Equal("Hello, world!", md);
    }

    [Fact]
    public void ConvertToBlockquote_FollowedByNormalText()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("Quoted text");
        doc.WriteParagraph("Normal text");

        doc.ConvertToBlockquote(0);

        var md = doc.ToMarkdown("\n");

        Assert.Equal("> Quoted text\n\nNormal text", md);
    }

    [Fact]
    public void ConvertToBlockquote_MixedLevels()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("Level one");
        doc.WriteParagraph("Level two");
        doc.WriteParagraph("Level three");
        doc.WriteParagraph("Level three");
        doc.WriteParagraph("Level two");
        doc.WriteParagraph("Level one");

        doc.ConvertToBlockquote(0, 1);
        doc.ConvertToBlockquote(1, 1);
        doc.ConvertToBlockquote(2, 2);
        doc.ConvertToBlockquote(3, 2);
        doc.ConvertToBlockquote(4, 3);
        doc.ConvertToBlockquote(5, 3);
        doc.ConvertToBlockquote(6, 3);
        doc.ConvertToBlockquote(7, 2);
        doc.ConvertToBlockquote(8, 2);
        doc.ConvertToBlockquote(9, 1);
        doc.ConvertToBlockquote(10, 1);

        var md = doc.ToMarkdown("\n");

        Assert.Equal("""
        > Level one
        > 
        > > Level two
        > > 
        > > > Level three
        > > > 
        > > > Level three
        > > 
        > > Level two
        > 
        > Level one
        """, md);
    }
}
