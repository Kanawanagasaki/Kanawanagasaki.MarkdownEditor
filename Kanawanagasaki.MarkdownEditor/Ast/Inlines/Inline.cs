namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// Base class for all inline-level AST nodes.
/// Inlines are stored as a doubly-linked list within their parent container.
/// </summary>
public abstract class Inline : MarkdownObject
{
    /// <summary>
    /// The previous sibling in the doubly-linked list, or null if this is the first.
    /// </summary>
    public Inline? PreviousSibling { get; internal set; }

    /// <summary>
    /// The next sibling in the doubly-linked list, or null if this is the last.
    /// </summary>
    public Inline? NextSibling { get; internal set; }

    /// <summary>
    /// The parent container inline, or null if this inline is at the root level of a LeafBlock.
    /// </summary>
    public ContainerInline? ParentInline { get; internal set; }

    /// <summary>
    /// Inserts this inline before the specified sibling in the linked list.
    /// </summary>
    internal void InsertBefore(Inline sibling)
    {
        if (sibling.PreviousSibling != null)
        {
            sibling.PreviousSibling.NextSibling = this;
            PreviousSibling = sibling.PreviousSibling;
        }
        else if (sibling.ParentInline != null)
        {
            sibling.ParentInline.FirstChild = this;
        }

        NextSibling = sibling;
        sibling.PreviousSibling = this;
    }

    /// <summary>
    /// Inserts this inline after the specified sibling in the linked list.
    /// </summary>
    internal void InsertAfter(Inline sibling)
    {
        if (sibling.NextSibling != null)
        {
            sibling.NextSibling.PreviousSibling = this;
            NextSibling = sibling.NextSibling;
        }

        PreviousSibling = sibling;
        sibling.NextSibling = this;
    }

    /// <summary>
    /// Removes this inline from the doubly-linked list.
    /// </summary>
    public void Remove()
    {
        if (PreviousSibling != null)
            PreviousSibling.NextSibling = NextSibling;
        else if (ParentInline != null)
            ParentInline.FirstChild = NextSibling;

        if (NextSibling != null)
            NextSibling.PreviousSibling = PreviousSibling;

        PreviousSibling = null;
        NextSibling = null;
        ParentInline = null;
    }
}

/// <summary>
/// An inline that can contain other child inlines.
/// Examples: EmphasisInline, LinkInline.
/// Inlines form a doubly-linked list; a ContainerInline owns the first child
/// and children are linked via PreviousSibling/NextSibling.
/// </summary>
public abstract class ContainerInline : Inline
{
    /// <summary>
    /// The first child inline in this container's children linked list.
    /// </summary>
    public Inline? FirstChild { get; internal set; }

    /// <summary>
    /// Gets the last child inline by traversing from FirstChild.
    /// </summary>
    public Inline? LastChild
    {
        get
        {
            var child = FirstChild;
            if (child is null) return null;
            while (child.NextSibling is not null)
                child = child.NextSibling;
            return child;
        }
    }

    /// <summary>
    /// Appends a child inline to the end of this container's children list.
    /// </summary>
    public void AppendChild(Inline inline)
    {
        ArgumentNullException.ThrowIfNull(inline);

        // Remove from previous position
        inline.Remove();

        inline.ParentInline = this;
        inline.Parent = this;

        if (FirstChild is null)
        {
            FirstChild = inline;
        }
        else
        {
            var last = LastChild!;
            last.NextSibling = inline;
            inline.PreviousSibling = last;
        }
    }

    /// <summary>
    /// Inserts a child inline at the beginning of this container's children list.
    /// </summary>
    public void PrependChild(Inline inline)
    {
        ArgumentNullException.ThrowIfNull(inline);

        inline.Remove();

        inline.ParentInline = this;
        inline.Parent = this;

        if (FirstChild is not null)
        {
            FirstChild.PreviousSibling = inline;
            inline.NextSibling = FirstChild;
        }

        FirstChild = inline;
    }

    /// <summary>
    /// Removes all children from this container.
    /// </summary>
    public void ClearChildren()
    {
        var child = FirstChild;
        while (child is not null)
        {
            var next = child.NextSibling;
            child.PreviousSibling = null;
            child.NextSibling = null;
            child.ParentInline = null;
            child = next;
        }
        FirstChild = null;
    }

    /// <summary>
    /// Returns an enumeration of all child inlines in order.
    /// </summary>
    public IEnumerable<Inline> GetChildren()
    {
        var child = FirstChild;
        while (child is not null)
        {
            yield return child;
            child = child.NextSibling;
        }
    }
}

/// <summary>
/// An inline with no children (a leaf in the inline tree).
/// Examples: LiteralInline, CodeInline.
/// </summary>
public abstract class LeafInline : Inline
{
}
