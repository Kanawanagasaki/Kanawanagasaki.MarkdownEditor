namespace Kanawanagasaki.MarkdownEditor.Ast;

public class CodeBlock : LeafBlock
{
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
