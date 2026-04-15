using Kanawanagasaki.MarkdownEditor.Ast;

namespace Kanawanagasaki.MarkdownEditor.Editing;

internal sealed class RangeSplitResult
{
    public Inline? StartBoundary { get; }
    public Inline? EndBoundary { get; }
    public LeafBlock Block { get; }
    public InlineOffsetMap OffsetMap { get; }

    public RangeSplitResult(Inline? startBoundary, Inline? endBoundary, LeafBlock block, InlineOffsetMap offsetMap)
    {
        StartBoundary = startBoundary;
        EndBoundary = endBoundary;
        Block = block;
        OffsetMap = offsetMap;
    }

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
