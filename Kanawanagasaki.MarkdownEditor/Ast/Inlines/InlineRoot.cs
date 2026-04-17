namespace Kanawanagasaki.MarkdownEditor.Ast;

public class InlineRoot : ContainerInline
{
    public override void Accept(IMarkdownObjectVisitor visitor)
    {
        visitor.Visit(this);
    }
}
