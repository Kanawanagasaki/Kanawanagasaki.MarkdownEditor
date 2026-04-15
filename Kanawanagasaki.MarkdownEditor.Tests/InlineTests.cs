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
