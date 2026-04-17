namespace Kanawanagasaki.MarkdownEditor.Ast;

public interface IMarkdownObjectVisitor
{
    void Visit(MarkdownDocument doc);
    void Visit(ParagraphBlock block);
    void Visit(HeadingBlock block);
    void Visit(ListBlock block);
    void Visit(ListItemBlock block);
    void Visit(QuoteBlock block);
    void Visit(FencedCodeBlock block);
    void Visit(CodeBlock block);
    void Visit(ThematicBreakBlock block);
    void Visit(HtmlBlock block);
    void Visit(LiteralInline inline);
    void Visit(EmphasisInline inline);
    void Visit(CodeInline inline);
    void Visit(LinkInline inline);
    void Visit(AutolinkInline inline);
    void Visit(LineInline inline);
    void Visit(HtmlInline inline);
    void Visit(HtmlEntityInline inline);
    void Visit(InlineRoot root);
}
