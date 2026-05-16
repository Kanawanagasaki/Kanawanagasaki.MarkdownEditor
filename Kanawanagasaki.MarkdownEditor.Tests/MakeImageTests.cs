namespace Kanawanagasaki.MarkdownEditor.Tests;

public class MakeImageTests
{
    [Fact]
    public void BasicImage_WithTextOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("See the alt text here");
        doc.MakeImage(new TextOffset(8), new TextOffset(16), "https://example.com/img.png");

        Assert.Equal("See the ![alt text](https://example.com/img.png) here", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void BasicImage_WithTextRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("See ");
        var range = doc.Write("photo");
        doc.Write(" here");
        doc.MakeImage(range, "https://example.com/img.png");

        Assert.Equal("See ![photo](https://example.com/img.png) here", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void WithTitle()
    {
        var doc = new MarkdownDocument();
        doc.Write("Photo here");
        var range = doc.Find("here");
        Assert.NotNull(range);
        doc.MakeImage(range.Value, "https://example.com/photo.png", "A photo");

        Assert.Equal("Photo ![here](https://example.com/photo.png \"A photo\")", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void EntireText()
    {
        var doc = new MarkdownDocument();
        var range = doc.Write("logo");
        doc.MakeImage(range, "https://example.com/logo.png");

        Assert.Equal("![logo](https://example.com/logo.png)", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ImageThenBold_BoldInsideAltText()
    {
        var doc = new MarkdownDocument();
        doc.Write("View the Logo icon");
        var range = doc.Find("Logo");
        Assert.NotNull(range);
        doc.MakeImage(range.Value, "https://example.com/logo.png");
        doc.ApplyBold(range.Value);

        Assert.Equal("View the ![**Logo**](https://example.com/logo.png) icon", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void BoldThenImage_BoldWrapsEntireImage()
    {
        var doc = new MarkdownDocument();
        doc.Write("View the Logo icon");
        var range = doc.Find("Logo");
        Assert.NotNull(range);
        doc.ApplyBold(range.Value);
        doc.MakeImage(range.Value, "https://example.com/logo.png");

        Assert.Equal("View the **![Logo](https://example.com/logo.png)** icon", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ImageInsideBold()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three four");

        var threeRange = doc.Find("three");
        Assert.NotNull(threeRange);
        doc.MakeImage(threeRange.Value, "https://example.com/logo.png");

        var oneTwoThreeRange = doc.Find("two three four");
        Assert.NotNull(oneTwoThreeRange);
        doc.ApplyBold(oneTwoThreeRange.Value);

        Assert.Equal("One **two ![three](https://example.com/logo.png) four**", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void BoldWithImageInside()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three four");

        var oneTwoThreeRange = doc.Find("two three four");
        Assert.NotNull(oneTwoThreeRange);
        doc.ApplyBold(oneTwoThreeRange.Value);

        var threeRange = doc.Find("three");
        Assert.NotNull(threeRange);
        doc.MakeImage(threeRange.Value, "https://example.com/logo.png");

        Assert.Equal("One **two ![three](https://example.com/logo.png) four**", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void BoldStartsInsideImage()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three four");

        var twoRange = doc.Find("two");
        Assert.NotNull(twoRange);
        doc.MakeImage(twoRange.Value, "https://example.com/logo.png");

        var twoThreeRange = doc.Find("two three");
        Assert.NotNull(twoThreeRange);
        doc.ApplyBold(twoThreeRange.Value);

        Assert.Equal("One ![**two**](https://example.com/logo.png)** three** four", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void BoldStartsOutsideImage()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three four");

        var twoThreeRange = doc.Find("two three");
        Assert.NotNull(twoThreeRange);
        doc.ApplyBold(twoThreeRange.Value);

        var twoRange = doc.Find("two");
        Assert.NotNull(twoRange);
        doc.MakeImage(twoRange.Value, "https://example.com/logo.png");

        Assert.Equal("One **![two](https://example.com/logo.png) three** four", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ImageInsideBoldPhrase()
    {
        var doc = new MarkdownDocument();
        doc.Write("Download our App now");
        var imgRange = doc.Find("App");
        Assert.NotNull(imgRange);
        doc.MakeImage(imgRange.Value, "https://example.com/app-icon.png");
        doc.ApplyBold(new TextOffset(0), new TextOffset(21));

        Assert.Equal("**Download our ![App](https://example.com/app-icon.png) now**", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void WriteThenMakeImage_FullChain()
    {
        var doc = new MarkdownDocument();
        doc.Write("See ");
        var range = doc.Write("logo");
        doc.Write(" icon");
        doc.MakeImage(range, "https://example.com/logo.png");
        doc.ApplyBold(range);

        Assert.Equal("See ![**logo**](https://example.com/logo.png) icon", doc.ToMarkdown("\n"));
    }
}
