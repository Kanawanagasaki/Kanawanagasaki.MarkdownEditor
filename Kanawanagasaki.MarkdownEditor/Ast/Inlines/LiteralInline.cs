namespace Kanawanagasaki.MarkdownEditor.Ast;

public class LiteralInline : LeafInline
{
    public string Content { get; set; } = string.Empty;

    public LiteralInline() { }

    public LiteralInline(string content)
    {
        Content = content;
    }

    public override void Accept(IMarkdownObjectVisitor visitor) => visitor.Visit(this);

    public override string ToString() => Content;
}
