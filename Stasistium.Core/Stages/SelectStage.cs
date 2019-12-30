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

        private readonly Dictionary<string, (Start @in, StageBase<TResult, TItemCache> @out)> startLookup = new Dictionary<string, (Start @in, StageBase<TResult, TItemCache> @out)>();

        private readonly Func<StageBase<TInput, GeneratedHelper.CacheId<string>>, StageBase<TResult, TItemCache>> createPipline;

        private readonly StagePerformHandler<TInput, TInputItemCache, TInputCache> input;

        public SelectStage(StagePerformHandler<TInput, TInputItemCache, TInputCache> input, Func<StageBase<TInput, GeneratedHelper.CacheId<string>>, StageBase<TResult, TItemCache>> createPipline, GeneratorContext context) : base(context)
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

                   if (this.startLookup.TryGetValue(item.Id, out var pipe))
                   {
                       pipe.@in.In = item;
                   }
                   else
                   {
                       var start = new SelectStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache>.Start(item, this.Context);
                       var end = this.createPipline(start);
                       pipe = (start, end);
                       this.startLookup.Add(item.Id, pipe);
                   }

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
                    hasChanges = newCache.OutputIdOrder.SequenceEqual(cache.OutputIdOrder) || work.Any(x => x.HasChanges);
                }

            }

            return StageResultList.Create(task, hasChanges, ids.ToImmutableList());
        }




        private class Start : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple0List0StageBase<TInput>
        {
            private string? lastHash;
            private StageResult<TInput, TInputItemCache> @in;

            public Start(StageResult<TInput, TInputItemCache> initial, GeneratorContext context) : base(context)
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
