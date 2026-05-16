namespace Kanawanagasaki.MarkdownEditor.Tests;

public class ConvertToOrderedListTests
{
    [Fact]
    public void SingleItem()
    {
        var doc = new MarkdownDocument();
        doc.Write("First item");
        doc.ConvertToOrderedList(new BlockIndex(0));

        Assert.Equal("1. First item", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ThreeItems_IndividualConversion()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("First");
        doc.WriteLine("Second");
        doc.Write("Third");
        doc.ConvertToOrderedList(new BlockIndex(0));

        Assert.Equal("1. First\n2. Second\n3. Third", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ParagraphsConversion()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("First");
        doc.WriteParagraph("Second");
        doc.WriteParagraph("Third");
        doc.ConvertToOrderedList(new BlockIndex(0));
        doc.ConvertToOrderedList(new BlockIndex(1));
        doc.ConvertToOrderedList(new BlockIndex(2));

        Assert.Equal("1. First\n\n2. Second\n\n3. Third", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void BatchConversion_PartialRange()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("Alpha");
        doc.WriteParagraph("Beta 1");
        doc.WriteNewLine("Beta 2");
        doc.WriteNewLine("Beta 3");
        doc.WriteParagraph("Gamma");
        doc.WriteParagraph("Delta");
        doc.ConvertToOrderedList(new BlockIndex(1));
        doc.ConvertToOrderedList(new BlockIndex(3));

        Assert.Equal("Alpha\n\n1. Beta 1\n2. Beta 2\n3. Beta 3\n\nGamma\n\n1. Delta", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void WithStyledText()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bold item");
        doc.ApplyBold(new TextOffset(0), new TextOffset(4));
        doc.ConvertToOrderedList(new BlockIndex(0));

        Assert.Equal("1. **Bold** item", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void LineWithinBlock_SingleLine()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("Line one");
        doc.WriteLine("Line two");
        doc.ConvertToOrderedList(new BlockIndex(0), lineWithinBlock: 1);

        Assert.Equal("Line one\n1. Line two\n", doc.ToMarkdown("\n"));
    }
}
