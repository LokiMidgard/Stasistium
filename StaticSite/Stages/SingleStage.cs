using StaticSite.Documents;
using System;
using System.Threading.Tasks;

namespace StaticSite.Stages
{
    public class SingleStage<TIn, TPreviousItemCache, TPreviousCache> : OutputSingleInputSingle0List1StageBase<TIn, TPreviousItemCache, TPreviousCache, TIn, string>
    {
        public SingleStage(StagePerformHandler<TIn, TPreviousItemCache, TPreviousCache> input, GeneratorContext context) : base(input, context)
        {
        }


        protected override async Task<(IDocument<TIn> result, BaseCache<string> cache)> Work(StageResultList<TIn, TPreviousItemCache, TPreviousCache> inputList0, OptionToken options)
        {
            if (inputList0 is null)
                throw new ArgumentNullException(nameof(inputList0));
            var (result, cache) = await inputList0.Perform;

            if (result.Count != 1)
                throw this.Context.Exception($"There should only be one Document but where {result.Count}");
            var element = await result[0].Perform;
            return (element.result, BaseCache.Create(element.result.Hash, cache));
        }
    }


}
