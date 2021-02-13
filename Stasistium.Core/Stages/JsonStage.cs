using Stasistium.Documents;
using Stasistium.Stages;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class JsonStage<TOut> : StageBaseSimple<Stream, TOut?>
        where TOut : class
    {
        public JsonStage(IGeneratorContext context, string? name = null) : base(context, name)
        {
        }

        protected override Task<IDocument<TOut?>> Work(IDocument<Stream> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            var ser = new Newtonsoft.Json.JsonSerializer();

            TOut? json;
            using (var stream = input.Value)
            using (var textReader = new StreamReader(stream))
            using (var jsonReader = new Newtonsoft.Json.JsonTextReader(textReader))
                json = ser.Deserialize<TOut?>(jsonReader);

            var document = input.With(json, this.Context.GetHashForObject(json));
            return Task.FromResult(document);
        }
    }
}
