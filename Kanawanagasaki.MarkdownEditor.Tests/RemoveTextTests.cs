namespace Kanawanagasaki.MarkdownEditor.Tests;

public class RemoveText
{
    [Fact]
    public void RemoveText_MiddleOfParagraph()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three");
        doc.RemoveText(4, 8);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("One three", md);
    }

    [Fact]
    public void RemoveText_StartOfParagraph()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three");
        doc.RemoveText(0, 4);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("two three", md);
    }

    [Fact]
    public void RemoveText_EndOfParagraph()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three");
        doc.RemoveText(7, 13);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("One two", md);
    }

    [Fact]
    public void RemoveText_EntireText()
    {
        var doc = new MarkdownDocument();
        doc.Write("Everything");
        doc.RemoveText(0, 10);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("", md);
    }

    [Fact]
    public void RemoveText_SingleCharacter()
    {
        var doc = new MarkdownDocument();
        doc.Write("abc");
        doc.RemoveText(1, 2);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("ac", md);
    }

    [Fact]
    public void RemoveText_OutOfRange_ClampsToContent()
    {
        var doc = new MarkdownDocument();
        doc.Write("abc");
        doc.RemoveText(0, 100);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("", md);
    }

    [Fact]
    public void RemoveText_EmptyRange_NoOp()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello");
        doc.RemoveText(2, 2);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Hello", md);
    }

    [Fact]
    public void RemoveText_NegativeStart_ClampsToZero()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello");
        doc.RemoveText(-5, 2);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("llo", md);
    }

    [Fact]
    public void RemoveText_ShouldClearEmpty()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three four five");

        var twoStart = "One ".Length;
        var threeEnd = twoStart + "two three".Length;

        doc.ApplyBold(twoStart, threeEnd);
        doc.RemoveText(twoStart, threeEnd + 1);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("One four five", md);
    }

    [Fact]
    public void RemoveText_CrossBorder()
    {
        var doc = new MarkdownDocument();
        doc.Write("One two three four five");

        var twoStart = "One ".Length;
        var threeStart = twoStart + "two ".Length;
        var threeEnd = threeStart + "three".Length;
        var fourEnd = threeEnd + " four".Length;

        doc.ApplyBold(twoStart, threeEnd);
        doc.RemoveText(threeStart, fourEnd + 1);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("One **two **five", md);
    }
}
