using Microsoft.Toolkit.Parsers.Markdown;
using StaticSite.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StaticSite.Stages
{


    public class MarkdownStreamStage<TPreviousCache> : SingleInputStageBase<MarkdownDocument, string, Stream, TPreviousCache>
    {
        public MarkdownStreamStage(StagePerformHandler<Stream, TPreviousCache> input, GeneratorContext context) : base(input, context)
        {
        }

        protected override async Task<(IDocument<MarkdownDocument> result, BaseCache<string> cache)> Work((IDocument<Stream> result, BaseCache<TPreviousCache> cache) input, bool previousHadChanges, OptionToken options)
        {
            var document = new MarkdownDocument();
            string content;
            using (var stream = input.result.Value)
            using (var reader = new StreamReader(stream))
                content = await reader.ReadToEndAsync().ConfigureAwait(false);
            document.Parse(content);

            var hash = this.Context.GetHashForString(document.ToString());
            return (input.result.With(document, hash), BaseCache.Create(hash, input.cache));
        }


    }
}
