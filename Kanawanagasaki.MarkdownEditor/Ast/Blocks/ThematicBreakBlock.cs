namespace Kanawanagasaki.MarkdownEditor.Ast;

public class ThematicBreakBlock : Block
{
    public char Character { get; set; } = '-';

    public override void Accept(IMarkdownObjectVisitor visitor)
    {
        visitor.Visit(this);
    }
}
