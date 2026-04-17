namespace Kanawanagasaki.MarkdownEditor.Ast;

public abstract class Block : MarkdownObject
{
}

public abstract class ContainerBlock : Block
{
    private readonly List<Block> _children = [];

    public IReadOnlyList<Block> Children => _children;

    public void AddChild(Block block)
    {
        ArgumentNullException.ThrowIfNull(block);
        block.Parent = this;
        _children.Add(block);
    }

    public void InsertChild(int index, Block block)
    {
        ArgumentNullException.ThrowIfNull(block);
        block.Parent = this;
        _children.Insert(index, block);
    }

    public bool RemoveChild(Block block)
    {
        if (_children.Remove(block))
        {
            block.Parent = null;
            return true;
        }
        return false;
    }

    public void RemoveChildAt(int index)
    {
        var block = _children[index];
        block.Parent = null;
        _children.RemoveAt(index);
    }

    public void ReplaceChild(Block oldBlock, Block newBlock)
    {
        var index = _children.IndexOf(oldBlock);
        if (index < 0)
            throw new ArgumentException("oldBlock is not a child of this container.", nameof(oldBlock));
        oldBlock.Parent = null;
        newBlock.Parent = this;
        _children[index] = newBlock;
    }

    public void ClearChildren()
    {
        foreach (var child in _children)
            child.Parent = null;
        _children.Clear();
    }

    public int IndexOfChild(Block block) => _children.IndexOf(block);
}

public abstract class LeafBlock : Block
{
    public virtual ContainerInline? Inline { get; set; }
}
