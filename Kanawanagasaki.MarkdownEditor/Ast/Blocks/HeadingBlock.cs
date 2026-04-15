namespace Kanawanagasaki.MarkdownEditor.Ast;

public class HeadingBlock : LeafBlock
{
    public int Level { get; set; } = 1;

    public HeadingBlock()
    {
        Inline = new InlineRoot();
    }

    public HeadingBlock(int level)
    {
        Level = level;
        Inline = new InlineRoot();
    }

    public override void Accept(IMarkdownObjectVisitor visitor)
    {
        visitor.Visit(this);
        Inline?.Accept(visitor);
    }
}
