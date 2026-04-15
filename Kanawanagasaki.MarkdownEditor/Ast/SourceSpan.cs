namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// Represents a span of text in the source document by its start and end positions (inclusive).
/// </summary>
public readonly struct SourceSpan : IEquatable<SourceSpan>
{
    /// <summary>
    /// An empty/undefined span.
    /// </summary>
    public static readonly SourceSpan Undefined = new(-1, -1);

    /// <summary>
    /// Start position in the source text (inclusive).
    /// </summary>
    public int Start { get; }

    /// <summary>
    /// End position in the source text (inclusive).
    /// </summary>
    public int End { get; }

    /// <summary>
    /// Length of the span. Returns 0 if the span is undefined.
    /// </summary>
    public int Length => Start >= 0 && End >= Start ? End - Start + 1 : 0;

    /// <summary>
    /// Whether this span is defined (has valid start and end positions).
    /// </summary>
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
