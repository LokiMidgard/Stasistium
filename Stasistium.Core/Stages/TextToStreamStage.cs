using Stasistium.Documents;
using System;
using System.Threading.Tasks;
using System.IO;

namespace Stasistium.Stages
{
    public class TextToStreamStage<TInputCache> : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple1List0StageBase<string, TInputCache, Stream>
        where TInputCache : class
    {
        public TextToStreamStage(StagePerformHandler<string, TInputCache> inputSingle0, GeneratorContext context) : base(inputSingle0, context)
        {
        }

        protected override Task<IDocument<Stream>> Work(IDocument<string> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            var output = input.With(() => new MemoryStream(System.Text.Encoding.UTF8.GetBytes(input.Value)), input.Hash);
            return Task.FromResult<IDocument<Stream>>(output);
        }
    }

}
