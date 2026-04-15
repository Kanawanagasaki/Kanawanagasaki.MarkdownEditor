namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// A single list item within a ListBlock. A ContainerBlock that can contain
/// paragraphs, nested lists, and other block-level content.
/// </summary>
public class ListItemBlock : ContainerBlock
{
    public override void Accept(IMarkdownObjectVisitor visitor)
    {
        visitor.Visit(this);
        foreach (var child in Children)
            child.Accept(visitor);
    }
}
