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
    public class SelectStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache> : MultiStageBase<TResult, TItemCache, SelectCache<TInputCache, TItemCache>>
    where TItemCache : class
    where TInputItemCache : class
    where TInputCache : class
    {

        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, (Start @in, StageBase<TResult, TItemCache> @out)> startLookup = new System.Collections.Concurrent.ConcurrentDictionary<string, (Start @in, StageBase<TResult, TItemCache> @out)>();

        private readonly Func<StageBase<TInput, StartCache<TInputCache>>, StageBase<TResult, TItemCache>> createPipline;

        private readonly StagePerformHandler<TInput, TInputItemCache, TInputCache> input;

        public SelectStage(StagePerformHandler<TInput, TInputItemCache, TInputCache> input, Func<StageBase<TInput, StartCache<TInputCache>>, StageBase<TResult, TItemCache>> createPipline, IGeneratorContext context, string? name = null) : base(context, name)
        {
            this.input = input ?? throw new ArgumentNullException(nameof(input));
            this.createPipline = createPipline ?? throw new ArgumentNullException(nameof(createPipline));
        }

        protected override async Task<StageResultList<TResult, TItemCache, SelectCache<TInputCache, TItemCache>>> DoInternal([AllowNull] SelectCache<TInputCache, TItemCache>? cache, OptionToken options)
        {
            var input = await this.input(cache?.PreviousCache, options).ConfigureAwait(false);

            var task = LazyTask.Create(async () =>
            {
                var (inputResult, inputCache) = await input.Perform;

                var resultList = await Task.WhenAll(inputResult.Select(async item =>
               {

                   var pipe = this.startLookup.GetOrAdd(item.Id, id =>
                   {
                       var start = new Start(this, id, this.Context);
                       var end = this.createPipline(start);
                       return (start, end);
                   });

                   if (cache == null || !cache.InputItemCacheLookup.TryGetValue(item.Id, out TItemCache? lastCache) || !cache.InputItemHashLookup.TryGetValue(item.Id, out string? lastHash) || !cache.InputItemOutputIdLookup.TryGetValue(item.Id, out string? lastOutputId))
                   {
                       lastCache = null;
                       lastHash = null;
                       lastOutputId = null;
                   }
                   var pipeDone = await pipe.@out.DoIt(lastCache, options).ConfigureAwait(false);

                   if (pipeDone.HasChanges)
                   {
                       var (itemResult, itemCache) = await pipeDone.Perform;

                       return (result: StageResult.Create(itemResult, itemCache, itemResult.Hash != lastHash, itemResult.Id), lastCache: itemCache, lastHash: itemResult.Hash, inputId: item.Id);
                   }
                   else
                   {
                       if (lastOutputId is null)
                           throw new InvalidOperationException("This should not happen.");

                       return (result: StageResult.Create(pipeDone.Perform, false, lastOutputId), lastCache: lastCache, lastHash: lastHash, inputId: item.Id);

                   }
               })).ConfigureAwait(false);



                var newCache = new SelectCache<TInputCache, TItemCache>()
                {
                    InputItemCacheLookup = resultList.ToDictionary(x => x.inputId, x => x.lastCache),
                    InputItemHashLookup = resultList.ToDictionary(x => x.inputId, x => x.lastHash),
                    InputItemOutputIdLookup = resultList.ToDictionary(x => x.inputId, x => x.result.Id),
                    OutputIdOrder = resultList.Select(x => x.result.Id).ToArray(),
                    PreviousCache = inputCache
                };


                return (resultList.Select(x => x.result).ToImmutableList(), newCache);
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
                var input = await this.parent.input(cache?.PreviousCache, options).ConfigureAwait(false);

                var task = LazyTask.Create(async () =>
                {
                    var (inputResult, inputCache) = await input.Perform;
                    var current = inputResult.Single(x => x.Id == this.key);
                    var subResult = await current.Perform;

                    var newCache = new StartCache<TInputCache>()
                    {
                        PreviousCache = inputCache,
                        Id = subResult.result.Id,
                        Hash = subResult.result.Hash
                    };
                    return (subResult.result, newCache);
                });

                string id;

                bool hasChanges = input.HasChanges;
                if (hasChanges || cache is null)
                {
                    var list = await input.Perform;
                    var current = list.result.Single(x => x.Id == this.key);
                    if (current.HasChanges || cache is null)
                    {
                        var (result, newCache) = await task;
                        id = result.Id;
                        hasChanges = cache is null || result.Hash != cache.Hash;
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
                return StageResult.Create(task, hasChanges, id);
            }


        }
    }


    public class StartCache<TInputCache>
    {
        public TInputCache PreviousCache { get; set; }
        public string Id { get; set; }
        public string Hash { get; set; }
    }
}
