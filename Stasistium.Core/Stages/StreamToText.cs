using Stasistium.Documents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Stasistium.Stages;

namespace Stasistium.Stages
{
    public class StreamToText<TInCache> : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple1List0StageBase<Stream, TInCache, string>
        where TInCache : class
    {
        public StreamToText(StagePerformHandler<Stream, TInCache> input, Encoding? encoding, IGeneratorContext context, string? name = null) : base(input, context, name)
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

namespace Stasistium
{
    public static partial class Stage
    {
        public static StreamToText<T> ToText<T>(this StageBase<Stream, T> input, Encoding? encoding = null, string? name = null)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new StreamToText<T>(input.DoIt, encoding, input.Context, name);
        }
    }
}
