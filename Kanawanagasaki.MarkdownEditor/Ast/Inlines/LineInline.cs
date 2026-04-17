namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// A line within a paragraph. A ContainerInline that groups inline elements
/// belonging to the same visual line. Multiple LineInline children within
/// a ParagraphBlock represent soft line breaks between them.
/// </summary>
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
