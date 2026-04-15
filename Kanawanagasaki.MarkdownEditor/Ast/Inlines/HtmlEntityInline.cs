namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// An HTML entity inline (e.g., &amp;amp;). A LeafInline representing a decoded or raw entity.
/// </summary>
public class HtmlEntityInline : LeafInline
{
    /// <summary>
    /// The original HTML entity text (e.g., "&amp;amp;").
    /// </summary>
    public string Original { get; set; } = string.Empty;

    /// <summary>
    /// The decoded/translated text of the entity (e.g., "&").
    /// </summary>
    public string Transcoded { get; set; } = string.Empty;

    public HtmlEntityInline() { }

    public HtmlEntityInline(string original, string transcoded)
    {
        Original = original;
        Transcoded = transcoded;
    }

    public override void Accept(IMarkdownObjectVisitor visitor) => visitor.Visit(this);

    public override string ToString() => Original;
}
