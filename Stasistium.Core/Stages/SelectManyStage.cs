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
    public class SelectManyStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache, TCache> : MultiStageBase<TResult, GeneratedHelper.CacheId<string, GeneratedHelper.CacheId<string, StartCache<TCache>>>, SelectManyCache<TInputCache, TItemCache, TCache>>
where TItemCache : class
where TInputItemCache : class
where TInputCache : class
        where TCache : class
    {

        private readonly System.Collections.Concurrent.ConcurrentDictionary<(string key, Guid generationId), (Start @in, SelectStage<TResult, TItemCache, TCache, TResult, GeneratedHelper.CacheId<string, StartCache<TCache>>> @out)> startLookup = new System.Collections.Concurrent.ConcurrentDictionary<(string key, Guid generationId), (Start @in, SelectStage<TResult, TItemCache, TCache, TResult, GeneratedHelper.CacheId<string, StartCache<TCache>>> @out)>();

        private readonly Func<StageBase<TInput, GeneratedHelper.CacheId<string>>, MultiStageBase<TResult, TItemCache, TCache>> createPipline;

        private readonly MultiStageBase<TInput, TInputItemCache, TInputCache> input;

        public SelectManyStage(MultiStageBase<TInput, TInputItemCache, TInputCache> input, Func<StageBase<TInput, GeneratedHelper.CacheId<string>>, MultiStageBase<TResult, TItemCache, TCache>> createPipline, IGeneratorContext context, string? name = null) : base(context, name)
        {
            this.input = input ?? throw new ArgumentNullException(nameof(input));
            this.createPipline = createPipline ?? throw new ArgumentNullException(nameof(createPipline));
        }

        protected override async Task<StageResultList<TResult, GeneratedHelper.CacheId<string, GeneratedHelper.CacheId<string, StartCache<TCache>>>, SelectManyCache<TInputCache, TItemCache, TCache>>> DoInternal([AllowNull] SelectManyCache<TInputCache, TItemCache, TCache>? cache, OptionToken options)
        {
            var input = await this.input.DoIt(cache?.PreviousCache, options).ConfigureAwait(false);

            var task = LazyTask.Create(async () =>
            {
                var inputResult = await input.Perform;
                var inputCache = input.Cache;

                var resultList = (await Task.WhenAll(inputResult.Select(async item =>
                {

                    var pipe = this.startLookup.GetOrAdd((item.Id, options.GenerationId), key =>
                    {
                        var start = new SelectManyStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache, TCache>.Start(item, this.Context);
                        var end = this.createPipline(start).Select(x => new End(x, x.Context));
                        return (start, end);
                    });


                    if (cache == null
                        || !cache.InputCacheLookup.TryGetValue(item.Id, out var lastCache)
                        || !cache.InputItemToResultItemIdLookup.TryGetValue(item.Id, out var resultIds)
                        || !cache.InputItemToResultItemCacheLookup.TryGetValue(item.Id, out var resultOldCaches))
                    {
                        lastCache = null;
                        resultIds = Array.Empty<string>();
                        resultOldCaches = Array.Empty<GeneratedHelper.CacheId<string, GeneratedHelper.CacheId<string, StartCache<TCache>>>>();
                    }
                    var pipeDone = await pipe.@out.DoIt(lastCache, options).ConfigureAwait(false);

                    var list = new List<StageResult<TResult, GeneratedHelper.CacheId<string, GeneratedHelper.CacheId<string, StartCache<TCache>>>>>();
                    if (pipeDone.HasChanges || cache is null)
                    {
                        var itemResult = await pipeDone.Perform;
                        var itemCache = pipeDone.Cache;
                        foreach (var singleResult in itemResult)
                        {
                            if (cache == null || cache.OutputItemIdToHash.TryGetValue(singleResult.Id, out string? lastItemHash))
                                lastItemHash = null;

                            var performedSingle = await singleResult.Perform;
                            var singlePerformedResult = performedSingle;
                            list.Add(singleResult);
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
                                if (resultIds.Length != performedPipe.Count)
                                    throw this.Context.Exception("Number of Elements changed but input did not.");

                                var curentValue = performedPipe[currentIndex];
                                var currentPerform = await curentValue.Perform;
                                return currentPerform;
                            });
                            if (!cache.OutputItemIdToHash.TryGetValue(resultIds[i], out string lastItemHash))
                                throw this.Context.Exception("Should not happen");
                            var oldItemCache = resultOldCaches[i];

                            list.Add(this.Context.CreateStageResult(subTask, false, resultIds[i], oldItemCache, lastItemHash, pipeDone.Cache.InputItemCacheLookup[resultIds[i]].PreviousCache));
                        }
                    }

                    return (list, item.Id, lastCache);
                })).ConfigureAwait(false));



                var newCache = new SelectManyCache<TInputCache, TItemCache, TCache>()
                {
                    InputCacheLookup = resultList.ToDictionary(x => x.Id, x => x.lastCache),
                    InputItemToResultItemIdLookup = resultList.ToDictionary(x => x.Id, x => x.list.Select(x => x.Id).ToArray()),
                    OutputItemIdToHash = resultList.SelectMany(x => x.list).ToDictionary(x => x.Id, x => x.Hash),

                    InputItemToResultItemCacheLookup = resultList.ToDictionary(x => x.Id, x => x.list.Select(x => x.Cache).ToArray()),

                    OutputIdOrder = resultList.SelectMany(x => x.list.Select(y => y.Id)).ToArray(),
                    PreviousCache = inputCache,
                    Hash = this.Context.GetHashForObject(resultList.SelectMany(x => x.list).Select(x => x.Hash))
                };

                return (resultList.SelectMany(x => x.list).ToImmutableList(), newCache);
            });

            var hasChanges = input.HasChanges;
            var ids = cache?.OutputIdOrder;
            if (hasChanges || ids is null || cache is null)
            {
                var (work, newCache) = await task;

                ids = newCache.OutputIdOrder;

                if (cache != null)
                {
                    hasChanges = !newCache.OutputIdOrder.SequenceEqual(cache.OutputIdOrder) || work.Any(x => x.HasChanges);
                }
                return this.Context.CreateStageResultList(work, hasChanges, ids.ToImmutableList(), newCache, newCache.Hash, input.Cache);

            }
            else
            {
                var actualTask = LazyTask.Create(async () =>
                {
                    var temp = await task;
                    return temp.Item1;
                });

                return this.Context.CreateStageResultList(actualTask, hasChanges, ids.ToImmutableList(), cache, cache.Hash, input.Cache);
            }

        }


        private class End : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple1List0StageBase<TResult, StartCache<TCache>, TResult>
        {
            public End(StageBase<TResult, StartCache<TCache>> inputSingle0, IGeneratorContext context, string? name = null) : base(inputSingle0, context, name)
            {
            }

            protected override Task<IDocument<TResult>> Work(IDocument<TResult> inputSingle0, OptionToken options)
            {
                return Task.FromResult(inputSingle0);
            }
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

                return result;
            }
        }
    }



}
