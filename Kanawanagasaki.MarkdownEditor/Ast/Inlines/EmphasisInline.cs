namespace Kanawanagasaki.MarkdownEditor.Ast;

public class EmphasisInline : ContainerInline
{
    public char DelimiterChar { get; set; } = '*';

    public int DelimiterCount { get; set; } = 1;

    public bool IsBold => DelimiterCount == 2 && (DelimiterChar == '*' || DelimiterChar == '_');

    public bool IsItalic => DelimiterCount == 1 && (DelimiterChar == '*' || DelimiterChar == '_');

    public bool IsStrikethrough => DelimiterChar == '~';

    public EmphasisInline() { }

    public EmphasisInline(char delimiterChar, int delimiterCount)
    {
        DelimiterChar = delimiterChar;
        DelimiterCount = delimiterCount;
    }

    public static EmphasisInline Bold() => new('*', 2);

    public static EmphasisInline Italic() => new('*', 1);

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
