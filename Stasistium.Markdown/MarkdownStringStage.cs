using Microsoft.Toolkit.Parsers.Markdown;
using Stasistium.Documents;
using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class MarkdownStringStage<TPreviousCache> : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple1List0StageBase<string, TPreviousCache, MarkdownDocument>
        where TPreviousCache : class
    {
        public MarkdownStringStage(StagePerformHandler<string, TPreviousCache> input, GeneratorContext context) : base(input, context)
        {
        }

        protected override Task<IDocument<MarkdownDocument>> Work(IDocument<string> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            var document = new MarkdownDocument();
            document.Parse(input.Value);

            var hash = this.Context.GetHashForString(document.ToString());
            return Task.FromResult(input.With(document, hash));
        }

    }
}
