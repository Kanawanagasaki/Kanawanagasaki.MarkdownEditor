namespace Kanawanagasaki.MarkdownEditor.Ast;

public class LinkInline : ContainerInline
{
    public string Url { get; set; } = string.Empty;

    public string? Title { get; set; }

    public bool IsImage { get; set; }

    public bool IsReference { get; set; }

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
