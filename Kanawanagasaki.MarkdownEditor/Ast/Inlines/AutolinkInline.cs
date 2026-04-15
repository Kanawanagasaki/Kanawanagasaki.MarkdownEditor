namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// An autolink inline (&lt;url&gt;). A LeafInline that represents an automatically-linked URL.
/// </summary>
public class AutolinkInline : LeafInline
{
    /// <summary>
    /// The URL of the autolink.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    public AutolinkInline() { }

    public AutolinkInline(string url)
    {
        Url = url;
    }

    public override void Accept(IMarkdownObjectVisitor visitor) => visitor.Visit(this);

    public override string ToString() => $"<{Url}>";
}
