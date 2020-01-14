using Stasistium.Documents;
using System;
using System.Threading.Tasks;
using Microsoft.Toolkit.Parsers.Markdown;
using System.Text;
using Blocks = Microsoft.Toolkit.Parsers.Markdown.Blocks;
using Inlines = Microsoft.Toolkit.Parsers.Markdown.Inlines;
using System.Collections.Generic;

namespace Stasistium.Stages
{
    public class MarkdownToHtmlStage<TInputCache> : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple1List0StageBase<MarkdownDocument, TInputCache, string>
        where TInputCache : class
    {
        public MarkdownToHtmlStage(StagePerformHandler<MarkdownDocument, TInputCache> inputSingle0, IGeneratorContext context, string? name) : base(inputSingle0, context,name )
        {
        }

        protected override Task<IDocument<string>> Work(IDocument<MarkdownDocument> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            var builder = new StringBuilder();
            this.Render(builder, input.Value.Blocks);

            var text = builder.ToString();

            return Task.FromResult(input.With(text, this.Context.GetHashForString(text)));
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
                    builder.Append("</table>");
                    break;

                case Blocks.YamlHeaderBlock yaml:
                    // we ignore YAML header. It shoud be handled somewhere else.
                    break;

                default:
                    this.Context.Warning($"Unsuported MarkdownBlock {block.GetType()}");
                    break;
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
                    break;

                case Inlines.ItalicTextInline italic:
                    builder.Append("<em>");
                    this.Render(builder, italic.Inlines);
                    builder.Append("</em>");
                    break;

                case Inlines.LinkAnchorInline code:
                    break;

                case Inlines.MarkdownLinkInline code:
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
                    this.Context.Warning($"Unsuported MarkdownInline {inline.GetType()}");
                    break;
            }
        }
    }

}
