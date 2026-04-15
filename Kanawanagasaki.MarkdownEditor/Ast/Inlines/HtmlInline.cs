namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// Inline raw HTML content. A LeafInline that holds raw HTML markup.
/// </summary>
public class HtmlInline : LeafInline
{
    /// <summary>
    /// The raw HTML content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    public HtmlInline() { }

    public HtmlInline(string content)
    {
        Content = content;
    }

    public override void Accept(IMarkdownObjectVisitor visitor) => visitor.Visit(this);

    public override string ToString() => Content;
}
