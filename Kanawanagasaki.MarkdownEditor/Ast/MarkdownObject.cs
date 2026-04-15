namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// Base class for all AST nodes in the markdown document.
/// Provides source position tracking via Span, Line, and Column.
/// </summary>
public abstract class MarkdownObject
{
    /// <summary>
    /// Start and end positions (inclusive) in the source text.
    /// </summary>
    public SourceSpan Span { get; set; } = SourceSpan.Undefined;

    /// <summary>
    /// Zero-based line number in the source text.
    /// </summary>
    public int Line { get; set; }

    /// <summary>
    /// Zero-based column number in the source text.
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// The parent node in the AST hierarchy, or null if this is the root.
    /// </summary>
    public MarkdownObject? Parent { get; internal set; }

    /// <summary>
    /// Accepts a visitor for traversing the AST.
    /// </summary>
    public abstract void Accept(IMarkdownObjectVisitor visitor);
}
