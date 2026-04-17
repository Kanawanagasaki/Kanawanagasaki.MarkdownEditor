namespace Kanawanagasaki.MarkdownEditor;

public class EditRangeResult
{
    public int StartOffset { get; init; }
    public int EndOffset { get; init; }
    public int LineStartIndex { get; init; }
    public int LineEndIndex { get; init; }

    public override string ToString() =>
        $"[{StartOffset}..{EndOffset}) Lines [{LineStartIndex}..{LineEndIndex}]";
}
