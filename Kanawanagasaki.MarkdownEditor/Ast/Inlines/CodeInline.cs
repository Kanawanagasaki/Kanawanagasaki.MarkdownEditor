namespace Kanawanagasaki.MarkdownEditor.Ast;

public class CodeInline : LeafInline
{
    public string Content { get; set; } = string.Empty;

    public int DelimiterCount { get; set; } = 1;

    public string Prefix { get; set; } = string.Empty;

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
