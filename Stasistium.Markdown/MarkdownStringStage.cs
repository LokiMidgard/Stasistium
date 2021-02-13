using AdaptMark.Parsers.Markdown;
using Stasistium.Documents;
using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class MarkdownStringStage : StageBaseSimple<string, MarkdownDocument>
    {
        private readonly Func<MarkdownDocument>? generateDocuement;

        public MarkdownStringStage(Func<MarkdownDocument>? generateDocuement, IGeneratorContext context, string? name) : base(context, name)
        {
            this.generateDocuement = generateDocuement;
        }

        protected override Task<IDocument<MarkdownDocument>> Work(IDocument<string> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            var document = this.generateDocuement?.Invoke() ?? new MarkdownDocument();
            document.Parse(input.Value);

            var hash = this.Context.GetHashForString(document.ToString());
            return Task.FromResult(input.With(document, hash));
        }

    }
}
