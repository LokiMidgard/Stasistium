using Stasistium.Documents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class JsonStage<TInCache, TOut> : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple1List0StageBase<Stream, TInCache, TOut>
        where TInCache : class
    {
        public JsonStage(StageBase<Stream, TInCache> input, IGeneratorContext context, string? name = null) : base(input, context, name)
        {
        }

        protected override Task<IDocument<TOut>> Work(IDocument<Stream> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            var ser = new Newtonsoft.Json.JsonSerializer();

            TOut json;
            using (var stream = input.Value)
            using (var textReader = new StreamReader(stream))
            using (var jsonReader = new Newtonsoft.Json.JsonTextReader(textReader))
                json = ser.Deserialize<TOut>(jsonReader);

            return Task.FromResult(input.With(json, this.Context.GetHashForObject(json)));
        }


    }


}
