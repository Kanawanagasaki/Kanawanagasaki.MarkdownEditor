namespace Kanawanagasaki.MarkdownEditor.Tests;

public class GeneralTests
{
    [Fact]
    public void WriteThenHeadingThenOrderedList()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("Title");
        doc.WriteLine("Item");
        doc.ConvertToHeading(0, 1);
        doc.ConvertToOrderedList(1);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("# Title\n1. Item\n", md);
    }

    [Fact]
    public void MultipleStylesAndLinks()
    {
        var doc = new MarkdownDocument();
        doc.Write("Click bold here and see image logo");
        int boldStart = "Click ".Length;
        int boldEnd = boldStart + "bold".Length;
        int linkStart = "Click bold ".Length;
        int linkEnd = linkStart + "here".Length;
        int imgStart = "Click bold here and see image ".Length;
        int imgEnd = imgStart + "logo".Length;

        doc.ApplyBold(boldStart, boldEnd);
        doc.MakeLink(linkStart, linkEnd, "https://example.com");
        doc.MakeImage(imgStart, imgEnd, "https://example.com/logo.png");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Click **bold** [here](https://example.com) and see image ![logo](https://example.com/logo.png)", md);
    }

    [Fact]
    public void ListWithFirstItemBlockquote()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("First item");
        doc.WriteLine("Second item");
        doc.ConvertToOrderedList(0);
        doc.ConvertToOrderedList(1);
        doc.ConvertToBlockquote(0);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("> 1. First item\n1. Second item\n", md);
    }

    [Fact]
    public void BlockquoteWithListInside()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("First item");
        doc.WriteLine("Second item");
        doc.ConvertToOrderedList(0);
        doc.ConvertToOrderedList(1);
        doc.ConvertToBlockquote(0);
        doc.ConvertToBlockquote(1);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("> 1. First item\n> 2. Second item\n", md);
    }

    [Fact]
    public void BlockquoteWithListInsideNoMatterTheOrder()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("First item");
        doc.WriteLine("Second item");
        doc.ConvertToBlockquote(0);
        doc.ConvertToBlockquote(1);
        doc.ConvertToOrderedList(0);
        doc.ConvertToOrderedList(1);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("> 1. First item\n> 2. Second item\n", md);
    }

    [Fact]
    public void ApplyStyleThenClearThenReapply()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello world");
        doc.ApplyBold(0, 5);
        Assert.Equal("**Hello** world", doc.ToMarkdown("\n"));

        doc.ClearAllStyles();
        Assert.Equal("Hello world", doc.ToMarkdown("\n"));

        doc.ApplyItalic(6, 11);
        Assert.Equal("Hello *world*", doc.ToMarkdown("\n"));
    }
    
    [Fact]
    public void EmptyDocument_ToMarkdown_ReturnsEmpty()
    {
        var doc = new MarkdownDocument();
        Assert.Equal("", doc.ToMarkdown("\n"));
    }
}
