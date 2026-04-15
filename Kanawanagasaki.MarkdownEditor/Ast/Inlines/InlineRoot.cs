namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// A concrete ContainerInline used as the root container for inline content
/// within a LeafBlock. This is not rendered directly — it simply groups
/// the top-level inlines of a paragraph, heading, etc.
/// </summary>
public class InlineRoot : ContainerInline
{
    public override void Accept(IMarkdownObjectVisitor visitor)
    {
        visitor.Visit(this);
    }
}
