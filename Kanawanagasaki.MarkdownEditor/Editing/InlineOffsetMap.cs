using System.Text;
using Kanawanagasaki.MarkdownEditor.Ast;

namespace Kanawanagasaki.MarkdownEditor.Editing;

/// <summary>
/// Internal helper that computes the linear character offset of each inline node
/// within a leaf block, by flattening the inline tree into an ordered sequence
/// of literal content segments.
/// </summary>
internal sealed class InlineOffsetMap
{
    /// <summary>
    /// An entry representing a contiguous run of characters contributed by a single inline node.
    /// </summary>
    internal sealed class Entry
    {
        /// <summary>The inline node that contributes these characters.</summary>
        public Inline Inline { get; }
        /// <summary>The start offset (inclusive) within the flattened text.</summary>
        public int Start { get; }
        /// <summary>The end offset (inclusive) within the flattened text.</summary>
        public int End { get; }
        /// <summary>The character offset within this inline's own text content that this entry starts at.</summary>
        public int ContentOffset { get; }
        /// <summary>The length of content contributed by this entry.</summary>
        public int Length => End - Start + 1;

        public Entry(Inline inline, int start, int end, int contentOffset)
        {
            Inline = inline;
            Start = start;
            End = end;
            ContentOffset = contentOffset;
        }
    }

    private readonly List<Entry> _entries = [];

    /// <summary>
    /// The total length of the flattened inline text.
    /// </summary>
    public int TotalLength { get; private set; }

    public IReadOnlyList<Entry> Entries => _entries;

    /// <summary>
    /// Builds the offset map by traversing the inline tree of a leaf block.
    /// </summary>
    public static InlineOffsetMap Build(LeafBlock block)
    {
        var map = new InlineOffsetMap();
        if (block.Inline is not null)
            map.BuildFromContainer(block.Inline);
        return map;
    }

    /// <summary>
    /// Gets the flattened text represented by this map.
    /// </summary>
    public string GetText()
    {
        var sb = new StringBuilder();
        foreach (var entry in _entries)
        {
            if (entry.Inline is LiteralInline lit)
                sb.Append(lit.Content.AsSpan(entry.ContentOffset, entry.Length));
            else if (entry.Inline is CodeInline code)
                sb.Append(code.Content.AsSpan(entry.ContentOffset, entry.Length));
        }
        return sb.ToString();
    }

    /// <summary>
    /// Finds all entries that overlap with the given [rangeStart, rangeEnd] character range.
    /// </summary>
    public List<Entry> GetEntriesInRange(int rangeStart, int rangeEnd)
    {
        var result = new List<Entry>();
        foreach (var entry in _entries)
        {
            if (entry.End < rangeStart) continue;
            if (entry.Start > rangeEnd) break;
            result.Add(entry);
        }
        return result;
    }

    /// <summary>
    /// Finds the entry that contains the given character offset, or null if none.
    /// </summary>
    public Entry? FindEntryAt(int offset)
    {
        foreach (var entry in _entries)
        {
            if (offset >= entry.Start && offset <= entry.End)
                return entry;
        }
        return null;
    }

    /// <summary>
    /// Finds the LiteralInline entry at a given offset.
    /// </summary>
    public Entry? FindLiteralAt(int offset)
    {
        var entry = FindEntryAt(offset);
        while (entry is not null && entry.Inline is not LiteralInline)
        {
            // Move to the next entry
            var idx = _entries.IndexOf(entry);
            if (idx + 1 < _entries.Count)
                entry = _entries[idx + 1];
            else
                entry = null;
        }
        return entry;
    }

    private void BuildFromContainer(ContainerInline container)
    {
        var child = container.FirstChild;
        while (child is not null)
        {
            BuildFromInline(child);
            child = child.NextSibling;
        }
    }

    private void BuildFromInline(Inline inline)
    {
        switch (inline)
        {
            case LiteralInline lit:
                if (lit.Content.Length > 0)
                {
                    var start = TotalLength;
                    TotalLength += lit.Content.Length;
                    _entries.Add(new Entry(lit, start, TotalLength - 1, 0));
                }
                break;
            case CodeInline code:
                if (code.Content.Length > 0)
                {
                    var start = TotalLength;
                    TotalLength += code.Content.Length;
                    _entries.Add(new Entry(code, start, TotalLength - 1, 0));
                }
                break;
            case ContainerInline container:
                BuildFromContainer(container);
                break;
            case LineBreakInline:
                // Line breaks contribute no visible characters to the inline text
                break;
            case AutolinkInline auto:
                if (auto.Url.Length > 0)
                {
                    var start = TotalLength;
                    TotalLength += auto.Url.Length;
                    _entries.Add(new Entry(auto, start, TotalLength - 1, 0));
                }
                break;
            case HtmlInline html:
                if (html.Content.Length > 0)
                {
                    var start = TotalLength;
                    TotalLength += html.Content.Length;
                    _entries.Add(new Entry(html, start, TotalLength - 1, 0));
                }
                break;
            case HtmlEntityInline entity:
                if (entity.Transcoded.Length > 0)
                {
                    var start = TotalLength;
                    TotalLength += entity.Transcoded.Length;
                    _entries.Add(new Entry(entity, start, TotalLength - 1, 0));
                }
                break;
        }
    }
}
