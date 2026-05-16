namespace Kanawanagasaki.MarkdownEditor.Tests;

public class ConvertToHeadingTests
{
    [Fact]
    public void Level1()
    {
        var doc = new MarkdownDocument();
        doc.Write("Title");
        doc.ConvertToHeading(new BlockIndex(0), 1);

        Assert.Equal("# Title", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void Level3()
    {
        var doc = new MarkdownDocument();
        doc.Write("Subtitle");
        doc.ConvertToHeading(new BlockIndex(0), 3);

        Assert.Equal("### Subtitle", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void Level6()
    {
        var doc = new MarkdownDocument();
        doc.Write("Small");
        doc.ConvertToHeading(new BlockIndex(0), 6);

        Assert.Equal("###### Small", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void Level99ClampTo6()
    {
        var doc = new MarkdownDocument();
        doc.Write("Small");
        doc.ConvertToHeading(new BlockIndex(0), 99);

        Assert.Equal("###### Small", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void RemoveHeading_WithLevel0()
    {
        var doc = new MarkdownDocument();
        doc.Write("Heading");
        doc.ConvertToHeading(new BlockIndex(0), 1);
        Assert.Equal("# Heading", doc.ToMarkdown("\n"));

        doc.ConvertToHeading(new BlockIndex(0), 0);
        Assert.Equal("Heading", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ChangesExistingHeadingLevel()
    {
        var doc = new MarkdownDocument();
        doc.Write("Title");

        doc.ConvertToHeading(new BlockIndex(0), 1);

        Assert.Equal("# Title", doc.ToMarkdown("\n"));

        doc.ConvertToHeading(new BlockIndex(0), 3);

        Assert.Equal("### Title", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void PreservesInlineStyles()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bold Title");
        doc.ApplyBold(new TextOffset(0), new TextOffset(4));
        doc.ConvertToHeading(new BlockIndex(0), 2);

        Assert.Equal("## **Bold** Title", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void SecondParagraph_ByBlockIndex()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("One");
        doc.WriteParagraph("Two");
        doc.WriteParagraph("Three");
        doc.ConvertToHeading(new BlockIndex(1), 5);

        Assert.Equal("One\n\n##### Two\n\nThree", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void LineWithinBlock_TargetsSpecificLineInMultiLineParagraph()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("Line one");
        doc.WriteLine("Line two");
        doc.WriteLine("Line three");

        doc.ConvertToHeading(new BlockIndex(0), 2, lineWithinBlock: 1);

        Assert.Equal("Line one\n## Line two\nLine three\n", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void LineWithinBlock_FirstLine()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("Line one");
        doc.WriteLine("Line two");
        doc.ConvertToHeading(new BlockIndex(0), 1, lineWithinBlock: 0);

        Assert.Equal("# Line one\nLine two\n", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void LineWithinBlock_LastLine()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("Line one");
        doc.WriteLine("Line two");
        doc.ConvertToHeading(new BlockIndex(0), 3, lineWithinBlock: 1);

        Assert.Equal("Line one\n### Line two\n", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void LineWithinBlock_ExtractsAndConverts_LeavingOtherLinesInParagraph()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("AAA");
        doc.WriteLine("BBB");
        doc.WriteLine("CCC");
        doc.ConvertToHeading(new BlockIndex(0), 2, lineWithinBlock: 1);

        Assert.Equal("AAA\n## BBB\nCCC\n", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void LineWithinBlock_ManyLinesOneBlock()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("AAA");
        doc.WriteLine("BBB");
        doc.WriteLine("CCC");
        doc.ConvertToHeading(new BlockIndex(0), 3);

        Assert.Equal("### AAA\n### BBB\n### CCC\n", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void Batch_ConvertMultipleBlocks()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("One");
        doc.WriteParagraph("Two");
        doc.WriteParagraph("Three");
        doc.ConvertToHeading(new BlockIndex(1), new BlockIndex(2), 2);

        Assert.Equal("One\n\n## Two\n\n## Three", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void Batch_ConvertAllBlocks()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("Alpha");
        doc.WriteParagraph("Beta");
        doc.WriteParagraph("Gamma");
        doc.ConvertToHeading(new BlockIndex(0), new BlockIndex(2), 1);

        Assert.Equal("# Alpha\n\n# Beta\n\n# Gamma", doc.ToMarkdown("\n"));
    }
}
