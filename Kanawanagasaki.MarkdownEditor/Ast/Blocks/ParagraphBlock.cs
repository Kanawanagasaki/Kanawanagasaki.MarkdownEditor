namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// A paragraph block (&lt;p&gt;). A LeafBlock that contains inline content.
/// </summary>
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
