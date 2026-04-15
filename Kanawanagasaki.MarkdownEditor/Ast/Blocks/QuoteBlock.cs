namespace Kanawanagasaki.MarkdownEditor.Ast;

public class QuoteBlock : ContainerBlock
{
    public int NestingLevel { get; set; } = 1;

    public override void Accept(IMarkdownObjectVisitor visitor)
    {
        visitor.Visit(this);
        foreach (var child in Children)
            child.Accept(visitor);
    }
}
