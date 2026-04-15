namespace Kanawanagasaki.MarkdownEditor.Tests;

public class ConvertToHeadingTests
{
    [Fact]
    public void ConvertToHeading_Level1()
    {
        var doc = new MarkdownDocument();
        doc.Write("Title");
        doc.ConvertToHeading(0, 1);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("# Title", md);
    }

    [Fact]
    public void ConvertToHeading_Level3()
    {
        var doc = new MarkdownDocument();
        doc.Write("Subtitle");
        doc.ConvertToHeading(0, 3);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("### Subtitle", md);
    }

    [Fact]
    public void ConvertToHeading_Level6()
    {
        var doc = new MarkdownDocument();
        doc.Write("Small");
        doc.ConvertToHeading(0, 6);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("###### Small", md);
    }

    [Fact]
    public void ConvertToHeading_ClampsAboveMaxTo6()
    {
        var doc = new MarkdownDocument();
        doc.Write("Heading");
        doc.ConvertToHeading(0, 99);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("###### Heading", md);
    }

    [Fact]
    public void ConvertToHeading_RemoveHeading()
    {
        var doc = new MarkdownDocument();
        doc.Write("Heading");
        doc.ConvertToHeading(0, 1);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("# Heading", md);

        doc.ConvertToHeading(0, 0);

        md = doc.ToMarkdown("\n");
        Assert.Equal("Heading", md);
    }

    [Fact]
    public void ConvertToHeading_ChangesExistingHeadingLevel()
    {
        var doc = new MarkdownDocument();
        doc.Write("Title");
        doc.ConvertToHeading(0, 1);
        doc.ConvertToHeading(0, 3);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("### Title", md);
    }

    [Fact]
    public void ConvertToHeading_PreservesInlineStyles()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bold Title");
        doc.ApplyBold(0, 4);
        doc.ConvertToHeading(0, 2);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("## **Bold** Title", md);
    }

    [Fact]
    public void ConvertToHeading_ThirdLine()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("One");
        doc.WriteParagraph("Two");
        doc.WriteParagraph("Three");
        doc.ConvertToHeading(2, 5);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("One\n\n##### Two\n\nThree", md);
    }

    [Fact]
    public void ConvertToHeading_BetweenLines()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("One");
        doc.WriteParagraph("Two");
        doc.ConvertToHeading(1, 1);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("One\n# \nTwo", md);
    }
}
