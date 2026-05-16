namespace Kanawanagasaki.MarkdownEditor.Tests;

public class ConvertToUnorderedListTests
{
    [Fact]
    public void SingleItem()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bullet item");
        doc.ConvertToUnorderedList(new BlockIndex(0));

        Assert.Equal("- Bullet item", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void TwoItems_IndividualConversion()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("Apple");
        doc.WriteLine("Banana");
        doc.ConvertToUnorderedList(new BlockIndex(0));

        Assert.Equal("- Apple\n- Banana\n", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void BatchConversion()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("Apple");
        doc.WriteParagraph("Banana");
        doc.WriteParagraph("Cherry");
        doc.ConvertToUnorderedList(new BlockIndex(0));
        doc.ConvertToUnorderedList(new BlockIndex(1));
        doc.ConvertToUnorderedList(new BlockIndex(2));

        Assert.Equal("- Apple\n\n- Banana\n\n- Cherry", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void WithStyledText()
    {
        var doc = new MarkdownDocument();
        doc.Write("Italic item");
        doc.ApplyItalic(new TextOffset(0), new TextOffset(6));
        doc.ConvertToUnorderedList(new BlockIndex(0));

        Assert.Equal("- *Italic* item", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void LineWithinBlock()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("Line one");
        doc.WriteLine("Line two");
        doc.ConvertToUnorderedList(new BlockIndex(0), lineWithinBlock: 1);

        Assert.Equal("Line one\n- Line two\n", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void Batch_PartialRange_LeavesOthersUnchanged()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("Normal");
        doc.WriteParagraph("List A");
        doc.WriteParagraph("Also normal");
        doc.WriteParagraph("List B");
        doc.ConvertToUnorderedList(new BlockIndex(1));
        doc.ConvertToUnorderedList(new BlockIndex(3));

        Assert.Equal("Normal\n\n- List A\n\nAlso normal\n\n- List B", doc.ToMarkdown("\n"));
    }
}
