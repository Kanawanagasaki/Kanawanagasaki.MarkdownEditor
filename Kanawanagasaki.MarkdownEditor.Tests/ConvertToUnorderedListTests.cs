namespace Kanawanagasaki.MarkdownEditor.Tests;

public class ConvertToUnorderedListTests
{
    [Fact]
    public void ConvertToUnorderedList_SingleItem()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bullet item");
        doc.ConvertToUnorderedList(0);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("- Bullet item", md);
    }

    [Fact]
    public void ConvertToUnorderedList_TwoConsecutiveItems()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("Apple");
        doc.WriteLine("Banana");
        doc.ConvertToUnorderedList(0);
        doc.ConvertToUnorderedList(1);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("- Apple\n- Banana", md);
    }

    [Fact]
    public void ConvertToUnorderedList_WithStyledText()
    {
        var doc = new MarkdownDocument();
        doc.Write("Italic item");
        doc.ApplyItalic(0, 6);
        doc.ConvertToUnorderedList(0);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("- *Italic* item", md);
    }

    [Fact]
    public void ConvertToUnorderedList_Paragraph()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("Apple");
        doc.WriteParagraph("Banana");
        doc.ConvertToUnorderedList(0);
        doc.ConvertToUnorderedList(2);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("- Apple\n\n- Banana", md);
    }

    [Fact]
    public void ConvertToUnorderedList_FillingTheGap()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("Apple");
        doc.WriteParagraph("Banana");
        doc.ConvertToUnorderedList(0);
        doc.ConvertToUnorderedList(1);
        doc.ConvertToUnorderedList(2);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("- Apple\n- \n- Banana", md);
    }
}
