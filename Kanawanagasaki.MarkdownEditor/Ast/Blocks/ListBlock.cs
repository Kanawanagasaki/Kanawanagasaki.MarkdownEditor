namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// Represents the type of a list — ordered or unordered.
/// </summary>
public enum ListType
{
    Unordered,
    Ordered
}

/// <summary>
/// A list block (ordered or unordered). A ContainerBlock whose children are ListItemBlocks.
/// </summary>
public class ListBlock : ContainerBlock
{
    /// <summary>
    /// Whether this list is ordered or unordered.
    /// </summary>
    public ListType ListType { get; set; } = ListType.Unordered;

    /// <summary>
    /// The ordered list start number (only relevant when ListType is Ordered).
    /// </summary>
    public int StartNumber { get; set; } = 1;

    /// <summary>
    /// The bullet character used for unordered lists (e.g., '-', '*', '+').
    /// </summary>
    public char BulletCharacter { get; set; } = '-';

    /// <summary>
    /// Whether the list is tight (no spacing between items) or loose.
    /// </summary>
    public bool IsTight { get; set; } = true;

    public ListBlock() { }

    public ListBlock(ListType listType)
    {
        ListType = listType;
    }

    public override void Accept(IMarkdownObjectVisitor visitor)
    {
        visitor.Visit(this);
        foreach (var child in Children)
            child.Accept(visitor);
    }
}
