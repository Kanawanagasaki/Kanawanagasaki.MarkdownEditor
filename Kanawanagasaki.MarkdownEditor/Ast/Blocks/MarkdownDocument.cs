namespace Kanawanagasaki.MarkdownEditor.Ast;

public class MarkdownDocument : ContainerBlock
{
    public override void Accept(IMarkdownObjectVisitor visitor)
    {
        visitor.Visit(this);
        foreach (var child in Children)
            child.Accept(visitor);
    }
}
