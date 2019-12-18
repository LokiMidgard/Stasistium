using StaticSite.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace StaticSite.Stages
{
//    public abstract class SingleInputStageBase<TResult, TCache, TInput, TPreviousCache> : StageBase<TResult, CacheId<TCache>>
//        where TCache : class
//    {
//        private readonly StagePerformHandler<TInput, TPreviousCache> input;
//        private readonly bool updateOnRefresh;

//        public SingleInputStageBase(StagePerformHandler<TInput, TPreviousCache> input, GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//            this.input = input;
//            this.updateOnRefresh = updateOnRefresh;
//        }

//        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work((IDocument<TInput> result, BaseCache<TPreviousCache> cache) input, bool previousHadChanges, OptionToken options);

//        protected override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 1)
//                throw new ArgumentException($"This cache should have exactly one predecessor but had {cache.PreviousCache}");
//            var inputResult = await this.input(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);

//            var currentCache = cache as CacheId<TCache>;


//            var task = LazyTask.Create(async () =>
//            {
//                var previousPerform = await inputResult.Perform;
//                var source = previousPerform.result;
//                var work = await this.Work(previousPerform, inputResult.HasChanges, options).ConfigureAwait(false);
//                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
//            });


//            bool hasChanges = inputResult.HasChanges;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes

//                var result = await task;
//                currentCache = result.cache.Item;
//                hasChanges = await this.Changed(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//            }

//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Id;

//            return StageResult.Create(task, hasChanges, theId);
//        }

//        protected virtual Task<bool> Changed([AllowNull]TCache item1, [AllowNull] TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }

//    public abstract class SingleInputMultipleStageBase<TResult, TResultCache, TCache, TInput, TPreviousCache> : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//       where TCache : class
//    {
//        private readonly StagePerformHandler<TInput, TPreviousCache> input;
//        private readonly bool updateOnRefresh;

//        public SingleInputMultipleStageBase(StagePerformHandler<TInput, TPreviousCache> input, GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//            this.input = input;
//            this.updateOnRefresh = updateOnRefresh;
//        }

//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work((IDocument<TInput> result, BaseCache<TPreviousCache> cache) input, bool previousHadChanges, [AllowNull] TCache cache, [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches, OptionToken options);

//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 1)
//                throw new ArgumentException($"This cache should have exactly one predecessor but had {cache.PreviousCache}");
//            var inputResult = await this.input(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);

//            var currentCache = cache?.Item;


//            var task = LazyTask.Create(async () =>
//            {
//                var previousPerform = await inputResult.Perform;
//                var source = previousPerform.result;
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//                var work = await this.Work(previousPerform, inputResult.HasChanges, currentCache?.Data, oldChildCaches, options).ConfigureAwait(false);

//                var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
//                var ids = new (string id, string hash)[work.result.Count];
//                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
//                for (int i = 0; i < ids.Length; i++)
//                {
//                    if (work.result[i].HasChanges)
//                    {
//                        var itemResult = await work.result[i].Perform;
//                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
//                        childCaches.Add(itemResult.result.Id, itemResult.cache);
//                    }
//                    else
//                    {
//                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
//                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
//                        childCaches.Add(work.result[i].Id, childCache);
//                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
//                    }
//                }

//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });


//            bool hasChanges = inputResult.HasChanges;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes

//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//            }
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var previousHash = currentCache!.Ids;

//            return StageResult.Create(task, hasChanges, previousHash.Select(x => x.id).ToImmutableList());
//        }

//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }

//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;

//            if (item1 is null || item2 is null)
//                return false;

//            if (item1.Count != item2.Count)
//                return false;

//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }

//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }


//    }

////#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
////#pragma warning disable CA1819 // Properties should not return arrays
////    public class CacheId<TCache>
////             where TCache : class
////    {
////        public TCache? Data { get; set; }

////        public string Id { get; set; }
////    }

////    public class CacheIds<TCache>
////          where TCache : class
////    {
////        public TCache? Data { get; set; }

////        public (string id, string hash)[] Ids { get; set; }
////    }
////#pragma warning restore CA1819 // Properties should not return arrays
////#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


}