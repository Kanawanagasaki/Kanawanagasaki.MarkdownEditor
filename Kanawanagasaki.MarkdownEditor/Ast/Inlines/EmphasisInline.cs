namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// Emphasis inline (&lt;em&gt; or &lt;strong&gt;). A ContainerInline that wraps
/// child inlines with emphasis styling.
/// DelimiterChar is '*' or '_', DelimiterCount is 1 for italic/em or 2 for bold/strong.
/// </summary>
public class EmphasisInline : ContainerInline
{
    /// <summary>
    /// The delimiter character used: '*' or '_'.
    /// </summary>
    public char DelimiterChar { get; set; } = '*';

    /// <summary>
    /// The number of delimiter characters: 1 for italic (&lt;em&gt;), 2 for bold (&lt;strong&gt;),
    /// 3 for bold+italic. Also used for strikethrough with '~' delimiter.
    /// </summary>
    public int DelimiterCount { get; set; } = 1;

    /// <summary>
    /// Whether this emphasis represents bold (DelimiterCount == 2 and DelimiterChar is * or _).
    /// </summary>
    public bool IsBold => DelimiterCount == 2 && (DelimiterChar == '*' || DelimiterChar == '_');

    /// <summary>
    /// Whether this emphasis represents italic (DelimiterCount == 1 and DelimiterChar is * or _).
    /// </summary>
    public bool IsItalic => DelimiterCount == 1 && (DelimiterChar == '*' || DelimiterChar == '_');

    /// <summary>
    /// Whether this emphasis represents strikethrough (DelimiterChar is '~').
    /// </summary>
    public bool IsStrikethrough => DelimiterChar == '~';

    public EmphasisInline() { }

    public EmphasisInline(char delimiterChar, int delimiterCount)
    {
        DelimiterChar = delimiterChar;
        DelimiterCount = delimiterCount;
    }

    /// <summary>
    /// Creates a bold emphasis inline (DelimiterChar='*', DelimiterCount=2).
    /// </summary>
    public static EmphasisInline Bold() => new('*', 2);

    /// <summary>
    /// Creates an italic emphasis inline (DelimiterChar='*', DelimiterCount=1).
    /// </summary>
    public static EmphasisInline Italic() => new('*', 1);

    /// <summary>
    /// Creates a strikethrough emphasis inline (DelimiterChar='~', DelimiterCount=2).
    /// </summary>
    public static EmphasisInline Strikethrough() => new('~', 2);

    public override void Accept(IMarkdownObjectVisitor visitor)
    {
        visitor.Visit(this);
        foreach (var child in GetChildren())
            child.Accept(visitor);
    }

    public override string ToString()
    {
        var delim = new string(DelimiterChar, DelimiterCount);
        var inner = string.Join("", GetChildren().Select(c => c.ToString()));
        return $"{delim}{inner}{delim}";
    }
}
