using Stasistium.Documents;
using Stasistium.Stages;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class ToListStage<TIn, TPreviousCache> : GeneratedHelper.Multiple.Simple.OutputMultiSimpleInputSingle1List0StageBase<TIn, TPreviousCache, TIn>
    where TPreviousCache : class
    {
        public ToListStage(StageBase<TIn, TPreviousCache> input, IGeneratorContext context, string? name = null) : base(input, context, name)
        {
        }

        protected override Task<ImmutableList<IDocument<TIn>>> Work(IDocument<TIn> inputSingle0, OptionToken options)
        {
            return Task.FromResult(ImmutableList.Create(inputSingle0));
        }
    }


}
namespace Stasistium
{


    public static partial class StageExtensions
    {

        public static ToListStage<TCheck, TPreviousCache> ToList<TCheck, TPreviousCache>(this StageBase<TCheck, TPreviousCache> input, string? name = null)
            where TPreviousCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new ToListStage<TCheck, TPreviousCache>(input, input.Context, name);
        }
    }
}