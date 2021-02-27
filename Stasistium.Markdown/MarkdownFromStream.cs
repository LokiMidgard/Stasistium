using AdaptMark.Parsers.Markdown;
using Stasistium.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Stasistium.Stages
{

    public class MarkdownFromStream : StageBaseSimple<Stream, MarkdownDocument>
    {
        private readonly Func<MarkdownDocument>? generateDocuement;

        [StageName("Markdown")]
        public MarkdownFromStream(IGeneratorContext context, Func<MarkdownDocument>? generateDocuement = null, string? name = null) : base(context, name)
        {
            this.generateDocuement = generateDocuement;
        }

        protected override async Task<IDocument<MarkdownDocument>> Work(IDocument<Stream> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            var document = this.generateDocuement?.Invoke() ?? new MarkdownDocument();
            string content;
            using (var stream = input.Value)
            using (var reader = new StreamReader(stream))
                content = await reader.ReadToEndAsync().ConfigureAwait(false);
            document.Parse(content);

            var hash = this.Context.GetHashForString(document.ToString());
            return input.With(document, hash);
        }

    }
}
