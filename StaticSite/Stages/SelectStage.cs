using StaticSite.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace StaticSite.Stages
{
    public class SelectStage<TIn, TInItemCache, TInCache, TOut> : MultiStageBase<TOut, string, SelectStageCache<TInCache>>
        where TInCache : class
        where TInItemCache : class
    {
        private readonly StagePerformHandler<TIn, TInItemCache, TInCache> input;
        private readonly Func<IDocument<TIn>, Task<IDocument<TOut>>> transform;

        public SelectStage(StagePerformHandler<TIn, TInItemCache, TInCache> input, Func<IDocument<TIn>, Task<IDocument<TOut>>> selector, GeneratorContext context) : base(context)
        {
            this.input = input;
            this.transform = selector;
        }

        protected override async Task<StageResultList<TOut, string, SelectStageCache<TInCache>>> DoInternal([AllowNull] SelectStageCache<TInCache>? cache, OptionToken options)
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

                var newCache = new SelectStageCache<TInCache>()
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

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning disable CA1819 // Properties should not return arrays
#pragma warning disable CA2227 // Collection properties should be read only
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public class SelectStageCache<TInCache>
        where TInCache : class
    {
        public TInCache ParentCache { get; set; }

        public string[] OutputIdOrder { get; set; }
        public Dictionary<string, string> Transformed { get; set; }
        public Dictionary<string, string> InputToOutputId { get; set; }

    }
#pragma warning restore CA1819 // Properties should not return arrays
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.



}
