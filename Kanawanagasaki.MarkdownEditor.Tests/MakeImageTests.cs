namespace Kanawanagasaki.MarkdownEditor.Tests;

public class MakeImageTests
{
    [Fact]
    public void MakeImage_BasicImage()
    {
        var doc = new MarkdownDocument();
        doc.Write("See the alt text here");
        int start = "See the ".Length;
        int end = start + "alt text".Length;

        doc.MakeImage(start, end, "https://example.com/img.png");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("See the ![alt text](https://example.com/img.png) here", md);
    }

    [Fact]
    public void MakeImage_WithTitle()
    {
        var doc = new MarkdownDocument();
        doc.Write("Photo here");
        int start = "Photo ".Length;
        int end = start + "here".Length;

        doc.MakeImage(start, end, "https://example.com/photo.png", "A photo");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Photo ![here](https://example.com/photo.png \"A photo\")", md);
    }

    [Fact]
    public void MakeImage_EntireText()
    {
        var doc = new MarkdownDocument();
        doc.Write("logo");
        doc.MakeImage(0, 4, "https://example.com/logo.png");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("![logo](https://example.com/logo.png)", md);
    }

    [Fact]
    public void MakeImage_ThenBoldText()
    {
        var doc = new MarkdownDocument();
        doc.Write("View the Logo icon");
        int start = "View the ".Length;
        int end = start + "Logo".Length;

        doc.MakeImage(start, end, "https://example.com/logo.png");
        doc.ApplyBold(start, end);

        var md = doc.ToMarkdown("\n");
        // Logic: Bold markers are inside the alt text brackets
        Assert.Equal("View the ![**Logo**](https://example.com/logo.png) icon", md);
    }

    [Fact]
    public void MakeImage_OnBoldText()
    {
        var doc = new MarkdownDocument();
        doc.Write("View the Logo icon");
        int start = "View the ".Length;
        int end = start + "Logo".Length;

        doc.ApplyBold(start, end);
        doc.MakeImage(start, end, "https://example.com/logo.png");

        var md = doc.ToMarkdown("\n");
        // Logic: Bold markers are inside the alt text brackets
        Assert.Equal("View the **![Logo](https://example.com/logo.png)** icon", md);
    }

    [Fact]
    public void MakeImage_InsideBoldPhrase()
    {
        var doc = new MarkdownDocument();
        doc.Write("Download our App now");

        int imgStart = "Download our ".Length;
        int imgEnd = imgStart + "App".Length;

        int boldStart = 0;
        int boldEnd = "Download our App now".Length;

        doc.MakeImage(imgStart, imgEnd, "https://example.com/app-icon.png");
        doc.ApplyBold(boldStart, boldEnd);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("**Download our ![App](https://example.com/app-icon.png) now**", md);
    }
}
