namespace Kanawanagasaki.MarkdownEditor.Tests;

public class InlineTests
{
    [Fact]
    public void BoldTest()
    {
        var text = "One two three";
        var expected = "One **two** three";
        int twoStart = text.IndexOf("two");
        int twoEnd = twoStart + "two".Length;
        var doc = new MarkdownDocument();

        doc.Write(text);
        doc.ApplyBold(twoStart, twoEnd);

        var md = doc.ToMarkdown("\n");

        Assert.Equal(expected, md);
    }

    [Fact]
    public void BoldTest_SecondLine()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("First line");
        doc.WriteLine("Second line");

        var plainText = doc.GetPlainText();
        int start = plainText.IndexOf("Second line");
        int end = start + "Second line".Length;

        doc.ApplyBold(start, end);

        Assert.Equal("First line\n**Second line**\n", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void BoldTest_MultipleLines()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("First line");
        doc.WriteLine("Second line");
        doc.WriteLine("Third line");
        doc.WriteLine("Fourth line");

        var plainText = doc.GetPlainText();
        int start = plainText.IndexOf("Second line");
        int end = plainText.IndexOf("Third line") + "Third line".Length;

        doc.ApplyBold(start, end);

        Assert.Equal("First line\n**Second line**\n**Third line**\nFourth line\n", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ItalicTest()
    {
        var text = "One two three";
        var expected = "One *two* three";
        int twoStart = text.IndexOf("two");
        int twoEnd = twoStart + "two".Length;
        var doc = new MarkdownDocument();

        doc.Write(text);
        doc.ApplyItalic(twoStart, twoEnd);

        var md = doc.ToMarkdown("\n");

        Assert.Equal(expected, md);
    }

    [Fact]
    public void StrikethroughTest()
    {
        var text = "One two three";
        var expected = "One ~~two~~ three";
        int twoStart = text.IndexOf("two");
        int twoEnd = twoStart + "two".Length;
        var doc = new MarkdownDocument();

        doc.Write(text);
        doc.ApplyStrikethrough(twoStart, twoEnd);

        var md = doc.ToMarkdown("\n");

        Assert.Equal(expected, md);
    }

    [Fact]
    public void CodeTest()
    {
        var text = "One two three";
        var expected = "One `two` three";
        int twoStart = text.IndexOf("two");
        int twoEnd = twoStart + "two".Length;
        var doc = new MarkdownDocument();

        doc.Write(text);
        doc.ApplyCode(twoStart, twoEnd);

        var md = doc.ToMarkdown("\n");

        Assert.Equal(expected, md);
    }

    [Fact]
    public void BoldItalicTest()
    {
        var text = "One two three";
        var expected = "One ***two*** three";
        int twoStart = text.IndexOf("two");
        int twoEnd = twoStart + "two".Length;
        var doc = new MarkdownDocument();

        doc.Write(text);
        doc.ApplyBold(twoStart, twoEnd);
        doc.ApplyItalic(twoStart, twoEnd);

        var md = doc.ToMarkdown("\n");

        Assert.Equal(expected, md);
    }

    [Fact]
    public void BoldItalicStrikethroughTest()
    {
        var text = "One two three";
        var expected = "One ***~~two~~*** three";
        int twoStart = text.IndexOf("two");
        int twoEnd = twoStart + "two".Length;
        var doc = new MarkdownDocument();

        doc.Write(text);
        doc.ApplyBold(twoStart, twoEnd);
        doc.ApplyItalic(twoStart, twoEnd);
        doc.ApplyStrikethrough(twoStart, twoEnd);

        var md = doc.ToMarkdown("\n");

        Assert.Equal(expected, md);
    }

    [Fact]
    public void BoldItalicStrikethroughCodeTest()
    {
        var text = "One two three";
        var expected = "One ***~~`two`~~*** three";
        int twoStart = text.IndexOf("two");
        int twoEnd = twoStart + "two".Length;
        var doc = new MarkdownDocument();

        doc.Write(text);
        doc.ApplyBold(twoStart, twoEnd);
        doc.ApplyItalic(twoStart, twoEnd);
        doc.ApplyStrikethrough(twoStart, twoEnd);
        doc.ApplyCode(twoStart, twoEnd);

        var md = doc.ToMarkdown("\n");

        Assert.Equal(expected, md);
    }

    [Fact]
    public void CodeStrikethroughItalicBoldTest()
    {
        var text = "One two three";
        var expected = "One `~~***two***~~` three";
        int twoStart = text.IndexOf("two");
        int twoEnd = twoStart + "two".Length;
        var doc = new MarkdownDocument();

        doc.Write(text);
        doc.ApplyCode(twoStart, twoEnd);
        doc.ApplyStrikethrough(twoStart, twoEnd);
        doc.ApplyItalic(twoStart, twoEnd);
        doc.ApplyBold(twoStart, twoEnd);

        var md = doc.ToMarkdown("\n");

        Assert.Equal(expected, md);
    }

    [Fact]
    public void StrikethroughBoldCodeItalicTest()
    {
        var text = "One two three";
        var expected = "One ~~**`*two*`**~~ three";
        int twoStart = text.IndexOf("two");
        int twoEnd = twoStart + "two".Length;
        var doc = new MarkdownDocument();

        doc.Write(text);
        doc.ApplyStrikethrough(twoStart, twoEnd);
        doc.ApplyBold(twoStart, twoEnd);
        doc.ApplyCode(twoStart, twoEnd);
        doc.ApplyItalic(twoStart, twoEnd);

        var md = doc.ToMarkdown("\n");

        Assert.Equal(expected, md);
    }
}
