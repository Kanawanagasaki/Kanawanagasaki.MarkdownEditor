namespace Kanawanagasaki.MarkdownEditor.Tests;

public class ConvertToBlockquoteTests
{
    [Fact]
    public void BasicParagraph()
    {
        var doc = new MarkdownDocument();
        doc.Write("A quote");
        doc.ConvertToBlockquote(new BlockIndex(0));

        Assert.Equal("> A quote", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void PreservesInlineStyles()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bold quote");
        doc.ApplyBold(new TextOffset(0), new TextOffset(4));
        doc.ConvertToBlockquote(new BlockIndex(0));

        Assert.Equal("> **Bold** quote", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void MultipleBlocks()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("First quoted");
        doc.WriteParagraph("Second quoted");
        doc.ConvertToBlockquote(new BlockIndex(0));
        doc.ConvertToBlockquote(new BlockIndex(1));

        Assert.Equal("> First quoted\n>\n> Second quoted", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void RangeConversion()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("First");
        doc.WriteParagraph("Second");
        doc.WriteParagraph("Third");
        doc.ConvertToBlockquote(new BlockIndex(0), new BlockIndex(2));

        Assert.Equal("> First\n>\n> Second\n>\n> Third", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void NestedLevel1()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello, world!");
        doc.ConvertToBlockquote(new BlockIndex(0), 1);

        Assert.Equal("> Hello, world!", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void NestedLevel2()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello, world!");
        doc.ConvertToBlockquote(new BlockIndex(0), 2);

        Assert.Equal("> > Hello, world!", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void NestedLevel5()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello, world!");
        doc.ConvertToBlockquote(new BlockIndex(0), 5);

        Assert.Equal("> > > > > Hello, world!", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void NestedLevels()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello, world!");

        doc.ConvertToBlockquote(new BlockIndex(0), 1);

        Assert.Equal("> Hello, world!", doc.ToMarkdown("\n"));

        doc.ConvertToBlockquote(new BlockIndex(0), 2);

        Assert.Equal("> > Hello, world!", doc.ToMarkdown("\n"));

        doc.ConvertToBlockquote(new BlockIndex(0), 5);

        Assert.Equal("> > > > > Hello, world!", doc.ToMarkdown("\n"));

        doc.ConvertToBlockquote(new BlockIndex(0), 3);

        Assert.Equal("> > > Hello, world!", doc.ToMarkdown("\n"));

        doc.ConvertToBlockquote(new BlockIndex(0), 0);

        Assert.Equal("Hello, world!", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void RemoveBlockquote_WithLevel0()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello, world!");
        doc.ConvertToBlockquote(new BlockIndex(0), 2);

        Assert.Equal("> > Hello, world!", doc.ToMarkdown("\n"));

        doc.ConvertToBlockquote(new BlockIndex(0), 0);

        Assert.Equal("Hello, world!", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void FollowedByNormalText()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("Quoted text");
        doc.WriteParagraph("Normal text");
        doc.ConvertToBlockquote(new BlockIndex(0));

        Assert.Equal("> Quoted text\n\nNormal text", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void LineWithinBlock()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("Normal line");
        doc.WriteLine("Quoted line");
        doc.ConvertToBlockquote(new BlockIndex(0), 1, lineWithinBlock: 1);

        Assert.Equal("Normal line\n> Quoted line\n", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void MixedNestingLevels()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("Level one");
        doc.WriteParagraph("Level two");
        doc.WriteParagraph("Level three");

        doc.ConvertToBlockquote(new BlockIndex(0), 1);
        doc.ConvertToBlockquote(new BlockIndex(1), 2);
        doc.ConvertToBlockquote(new BlockIndex(2), 3);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("> Level one\n>\n> > Level two\n>\n> > > Level three", md);
    }

    [Fact]
    public void MultipleLinesOneBlock()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("First item");
        doc.WriteLine("Second item");
        doc.WriteLine("Third item");
        doc.ConvertToBlockquote(new BlockIndex(0));

        var md = doc.ToMarkdown("\n");
        Assert.Equal("> 1. First item\n> 2. Second item\n> 3. Third item\n", md);
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

        doc.ConvertToBlockquote(new BlockIndex(0), new BlockIndex(5));
        doc.ConvertToBlockquote(new BlockIndex(1), new BlockIndex(4));
        doc.ConvertToBlockquote(new BlockIndex(2), new BlockIndex(3));

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
