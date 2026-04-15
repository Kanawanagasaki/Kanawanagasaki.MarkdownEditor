namespace Kanawanagasaki.MarkdownEditor.Ast;

public readonly struct SourceSpan : IEquatable<SourceSpan>
{
    public static readonly SourceSpan Undefined = new(-1, -1);

    public int Start { get; }

    public int End { get; }

    public int Length => Start >= 0 && End >= Start ? End - Start + 1 : 0;

    public bool IsDefined => Start >= 0 && End >= Start;

    public SourceSpan(int start, int end)
    {
        Start = start;
        End = end;
    }

    public bool Equals(SourceSpan other) => Start == other.Start && End == other.End;

    public override bool Equals(object? obj) => obj is SourceSpan span && Equals(span);

    public override int GetHashCode() => HashCode.Combine(Start, End);

    public static bool operator ==(SourceSpan left, SourceSpan right) => left.Equals(right);

    public static bool operator !=(SourceSpan left, SourceSpan right) => !left.Equals(right);

    public override string ToString() => IsDefined ? $"[{Start}..{End}]" : "[undefined]";
}
