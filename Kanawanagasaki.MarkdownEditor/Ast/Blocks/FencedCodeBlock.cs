namespace Kanawanagasaki.MarkdownEditor.Ast;

public class FencedCodeBlock : LeafBlock
{
    public string? Info { get; set; }

    public char FenceChar { get; set; } = '`';

    public int FenceCount { get; set; } = 3;

    public List<string> Lines { get; } = [];

    public string GetContent() => string.Join("\n", Lines);

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
