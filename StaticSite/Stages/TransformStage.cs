using StaticSite.Documents;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace StaticSite.Stages
{
    public class TransformStage<TIn, TInItemCache, TInCache, TOut> : MultiStageBase<TOut, string, TransformStageCache<TInCache>>
        where TInCache : class
        where TInItemCache : class
    {
        private readonly StagePerformHandler<TIn, TInItemCache, TInCache> input;
        private readonly Func<IDocument<TIn>, Task<IDocument<TOut>>> transform;

        public TransformStage(StagePerformHandler<TIn, TInItemCache, TInCache> input, Func<IDocument<TIn>, Task<IDocument<TOut>>> selector, GeneratorContext context) : base(context)
        {
            this.input = input;
            this.transform = selector;
        }

        protected override async Task<StageResultList<TOut, string, TransformStageCache<TInCache>>> DoInternal([AllowNull] TransformStageCache<TInCache>? cache, OptionToken options)
        {

            var input = await this.input(cache?.ParentCache, options).ConfigureAwait(false);

            var task = LazyTask.Create(async () =>
            {

                var inputList = await input.Perform;


                var list = await Task.WhenAll(inputList.result.Select(async subInput =>
                {

                    if (subInput.HasChanges)
                    {
                        var subResult = await subInput.Perform;
                        var transformed = await this.transform(subResult.result).ConfigureAwait(false);
                        bool hasChanges = true;
                        if (cache != null && cache.Transformed.TryGetValue(transformed.Id, out var oldHash))
                            hasChanges = oldHash == transformed.Hash;

                        return (result: StageResult.Create(transformed, transformed.Hash, hasChanges, transformed.Id), inputId: subInput.Id, outputHash: transformed.Hash);
                    }
                    else
                    {
                        if (cache == null || !cache.InputToOutputId.TryGetValue(subInput.Id, out var oldOutputId) || !cache.Transformed.TryGetValue(subInput.Id, out var oldOutputHash))
                            throw this.Context.Exception("No changes, so old value should be there.");

                        return (result: StageResult.Create(LazyTask.Create(async () =>
                        {

                            var newSource = await subInput.Perform;
                            var transformed = await this.transform(newSource.result).ConfigureAwait(false);

                            return (transformed, transformed.Hash);
                        }), false, oldOutputId),
                        inputId: subInput.Id,
                        outputHash: oldOutputHash
                        );

                    }
                })).ConfigureAwait(false);

                var newCache = new TransformStageCache<TInCache>()
                {
                    InputToOutputId = list.ToDictionary(x => x.inputId, x => x.result.Id),
                    OutputIdOrder = list.Select(x => x.result.Id).ToArray(),
                    ParentCache = inputList.cache,
                    Transformed = list.ToDictionary(x => x.result.Id, x => x.outputHash)
                };
                return (result: list.Select(x => x.result).ToImmutableList(), cache: newCache);
            });

            bool hasChanges = input.HasChanges;
            var newCache = cache;
            if (input.HasChanges || newCache == null)
            {

                var (list, c) = await task;
                newCache = c;


                if (!hasChanges && list.Count != cache?.OutputIdOrder.Length)
                    hasChanges = true;

                if (!hasChanges && cache != null)
                {
                    for (int i = 0; i < cache.OutputIdOrder.Length && !hasChanges; i++)
                    {
                        if (list[i].Id != cache.OutputIdOrder[i])
                            hasChanges = true;
                        if (list[i].HasChanges)
                            hasChanges = true;
                    }
                }
            }

            return StageResultList.Create(task, hasChanges, newCache.OutputIdOrder.ToImmutableList());
        }



    }



}
