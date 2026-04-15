using Kanawanagasaki.MarkdownEditor.Ast;

namespace Kanawanagasaki.MarkdownEditor.Editing;

internal sealed class SplitResult
{
    public LiteralInline? Left { get; }
    public LiteralInline? Right { get; }

    public SplitResult(LiteralInline? left, LiteralInline? right)
    {
        Left = left;
        Right = right;
    }
}

internal static class InlineSplitter
{
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

        ReplaceInline(literal, left);
        InsertAfterInline(left, right);

        return new SplitResult(left, right);
    }

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

        if (oldInline.NextSibling != null)
        {
            oldInline.NextSibling.PreviousSibling = newInline;
            newInline.NextSibling = oldInline.NextSibling;
        }

        newInline.Parent = oldInline.Parent;

        oldInline.PreviousSibling = null;
        oldInline.NextSibling = null;
        oldInline.ParentInline = null;
    }

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
