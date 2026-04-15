namespace Kanawanagasaki.MarkdownEditor.Ast;

public abstract class MarkdownObject
{
    public SourceSpan Span { get; set; } = SourceSpan.Undefined;

    public int Line { get; set; }

    public int Column { get; set; }

    public MarkdownObject? Parent { get; internal set; }

    public abstract void Accept(IMarkdownObjectVisitor visitor);
}
