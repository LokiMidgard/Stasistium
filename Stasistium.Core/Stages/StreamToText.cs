using Stasistium.Documents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Stasistium.Stages;

namespace Stasistium.Stages
{
    public class StreamToText : StageBaseSimple<Stream, string>
    {
        public StreamToText(IGeneratorContext context, Encoding? encoding = null, string? name = null) : base(context, name)
        {
            this.Encoding = encoding ?? Encoding.UTF8;
        }

        public Encoding Encoding { get; }

        protected override async Task<IDocument<string>> Work(IDocument<Stream> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            using var stream = input.Value;
            using var reader = new StreamReader(stream, this.Encoding);
            var text = await reader.ReadToEndAsync().ConfigureAwait(false);
            return input.With(text, this.Context.GetHashForString(text));
        }
    }
}
