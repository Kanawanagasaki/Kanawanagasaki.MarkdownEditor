using System.Text.Json;
using Xunit.Abstractions;

namespace Kanawanagasaki.MarkdownEditor.Tests;

public class EditRangeResultTests(ITestOutputHelper _output)
{
    [Fact]
    public void Write_ReturnsCorrectOffset()
    {
        var doc = new MarkdownDocument();

        var startOffset = doc.GetPlainText().Length;
        var range = doc.Write("One ");
        var endOffset = doc.GetPlainText().Length;

        Assert.Equal(startOffset, range.StartOffset);
        Assert.Equal(endOffset, range.EndOffset);
        Assert.Equal(0, range.LineStartIndex);
        Assert.Equal(0, range.LineEndIndex);
    }

    [Fact]
    public void Write_ChainedReturnsCumulativeOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("One ");

        var startOffset = doc.GetPlainText().Length;
        var range = doc.Write("two");
        var endOffset = doc.GetPlainText().Length;

        doc.Write(" three");

        Assert.Equal(startOffset, range.StartOffset);
        Assert.Equal(endOffset, range.EndOffset);
        Assert.Equal(0, range.LineStartIndex);
        Assert.Equal(0, range.LineEndIndex);
    }

    [Fact]
    public void Write_EmptyString_ReturnsZeroLengthRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello");

        var startOffset = doc.GetPlainText().Length;
        var range = doc.Write("");
        var endOffset = doc.GetPlainText().Length;

        Assert.Equal(startOffset, range.StartOffset);
        Assert.Equal(endOffset, range.EndOffset);
    }

    [Fact]
    public void Write_CrossLine_ReturnsCorrectLineIndices()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("Line 1");

        var startOffset = doc.GetPlainText().Length;
        var range = doc.Write("Cross");
        var endOffset = doc.GetPlainText().Length;

        Assert.Equal(startOffset, range.StartOffset);
        Assert.Equal(endOffset, range.EndOffset);
        Assert.Equal(1, range.LineStartIndex);
        Assert.Equal(1, range.LineEndIndex);
    }

    [Fact]
    public void WriteLine_WithText_ReturnsCorrectRange()
    {
        var doc = new MarkdownDocument();

        var startOffset = doc.GetPlainText().Length;
        var range = doc.WriteLine("Hello");
        var endOffset = doc.GetPlainText().Length;

        Assert.Equal(startOffset, range.StartOffset);
        Assert.Equal(endOffset, range.EndOffset);
    }

    [Fact]
    public void WriteLine_WithoutText_ReturnsEmptyRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("Before");

        var startOffset = doc.GetPlainText().Length;
        var range = doc.WriteLine();
        var endOffset = doc.GetPlainText().Length;

        Assert.Equal(startOffset, range.StartOffset);
        Assert.Equal(endOffset, range.EndOffset);
    }

    [Fact]
    public void WriteParagraph_WithText_ReturnsCorrectRange()
    {
        var doc = new MarkdownDocument();

        var startOffset = doc.GetPlainText().Length;
        var range = doc.WriteParagraph("Hello");
        var endOffset = doc.GetPlainText().Length;

        Assert.Equal(startOffset, range.StartOffset);
        Assert.Equal(endOffset, range.EndOffset);
    }

    [Fact]
    public void WriteParagraph_WithoutText_ReturnsEmptyRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("Before");
        _output.WriteLine("1: " + JsonSerializer.Serialize(doc.GetPlainText()));

        var startOffset = doc.GetPlainText().Length;
        var range = doc.WriteParagraph();
        var endOffset = doc.GetPlainText().Length;

        _output.WriteLine("2: " + JsonSerializer.Serialize(doc.GetPlainText()));

        Assert.Equal(startOffset, range.StartOffset);
        Assert.Equal(endOffset, range.EndOffset);
    }

    [Fact]
    public void WriteParagraph_AfterContent_ReturnsOffsetAfterGap()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("First");
        _output.WriteLine("1: " + JsonSerializer.Serialize(doc.GetPlainText()));

        var startOffset = doc.GetPlainText().Length;
        var range = doc.WriteParagraph("Second");
        var endOffset = doc.GetPlainText().Length;

        _output.WriteLine("2: " + JsonSerializer.Serialize(doc.GetPlainText()));

        Assert.Equal(startOffset, range.StartOffset);
        Assert.Equal(endOffset, range.EndOffset);
    }

    [Fact]
    public void InsertLine_ReturnsCorrectRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("Original");
        _output.WriteLine("1: " + JsonSerializer.Serialize(doc.GetPlainText()));
        var range = doc.InsertLine(0, "Inserted");
        _output.WriteLine("2: " + JsonSerializer.Serialize(doc.GetPlainText()));

        Assert.Equal(0, range.StartOffset);
        Assert.Equal(9, range.EndOffset);
    }

    [Fact]
    public void InsertLine_InMiddle_ReturnsCorrectRange()
    {
        var doc = new MarkdownDocument();
        doc.WriteLine("First");
        doc.WriteLine("Third");
        var range = doc.InsertLine(1, "Second");

        Assert.Equal(6, range.StartOffset);
        Assert.Equal(13, range.EndOffset);
    }

    [Fact]
    public void InsertParagraph_ReturnsCorrectRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("Original");
        _output.WriteLine("1: " + JsonSerializer.Serialize(doc.GetPlainText()));
        var range = doc.InsertParagraph(0, "Inserted");
        _output.WriteLine("2: " + JsonSerializer.Serialize(doc.GetPlainText()));

        Assert.Equal(0, range.StartOffset);
        Assert.Equal(10, range.EndOffset);
    }

    [Fact]
    public void ApplyBold_RangeResultCanBeUsed()
    {
        var doc = new MarkdownDocument();
        doc.Write("One ");
        var range = doc.Write("two");
        doc.Write(" three");
        doc.ApplyBold(range.StartOffset, range.EndOffset);

        Assert.Equal("One **two** three", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ApplyBold_ReturnsSameOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello world");
        var result = doc.ApplyBold(0, 5);

        Assert.Equal(0, result.StartOffset);
        Assert.Equal(5, result.EndOffset);
        Assert.Equal(0, result.LineStartIndex);
        Assert.Equal(0, result.LineEndIndex);
    }

    [Fact]
    public void ApplyBold_ChainWithWrite()
    {
        var doc = new MarkdownDocument();
        doc.Write("A ");
        var r = doc.Write("B");
        doc.Write(" C");
        var boldResult = doc.ApplyBold(r.StartOffset, r.EndOffset);

        Assert.Equal(r.StartOffset, boldResult.StartOffset);
        Assert.Equal(r.EndOffset, boldResult.EndOffset);
        Assert.Equal("A **B** C", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ApplyItalic_RangeResultCanBeUsed()
    {
        var doc = new MarkdownDocument();
        doc.Write("One ");
        var range = doc.Write("two");
        doc.Write(" three");
        doc.ApplyItalic(range.StartOffset, range.EndOffset);

        Assert.Equal("One *two* three", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ApplyItalic_ReturnsSameOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("Hello world");
        var result = doc.ApplyItalic(6, 11);

        Assert.Equal(6, result.StartOffset);
        Assert.Equal(11, result.EndOffset);
        Assert.Equal(0, result.LineStartIndex);
        Assert.Equal(0, result.LineEndIndex);
    }

    [Fact]
    public void ApplyCode_RangeResultCanBeUsed()
    {
        var doc = new MarkdownDocument();
        doc.Write("Use ");
        var range = doc.Write("var");
        doc.Write(" keyword");
        doc.ApplyCode(range.StartOffset, range.EndOffset);

        Assert.Equal("Use `var` keyword", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ApplyCode_ReturnsSameOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("some code here");
        var result = doc.ApplyCode(5, 9);

        Assert.Equal(5, result.StartOffset);
        Assert.Equal(9, result.EndOffset);
        Assert.Equal(0, result.LineStartIndex);
        Assert.Equal(0, result.LineEndIndex);
    }

    [Fact]
    public void ApplyStrikethrough_RangeResultCanBeUsed()
    {
        var doc = new MarkdownDocument();
        doc.Write("Old ");
        var range = doc.Write("text");
        doc.Write(" new text");
        doc.ApplyStrikethrough(range.StartOffset, range.EndOffset);

        Assert.Equal("Old ~~text~~ new text", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ApplyStrikethrough_ReturnsSameOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("strike this");
        var result = doc.ApplyStrikethrough(7, 11);

        Assert.Equal(7, result.StartOffset);
        Assert.Equal(11, result.EndOffset);
        Assert.Equal(0, result.LineStartIndex);
        Assert.Equal(0, result.LineEndIndex);
    }

    [Fact]
    public void MakeLink_RangeResultCanBeUsed()
    {
        var doc = new MarkdownDocument();
        doc.Write("Click ");
        var range = doc.Write("here");
        doc.Write(" for more");
        doc.MakeLink(range.StartOffset, range.EndOffset, "https://example.com");

        Assert.Equal("Click [here](https://example.com) for more", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void MakeLink_ReturnsSameOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("Visit example site");
        var result = doc.MakeLink(6, 13, "https://example.com");

        Assert.Equal(6, result.StartOffset);
        Assert.Equal(13, result.EndOffset);
        Assert.Equal(0, result.LineStartIndex);
        Assert.Equal(0, result.LineEndIndex);
    }

    [Fact]
    public void MakeImage_RangeResultCanBeUsed()
    {
        var doc = new MarkdownDocument();
        doc.Write("See ");
        var range = doc.Write("photo");
        doc.Write(" here");
        doc.MakeImage(range.StartOffset, range.EndOffset, "https://example.com/img.png");

        Assert.Equal("See ![photo](https://example.com/img.png) here", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void MakeImage_ReturnsSameOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("Look at logo");
        var result = doc.MakeImage(8, 12, "https://example.com/logo.png");

        Assert.Equal(8, result.StartOffset);
        Assert.Equal(12, result.EndOffset);
        Assert.Equal(0, result.LineStartIndex);
        Assert.Equal(0, result.LineEndIndex);
    }

    [Fact]
    public void ConvertToHeading_ByLine_ProducesCorrectMarkdown()
    {
        var doc = new MarkdownDocument();
        var range = doc.Write("Title");

        doc.ConvertToHeading(range.LineStartIndex, 1);

        Assert.Equal("# Title", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ConvertToHeading_ByLines_ProducesCorrectMarkdown()
    {
        var doc = new MarkdownDocument();
        var titleRange = doc.WriteLine("Title");
        var subtitleRange = doc.WriteLine("Subtitle");
        var thirdLineRange = doc.WriteLine("Just a third line");

        doc.ConvertToHeading(titleRange.LineStartIndex, 1);
        doc.ConvertToHeading(subtitleRange.LineStartIndex, 2);
        doc.ConvertToHeading(thirdLineRange.LineStartIndex, 3);

        Assert.Equal("# Title\n## Subtitle\n### Just a third line", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ConvertToHeading_SecondParagraph_ProducesCorrectMarkdown()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("One");
        var range = doc.WriteParagraph("Two");
        doc.WriteParagraph("Three");

        doc.ConvertToHeading(range.LineStartIndex, 2);

        Assert.Equal("One\n\n## Two\n\nThree", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ConvertToHeading_SecondParagraphAllLines_ProducesCorrectMarkdown()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("One");
        var range = doc.WriteParagraph("Two");
        doc.WriteParagraph("Three");

        for (int i = range.LineStartIndex; i <= range.LineEndIndex; i++)
            doc.ConvertToHeading(i, 2);

        Assert.Equal("One\n\n## Two\n## \nThree", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ConvertToBlockquote_ReturnsCorrectRange()
    {
        var doc = new MarkdownDocument();

        doc.Write("A quote");

        var range = doc.ConvertToBlockquote(0);
        var endOffset = doc.GetPlainText().Length;

        Assert.Equal(0, range.StartOffset);
        Assert.Equal(endOffset, range.EndOffset);
        Assert.Equal(0, range.LineStartIndex);
        Assert.Equal(0, range.LineEndIndex);
    }

    [Fact]
    public void ConvertToBlockquote_NestedLevel_ReturnsCorrectRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("Deep quote");
        var range = doc.ConvertToBlockquote(0, 2);

        Assert.Equal(0, range.StartOffset);
        Assert.Equal(10, range.EndOffset);
        Assert.Equal("> > Deep quote", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ConvertToOrderedList_ReturnsCorrectRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("First item");
        var range = doc.ConvertToOrderedList(0);

        Assert.Equal(0, range.StartOffset);
        Assert.Equal(10, range.EndOffset);
        Assert.Equal(0, range.LineStartIndex);
        Assert.Equal(0, range.LineEndIndex);
        Assert.Equal("1. First item", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ConvertToOrderedList_MultipleItems_EachReturnsOwnRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("AAA");
        var range = doc.Write("BBB");
        doc.Write("CCC");

        doc.ConvertToOrderedList(range.LineStartIndex);

        Assert.Equal("1. AAABBBCCC", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ConvertToUnorderedList_ReturnsCorrectRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bullet item");
        var range = doc.ConvertToUnorderedList(0);

        Assert.Equal(0, range.StartOffset);
        Assert.Equal(11, range.EndOffset);
        Assert.Equal(0, range.LineStartIndex);
        Assert.Equal(0, range.LineEndIndex);
        Assert.Equal("- Bullet item", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void InsertHorizontalRule_ReturnsPointRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("Before");

        var startOffset = doc.GetPlainText().Length;
        var range = doc.InsertHorizontalRule();
        var endOffset = doc.GetPlainText().Length;

        _output.WriteLine("Markdown: " + JsonSerializer.Serialize(doc.ToMarkdown("\n")));
        _output.WriteLine("Plain text: " + JsonSerializer.Serialize(doc.GetPlainText()));

        Assert.Equal(startOffset, range.StartOffset);
        Assert.Equal(endOffset, range.EndOffset);
    }

    [Fact]
    public void InsertHorizontalRule_AtLineIndex_ReturnsPointRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("After");
        _output.WriteLine("1: " + JsonSerializer.Serialize(doc.GetPlainText()));
        var range = doc.InsertHorizontalRule(0);
        _output.WriteLine("2: " + JsonSerializer.Serialize(doc.GetPlainText()));

        Assert.Equal(0, range.StartOffset);
        Assert.Equal(2, range.EndOffset);
    }

    [Fact]
    public void ClearAllStyles_ReturnsFullRange()
    {
        var doc = new MarkdownDocument();
        doc.Write("Bold text");
        doc.ApplyBold(0, 4);
        var range = doc.ClearAllStyles();

        Assert.Equal(0, range.StartOffset);
        Assert.Equal(9, range.EndOffset);
        Assert.Equal("Bold text", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ClearAllStyles_MultipleParagraphs_ReturnsFullRange()
    {
        var doc = new MarkdownDocument();
        doc.WriteParagraph("AAA BBB");
        doc.ApplyBold(0, 3);
        doc.WriteParagraph("CCC DDD");
        doc.ApplyBold(9, 12);
        var range = doc.ClearAllStyles();

        Assert.Equal(0, range.StartOffset);
        Assert.Equal(16, range.EndOffset);
        Assert.Equal("AAA BBB\n\nCCC DDD", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ClearStylesForLine_ClearsStylesOnTargetLine()
    {
        var doc = new MarkdownDocument();
        var lineRange = doc.WriteLine("Bold here");
        doc.WriteLine("Plain here");
        doc.ApplyBold(0, 4);
        var range = doc.ClearStylesForLine(0);

        Assert.Equal(lineRange.StartOffset, range.StartOffset);
        Assert.Equal(lineRange.EndOffset, range.EndOffset);
        Assert.Equal("Bold here\nPlain here\n", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ClearStylesForRange_ReturnsSameOffsets()
    {
        var doc = new MarkdownDocument();
        doc.Write("AAA BBB CCC");
        doc.ApplyBold(4, 7);
        var range = doc.ClearStylesForRange(4, 7);

        Assert.Equal(4, range.StartOffset);
        Assert.Equal(7, range.EndOffset);
        Assert.Equal(0, range.LineStartIndex);
        Assert.Equal(0, range.LineEndIndex);
        Assert.Equal("AAA BBB CCC", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void ClearStylesForRange_PreservesUnaffectedStyles()
    {
        var doc = new MarkdownDocument();
        doc.Write("AAA BBB CCC");
        doc.ApplyBold(0, 3);
        doc.ApplyBold(8, 11);
        var range = doc.ClearStylesForRange(1, 5);

        Assert.Equal(1, range.StartOffset);
        Assert.Equal(5, range.EndOffset);
        Assert.Equal("**A**AA BBB **CCC**", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void Write_ApplyBold_MakeLink_FullChain()
    {
        var doc = new MarkdownDocument();
        doc.Write("Click ");
        var range = doc.Write("here");
        doc.Write(" for info");
        doc.ApplyBold(range.StartOffset, range.EndOffset);
        doc.MakeLink(range.StartOffset, range.EndOffset, "https://example.com");

        Assert.Equal("Click **[here](https://example.com)** for info", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void Write_MakeImage_ApplyBold_FullChain()
    {
        var doc = new MarkdownDocument();
        doc.Write("See ");
        var range = doc.Write("logo");
        doc.Write(" icon");
        doc.MakeImage(range.StartOffset, range.EndOffset, "https://example.com/logo.png");
        doc.ApplyBold(range.StartOffset, range.EndOffset);

        Assert.Equal("See ![**logo**](https://example.com/logo.png) icon", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void Write_ApplyItalic_ConvertToBlockquote_Chain()
    {
        var doc = new MarkdownDocument();
        doc.Write("A ");
        var range = doc.Write("quote");
        doc.ApplyItalic(range.StartOffset, range.EndOffset);
        doc.ConvertToBlockquote(range.LineStartIndex);

        Assert.Equal("> A *quote*", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void Write_ApplyBold_ApplyCode_ApplyStrikethrough_MultipleStyles()
    {
        var doc = new MarkdownDocument();
        doc.Write("ABC ");
        var r1 = doc.Write("DEF");
        doc.Write(" GHI");
        doc.ApplyBold(r1.StartOffset, r1.EndOffset);
        doc.ApplyCode(r1.StartOffset, r1.EndOffset);
        doc.ApplyStrikethrough(r1.StartOffset, r1.EndOffset);

        Assert.Equal("ABC **`~~DEF~~`** GHI", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void Write_ApplyBold_ClearStylesForRange_ReapplyBold()
    {
        var doc = new MarkdownDocument();
        doc.Write("One ");
        var range = doc.Write("two");
        doc.Write(" three");
        doc.ApplyBold(range.StartOffset, range.EndOffset);

        Assert.Equal("One **two** three", doc.ToMarkdown("\n"));

        var clearRange = doc.ClearStylesForRange(range.StartOffset, range.EndOffset);
        Assert.Equal("One two three", doc.ToMarkdown("\n"));

        Assert.Equal(range.StartOffset, clearRange.StartOffset);
        Assert.Equal(range.EndOffset, clearRange.EndOffset);

        doc.ApplyBold(clearRange.StartOffset, clearRange.EndOffset);
        Assert.Equal("One **two** three", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void Write_ConvertToOrderedList_MultipleItems_WithStyle()
    {
        var doc = new MarkdownDocument();
        var r1 = doc.WriteLine("Bold item");
        var r2 = doc.WriteLine("Plain item");
        var r3 = doc.Write("Styled");
        doc.ConvertToOrderedList(r1.LineStartIndex);
        doc.ConvertToOrderedList(r2.LineStartIndex);
        doc.ConvertToOrderedList(r3.LineStartIndex);
        doc.ApplyBold(r3.StartOffset, r3.EndOffset);

        Assert.Equal("1. Bold item\n2. Plain item\n3. **Styled**", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void Write_MakeLink_ApplyItalic()
    {
        var doc = new MarkdownDocument();
        doc.Write("Go ");
        var range = doc.Write("there");
        doc.Write(" now");
        doc.MakeLink(range.StartOffset, range.EndOffset, "https://example.com");
        doc.ApplyItalic(range.StartOffset, range.EndOffset);

        Assert.Equal("Go [*there*](https://example.com) now", doc.ToMarkdown("\n"));
    }

    [Fact]
    public void Write_ApplyStrikethrough_MakeLink_CombineStyles()
    {
        var doc = new MarkdownDocument();
        doc.Write("Old ");
        var range = doc.Write("link");
        doc.Write(" text");
        doc.ApplyStrikethrough(range.StartOffset, range.EndOffset);
        doc.MakeLink(range.StartOffset, range.EndOffset, "https://example.com");

        Assert.Equal("Old ~~[link](https://example.com)~~ text", doc.ToMarkdown("\n"));
    }
}
