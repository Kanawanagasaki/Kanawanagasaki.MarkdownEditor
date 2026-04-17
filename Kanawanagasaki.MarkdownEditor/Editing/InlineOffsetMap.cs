namespace Kanawanagasaki.MarkdownEditor.Editing;

using System.Text;
using Kanawanagasaki.MarkdownEditor.Ast;

internal sealed class InlineOffsetMap
{
    internal sealed class Entry
    {
        public Inline Inline { get; }
        public int Start { get; }
        public int End { get; }
        public int ContentOffset { get; }
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

    public int TotalLength { get; private set; }

    public IReadOnlyList<Entry> Entries => _entries;

    public static InlineOffsetMap Build(LeafBlock block)
    {
        var map = new InlineOffsetMap();
        if (block.Inline is not null)
            map.BuildFromContainer(block.Inline);
        return map;
    }

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

    public Entry? FindEntryAt(int offset)
    {
        foreach (var entry in _entries)
        {
            if (offset >= entry.Start && offset <= entry.End)
                return entry;
        }
        return null;
    }

    public Entry? FindLiteralAt(int offset)
    {
        var entry = FindEntryAt(offset);
        while (entry is not null && entry.Inline is not LiteralInline)
        {
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
