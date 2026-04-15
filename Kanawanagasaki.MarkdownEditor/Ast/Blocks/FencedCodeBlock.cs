namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// A fenced code block (``` or ~~~). Contains raw text lines rather than inline content.
/// </summary>
public class FencedCodeBlock : LeafBlock
{
    /// <summary>
    /// The info string (language identifier) after the opening fence, e.g. "csharp".
    /// </summary>
    public string? Info { get; set; }

    /// <summary>
    /// The character used for the fence (backtick ` or tilde ~).
    /// </summary>
    public char FenceChar { get; set; } = '`';

    /// <summary>
    /// The number of fence characters used (minimum 3).
    /// </summary>
    public int FenceCount { get; set; } = 3;

    /// <summary>
    /// The raw text lines of the code block content.
    /// </summary>
    public List<string> Lines { get; } = [];

    /// <summary>
    /// Gets the full text content of the code block by joining lines with newlines.
    /// </summary>
    public string GetContent() => string.Join("\n", Lines);

    /// <summary>
    /// Sets the full text content of the code block, splitting by newlines.
    /// </summary>
    public void SetContent(string content)
    {
        Lines.Clear();
        if (!string.IsNullOrEmpty(content))
            Lines.AddRange(content.Split('\n'));
    }

    public override void Accept(IMarkdownObjectVisitor visitor)
    {
        visitor.Visit(this);
    }
}
