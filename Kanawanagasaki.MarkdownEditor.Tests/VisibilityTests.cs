using System.Text.Json;
using Xunit.Abstractions;

namespace Kanawanagasaki.MarkdownEditor.Tests;

public class VisibilityTests(ITestOutputHelper _output)
{
    [Fact]
    public void BoldItalicStrikethroughCode()
    {
        var doc = new MarkdownDocument();
        doc.Write("One Two Three Four");
        doc.ApplyBold(new TextOffset(0), new TextOffset(3));
        doc.ApplyItalic(new TextOffset(4), new TextOffset(7));
        doc.ApplyStrikethrough(new TextOffset(8), new TextOffset(13));
        doc.ApplyCode(new TextOffset(14), new TextOffset(18));

        Assert.Equal("**One** *Two* ~~Three~~ `Four`", doc.ToMarkdown("\n"));
        Assert.Equal("One Two Three Four", doc.GetPlainText());

        var chunks = doc.GetVisibleChunks();
        Assert.Equal(7, chunks.Count);

        Assert.Equal("One", chunks[0].Text);
        var oneStyle = Assert.Single(chunks[0].Styles);
        Assert.Equal(TextChunkStyleKind.Bold, oneStyle.Kind);

        Assert.Equal(" ", chunks[1].Text);
        Assert.Empty(chunks[1].Styles);

        Assert.Equal("Two", chunks[2].Text);
        var twoStyle = Assert.Single(chunks[2].Styles);
        Assert.Equal(TextChunkStyleKind.Italic, twoStyle.Kind);

        Assert.Equal(" ", chunks[3].Text);
        Assert.Empty(chunks[3].Styles);

        Assert.Equal("Three", chunks[4].Text);
        var threeStyle = Assert.Single(chunks[4].Styles);
        Assert.Equal(TextChunkStyleKind.Strikethrough, threeStyle.Kind);

        Assert.Equal(" ", chunks[5].Text);
        Assert.Empty(chunks[5].Styles);

        Assert.Equal("Four", chunks[6].Text);
        var fourStyle = Assert.Single(chunks[6].Styles);
        Assert.Equal(TextChunkStyleKind.Code, fourStyle.Kind);
    }

    [Fact]
    public void BoldItalicStrikethroughCode_Multiline()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("One");
        doc.ApplyBold(new TextOffset(0), new TextOffset(3));
        doc.WriteLine("Two");
        doc.ApplyItalic(new TextOffset(4), new TextOffset(7));
        doc.WriteLine("Three");
        doc.ApplyStrikethrough(new TextOffset(8), new TextOffset(13));
        doc.WriteLine("Four");
        doc.ApplyCode(new TextOffset(14), new TextOffset(18));

        Assert.Equal("**One**\n*Two*\n~~Three~~\n`Four`\n", doc.ToMarkdown("\n"));
        Assert.Equal("One\nTwo\nThree\nFour\n", doc.GetPlainText());
    }

    [Fact]
    public void OrderedList()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("One");
        doc.WriteLine("Two");
        doc.WriteLine("Three");
        doc.WriteLine("Four");
        doc.ConvertToOrderedList(new BlockIndex(0));

        Assert.Equal("1. One\n2. Two\n3. Three\n4. Four\n", doc.ToMarkdown("\n"));
        Assert.Equal("One\nTwo\nThree\nFour\n", doc.GetPlainText());
    }

    [Fact]
    public void UnorderedList()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("One");
        doc.WriteLine("Two");
        doc.WriteLine("Three");
        doc.WriteLine("Four");
        doc.ConvertToUnorderedList(new BlockIndex(0));

        Assert.Equal("- One\n- Two\n- Three\n- Four\n", doc.ToMarkdown("\n"));
        Assert.Equal("One\nTwo\nThree\nFour\n", doc.GetPlainText());
    }

    [Fact]
    public void Blockquote()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("One");
        doc.WriteParagraph("Two");
        doc.WriteParagraph("Three");
        doc.WriteParagraph("Four");

        doc.ConvertToBlockquote(new BlockIndex(0), 1);
        doc.ConvertToBlockquote(new BlockIndex(1), 2);
        doc.ConvertToBlockquote(new BlockIndex(2), 3);
        doc.ConvertToBlockquote(new BlockIndex(3), 4);

        Assert.Equal("> One\n>\n> > Two\n>\n> > > Three\n>\n> > > > Four", doc.ToMarkdown("\n"));
        Assert.Equal("One\nTwo\nThree\nFour", doc.GetPlainText());
    }

    [Fact]
    public void EmptyLineAtTheEnd()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello, world!");
        doc.WriteLine();

        Assert.Equal("Hello, world!\n", doc.ToMarkdown("\n"));
        Assert.Equal("Hello, world!\n", doc.GetPlainText());
    }

    [Fact]
    public void EmptyLinesAtTheEnd()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello, world!");
        doc.WriteLine();
        doc.WriteLine();
        doc.WriteLine();
        doc.WriteLine();

        Assert.Equal("Hello, world!\n\n\n\n", doc.ToMarkdown("\n"));
        Assert.Equal("Hello, world!\n\n\n\n", doc.GetPlainText());
    }

    [Fact]
    public void EmptyParagraphAtTheEnd()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello, world!");
        doc.WriteParagraph();

        Assert.Equal("Hello, world!\n\n", doc.ToMarkdown("\n"));
        Assert.Equal("Hello, world!\n\n", doc.GetPlainText());
    }
}
