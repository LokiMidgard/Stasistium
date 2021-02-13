using Stasistium.Documents;
using System;
using System.Threading.Tasks;
using System.IO;
using Stasistium.Stages;

namespace Stasistium.Stages
{
    public class TextToStreamStage : StageBaseSimple<string, Stream>
    {
        public TextToStreamStage(IGeneratorContext context, string? name = null) : base(context, name)
        {
        }

        protected override Task<IDocument<Stream>> Work(IDocument<string> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            var output = input.With(() => new MemoryStream(System.Text.Encoding.UTF8.GetBytes(input.Value)), this.Context.GetHashForString(input.Value));
            return Task.FromResult<IDocument<Stream>>(output);
        }
    }

}
