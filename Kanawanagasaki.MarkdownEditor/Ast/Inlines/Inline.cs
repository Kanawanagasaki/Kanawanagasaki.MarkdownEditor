namespace Kanawanagasaki.MarkdownEditor.Ast;

public abstract class Inline : MarkdownObject
{
    public Inline? PreviousSibling { get; internal set; }

    public Inline? NextSibling { get; internal set; }

    public ContainerInline? ParentInline { get; internal set; }

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

public abstract class ContainerInline : Inline
{
    public Inline? FirstChild { get; internal set; }

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

    public void AppendChild(Inline inline)
    {
        ArgumentNullException.ThrowIfNull(inline);

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

public abstract class LeafInline : Inline
{
}
