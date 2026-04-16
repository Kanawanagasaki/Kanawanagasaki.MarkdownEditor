namespace Kanawanagasaki.MarkdownEditor;

public enum TextChunkStyleKind
{
    Bold,
    Italic,
    Strikethrough,
    Code,
    Heading,
    OrderedList,
    UnorderedList,
    Blockquote,
    Link,
    Image
}

public sealed class TextChunkStyle
{
    public TextChunkStyleKind Kind { get; }
    public int? Level { get; init; }
    public int? ListItemIndex { get; init; }
    public int? NestingLevel { get; init; }
    public string? Url { get; init; }
    public string? Title { get; init; }

    private TextChunkStyle(TextChunkStyleKind kind) => Kind = kind;

    public static TextChunkStyle Bold() => new(TextChunkStyleKind.Bold);
    public static TextChunkStyle Italic() => new(TextChunkStyleKind.Italic);
    public static TextChunkStyle Strikethrough() => new(TextChunkStyleKind.Strikethrough);
    public static TextChunkStyle Code() => new(TextChunkStyleKind.Code);
    public static TextChunkStyle Heading(int level) => new(TextChunkStyleKind.Heading) { Level = level };
    public static TextChunkStyle OrderedList(int index) => new(TextChunkStyleKind.OrderedList) { ListItemIndex = index };
    public static TextChunkStyle UnorderedList() => new(TextChunkStyleKind.UnorderedList);
    public static TextChunkStyle Blockquote(int nestingLevel) => new(TextChunkStyleKind.Blockquote) { NestingLevel = nestingLevel };
    public static TextChunkStyle Link(string url, string? title = null) => new(TextChunkStyleKind.Link) { Url = url, Title = title };
    public static TextChunkStyle Image(string url, string? title = null) => new(TextChunkStyleKind.Image) { Url = url, Title = title };
}

public sealed class VisibleTextChunk
{
    public string Text { get; }
    public IReadOnlyList<TextChunkStyle> Styles { get; }

    public VisibleTextChunk(string text, IReadOnlyList<TextChunkStyle> styles)
    {
        Text = text ?? string.Empty;
        Styles = styles;
    }
}
