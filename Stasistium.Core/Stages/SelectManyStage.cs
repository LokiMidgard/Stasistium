using Stasistium.Documents;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using Stasistium.Core;

namespace Stasistium.Stages
{
    public class SelectManyStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache, TCache> : MultiStageBase<TResult, TItemCache, SelectManyCache<TInputCache, TItemCache, TCache>>
where TItemCache : class
where TInputItemCache : class
where TInputCache : class
        where TCache : class
    {

        private readonly Dictionary<string, (Start @in, MultiStageBase<TResult, TItemCache, TCache> @out)> startLookup = new Dictionary<string, (Start @in, MultiStageBase<TResult, TItemCache, TCache> @out)>();

        private readonly Func<StageBase<TInput, GeneratedHelper.CacheId<string>>, MultiStageBase<TResult, TItemCache, TCache>> createPipline;

        private readonly StagePerformHandler<TInput, TInputItemCache, TInputCache> input;

        public SelectManyStage(StagePerformHandler<TInput, TInputItemCache, TInputCache> input, Func<StageBase<TInput, GeneratedHelper.CacheId<string>>, MultiStageBase<TResult, TItemCache, TCache>> createPipline, IGeneratorContext context, string? name = null) : base(context, name)
        {
            this.input = input ?? throw new ArgumentNullException(nameof(input));
            this.createPipline = createPipline ?? throw new ArgumentNullException(nameof(createPipline));
        }

        protected override async Task<StageResultList<TResult, TItemCache, SelectManyCache<TInputCache, TItemCache, TCache>>> DoInternal([AllowNull] SelectManyCache<TInputCache, TItemCache, TCache>? cache, OptionToken options)
        {
            var input = await this.input(cache?.PreviousCache, options).ConfigureAwait(false);

            var task = LazyTask.Create(async () =>
            {
                var (inputResult, inputCache) = await input.Perform;

                var resultList = (await Task.WhenAll(inputResult.Select(async item =>
                {

                    if (this.startLookup.TryGetValue(item.Id, out var pipe))
                    {
                        pipe.@in.In = item;
                    }
                    else
                    {
                        var start = new SelectManyStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache, TCache>.Start(item, this.Context);
                        var end = this.createPipline(start);
                        pipe = (start, end);
                        this.startLookup.Add(item.Id, pipe);
                    }

                    if (cache == null || !cache.InputCacheLookup.TryGetValue(item.Id, out TCache? lastCache) || !cache.InputItemToResultItemIdLookup.TryGetValue(item.Id, out var resultIds))
                    {
                        lastCache = null;
                        resultIds = Array.Empty<string>();
                    }
                    var pipeDone = await pipe.@out.DoIt(lastCache, options).ConfigureAwait(false);

                    var list = new List<(StageResult<TResult, TItemCache> result, string lastItemHash)>();
                    if (pipeDone.HasChanges || cache is null)
                    {
                        var (itemResult, itemCache) = await pipeDone.Perform;
                        foreach (var singleResult in itemResult)
                        {
                            if (cache == null || cache.OutputItemIdToHash.TryGetValue(singleResult.Id, out string? lastItemHash))
                                lastItemHash = null;

                            var performedSingle = await singleResult.Perform;
                            var singlePerformedResult = performedSingle.result;
                            list.Add((StageResult.Create(singlePerformedResult, performedSingle.cache, singlePerformedResult.Hash != lastItemHash, singlePerformedResult.Id), singlePerformedResult.Hash));
                        }
                        lastCache = itemCache;
                    }
                    else
                    {
                        for (int i = 0; i < resultIds.Length; i++)
                        {
                            var currentIndex = i; // need to assing this because of lambda
                            var subTask = LazyTask.Create(async () =>
                            {

                                var performedPipe = await pipeDone.Perform;
                                if (resultIds.Length != performedPipe.result.Count)
                                    throw this.Context.Exception("Number of Elements changed but input did not.");

                                var curentValue = performedPipe.result[currentIndex];
                                var currentPerform = await curentValue.Perform;
                                return currentPerform;
                            });
                            if (!cache.OutputItemIdToHash.TryGetValue(resultIds[i], out string lastItemHash))
                                throw this.Context.Exception("Should not happen");

                            list.Add((result: StageResult.Create(subTask, false, resultIds[i]), lastItemHash));
                        }
                    }

                    return (list, item.Id, lastCache);
                })).ConfigureAwait(false));



                var newCache = new SelectManyCache<TInputCache, TItemCache, TCache>()
                {
                    InputCacheLookup = resultList.ToDictionary(x => x.Id, x => x.lastCache),
                    InputItemToResultItemIdLookup = resultList.ToDictionary(x => x.Id, x => x.list.Select(x => x.result.Id).ToArray()),
                    OutputItemIdToHash = resultList.SelectMany(x => x.list).ToDictionary(x => x.result.Id, x => x.lastItemHash),

                    OutputIdOrder = resultList.SelectMany(x => x.list.Select(y => y.result.Id)).ToArray(),
                    PreviousCache = inputCache
                };

                return (resultList.SelectMany(x => x.list.Select(y => y.result)).ToImmutableList(), newCache);
            });

            var hasChanges = input.HasChanges;
            var ids = cache?.OutputIdOrder;
            if (hasChanges || ids is null)
            {
                var (work, newCache) = await task;

                ids = newCache.OutputIdOrder;

                if (cache != null)
                {
                    hasChanges = !newCache.OutputIdOrder.SequenceEqual(cache.OutputIdOrder) || work.Any(x => x.HasChanges);
                }

            }

            return StageResultList.Create(task, hasChanges, ids.ToImmutableList());
        }




        private class Start : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple0List0StageBase<TInput>
        {
            private string? lastHash;
            private StageResult<TInput, TInputItemCache> @in;

            public Start(StageResult<TInput, TInputItemCache> initial, IGeneratorContext context, string? name = null) : base(context, name)
            {
                this.@in = initial;
            }

            public StageResult<TInput, TInputItemCache> In
            {
                get => this.@in; set
                {
                    this.@in = value;
                    this.lastHash = null;
                }
            }



            protected override Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options)
            {
                return Task.FromResult<bool?>(this.In.Id != id || this.lastHash != hash || hash is null);
            }


            protected override async Task<IDocument<TInput>> Work(OptionToken options)
            {
                var result = await this.In.Perform;

                return result.result;
            }
        }
    }



}
