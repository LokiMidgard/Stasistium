using Stasistium.Documents;
using System;
using System.Threading.Tasks;
using AdaptMark.Parsers.Markdown;
using System.Text;
using Blocks = AdaptMark.Parsers.Markdown.Blocks;
using Inlines = AdaptMark.Parsers.Markdown.Inlines;
using System.Collections.Generic;
using AdaptMark.Parsers.Markdown.Blocks;
using AdaptMark.Parsers.Markdown.Inlines;
using System.Linq;

namespace Stasistium.Stages
{
    public class MarkdownToHtmlStage<TInputCache> : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple1List0StageBase<MarkdownDocument, TInputCache, string>
        where TInputCache : class
    {
        public MarkdownRenderer Renderer { get; }

        public MarkdownToHtmlStage(StageBase<MarkdownDocument, TInputCache> inputSingle0, MarkdownRenderer? renderer, IGeneratorContext context, string? name) : base(inputSingle0, context, name)
        {
            this.Renderer = renderer ?? new MarkdownRenderer();
        }

        protected override Task<IDocument<string>> Work(IDocument<MarkdownDocument> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            var text = this.Renderer.Render(input.Value);
            return Task.FromResult(input.With(text, this.Context.GetHashForString(text)));
        }
    }

    public class MarkdownRenderer
    {

        public static string GetHeaderText(HeaderBlock headerBlock)
        {
            if (headerBlock is null)
                throw new ArgumentNullException(nameof(headerBlock));
            return ToText2(headerBlock.Inlines);

            static string ToText2(IEnumerable<MarkdownInline> inlines)
            {
                return string.Join(" ", inlines.Select(ToText));
            }

            static string ToText(MarkdownInline inline)
            {
                if (inline is TextRunInline textRun)
                    return textRun.Text;
                if (inline is BoldTextInline bold)
                    return ToText2(bold.Inlines);
                if (inline is ItalicTextInline italic)
                    return ToText2(italic.Inlines);
                if (inline is StrikethroughTextInline strikethrough)
                    return ToText2(strikethrough.Inlines);

                return inline.ToString()!;
            }
        }

        public string Render(MarkdownDocument document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));
            var builder = new StringBuilder();
            this.Render(builder, document.Blocks);
            var text = builder.ToString();
            return text;
        }

        protected void Render(StringBuilder builder, IEnumerable<Blocks.MarkdownBlock> blocks)
        {
            if (blocks is null)
                throw new ArgumentNullException(nameof(blocks));
            foreach (var block in blocks)
                this.Render(builder, block);
        }
        protected virtual void Render(StringBuilder builder, Blocks.MarkdownBlock block)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
            if (block is null)
                throw new ArgumentNullException(nameof(block));
            switch (block)
            {
                case Blocks.ParagraphBlock paragraph:
                    builder.Append("<p>");
                    foreach (var item in paragraph.Inlines)
                        this.Render(builder, item);
                    builder.Append("</p>");
                    break;

                case Blocks.CodeBlock code:
                    builder.Append("<pre>");
                    builder.Append(System.Web.HttpUtility.HtmlEncode(code.Text));
                    builder.Append("</pre>");
                    break;

                case Blocks.HeaderBlock header:
                    builder.Append("<h");
                    builder.Append(header.HeaderLevel.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    var id = GetHeaderText(header).Replace(' ', '-');
                    if (id.Length > 0)
                    {
                        builder.Append(" id=\"");
                        builder.Append(id);
                        builder.Append("\" ");
                    }
                    builder.Append(">");

                    foreach (var item in header.Inlines)
                        this.Render(builder, item);
                    builder.Append("</h");
                    builder.Append(header.HeaderLevel.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    builder.Append(">");
                    break;

                case Blocks.HorizontalRuleBlock hr:
                    builder.Append("<hr/>");
                    break;

                case Blocks.ListBlock list:
                    var listStyle = list.Style == ListStyle.Bulleted ? "ul" : "ol";

                    builder.Append("<");
                    builder.Append(listStyle);
                    builder.Append(">");

                    foreach (var item in list.Items)
                    {
                        builder.Append("<li>");
                        this.Render(builder, item.Blocks);
                        builder.Append("</li>");
                    }

                    builder.Append("</");
                    builder.Append(listStyle);
                    builder.Append(">");
                    break;

                case Blocks.QuoteBlock quote:

                    builder.Append("<blockquote>");

                    this.Render(builder, quote.Blocks);
                    builder.Append("</blockquote>");
                    break;

                case Blocks.TableBlock table:
                    builder.Append("<table>");

                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        builder.Append("<tr>");
                        for (int j = 0; j < table.Rows[i].Cells.Count; j++)
                        {
                            builder.Append("<td>");
                            this.Render(builder, table.Rows[i].Cells[j].Inlines);
                            builder.Append("</td>");
                        }
                        builder.Append("</tr>");
                    }

                    builder.Append("</table>");
                    break;

                case Blocks.YamlHeaderBlock yaml:
                    // we ignore YAML header. It shoud be handled somewhere else.
                    break;

                default:
                    throw new NotSupportedException($"Unsuported MarkdownBlock {block.GetType()}");
            }


        }

        protected void Render(StringBuilder builder, IEnumerable<Inlines.MarkdownInline> inlines)
        {
            if (inlines is null)
                throw new ArgumentNullException(nameof(inlines));
            foreach (var inline in inlines)
                this.Render(builder, inline);
        }
        protected virtual void Render(StringBuilder builder, Inlines.MarkdownInline inline)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
            if (inline is null)
                throw new ArgumentNullException(nameof(inline));
            switch (inline)
            {
                case Inlines.BoldTextInline bold:
                    builder.Append("<b>");
                    this.Render(builder, bold.Inlines);
                    builder.Append("</b>");
                    break;

                case Inlines.CodeInline code:
                    builder.Append("<code>");
                    builder.Append(System.Web.HttpUtility.HtmlEncode(code.Text));
                    builder.Append("</code>");
                    break;

                case Inlines.EmojiInline emoji:
                    builder.Append(emoji.Text);
                    break;

                case Inlines.HyperlinkInline hyperlink:
                    builder.Append("<a href=\"");
                    builder.Append(hyperlink.Url);
                    builder.Append("\" >");
                    builder.Append(hyperlink.Text);
                    builder.Append("</a>");
                    break;

                case Inlines.ImageInline image:

                    builder.Append("<img src=\"");
                    builder.Append(image.Url);
                    builder.Append("\"");
                    if (image.ImageHeight > 0)
                    {
                        builder.Append("height=\"");
                        builder.Append(image.ImageHeight);
                        builder.Append("\"");
                    }
                    if (image.ImageWidth > 0)
                    {
                        builder.Append("width=\"");
                        builder.Append(image.ImageWidth);
                        builder.Append("\"");
                    }

                    builder.Append(" />");

                    break;

                case Inlines.ItalicTextInline italic:
                    builder.Append("<em>");
                    this.Render(builder, italic.Inlines);
                    builder.Append("</em>");
                    break;

                case Inlines.LinkAnchorInline code:
                    break;

                case Inlines.MarkdownLinkInline hyperlink:
                    if (hyperlink.Url != null)
                    {

                        builder.Append("<a href=\"");
                        builder.Append(hyperlink.Url);
                        builder.Append("\" ");
                        if (hyperlink.Tooltip != null)
                        {
                            builder.Append("alt=\"");
                            builder.Append(hyperlink.Tooltip);
                            builder.Append("\" ");
                        }
                        builder.Append(" >");
                        this.Render(builder, hyperlink.Inlines);
                        builder.Append("</a>");
                    }
                    break;

                case Inlines.StrikethroughTextInline strike:
                    builder.Append("<s>");
                    this.Render(builder, strike.Inlines);
                    builder.Append("</s>");
                    break;

                case Inlines.SubscriptTextInline sub:
                    builder.Append("<sub>");
                    this.Render(builder, sub.Inlines);
                    builder.Append("</sub>");
                    break;

                case Inlines.SuperscriptTextInline sup:
                    builder.Append("<sup>");
                    this.Render(builder, sup.Inlines);
                    builder.Append("</sup>");
                    break;

                case Inlines.TextRunInline text:
                    builder.Append(text.Text);
                    break;

                default:
                    throw new NotSupportedException($"Unsuported MarkdownInline {inline.GetType()}");
            }
        }

    }

}
