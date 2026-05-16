namespace Kanawanagasaki.MarkdownEditor.Tests;

public class GeneralTests
{
    [Fact]
    public void WriteThenHeadingThenOrderedList()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("Title");
        doc.WriteLine("Item");
        doc.ConvertToHeading(new BlockIndex(0), 1);
        doc.ConvertToOrderedList(new BlockIndex(1));

        var md = doc.ToMarkdown("\n");
        Assert.Equal("# Title\n1. Item\n", md);
    }

    [Fact]
    public void MultipleStylesAndLinks()
    {
        var doc = new MarkdownDocument();
        doc.Write("Click ");
        var boldRange = doc.Write("bold");
        doc.Write(" ");
        var linkRange = doc.Write("here");
        doc.Write(" and see image ");
        var imgRange = doc.Write("logo");

        doc.ApplyBold(boldRange);
        doc.MakeLink(linkRange, "https://example.com");
        doc.MakeImage(imgRange, "https://example.com/logo.png");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Click **bold** [here](https://example.com) and see image ![logo](https://example.com/logo.png)", md);
    }

    [Fact]
    public void BlockquoteWithListInside()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("First item");
        doc.WriteLine("Second item");
        doc.ConvertToOrderedList(new BlockIndex(0));
        doc.ConvertToBlockquote(new BlockIndex(0));

        var md = doc.ToMarkdown("\n");
        Assert.Equal("> 1. First item\n> 2. Second item\n", md);
    }

    [Fact]
    public void ApplyStyleThenClearThenReapply()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello world");
        doc.ApplyBold(new TextOffset(0), new TextOffset(5));
        Assert.Equal("**Hello** world", doc.ToMarkdown("\n"));

        doc.ClearAllStyles();
        Assert.Equal("Hello world", doc.ToMarkdown("\n"));

        doc.ApplyItalic(new TextOffset(6), new TextOffset(11));
        Assert.Equal("Hello *world*", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void EmptyDocument_ToMarkdown_ReturnsEmpty()
    {
        var doc = new MarkdownDocument();
        Assert.Equal("", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void EmptyDocument_PlainTextLength_IsZero()
    {
        var doc = new MarkdownDocument();
        Assert.Equal(0, doc.PlainTextLength);
        Assert.Equal(0, doc.Length);
    }

    [Fact]
    public void HeadingWithStyledText_ThenListWithStyledItems()
    {
        var doc = new MarkdownDocument();
        var headingRange = doc.WriteParagraph("The Guide");
        doc.ApplyBold(headingRange.StartOffset, new TextOffset(headingRange.StartOffset.Value + 3));
        doc.ConvertToHeading(headingRange.StartBlock, 1);

        var item1Range = doc.WriteParagraph("First item");
        var item2Range = doc.WriteParagraph("Second item");
        doc.ApplyItalic(item2Range);
        doc.ConvertToOrderedList(item1Range.StartBlock);
        doc.ConvertToOrderedList(item2Range.StartBlock);

        Assert.Equal("# **The** Guide\n\n1. First item\n\n2. *Second item*", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void FullDocument_ComplexEditing()
    {
        var doc = new MarkdownDocument();

        var titleRange = doc.WriteParagraph("The Comprehensive Markdown Guide");
        doc.ConvertToHeading(titleRange.StartBlock, 1);

        var subtitleRange = doc.WriteParagraph("Introduction");
        doc.ConvertToHeading(subtitleRange.StartBlock, 2);

        doc.WriteParagraph("This document features ");
        var boldRange = doc.Write("bold text");
        doc.Write(", ");
        var italicRange = doc.Write("italicized emphasis");
        doc.Write(", and ");
        var strikeRange = doc.Write("outdated information");
        doc.Write(" alongside ");
        var codeRange = doc.Write("var x = 10;");
        doc.WriteLine("."); // writes dot and \n
        doc.Write("Here is another line!"); // writes without \n
        doc.WriteNewLine("And another one!"); // writes \n then text
        doc.ApplyBold(boldRange);
        doc.ApplyItalic(italicRange);
        doc.ApplyStrikethrough(strikeRange);
        doc.ApplyCode(codeRange);

        doc.InsertHorizontalRule();

        doc.WriteParagraph("Unordered List Item 1");
        doc.WriteParagraph("Unordered List Item 2");
        doc.WriteParagraph("Unordered List Item 3");
        doc.ConvertToUnorderedList(new BlockIndex(4));
        doc.ConvertToUnorderedList(new BlockIndex(5));
        doc.ConvertToUnorderedList(new BlockIndex(6));

        var md = doc.ToMarkdown("\n");
        Assert.Contains("""
            # The Comprehensive Markdown Guide

            ## Introduction

            This document features **bold text**, *italicized emphasis*, and ~~outdated information~~ alongside `var x = 10;`.
            Here is another line!
            And another one!

            1. Unordered List Item 1

            2. Unordered List Item 2

            3. Unordered List Item 3
            """, md);
    }
}
