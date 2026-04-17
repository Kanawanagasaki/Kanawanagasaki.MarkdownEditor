namespace Kanawanagasaki.MarkdownEditor.Ast;

public class ParagraphBlock : LeafBlock
{
    public List<LineInline> Lines { get; } = [];

    public bool HasTrailingLineBreak { get; set; }

    public override ContainerInline? Inline
    {
        get => BuildMergedInline();
        set => ParseInlineToLines(value);
    }

    private InlineRoot? _cachedInline;
    private bool _inlineDirty = true;

    public void MarkInlineDirty() => _inlineDirty = true;

    public override void Accept(IMarkdownObjectVisitor visitor)
    {
        visitor.Visit(this);
        Inline?.Accept(visitor);
    }

    public LineInline GetOrCreateLine()
    {
        SyncFromMergedIfNeeded();
        if (Lines.Count == 0)
        {
            var line = new LineInline();
            Lines.Add(line);
            return line;
        }
        return Lines[^1];
    }

    public void SyncFromMergedIfNeeded()
    {
        bool allLinesEmpty = Lines.Count == 0 || Lines.All(l => l.IsEmpty);
        if (allLinesEmpty && _cachedInline is not null && _cachedInline.FirstChild is not null)
        {
            RebuildLinesFromMerged(_cachedInline);
            _inlineDirty = true;
        }
    }

    private InlineRoot BuildMergedInline()
    {
        SyncFromMergedIfNeeded();

        if (!_inlineDirty && _cachedInline is not null)
            return _cachedInline;

        var root = new InlineRoot();

        foreach (var line in Lines)
        {
            root.AppendChild(line);
        }

        _cachedInline = root;
        _inlineDirty = false;
        return root;
    }

    private void ParseInlineToLines(ContainerInline? value)
    {
        Lines.Clear();
        HasTrailingLineBreak = false;
        _inlineDirty = true;
        _cachedInline = null;

        if (value is null) return;

        var child = value.FirstChild;
        while (child is not null)
        {
            var next = child.NextSibling;
            child.Remove();
            if (child is LineInline lineInline)
            {
                Lines.Add(lineInline);
            }
            else
            {
                if (Lines.Count == 0)
                    Lines.Add(new LineInline());
                Lines[^1].AppendChild(child);
            }
            child = next;
        }
    }

    private void RebuildLinesFromMerged(InlineRoot merged)
    {
        Lines.Clear();
        HasTrailingLineBreak = false;

        var child = merged.FirstChild;
        while (child is not null)
        {
            var next = child.NextSibling;
            child.Remove();
            if (child is LineInline lineInline)
            {
                Lines.Add(lineInline);
            }
            else
            {
                if (Lines.Count == 0)
                    Lines.Add(new LineInline());
                Lines[^1].AppendChild(child);
            }
            child = next;
        }
    }
}
