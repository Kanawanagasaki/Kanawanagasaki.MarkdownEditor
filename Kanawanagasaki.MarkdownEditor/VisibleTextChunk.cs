using Kanawanagasaki.MarkdownEditor.Ast;

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
    public MarkdownObject? Node { get; }
    public int? Level { get; init; }
    public int? ListItemIndex { get; init; }
    public int? NestingLevel { get; init; }
    public string? Url { get; init; }
    public string? Title { get; init; }

    private TextChunkStyle(TextChunkStyleKind kind, MarkdownObject? node) => (Kind, Node) = (kind, node);

    public static TextChunkStyle Bold(MarkdownObject? node = null) => new(TextChunkStyleKind.Bold, node);
    public static TextChunkStyle Italic(MarkdownObject? node = null) => new(TextChunkStyleKind.Italic, node);
    public static TextChunkStyle Strikethrough(MarkdownObject? node = null) => new(TextChunkStyleKind.Strikethrough, node);
    public static TextChunkStyle Code(MarkdownObject? node = null) => new(TextChunkStyleKind.Code, node);
    public static TextChunkStyle Heading(int level, MarkdownObject? node = null) => new(TextChunkStyleKind.Heading, node) { Level = level };
    public static TextChunkStyle OrderedList(int index, MarkdownObject? node = null) => new(TextChunkStyleKind.OrderedList, node) { ListItemIndex = index };
    public static TextChunkStyle UnorderedList(MarkdownObject? node = null) => new(TextChunkStyleKind.UnorderedList, node);
    public static TextChunkStyle Blockquote(int nestingLevel, MarkdownObject? node = null) => new(TextChunkStyleKind.Blockquote, node) { NestingLevel = nestingLevel };
    public static TextChunkStyle Link(string url, string? title = null, MarkdownObject? node = null) => new(TextChunkStyleKind.Link, node) { Url = url, Title = title };
    public static TextChunkStyle Image(string url, string? title = null, MarkdownObject? node = null) => new(TextChunkStyleKind.Image, node) { Url = url, Title = title };
}

public sealed class VisibleTextChunk
{
    public string Text { get; }
    public int Start { get; }
    public int End { get; }
    public IReadOnlyList<TextChunkStyle> Styles { get; }

    public VisibleTextChunk(string text, int start, IReadOnlyList<TextChunkStyle> styles)
    {
        Text = text ?? string.Empty;
        Start = start;
        End = start + Text.Length;
        Styles = styles;
    }
}
