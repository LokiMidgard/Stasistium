﻿using Microsoft.Toolkit.Parsers.Markdown;
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


    public class MarkdownStreamStage<TPreviousCache> : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple1List0StageBase<Stream, TPreviousCache, MarkdownDocument>
        where TPreviousCache : class
    {
        public MarkdownStreamStage(StagePerformHandler<Stream, TPreviousCache> input, GeneratorContext context) : base(input, context)
        {
        }

        protected override async Task<IDocument<MarkdownDocument>> Work(IDocument<Stream> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            var document = new MarkdownDocument();
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
