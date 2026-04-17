namespace Kanawanagasaki.MarkdownEditor.Ast;

public class HtmlInline : LeafInline
{
    public string Content { get; set; } = string.Empty;

    public HtmlInline() { }

    public HtmlInline(string content)
    {
        Content = content;
    }

    public override void Accept(IMarkdownObjectVisitor visitor) => visitor.Visit(this);

    public override string ToString() => Content;
}
