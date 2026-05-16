namespace Kanawanagasaki.MarkdownEditor.Tests;

public class FindTests
{
    [Fact]
    public void IndexOf_FindsText_ReturnsPosition()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello world");
        var pos = doc.IndexOf("world");

        Assert.NotNull(pos);
        Assert.Equal(new TextOffset(6), pos.Value.Offset);
        Assert.Equal(new BlockIndex(0), pos.Value.Block);
    }

    [Fact]
    public void IndexOf_NotFound_ReturnsNull()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello world");
        var pos = doc.IndexOf("missing");

        Assert.Null(pos);
    }

    [Fact]
    public void IndexOf_StartFromOffset_SkipsEarlierMatches()
    {
        var doc = new MarkdownDocument();
        doc.Write("abc abc abc");
        var first = doc.IndexOf("abc");
        Assert.NotNull(first);
        Assert.Equal(new TextOffset(0), first.Value.Offset);

        var second = doc.IndexOf("abc", new TextOffset(1));
        Assert.NotNull(second);
        Assert.Equal(new TextOffset(4), second.Value.Offset);

        var third = doc.IndexOf("abc", new TextOffset(5));
        Assert.NotNull(third);
        Assert.Equal(new TextOffset(8), third.Value.Offset);
    }

    [Fact]
    public void IndexOf_AcrossParagraphs_ReturnsCorrectBlock()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("First paragraph");
        doc.WriteParagraph("Second paragraph");
        doc.WriteParagraph("Third paragraph");

        var pos = doc.IndexOf("Third paragraph");
        Assert.NotNull(pos);
        Assert.Equal(new TextOffset(35), pos.Value.Offset);
        Assert.Equal(new BlockIndex(2), pos.Value.Block);
    }

    [Fact]
    public void IndexOf_InStyledText_FindsUnstyledContent()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello world");
        doc.ApplyBold(new TextOffset(0), new TextOffset(5));

        var pos = doc.IndexOf("Hello");
        Assert.NotNull(pos);
        Assert.Equal(new TextOffset(0), pos.Value.Offset);

        pos = doc.IndexOf("world");
        Assert.NotNull(pos);
        Assert.Equal(new TextOffset(6), pos.Value.Offset);
    }

    [Fact]
    public void IndexOf_CrossborderStyledText()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three four");
        doc.ApplyBold(new TextOffset(4), new TextOffset(7));

        var oneTwoPos = doc.IndexOf("One two");
        Assert.NotNull(oneTwoPos);
        Assert.Equal(new TextOffset(0), oneTwoPos.Value.Offset);

        var twoThreePos = doc.IndexOf("two three");
        Assert.NotNull(twoThreePos);
        Assert.Equal(new TextOffset(4), twoThreePos.Value.Offset);

        var oneTwoThreePos = doc.IndexOf("One two three");
        Assert.NotNull(oneTwoThreePos);
        Assert.Equal(new TextOffset(0), oneTwoThreePos.Value.Offset);
    }

    [Fact]
    public void Find_FindsText_ReturnsRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("Click here for info");
        var range = doc.Find("here");

        Assert.NotNull(range);
        Assert.Equal(new TextOffset(6), range.Value.StartOffset);
        Assert.Equal(new TextOffset(10), range.Value.EndOffset);
    }

    [Fact]
    public void Find_NotFound_ReturnsNull()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello world");
        var range = doc.Find("missing");

        Assert.Null(range);
    }

    [Fact]
    public void Find_ResultCanBeUsedForApplyBold()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three");
        var range = doc.Find("two");
        Assert.NotNull(range);
        doc.ApplyBold(range.Value);

        Assert.Equal("One **two** three", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void Find_ResultCanBeUsedForMakeLink()
    {
        var doc = new MarkdownDocument();
        doc.Write("Visit GitHub for more");
        var range = doc.Find("GitHub");
        Assert.NotNull(range);
        doc.MakeLink(range.Value, "https://github.com");

        Assert.Equal("Visit [GitHub](https://github.com) for more", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void Find_ResultCanBeUsedForMakeImage()
    {
        var doc = new MarkdownDocument();
        doc.Write("See the logo here");
        var range = doc.Find("logo");
        Assert.NotNull(range);
        doc.MakeImage(range.Value, "https://example.com/logo.png");

        Assert.Equal("See the ![logo](https://example.com/logo.png) here", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void Find_AcrossParagraphs()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("First paragraph");
        doc.WriteParagraph("Second paragraph");
        var range = doc.Find("Second paragraph");

        Assert.NotNull(range);
        Assert.Equal(new TextOffset(17), range.Value.StartOffset);
        Assert.Equal(new TextOffset(33), range.Value.EndOffset);
        Assert.Equal(new BlockIndex(1), range.Value.StartBlock);
        Assert.Equal(new BlockIndex(1), range.Value.EndBlock);
    }

    [Fact]
    public void IndexOf_ReturnsLineWithinBlock()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("Line one");
        doc.WriteLine("Line two");
        var pos = doc.IndexOf("Line two");

        Assert.NotNull(pos);
        Assert.Equal(new BlockIndex(0), pos.Value.Block);
        Assert.Equal(1, pos.Value.StartLineWithinBlock);
        Assert.Equal(1, pos.Value.EndLineWithinBlock);
    }

    [Fact]
    public void Find_ThenApplyCode()
    {
        var doc = new MarkdownDocument();
        doc.Write("Use the Console.WriteLine method");
        var range = doc.Find("Console.WriteLine");
        Assert.NotNull(range);
        doc.ApplyCode(range.Value);

        Assert.Equal("Use the `Console.WriteLine` method", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void OneFind_MultipleStyles()
    {
        var doc = new MarkdownDocument();
        doc.Write("Foo bar baz");

        var range = doc.Find("bar");

        Assert.NotNull(range);

        doc.ApplyBold(range.Value);
        doc.ApplyItalic(range.Value);
        doc.ApplyCode(range.Value);

        Assert.Equal("Foo ***`bar`*** baz", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void MultipleFinds_MultipleStyles()
    {
        var doc = new MarkdownDocument();
        doc.Write("Click bold here and see image logo");

        var boldRange = doc.Find("bold");
        var linkRange = doc.Find("here");
        var imgRange = doc.Find("logo");

        Assert.NotNull(boldRange);
        Assert.NotNull(linkRange);
        Assert.NotNull(imgRange);

        doc.ApplyBold(boldRange.Value);
        doc.MakeLink(linkRange.Value, "https://example.com");
        doc.MakeImage(imgRange.Value, "https://example.com/logo.png");

        Assert.Equal("Click **bold** [here](https://example.com) and see image ![logo](https://example.com/logo.png)", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void Find_StartFromOffset_FindsSecondOccurrence()
    {
        var doc = new MarkdownDocument();
        doc.Write("cat dog cat fish");

        var first = doc.Find("cat");
        Assert.NotNull(first);
        Assert.Equal(new TextOffset(0), first.Value.StartOffset);

        var second = doc.Find("cat", new TextOffset(1));
        Assert.NotNull(second);
        Assert.Equal(new TextOffset(8), second.Value.StartOffset);
    }
}
