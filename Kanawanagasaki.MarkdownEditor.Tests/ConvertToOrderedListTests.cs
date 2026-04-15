namespace Kanawanagasaki.MarkdownEditor.Tests;

public class ConvertToOrderedListTests
{
    [Fact]
    public void ConvertToOrderedList_SingleItem()
    {
        var doc = new MarkdownDocument();
        doc.Write("First item");
        doc.ConvertToOrderedList(0);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("1. First item", md);
    }

    [Fact]
    public void ConvertToOrderedList_ThreeConsecutiveItems()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("First");
        doc.WriteLine("Second");
        doc.WriteLine("Third");
        doc.ConvertToOrderedList(0);
        doc.ConvertToOrderedList(1);
        doc.ConvertToOrderedList(2);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("1. First\n2. Second\n3. Third\n", md);
    }

    [Fact]
    public void ConvertToOrderedList_WithStyledText()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bold item");
        doc.ApplyBold(0, 4);
        doc.ConvertToOrderedList(0);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("1. **Bold** item", md);
    }

    [Fact]
    public void ConvertToOrderedList_Paragraphs()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("Hello");
        doc.WriteParagraph("World");

        doc.ConvertToOrderedList(0);
        doc.ConvertToOrderedList(2);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("1. Hello\n\n1. World", md);
    }

    [Fact]
    public void ConvertToOrderedList_FillingTheGap()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("Hello");
        doc.WriteParagraph("World");

        doc.ConvertToOrderedList(0);
        doc.ConvertToOrderedList(1);
        doc.ConvertToOrderedList(2);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("1. Hello\n2. \n3. World", md);
    }
}
