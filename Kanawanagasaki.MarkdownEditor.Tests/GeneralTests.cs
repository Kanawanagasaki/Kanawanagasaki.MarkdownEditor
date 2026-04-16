namespace Kanawanagasaki.MarkdownEditor.Tests;

public class GeneralTests
{
    [Fact]
    public void WriteThenHeadingThenOrderedList()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("Title");
        doc.WriteLine("Item");
        doc.ConvertToHeading(0, 1);
        doc.ConvertToOrderedList(1);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("# Title\n1. Item\n", md);
    }

    [Fact]
    public void MultipleStylesAndLinks()
    {
        var doc = new MarkdownDocument();
        doc.Write("Click bold here and see image logo");
        int boldStart = "Click ".Length;
        int boldEnd = boldStart + "bold".Length;
        int linkStart = "Click bold ".Length;
        int linkEnd = linkStart + "here".Length;
        int imgStart = "Click bold here and see image ".Length;
        int imgEnd = imgStart + "logo".Length;

        doc.ApplyBold(boldStart, boldEnd);
        doc.MakeLink(linkStart, linkEnd, "https://example.com");
        doc.MakeImage(imgStart, imgEnd, "https://example.com/logo.png");

        var md = doc.ToMarkdown("\n");
        Assert.Equal("Click **bold** [here](https://example.com) and see image ![logo](https://example.com/logo.png)", md);
    }

    [Fact]
    public void ListWithFirstItemBlockquote()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("First item");
        doc.WriteLine("Second item");
        doc.ConvertToOrderedList(0);
        doc.ConvertToOrderedList(1);
        doc.ConvertToBlockquote(0);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("> 1. First item\n1. Second item\n", md);
    }

    [Fact]
    public void BlockquoteWithListInside()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("First item");
        doc.WriteLine("Second item");
        doc.ConvertToOrderedList(0);
        doc.ConvertToOrderedList(1);
        doc.ConvertToBlockquote(0);
        doc.ConvertToBlockquote(1);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("> 1. First item\n> 2. Second item\n", md);
    }

    [Fact]
    public void ListWithBlockquoteInside()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("First item");
        doc.WriteLine("Second item");
        doc.ConvertToBlockquote(0);
        doc.ConvertToBlockquote(1);
        doc.ConvertToOrderedList(0);
        doc.ConvertToOrderedList(1);

        var md = doc.ToMarkdown("\n");
        Assert.Equal("1. > First item\n2. > Second item\n", md);
    }

    [Fact]
    public void ApplyStyleThenClearThenReapply()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello world");
        doc.ApplyBold(0, 5);
        Assert.Equal("**Hello** world", doc.ToMarkdown("\n"));

        doc.ClearAllStyles();
        Assert.Equal("Hello world", doc.ToMarkdown("\n"));

        doc.ApplyItalic(6, 11);
        Assert.Equal("Hello *world*", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void EmptyDocument_ToMarkdown_ReturnsEmpty()
    {
        var doc = new MarkdownDocument();
        Assert.Equal("", doc.ToMarkdown("\n"));
    }

    // [Fact]
    // public void ComplexDocument()
    // {
    //     var doc = new MarkdownDocument();

    //     doc.WriteParagraph("The Comprehensive Markdown Guide");
    //     doc.ConvertToHeading(lineIndex: 0, level: 1);

    //     doc.WriteParagraph("Introduction");
    //     doc.ConvertToHeading(lineIndex: 2, level: 2);

    //     doc.Write("This document features ");
    //     int boldStart = doc.GetPlainText().Length;
    //     doc.Write("bold text");
    //     int boldEnd = doc.GetPlainText().Length;

    //     doc.Write(", ");
    //     int italicStart = doc.GetPlainText().Length;
    //     doc.Write("italicized emphasis");
    //     int italicEnd = doc.GetPlainText().Length;

    //     doc.Write(", and ");
    //     int strikeStart = doc.GetPlainText().Length;
    //     doc.Write("outdated information");
    //     int strikeEnd = doc.GetPlainText().Length;

    //     doc.Write(" alongside ");
    //     int codeStart = doc.GetPlainText().Length;
    //     doc.Write("var x = 10;");
    //     int codeEnd = doc.GetPlainText().Length;
    //     doc.WriteLine(".");

    //     doc.ApplyBold(boldStart, boldEnd);
    //     doc.ApplyItalic(italicStart, italicEnd);
    //     doc.ApplyStrikethrough(strikeStart, strikeEnd);
    //     doc.ApplyCode(codeStart, codeEnd);

    //     doc.InsertHorizontalRule();

    //     doc.WriteLine("Unordered List Item 1");
    //     doc.WriteLine("Unordered List Item 2");
    //     doc.WriteLine("Unordered List Item 3");

    //     doc.ConvertToUnorderedList(lineIndex: 5);
    //     doc.ConvertToUnorderedList(lineIndex: 6);
    //     doc.ConvertToUnorderedList(lineIndex: 7);

    //     doc.WriteLine();
    //     doc.WriteLine("Step One");
    //     doc.WriteLine("Step Two");
    //     doc.ConvertToOrderedList(lineIndex: 8);
    //     doc.ConvertToOrderedList(lineIndex: 9);

    //     doc.WriteParagraph("This is a profound quote that will be wrapped in a blockquote.");
    //     doc.ConvertToBlockquote(lineIndex: 10);

    //     doc.ConvertToBlockquote(lineIndex: 10, nestedLevel: 2);

    //     doc.WriteParagraph("Check out GitHub for more info or see the logo below:");
    //     string plainText = doc.GetPlainText();
    //     int linkStart = plainText.IndexOf("GitHub");
    //     int linkEnd = linkStart + "GitHub".Length;
    //     doc.MakeLink(linkStart, linkEnd, "https://github.com", "GitHub Homepage");

    //     doc.WriteParagraph("LogoAltText");
    //     string imgText = doc.GetPlainText();
    //     int imgStart = imgText.LastIndexOf("LogoAltText");
    //     int imgEnd = imgStart + "LogoAltText".Length;
    //     doc.MakeImage(imgStart, imgEnd, "https://example.com/logo.png", "Company Logo");

    //     doc.InsertLine(0, "Draft Version: 2.0");

    //     doc.InsertParagraph(5, "--- Additional context inserted via API ---");

    //     string finalScan = doc.GetPlainText();
    //     int removeStart = finalScan.IndexOf("outdated");
    //     int removeEnd = finalScan.IndexOf("alongside");
    //     if (removeStart != -1)
    //         doc.RemoveText(removeStart, removeEnd);

    //     Assert.Equal("""
    //     Draft Version: 2.0
    //     # The Comprehensive Markdown Guide

    //     ## Introduction

    //     --- Additional context inserted via API ---

    //     This document features **bold text**, *italicized emphasis*, and alongside `var x = 10;`.

    //     ---

    //     * Unordered List Item 1
    //     * Unordered List Item 2
    //     * Unordered List Item 3

    //     1. Step One
    //     2. Step Two

    //     > > This is a profound quote that will be wrapped in a blockquote.

    //     Check out [GitHub](https://github.com "GitHub Homepage") for more info or see the logo below:

    //     ![LogoAltText](https://example.com/logo.png "Company Logo")
    //     """, doc.ToMarkdown("\n"));
    // }
}
