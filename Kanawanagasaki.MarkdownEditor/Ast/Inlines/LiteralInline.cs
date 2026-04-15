namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// Plain text content. A LeafInline that holds a string of literal characters.
/// </summary>
public class LiteralInline : LeafInline
{
    /// <summary>
    /// The literal text content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    public LiteralInline() { }

    public LiteralInline(string content)
    {
        Content = content;
    }

    public override void Accept(IMarkdownObjectVisitor visitor) => visitor.Visit(this);

    public override string ToString() => Content;
}
