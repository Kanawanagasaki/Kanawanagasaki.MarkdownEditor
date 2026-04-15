using Kanawanagasaki.MarkdownEditor.Ast;

namespace Kanawanagasaki.MarkdownEditor.Editing;

/// <summary>
/// Result of applying a range-based operation within a leaf block's inline tree.
/// Contains references to the split boundaries so the caller can manipulate
/// the inline nodes between them.
/// </summary>
internal sealed class RangeSplitResult
{
    /// <summary>The inline node at or just before the start boundary (after splitting).</summary>
    public Inline? StartBoundary { get; }
    /// <summary>The inline node at or just after the end boundary (after splitting).</summary>
    public Inline? EndBoundary { get; }
    /// <summary>The leaf block that was operated on.</summary>
    public LeafBlock Block { get; }
    /// <summary>The offset map used for this operation.</summary>
    public InlineOffsetMap OffsetMap { get; }

    public RangeSplitResult(Inline? startBoundary, Inline? endBoundary, LeafBlock block, InlineOffsetMap offsetMap)
    {
        StartBoundary = startBoundary;
        EndBoundary = endBoundary;
        Block = block;
        OffsetMap = offsetMap;
    }

    /// <summary>
    /// Collects all inline nodes between StartBoundary and EndBoundary (inclusive),
    /// at the same sibling level. Returns the inlines that are direct siblings
    /// in the linked list between the two boundaries.
    /// </summary>
    public List<Inline> GetInlinesInRange()
    {
        var result = new List<Inline>();
        if (StartBoundary is null || EndBoundary is null)
            return result;

        var current = StartBoundary;
        while (current is not null)
        {
            result.Add(current);
            if (ReferenceEquals(current, EndBoundary))
                break;
            current = current.NextSibling;
        }
        return result;
    }
}
