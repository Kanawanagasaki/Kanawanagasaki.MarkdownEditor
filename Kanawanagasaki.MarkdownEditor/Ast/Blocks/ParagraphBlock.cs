namespace Kanawanagasaki.MarkdownEditor.Ast;

public class ParagraphBlock : LeafBlock
{
    public ParagraphBlock()
    {
        Inline = new InlineRoot();
    }

    public override void Accept(IMarkdownObjectVisitor visitor)
    {
        visitor.Visit(this);
        Inline?.Accept(visitor);
    }
}
