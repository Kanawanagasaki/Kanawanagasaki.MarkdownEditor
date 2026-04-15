namespace Kanawanagasaki.MarkdownEditor.Tests;

public class MakeLinkTests
{
    [Fact]
    public void MakeLink_BasicLink()
    {
        var doc = new MarkdownDocument();
        doc.Write("Click here for info");
        int start = "Click ".Length;
        int end = start + "here".Length;

        doc.MakeLink(start, end, "https://example.com");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Click [here](https://example.com) for info", md);
    }

    [Fact]
    public void MakeLink_WithTitle()
    {
        var doc = new MarkdownDocument();
        doc.Write("Visit example");
        int start = "Visit ".Length;
        int end = start + "example".Length;

        doc.MakeLink(start, end, "https://example.com", "Example Site");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Visit [example](https://example.com \"Example Site\")", md);
    }

    [Fact]
    public void MakeLink_EntireText()
    {
        var doc = new MarkdownDocument();
        doc.Write("Google");
        doc.MakeLink(0, "Google".Length, "https://google.com");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("[Google](https://google.com)", md);
    }
    [Fact]
    public void MakeLink_ThenBold()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bold link text");
        int start = "Bold ".Length;
        int end = start + "link".Length;

        doc.MakeLink(start, end, "https://example.com");
        doc.ApplyBold(start, end);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Bold [**link**](https://example.com) text", md);
    }

    [Fact]
    public void MakeLink_OnBoldText()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bold link text");
        int start = "Bold ".Length;
        int end = start + "link".Length;

        doc.ApplyBold(start, end);
        doc.MakeLink(start, end, "https://example.com");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Bold **[link](https://example.com)** text", md);
    }

    [Fact]
    public void MakeLink_BoldTextInside()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bold link text");
        int boldStart = "Bold ".Length;
        int boldEnd = boldStart + "link".Length;
        int end = "Bold link text".Length;

        doc.ApplyBold(boldStart, boldEnd);
        doc.MakeLink(0, end, "https://example.com");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("[Bold **link** text](https://example.com)", md);
    }
}
