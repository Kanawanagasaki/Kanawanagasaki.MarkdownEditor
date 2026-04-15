namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// Base class for all block-level AST nodes.
/// Blocks are structural elements like paragraphs, headings, lists, etc.
/// </summary>
public abstract class Block : MarkdownObject
{
}

/// <summary>
/// A block that can contain other child blocks.
/// Examples: MarkdownDocument, ListBlock, ListItemBlock, QuoteBlock.
/// </summary>
public abstract class ContainerBlock : Block
{
    private readonly List<Block> _children = [];

    /// <summary>
    /// The child blocks contained within this container.
    /// </summary>
    public IReadOnlyList<Block> Children => _children;

    /// <summary>
    /// Adds a child block to this container.
    /// </summary>
    public void AddChild(Block block)
    {
        ArgumentNullException.ThrowIfNull(block);
        block.Parent = this;
        _children.Add(block);
    }

    /// <summary>
    /// Inserts a child block at the specified index.
    /// </summary>
    public void InsertChild(int index, Block block)
    {
        ArgumentNullException.ThrowIfNull(block);
        block.Parent = this;
        _children.Insert(index, block);
    }

    /// <summary>
    /// Removes a child block from this container.
    /// </summary>
    public bool RemoveChild(Block block)
    {
        if (_children.Remove(block))
        {
            block.Parent = null;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Removes the child block at the specified index.
    /// </summary>
    public void RemoveChildAt(int index)
    {
        var block = _children[index];
        block.Parent = null;
        _children.RemoveAt(index);
    }

    /// <summary>
    /// Replaces an existing child block with a new one.
    /// </summary>
    public void ReplaceChild(Block oldBlock, Block newBlock)
    {
        var index = _children.IndexOf(oldBlock);
        if (index < 0)
            throw new ArgumentException("oldBlock is not a child of this container.", nameof(oldBlock));
        oldBlock.Parent = null;
        newBlock.Parent = this;
        _children[index] = newBlock;
    }

    /// <summary>
    /// Clears all children from this container.
    /// </summary>
    public void ClearChildren()
    {
        foreach (var child in _children)
            child.Parent = null;
        _children.Clear();
    }

    /// <summary>
    /// Gets the index of a child block, or -1 if not found.
    /// </summary>
    public int IndexOfChild(Block block) => _children.IndexOf(block);
}

/// <summary>
/// A block that contains inline content rather than child blocks.
/// Examples: ParagraphBlock, HeadingBlock, CodeBlock.
/// A LeafBlock has an Inline property that is the root of its inline content.
/// </summary>
public abstract class LeafBlock : Block
{
    /// <summary>
    /// The root inline container for this leaf block's inline content.
    /// May be null if the block has no inline content (e.g., code blocks).
    /// </summary>
    public ContainerInline? Inline { get; set; }
}
