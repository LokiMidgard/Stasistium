using Stasistium.Documents;
using Stasistium.Stages;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class SingleStage<TIn, TPreviousItemCache, TPreviousCache> : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple0List1StageBase<TIn, TPreviousItemCache, TPreviousCache, TIn>
        where TPreviousCache : class
        where TPreviousItemCache : class
    {
        public SingleStage(MultiStageBase<TIn, TPreviousItemCache, TPreviousCache> input, IGeneratorContext context, string? name = null) : base(input, context, name)
        {
        }

        protected override Task<IDocument<TIn>> Work(ImmutableList<IDocument<TIn>> result, OptionToken options)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));
            if (result.Count != 1)
                throw this.Context.Exception($"There should only be one Document but where {result.Count}");
            var element = result[0];
            return Task.FromResult(element);
        }
    }


}
namespace Stasistium
{


    public static partial class StageExtensions
    {

        public static SingleStage<TCheck, TPreviousItemCache, TPreviousCache> SingleEntry<TCheck, TPreviousItemCache, TPreviousCache>(this MultiStageBase<TCheck, TPreviousItemCache, TPreviousCache> input, string? name = null)
            where TPreviousCache : class
            where TPreviousItemCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new SingleStage<TCheck, TPreviousItemCache, TPreviousCache>(input, input.Context, name);
        }
    }
}