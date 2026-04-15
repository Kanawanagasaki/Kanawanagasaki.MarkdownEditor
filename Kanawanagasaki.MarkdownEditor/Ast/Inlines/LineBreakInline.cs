namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// A line break inline (hard or soft). A LeafInline with no text content.
/// </summary>
public class LineBreakInline : LeafInline
{
    /// <summary>
    /// Whether this is a hard line break (two trailing spaces or backslash at end of line)
    /// vs a soft line break (just a newline in the source).
    /// </summary>
    public bool IsHard { get; set; }

    /// <summary>
    /// Whether this is a soft line break.
    /// </summary>
    public bool IsSoft => !IsHard;

    public LineBreakInline() { }

    public LineBreakInline(bool isHard)
    {
        IsHard = isHard;
    }

    public override void Accept(IMarkdownObjectVisitor visitor) => visitor.Visit(this);

    public override string ToString() => IsHard ? "  \n" : "\n";
}
