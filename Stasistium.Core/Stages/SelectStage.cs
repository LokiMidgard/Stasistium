using Stasistium.Documents;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using Stasistium.Core;
using Stasistium.Stages;

namespace Stasistium.Stages
{
    public class SelectStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache> : MultiStageBase<TResult, GeneratedHelper.CacheId<string, TItemCache>, SelectCache<TInputCache, TItemCache>>
    where TItemCache : class
    where TInputItemCache : class
    where TInputCache : class
    {

        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, (Start @in, End<TResult, TItemCache> @out)> startLookup = new System.Collections.Concurrent.ConcurrentDictionary<string, (Start @in, End<TResult, TItemCache> @out)>();

        private readonly Func<StageBase<TInput, StartCache<TInputCache>>, StageBase<TResult, TItemCache>> createPipline;

        private readonly MultiStageBase<TInput, TInputItemCache, TInputCache> input;

        public SelectStage(MultiStageBase<TInput, TInputItemCache, TInputCache> input, Func<StageBase<TInput, StartCache<TInputCache>>, StageBase<TResult, TItemCache>> createPipline, IGeneratorContext context, string? name = null) : base(context, name)
        {
            this.input = input ?? throw new ArgumentNullException(nameof(input));
            this.createPipline = createPipline ?? throw new ArgumentNullException(nameof(createPipline));
        }

        protected override async Task<StageResultList<TResult, GeneratedHelper.CacheId<string, TItemCache>, SelectCache<TInputCache, TItemCache>>> DoInternal([AllowNull] SelectCache<TInputCache, TItemCache>? cache, OptionToken options)
        {
            var input = await this.input.DoIt(cache?.PreviousCache, options).ConfigureAwait(false);

            var task = LazyTask.Create(async () =>
            {
                var inputResult = await input.Perform;
                var inputCache = input.Cache;
                var resultList = await Task.WhenAll(inputResult.Select(async item =>
                {

                    var pipe = this.startLookup.GetOrAdd(item.Id, id =>
                    {
                        var start = new Start(this, id, this.Context);
                        var end = new End<TResult, TItemCache>(this.createPipline(start), this.Context);
                        return (start, end);
                    });

                    if (cache == null || !cache.InputItemCacheLookup.TryGetValue(item.Id, out GeneratedHelper.CacheId<string, TItemCache>? lastCache) || !cache.InputItemHashLookup.TryGetValue(item.Id, out string? lastHash) || !cache.InputItemOutputIdLookup.TryGetValue(item.Id, out string? lastOutputId))
                    {
                        lastCache = null;
                        lastHash = null;
                        lastOutputId = null;
                    }
                    var pipeDone = await pipe.@out.DoIt(lastCache, options).ConfigureAwait(false);

                    return (result: pipeDone, inputId: item.Id);
                })).ConfigureAwait(false);



                var newCache = new SelectCache<TInputCache, TItemCache>()
                {
                    InputItemCacheLookup = resultList.ToDictionary(x => x.inputId, x => x.result.Cache),
                    InputItemHashLookup = resultList.ToDictionary(x => x.inputId, x => x.result.Hash),
                    InputItemOutputIdLookup = resultList.ToDictionary(x => x.inputId, x => x.result.Id),
                    OutputIdOrder = resultList.Select(x => x.result.Id).ToArray(),
                    PreviousCache = inputCache,
                    Hash = this.Context.GetHashForObject(resultList.Select(x => x.result.Hash)),
                };


                return (resultList.Select(x => x.result).ToImmutableList(), newCache);
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

        private class End<T, TPreviousCache> : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple1List0StageBase<T, TPreviousCache, T>
            where TPreviousCache : class
        {
            public End(StageBase<T, TPreviousCache> inputSingle0, IGeneratorContext context, string? name = null) : base(inputSingle0, context, name)
            {
            }

            protected override Task<IDocument<T>> Work(IDocument<T> inputSingle0, OptionToken options)
            {
                return Task.FromResult(inputSingle0);
            }
        }
        private class Start : StageBase<TInput, StartCache<TInputCache>>
        {
            private readonly SelectStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache> parent;

            private readonly string key;

            public Start(SelectStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache> parent, string key, IGeneratorContext context, string? name = null) : base(context, name)
            {
                this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
                this.key = key;
            }


            protected override async Task<StageResult<TInput, StartCache<TInputCache>>> DoInternal([AllowNull] StartCache<TInputCache>? cache, OptionToken options)
            {
                var input = await this.parent.input.DoIt(cache?.PreviousCache, options).ConfigureAwait(false);

                var task = LazyTask.Create(async () =>
                {
                    var inputResult = await input.Perform;
                    var inputCache = input.Cache;
                    var current = inputResult.Single(x => x.Id == this.key);
                    var subResult = await current.Perform;

                    var newCache = new StartCache<TInputCache>()
                    {
                        PreviousCache = inputCache,
                        Id = subResult.Id,
                        Hash = subResult.Hash
                    };
                    return (result: subResult, newCache);
                });

                string id;

                bool hasChanges = input.HasChanges;
                if (hasChanges || cache is null)
                {
                    var list = await input.Perform;
                    var current = list.Single(x => x.Id == this.key);
                    if (current.HasChanges || cache is null)
                    {
                        var result = await task;
                        id = result.result.Id;
                        hasChanges = cache is null || result.result.Hash != cache.Hash;
                        return this.Context.CreateStageResult(result.result, hasChanges, id, result.newCache, result.newCache.Hash, input.Cache);
                    }
                    else
                    {
                        id = cache.Id;
                    }




                }
                else
                {
                    id = cache.Id;
                }

                var actualTask = LazyTask.Create(async () =>
                {
                    var tmep = await task;
                    return tmep.result;
                });

                return this.Context.CreateStageResult(actualTask, hasChanges, id, cache, cache.Hash, input.Cache);
            }


        }

    }




    public class StartCache<TInputCache> : IHavePreviousCache<TInputCache>
        where TInputCache : class
    {
        public TInputCache PreviousCache { get; set; }
        public string Id { get; set; }
        public string Hash { get; set; }
    }
}

namespace Stasistium
{
    public partial class StageExtensions
    {
        public static SelectStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache> Select<TInput, TInputItemCache, TInputCache, TResult, TItemCache>(this MultiStageBase<TInput, TInputItemCache, TInputCache> input, Func<StageBase<TInput, StartCache<TInputCache>>, StageBase<TResult, TItemCache>> createPipline, string? name = null)
    where TInputCache : class
    where TInputItemCache : class
    where TItemCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new SelectStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache>(input, createPipline, input.Context, name);
        }


    }
}