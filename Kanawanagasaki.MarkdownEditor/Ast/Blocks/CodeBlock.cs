namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// An indented code block (4 spaces or 1 tab indentation).
/// Contains raw text lines rather than inline content.
/// </summary>
public class CodeBlock : LeafBlock
{
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
