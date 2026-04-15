namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// An inline code span. A LeafInline that holds code content without interpretation.
/// Supports optional prefix/suffix for emphasis delimiters that appear inside the code span
/// (e.g., <code>~~*text*~~</code> where ~~ and * are literal text inside the backticks).
/// </summary>
public class CodeInline : LeafInline
{
    /// <summary>
    /// The code content (without backtick delimiters or prefix/suffix).
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The number of backticks used to delimit this code span.
    /// </summary>
    public int DelimiterCount { get; set; } = 1;

    /// <summary>
    /// Emphasis delimiter characters that appear BEFORE the content inside the code span.
    /// These are rendered as literal text between the backtick and the content.
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Emphasis delimiter characters that appear AFTER the content inside the code span.
    /// These are rendered as literal text between the content and the closing backtick.
    /// </summary>
    public string Suffix { get; set; } = string.Empty;

    public CodeInline() { }

    public CodeInline(string content)
    {
        Content = content;
    }

    public override void Accept(IMarkdownObjectVisitor visitor) => visitor.Visit(this);

    public override string ToString()
    {
        var delim = new string('`', DelimiterCount);
        return $"{delim}{Prefix}{Content}{Suffix}{delim}";
    }
}
