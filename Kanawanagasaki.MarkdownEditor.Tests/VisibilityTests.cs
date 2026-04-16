namespace Kanawanagasaki.MarkdownEditor.Tests;

public class VisibilityTests
{
    [Fact]
    public void Visibility_BoldItalicStrikethroughCode()
    {
        var doc = new MarkdownDocument();
        doc.Write("One Two Three Four");
        doc.ApplyBold(0, 3);
        doc.ApplyItalic(4, 7);
        doc.ApplyStrikethrough(8, 13);
        doc.ApplyCode(14, 18);

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
    public void Visibility_BoldItalicStrikethroughCode_Multiline()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("One");
        doc.ApplyBold(0, 3);
        doc.WriteLine("Two");
        doc.ApplyItalic(4, 7);
        doc.WriteLine("Three");
        doc.ApplyStrikethrough(8, 13);
        doc.WriteLine("Four");
        doc.ApplyCode(14, 18);

        Assert.Equal("**One**\n*Two*\n~~Three~~\n`Four`\n", doc.ToMarkdown("\n"));
        Assert.Equal("One\nTwo\nThree\nFour\n", doc.GetPlainText());
    }

    [Fact]
    public void Visibility_OrderedList()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("One");
        doc.ConvertToOrderedList(0);
        doc.WriteLine("Two");
        doc.ConvertToOrderedList(1);
        doc.WriteLine("Three");
        doc.ConvertToOrderedList(2);
        doc.WriteLine("Four");
        doc.ConvertToOrderedList(3);

        Assert.Equal("1. One\n2. Two\n3. Three\n4. Four\n", doc.ToMarkdown("\n"));
        Assert.Equal("One\nTwo\nThree\nFour\n", doc.GetPlainText());
    }

    [Fact]
    public void Visibility_UnorderedList()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("One");
        doc.ConvertToUnorderedList(0);
        doc.WriteLine("Two");
        doc.ConvertToUnorderedList(1);
        doc.WriteLine("Three");
        doc.ConvertToUnorderedList(2);
        doc.WriteLine("Four");
        doc.ConvertToUnorderedList(3);

        Assert.Equal("- One\n- Two\n- Three\n- Four\n", doc.ToMarkdown("\n"));
        Assert.Equal("One\nTwo\nThree\nFour\n", doc.GetPlainText());
    }

    [Fact]
    public void Visibility_Blockquote()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("One");
        doc.ConvertToBlockquote(0, 1);
        doc.WriteLine("Two");
        doc.ConvertToBlockquote(0, 2);
        doc.WriteLine("Three");
        doc.ConvertToBlockquote(0, 3);
        doc.WriteLine("Four");
        doc.ConvertToBlockquote(0, 4);

        Assert.Equal("> One\n> > Two\n> > > Three\n> > > > Four\n", doc.ToMarkdown("\n"));
        Assert.Equal("One\nTwo\nThree\nFour\n", doc.GetPlainText());
    }
}
