namespace Kanawanagasaki.MarkdownEditor.Ast;

public enum ListType
{
    Unordered,
    Ordered
}

public class ListBlock : ContainerBlock
{
    public ListType ListType { get; set; } = ListType.Unordered;

    public int StartNumber { get; set; } = 1;

    public char BulletCharacter { get; set; } = '-';

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
