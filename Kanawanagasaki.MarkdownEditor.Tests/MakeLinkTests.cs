namespace Kanawanagasaki.MarkdownEditor.Tests;

public class MakeLinkTests
{
    [Fact]
    public void BasicLink_WithTextOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("Click here for info");
        doc.MakeLink(new TextOffset(6), new TextOffset(10), "https://example.com");

        Assert.Equal("Click [here](https://example.com) for info", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void BasicLink_WithTextRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("Click ");
        var range = doc.Write("here");
        doc.Write(" for more");
        doc.MakeLink(range, "https://example.com");

        Assert.Equal("Click [here](https://example.com) for more", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void WithTitle()
    {
        var doc = new MarkdownDocument();
        doc.Write("Visit example");
        doc.MakeLink(new TextOffset(6), new TextOffset(13), "https://example.com", "Example Site");

        Assert.Equal("Visit [example](https://example.com \"Example Site\")", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void EntireText()
    {
        var doc = new MarkdownDocument();
        var range = doc.Write("Google");
        doc.MakeLink(range, "https://google.com");

        Assert.Equal("[Google](https://google.com)", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void LinkThenBold_BoldInsideLinkText()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bold link text");
        var range = doc.Find("link");
        Assert.NotNull(range);
        doc.MakeLink(range.Value, "https://example.com");
        doc.ApplyBold(range.Value);

        Assert.Equal("Bold [**link**](https://example.com) text", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void BoldThenLink_BoldWrapsEntireLink()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bold link text");
        var range = doc.Find("link");
        Assert.NotNull(range);
        doc.ApplyBold(range.Value);
        doc.MakeLink(range.Value, "https://example.com");

        Assert.Equal("Bold **[link](https://example.com)** text", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void BoldTextInsideLinkRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bold link text");
        if (doc.Find("link") is { } boldRange)
            doc.ApplyBold(boldRange);
        doc.MakeLink(new TextOffset(0), new TextOffset(15), "https://example.com");

        Assert.Equal("[Bold **link** text](https://example.com)", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void WriteThenMakeLink_FullChain()
    {
        var doc = new MarkdownDocument();
        doc.Write("Click ");
        var range = doc.Write("here");
        doc.Write(" for info");
        doc.ApplyBold(range);
        doc.MakeLink(range, "https://example.com");

        Assert.Equal("Click **[here](https://example.com)** for info", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void LinkWithItalic()
    {
        var doc = new MarkdownDocument();
        doc.Write("Go ");
        var range = doc.Write("there");
        doc.Write(" now");
        doc.MakeLink(range, "https://example.com");
        doc.ApplyItalic(range);

        Assert.Equal("Go [*there*](https://example.com) now", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void StrikethroughThenLink()
    {
        var doc = new MarkdownDocument();
        doc.Write("Old ");
        var range = doc.Write("link");
        doc.Write(" text");
        doc.ApplyStrikethrough(range);
        doc.MakeLink(range, "https://example.com");

        Assert.Equal("Old ~~[link](https://example.com)~~ text", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void MakeLink_ReturnsSameOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("Visit example site");
        var result = doc.MakeLink(new TextOffset(6), new TextOffset(13), "https://example.com");

        Assert.Equal(new TextOffset(6), result.StartOffset);
        Assert.Equal(new TextOffset(13), result.EndOffset);
    }
}
