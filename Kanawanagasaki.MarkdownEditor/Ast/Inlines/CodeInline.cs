namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// An inline code span. A LeafInline that holds code content without interpretation.
/// </summary>
public class CodeInline : LeafInline
{
    /// <summary>
    /// The code content (without backtick delimiters).
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The number of backticks used to delimit this code span.
    /// </summary>
    public int DelimiterCount { get; set; } = 1;

    public CodeInline() { }

    public CodeInline(string content)
    {
        Content = content;
    }

    public override void Accept(IMarkdownObjectVisitor visitor) => visitor.Visit(this);

    public override string ToString()
    {
        var delim = new string('`', DelimiterCount);
        return $"{delim}{Content}{delim}";
    }
}
