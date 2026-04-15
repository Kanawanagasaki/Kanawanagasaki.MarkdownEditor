namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// A heading block (&lt;h1&gt;–&lt;h6&gt;). A LeafBlock with a Level property.
/// </summary>
public class HeadingBlock : LeafBlock
{
    /// <summary>
    /// The heading level (1–6), corresponding to &lt;h1&gt;–&lt;h6&gt;.
    /// </summary>
    public int Level { get; set; } = 1;

    public HeadingBlock()
    {
        Inline = new InlineRoot();
    }

    public HeadingBlock(int level)
    {
        Level = level;
        Inline = new InlineRoot();
    }

    public override void Accept(IMarkdownObjectVisitor visitor)
    {
        visitor.Visit(this);
        Inline?.Accept(visitor);
    }
}
