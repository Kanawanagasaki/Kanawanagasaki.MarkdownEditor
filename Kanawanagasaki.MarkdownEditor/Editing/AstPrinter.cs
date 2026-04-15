using System.Text;
using Kanawanagasaki.MarkdownEditor.Ast;
using AstMarkdownDocument = Kanawanagasaki.MarkdownEditor.Ast.MarkdownDocument;

namespace Kanawanagasaki.MarkdownEditor.Editing;

/// <summary>
/// Debugging utility that prints the AST structure to a string.
/// Useful for testing and visualizing the document tree.
/// </summary>
public sealed class AstPrinter : IMarkdownObjectVisitor
{
    private readonly StringBuilder _sb = new();
    private int _indent;

    public static string Print(AstMarkdownDocument doc)
    {
        var printer = new AstPrinter();
        doc.Accept(printer);
        return printer._sb.ToString();
    }

    private void WriteNode(string name, Action? writeProps = null)
    {
        _sb.Append(new string(' ', _indent * 2));
        _sb.Append(name);
        writeProps?.Invoke();
        _sb.AppendLine();
    }

    private void WriteProps(params (string Key, string Value)[] props)
    {
        foreach (var (key, value) in props)
        {
            _sb.Append($" {key}={value}");
        }
    }

    public void Visit(AstMarkdownDocument doc)
    {
        WriteNode("MarkdownDocument");
        _indent++;
        foreach (var child in doc.Children)
            child.Accept(this);
        _indent--;
    }

    public void Visit(ParagraphBlock block)
    {
        WriteNode("ParagraphBlock");
        _indent++;
        block.Inline?.Accept(this);
        _indent--;
    }

    public void Visit(HeadingBlock block)
    {
        WriteNode("HeadingBlock", () => WriteProps(("Level", block.Level.ToString())));
        _indent++;
        block.Inline?.Accept(this);
        _indent--;
    }

    public void Visit(ListBlock block)
    {
        WriteNode("ListBlock", () => WriteProps(("Type", block.ListType.ToString())));
        _indent++;
        foreach (var child in block.Children)
            child.Accept(this);
        _indent--;
    }

    public void Visit(ListItemBlock block)
    {
        WriteNode("ListItemBlock");
        _indent++;
        foreach (var child in block.Children)
            child.Accept(this);
        _indent--;
    }

    public void Visit(QuoteBlock block)
    {
        WriteNode("QuoteBlock", () => WriteProps(("Nesting", block.NestingLevel.ToString())));
        _indent++;
        foreach (var child in block.Children)
            child.Accept(this);
        _indent--;
    }

    public void Visit(FencedCodeBlock block)
    {
        WriteNode("FencedCodeBlock", () => WriteProps(("Info", block.Info ?? ""), ("Lines", block.Lines.Count.ToString())));
    }

    public void Visit(CodeBlock block)
    {
        WriteNode("CodeBlock", () => WriteProps(("Lines", block.Lines.Count.ToString())));
    }

    public void Visit(ThematicBreakBlock block)
    {
        WriteNode("ThematicBreakBlock");
    }

    public void Visit(HtmlBlock block)
    {
        WriteNode("HtmlBlock");
    }

    public void Visit(LiteralInline inline)
    {
        WriteNode("LiteralInline", () => WriteProps(("Content", $"\"{inline.Content}\"")));
    }

    public void Visit(EmphasisInline inline)
    {
        var style = inline.IsBold ? "bold" : inline.IsItalic ? "italic" : inline.IsStrikethrough ? "strikethrough" : "custom";
        WriteNode("EmphasisInline", () => WriteProps(("Style", style), ("DelimChar", inline.DelimiterChar.ToString()), ("DelimCount", inline.DelimiterCount.ToString())));
        _indent++;
        foreach (var child in inline.GetChildren())
            child.Accept(this);
        _indent--;
    }

    public void Visit(CodeInline inline)
    {
        WriteNode("CodeInline", () => WriteProps(("Content", $"\"{inline.Content}\"")));
    }

    public void Visit(LinkInline inline)
    {
        WriteNode("LinkInline", () => WriteProps(("Url", inline.Url), ("IsImage", inline.IsImage.ToString())));
        _indent++;
        foreach (var child in inline.GetChildren())
            child.Accept(this);
        _indent--;
    }

    public void Visit(AutolinkInline inline)
    {
        WriteNode("AutolinkInline", () => WriteProps(("Url", inline.Url)));
    }

    public void Visit(LineBreakInline inline)
    {
        WriteNode("LineBreakInline", () => WriteProps(("IsHard", inline.IsHard.ToString())));
    }

    public void Visit(HtmlInline inline)
    {
        WriteNode("HtmlInline", () => WriteProps(("Content", $"\"{inline.Content}\"")));
    }

    public void Visit(HtmlEntityInline inline)
    {
        WriteNode("HtmlEntityInline", () => WriteProps(("Original", inline.Original), ("Transcoded", inline.Transcoded)));
    }

    public void Visit(InlineRoot root)
    {
        // InlineRoot is transparent — just visit children, don't print anything
        foreach (var child in root.GetChildren())
            child.Accept(this);
    }
}
