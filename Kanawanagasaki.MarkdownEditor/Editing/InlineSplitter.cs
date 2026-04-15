using Kanawanagasaki.MarkdownEditor.Ast;

namespace Kanawanagasaki.MarkdownEditor.Editing;

/// <summary>
/// The result of splitting a LiteralInline at a character offset.
/// Produces up to three nodes: a left part (before the split), and a right part (after the split).
/// If the split is at the start or end, the corresponding part will be null.
/// </summary>
internal sealed class SplitResult
{
    /// <summary>The left portion of the split (may be null if split at offset 0).</summary>
    public LiteralInline? Left { get; }
    /// <summary>The right portion of the split (may be null if split at end).</summary>
    public LiteralInline? Right { get; }

    public SplitResult(LiteralInline? left, LiteralInline? right)
    {
        Left = left;
        Right = right;
    }
}

/// <summary>
/// Utility for splitting LiteralInline nodes at a given character offset
/// within the flattened inline text of a leaf block.
/// </summary>
internal static class InlineSplitter
{
    /// <summary>
    /// Splits the LiteralInline at the given relative offset within its content.
    /// The original inline is removed from the tree and replaced by up to two new inlines
    /// (left and right parts).
    /// </summary>
    /// <param name="literal">The LiteralInline to split.</param>
    /// <param name="relativeOffset">The offset within the literal's content to split at.</param>
    /// <returns>A SplitResult with left and right parts.</returns>
    public static SplitResult Split(LiteralInline literal, int relativeOffset)
    {
        var content = literal.Content;
        if (relativeOffset <= 0)
            return new SplitResult(null, literal);

        if (relativeOffset >= content.Length)
            return new SplitResult(literal, null);

        var leftContent = content[..relativeOffset];
        var rightContent = content[relativeOffset..];

        var left = new LiteralInline(leftContent);
        var right = new LiteralInline(rightContent);

        // Replace the original literal in the linked list with left then right
        ReplaceInline(literal, left);
        InsertAfterInline(left, right);

        return new SplitResult(left, right);
    }

    /// <summary>
    /// Replaces an inline node in the linked list with a new inline node.
    /// </summary>
    public static void ReplaceInline(Inline oldInline, Inline newInline)
    {
        // Update previous sibling
        if (oldInline.PreviousSibling != null)
        {
            oldInline.PreviousSibling.NextSibling = newInline;
            newInline.PreviousSibling = oldInline.PreviousSibling;
        }
        else if (oldInline.ParentInline != null)
        {
            oldInline.ParentInline.FirstChild = newInline;
            newInline.ParentInline = oldInline.ParentInline;
        }

        // Update next sibling
        if (oldInline.NextSibling != null)
        {
            oldInline.NextSibling.PreviousSibling = newInline;
            newInline.NextSibling = oldInline.NextSibling;
        }

        newInline.Parent = oldInline.Parent;

        // Detach old inline
        oldInline.PreviousSibling = null;
        oldInline.NextSibling = null;
        oldInline.ParentInline = null;
    }

    /// <summary>
    /// Inserts newInline right after targetInline in the linked list.
    /// </summary>
    public static void InsertAfterInline(Inline targetInline, Inline newInline)
    {
        newInline.PreviousSibling = targetInline;
        newInline.NextSibling = targetInline.NextSibling;
        newInline.ParentInline = targetInline.ParentInline;
        newInline.Parent = targetInline.Parent;

        if (targetInline.NextSibling != null)
            targetInline.NextSibling.PreviousSibling = newInline;

        targetInline.NextSibling = newInline;
    }

    /// <summary>
    /// Inserts newInline right before targetInline in the linked list.
    /// </summary>
    public static void InsertBeforeInline(Inline targetInline, Inline newInline)
    {
        newInline.NextSibling = targetInline;
        newInline.PreviousSibling = targetInline.PreviousSibling;
        newInline.ParentInline = targetInline.ParentInline;
        newInline.Parent = targetInline.Parent;

        if (targetInline.PreviousSibling != null)
            targetInline.PreviousSibling.NextSibling = newInline;
        else if (targetInline.ParentInline != null)
            targetInline.ParentInline.FirstChild = newInline;

        targetInline.PreviousSibling = newInline;
    }
}
