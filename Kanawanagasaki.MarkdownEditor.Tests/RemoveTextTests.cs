namespace Kanawanagasaki.MarkdownEditor.Tests;

public class RemoveTextTests
{
    [Fact]
    public void MiddleOfParagraph_WithTextOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three");
        doc.RemoveText(new TextOffset(4), new TextOffset(8));

        Assert.Equal("One three", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void StartOfParagraph()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three");
        doc.RemoveText(new TextOffset(0), new TextOffset(4));

        Assert.Equal("two three", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void EndOfParagraph()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three");
        doc.RemoveText(new TextOffset(7), new TextOffset(13));

        Assert.Equal("One two", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void EntireText()
    {
        var doc = new MarkdownDocument();
        doc.Write("Everything");
        doc.RemoveText(new TextOffset(0), new TextOffset(10));

        Assert.Equal("", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void SingleCharacter()
    {
        var doc = new MarkdownDocument();
        doc.Write("abc");
        doc.RemoveText(new TextOffset(1), new TextOffset(2));

        Assert.Equal("ac", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void EmptyRange_NoOp()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello");
        doc.RemoveText(new TextOffset(2), new TextOffset(2));

        Assert.Equal("Hello", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void WithTextRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("One ");
        var range = doc.Write("two");
        doc.Write(" three");
        doc.RemoveText(range);

        Assert.Equal("One  three", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void RemovesStyledText_CleansUpEmptyStyle()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three four five");
        var range = doc.Find("two three");
        Assert.NotNull(range);
        doc.ApplyBold(range.Value);
        doc.RemoveText(range.Value.StartOffset, new TextOffset(range.Value.EndOffset.Value + 1));

        Assert.Equal("One four five", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void CrossBorderRemoval_LeavesStylesOnRemainingText()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three four five");
        if (doc.Find("two") is { } twoRange && doc.Find("three") is { } threeRange)
        {
            doc.ApplyBold(twoRange.StartOffset, threeRange.EndOffset);
            doc.RemoveText(threeRange.StartOffset, new TextOffset(threeRange.EndOffset.Value + 5));
        }

        Assert.Equal("One **two **five", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void RemoveAfterWrite_UsingRanges()
    {
        var doc = new MarkdownDocument();
        doc.Write("Prefix ");
        var removable = doc.Write("DELETE");
        doc.Write(" suffix");
        doc.RemoveText(removable);

        Assert.Equal("Prefix  suffix", doc.ToMarkdown("\n"));
    }
}
