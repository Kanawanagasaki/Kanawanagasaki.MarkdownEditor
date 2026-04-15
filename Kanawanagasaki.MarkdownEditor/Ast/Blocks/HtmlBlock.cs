namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// A raw HTML block. Contains raw HTML content as text lines.
/// </summary>
public class HtmlBlock : LeafBlock
{
    /// <summary>
    /// The raw HTML lines of content.
    /// </summary>
    public List<string> Lines { get; } = [];

    /// <summary>
    /// Gets the full HTML content by joining lines with newlines.
    /// </summary>
    public string GetContent() => string.Join("\n", Lines);

    /// <summary>
    /// Sets the full HTML content, splitting by newlines.
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
