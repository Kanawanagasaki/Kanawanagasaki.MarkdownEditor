namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// The root node of a markdown document. A ContainerBlock that holds top-level blocks.
/// </summary>
public class MarkdownDocument : ContainerBlock
{
    public override void Accept(IMarkdownObjectVisitor visitor)
    {
        visitor.Visit(this);
        foreach (var child in Children)
            child.Accept(visitor);
    }
}
