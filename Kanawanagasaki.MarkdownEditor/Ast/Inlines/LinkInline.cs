namespace Kanawanagasaki.MarkdownEditor.Ast;

/// <summary>
/// A link or image inline. A ContainerInline that wraps child inlines as the link text
/// and has Url/Title metadata.
/// </summary>
public class LinkInline : ContainerInline
{
    /// <summary>
    /// The URL that the link points to.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Optional title attribute for the link.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Whether this is an image link (![...]) rather than a regular link ([...]).
    /// </summary>
    public bool IsImage { get; set; }

    /// <summary>
    /// Whether this is a reference-style link (has a label instead of inline URL).
    /// </summary>
    public bool IsReference { get; set; }

    /// <summary>
    /// The reference label for reference-style links.
    /// </summary>
    public string? ReferenceLabel { get; set; }

    public LinkInline() { }

    public LinkInline(string url, bool isImage = false)
    {
        Url = url;
        IsImage = isImage;
    }

    public override void Accept(IMarkdownObjectVisitor visitor)
    {
        visitor.Visit(this);
        foreach (var child in GetChildren())
            child.Accept(visitor);
    }

    public override string ToString()
    {
        var prefix = IsImage ? "!" : "";
        var inner = string.Join("", GetChildren().Select(c => c.ToString()));
        var titlePart = !string.IsNullOrEmpty(Title) ? $" \"{Title}\"" : "";
        return $"{prefix}[{inner}]({Url}{titlePart})";
    }
}
