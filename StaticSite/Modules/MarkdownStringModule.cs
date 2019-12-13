using Microsoft.Toolkit.Parsers.Markdown;
using StaticSite.Documents;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StaticSite.Modules
{
    public class MarkdownStringModule<TPreviousCache> : SingleInputModuleBase<MarkdownDocument, string, string, TPreviousCache>
    {
        public MarkdownStringModule(ModulePerformHandler<string, TPreviousCache> input, GeneratorContext context) : base(input, context)
        {
        }

        protected override Task<(IDocument<MarkdownDocument> result, BaseCache<string> cache)> Work((IDocument<string> result, BaseCache<TPreviousCache> cache) input, bool previousHadChanges, OptionToken options)
        {
            var document = new MarkdownDocument();
            document.Parse(input.result.Value);

            var hash = this.Context.GetHashForString(document.ToString());
            return Task.FromResult((input.result.With(document, hash), BaseCache.Create(hash, input.cache)));
        }


    }
}
