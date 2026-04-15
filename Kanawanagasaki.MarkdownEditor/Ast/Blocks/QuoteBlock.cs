namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// A blockquote block. A ContainerBlock that can contain other blocks,
/// including nested blockquotes.
/// </summary>
public class QuoteBlock : ContainerBlock
{
    /// <summary>
    /// The nesting level of this blockquote (1 for top-level, 2 for nested, etc.).
    /// </summary>
    public int NestingLevel { get; set; } = 1;

    public override void Accept(IMarkdownObjectVisitor visitor)
    {
        visitor.Visit(this);
        foreach (var child in Children)
            child.Accept(visitor);
    }
}
