using Stasistium.Documents;
using Stasistium.Stages;
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
namespace Stasistium
{


    public static partial class StageExtensions
    {




        public class JsonHelper<TInCache>
            where TInCache : class
        {
            private readonly StageBase<Stream, TInCache> input;
            private readonly string? name;

            internal JsonHelper(StageBase<Stream, TInCache> input, string? name)
            {
                this.input = input;
                this.name = name;
            }

            public JsonStage<TInCache, TOut> For<TOut>()
            {
                return new JsonStage<TInCache, TOut>(this.input, this.input.Context, this.name);
            }
        }
        public static JsonHelper<TInCache> Json<TInCache>(this StageBase<Stream, TInCache> input, string? name = null)
            where TInCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new JsonHelper<TInCache>(input, name);
        }
    }
}