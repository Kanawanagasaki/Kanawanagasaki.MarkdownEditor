namespace Kanawanagasaki.MarkdownEditor.Ast;

public class HtmlEntityInline : LeafInline
{
    public string Original { get; set; } = string.Empty;

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
