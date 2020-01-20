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
    public class GroupByStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache, TCache, TKey> : MultiStageBase<TResult, TItemCache, GroupByStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache, TCache, TKey>.GroupByCache>
    where TItemCache : class
    where TCache : class
    where TInputItemCache : class
    where TInputCache : class
    {

        private readonly Dictionary<TKey, (Start @in, MultiStageBase<TResult, TItemCache, TCache> @out)> startLookup = new Dictionary<TKey, (Start @in, MultiStageBase<TResult, TItemCache, TCache> @out)>();

        private readonly Func<MultiStageBase<TInput, TInputItemCache, StartCache<TInputCache, TKey>>, MultiStageBase<TResult, TItemCache, TCache>> createPipline;

        private readonly Func<IDocument<TInput>, TKey> keySelector;

        private readonly StagePerformHandler<TInput, TInputItemCache, TInputCache> input;

        public GroupByStage(StagePerformHandler<TInput, TInputItemCache, TInputCache> input, Func<IDocument<TInput>, TKey> keySelector, Func<MultiStageBase<TInput, TInputItemCache, StartCache<TInputCache, TKey>>, MultiStageBase<TResult, TItemCache, TCache>> createPipline, IGeneratorContext context, string? name = null) : base(context, name)
        {
            this.input = input ?? throw new ArgumentNullException(nameof(input));
            this.createPipline = createPipline ?? throw new ArgumentNullException(nameof(createPipline));
            this.keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        }

        protected override async Task<StageResultList<TResult, TItemCache, GroupByCache>> DoInternal([AllowNull] GroupByCache? cache, OptionToken options)
        {
            var input = await this.input(cache?.PreviousCache, options).ConfigureAwait(false);

            var task = LazyTask.Create(async () =>
            {
                var (inputResult, inputCache) = await input.Perform;

                var keyValues = await Task.WhenAll(inputResult.Select(async x =>
                {
                    if (x.HasChanges || cache is null || !cache.InputIdToKey.TryGetValue(x.Id, out var oldKey))
                    {
                        var performed = await x.Perform;
                        var key = this.keySelector(performed.result);
                        return (Key: key, Document: x);
                    }
                    else
                    {
                        return (Key: oldKey, Document: x);
                    }
                })).ConfigureAwait(false);


                var resultList = await Task.WhenAll(keyValues.GroupBy(x => x.Key).Select(async x =>
                    {
                        if (this.startLookup.TryGetValue(x.Key, out var pipe))
                        {

                        }
                        else
                        {
                            var start = new Start(this, x.Key, this.Context);
                            var end = this.createPipline(start);
                            pipe = (start, end);
                            this.startLookup.Add(x.Key, pipe);
                        }

                        if (cache == null || !cache.InputItemCacheLookup.TryGetValue(x.Key, out TCache? lastCache))
                        {
                            lastCache = null;
                        }

                        var pipeDone = await pipe.@out.DoIt(lastCache, options).ConfigureAwait(false);

                        if (pipeDone.HasChanges)
                        {
                            var (itemResult, itemCache) = await pipeDone.Perform;

                            return (result: StageResultList.Create(itemResult, itemCache, true, itemResult.Select(x => x.Id).ToImmutableList()), lastCache: itemCache, key: x.Key);
                        }
                        else
                        {
                            return (result: StageResultList.Create(pipeDone.Perform, false, pipeDone.Ids), lastCache: lastCache, key: x.Key);

                        }
                    })).ConfigureAwait(false);


                var finishedList2 = await Task.WhenAll(resultList.Select(x => x.result.Perform.AsTask())).ConfigureAwait(false);
                var finishedList = finishedList2.SelectMany(x => x.result).ToImmutableList();
                var newCache = new GroupByCache()
                {
                    InputItemCacheLookup = resultList.ToDictionary(x => x.key, x => x.lastCache),
                    //InputItemHashLookup = resultList.ToDictionary(x => x.inputId, x => x.lastHash),
                    //InputItemOutputIdLookup = resultList.ToDictionary(x => x.inputId, x => x.result.Id),
                    OutputIdOrder = finishedList.Select(x => x.Id).ToArray(),
                    InputIdToKey = keyValues.ToDictionary(x => x.Document.Id, x => x.Key),
                    KeyToOutputId = resultList.ToDictionary(x => x.key, x => x.result.Ids.ToArray()),
                    PreviousCache = inputCache
                };

                return (finishedList, newCache);
            });

            bool hasChanges;
            ImmutableList<string> ids;
            if (input.HasChanges || cache is null)
            {
                var (work, newCache) = await task;

                ids = newCache.OutputIdOrder.ToImmutableList();

                if (cache != null)
                {
                    hasChanges = !newCache.OutputIdOrder.SequenceEqual(cache.OutputIdOrder) || work.Any(x => x.HasChanges);
                }
                else
                    hasChanges = false;
            }
            else
            {
                hasChanges = false;
                ids = cache.OutputIdOrder.ToImmutableList();
            }

            return StageResultList.Create(task, hasChanges, ids);
        }



        private class Start : MultiStageBase<TInput, TInputItemCache, StartCache<TInputCache, TKey>>
        {
            private readonly GroupByStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache, TCache, TKey> parent;

            private readonly TKey key;

            public Start(GroupByStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache, TCache, TKey> parent, TKey key, IGeneratorContext context, string? name = null) : base(context, name)
            {
                this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
                this.key = key;
            }


            protected override async Task<StageResultList<TInput, TInputItemCache, StartCache<TInputCache, TKey>>> DoInternal([AllowNull] StartCache<TInputCache, TKey>? cache, OptionToken options)
            {
                var input = await this.parent.input(cache?.PreviousCache, options).ConfigureAwait(false);

                var task = LazyTask.Create(async () =>
                {
                    var (inputResult, inputCache) = await input.Perform;

                    var itemToKey = await Task.WhenAll(inputResult.Select(async x =>
                       {
                           if (x.HasChanges || cache is null || !cache.InputIdToKey.TryGetValue(x.Id, out var oldKey))
                           {
                               var performed = await x.Perform;
                               var key = this.parent.keySelector(performed.result);
                               return (Key: key, Document: x);
                           }
                           else
                           {
                               return (Key: oldKey, Document: x);
                           }
                       })).ConfigureAwait(false);
                    var item = itemToKey.Where<(TKey Key, StageResult<TInput, TInputItemCache> Document)>(x => Equals(x.Key, this.key)).Select(x => x.Document);



                    var resultList = await Task.WhenAll<StageResult<TInput, TInputItemCache>>(item.Select(async input =>
                    {
                        if (input.HasChanges || cache is null)
                        {
                            var (currentResult, currentCache) = await input.Perform;
                            return StageResult.Create(currentResult, currentCache, input.HasChanges, currentResult.Id);
                        }
                        else
                            return StageResult.Create(input.Perform, input.HasChanges, input.Id);
                    })).ConfigureAwait(false);






                    var newCache = new StartCache<TInputCache, TKey>()
                    {
                        PreviousCache = inputCache,
                        InputIdToKey = itemToKey.ToDictionary(x => x.Document.Id, x => x.Key),
                        Ids = item.Select(x => x.Id).ToArray()
                    };
                    return (resultList.ToImmutableList(), newCache);
                });

                ImmutableList<string> ids;

                bool hasChanges = input.HasChanges;
                if (hasChanges || cache is null)
                {
                    var (result, newCache) = await task;
                    ids = result.Select(x => x.Id).ToImmutableList();

                    if (cache != null)
                        hasChanges = !cache.Ids.SequenceEqual(newCache.Ids)
                                || result.Any(x => x.HasChanges);


                }
                else
                {
                    ids = cache.Ids.ToImmutableList();
                }
                return StageResultList.Create(task, hasChanges, ids);
            }


        }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning disable CA2227 // Collection properties should be read only
#pragma warning disable CA1819 // Properties should not return arrays
        public class GroupByCache
        {
            public TInputCache PreviousCache { get; set; }
            /// <summary>
            /// Output Ids ORderd
            /// </summary>
            public string[] OutputIdOrder { get; set; }
            /// <summary>
            /// InputId to cache
            /// </summary>
            public Dictionary<TKey, TCache> InputItemCacheLookup { get; set; }
            ///// <summary>
            ///// InputId to OutputHash
            ///// </summary>
            //public Dictionary<string, string> InputItemHashLookup { get; set; }
            /// <summary>
            /// InputId to OutputId
            /// </summary>
            //public Dictionary<string, string> InputItemOutputIdLookup { get; set; }
            public Dictionary<string, TKey> InputIdToKey { get; set; }
            public Dictionary<TKey, string[]> KeyToOutputId { get; set; }
        }
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning restore CA1819 // Properties should not return arrays


    }

    public class StartCache<TInputCache, TKey>
    {
        public TInputCache PreviousCache { get; set; }
        public Dictionary<string, TKey> InputIdToKey { get; set; }
        public string[] Ids { get; set; }
    }


}
namespace Stasistium
{
    public static partial class StageExtensions
    {
        public static GroupByStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache, TCache, TKey> GroupBy<TInput, TInputItemCache, TInputCache, TResult, TItemCache, TCache, TKey>(
            this MultiStageBase<TInput, TInputItemCache, TInputCache> input,
            Func<IDocument<TInput>, TKey> keySelector,
            Func<MultiStageBase<TInput, TInputItemCache, StartCache<TInputCache, TKey>>, MultiStageBase<TResult, TItemCache, TCache>> createPipline, string? name = null)
            where TItemCache : class
    where TCache : class
    where TInputItemCache : class
    where TInputCache : class
        {
            return new GroupByStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache, TCache, TKey>(input.DoIt, keySelector, createPipline, input.Context, name);
        }
    }
}
