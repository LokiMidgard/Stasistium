using Stasistium.Documents;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Toolkit.Parsers.Markdown;
using Stasistium.Stages;

namespace Stasistium
{

    public static class MarkdownStageExtensions
    {
        public static MarkdownStreamStage<T> Markdown<T>(this StageBase<Stream, T> input)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new MarkdownStreamStage<T>(input.DoIt, input.Context);
        }
        public static MarkdownStringStage<T> Markdown<T>(this StageBase<string, T> input)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new MarkdownStringStage<T>(input.DoIt, input.Context);
        }

        public static MarkdownToHtmlStage<T> MarkdownToHtml<T>(this StageBase<MarkdownDocument, T> input)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new MarkdownToHtmlStage<T>(input.DoIt, input.Context);
        }

    }

}
