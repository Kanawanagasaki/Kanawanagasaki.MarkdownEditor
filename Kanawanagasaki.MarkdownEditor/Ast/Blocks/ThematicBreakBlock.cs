namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// A thematic break / horizontal rule (&lt;hr&gt;). Has no content.
/// </summary>
public class ThematicBreakBlock : Block
{
    /// <summary>
    /// The character used for the thematic break (*, -, or _).
    /// </summary>
    public char Character { get; set; } = '-';

    public override void Accept(IMarkdownObjectVisitor visitor)
    {
        visitor.Visit(this);
    }
}
