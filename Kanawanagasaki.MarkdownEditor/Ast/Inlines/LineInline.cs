namespace Kanawanagasaki.MarkdownEditor.Ast;

public class LineInline : ContainerInline
{
    public bool IsEmpty => FirstChild is null;

    public void AppendLiteral(string text)
    {
        var lastChild = LastChild;
        if (lastChild is LiteralInline lastLit)
            lastLit.Content += text;
        else
            AppendChild(new LiteralInline(text));
    }

    public override void Accept(IMarkdownObjectVisitor visitor) => visitor.Visit(this);
}
