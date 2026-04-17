namespace Kanawanagasaki.MarkdownEditor.Ast;

public class AutolinkInline : LeafInline
{
    public string Url { get; set; } = string.Empty;

    public AutolinkInline() { }

    public AutolinkInline(string url)
    {
        Url = url;
    }

    public override void Accept(IMarkdownObjectVisitor visitor) => visitor.Visit(this);

    public override string ToString() => $"<{Url}>";
}
