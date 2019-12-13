using StaticSite.Documents;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace StaticSite.Stages
{
 public abstract class OutputSingleInputSingle0List1StageBase<
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingle0List1StageBase(
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                    this.inputList0 = inputList0;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 1)
                throw new ArgumentException($"This cache should have exactly 1 predecessor but had {cache.PreviousCache}");
            

            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputList0Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputList0Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle0List1StageBase<
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly bool updateOnRefresh;

        public OutputMultiInputSingle0List1StageBase(
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                    this.inputList0 = inputList0;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
           [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 1)
                throw new ArgumentException($"This cache should have exactly 1 predecessor but had {cache.PreviousCache}");
            

            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputList0Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputList0Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle0List2StageBase<
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingle0List2StageBase(
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 2)
                throw new ArgumentException($"This cache should have exactly 2 predecessor but had {cache.PreviousCache}");
            

            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputList0Result,
            inputList1Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle0List2StageBase<
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly bool updateOnRefresh;

        public OutputMultiInputSingle0List2StageBase(
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
           [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 2)
                throw new ArgumentException($"This cache should have exactly 2 predecessor but had {cache.PreviousCache}");
            

            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputList0Result,
            inputList1Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle0List3StageBase<
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingle0List3StageBase(
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
            
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 3)
                throw new ArgumentException($"This cache should have exactly 3 predecessor but had {cache.PreviousCache}");
            

            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputList0Result,
            inputList1Result,
            inputList2Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle0List3StageBase<
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly bool updateOnRefresh;

        public OutputMultiInputSingle0List3StageBase(
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
           [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 3)
                throw new ArgumentException($"This cache should have exactly 3 predecessor but had {cache.PreviousCache}");
            

            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputList0Result,
            inputList1Result,
            inputList2Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle0List4StageBase<
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TInputList3, TPreviousItemCache3, TPreviousListCache3,
     TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingle0List4StageBase(
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.inputList3 = inputList3;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
            StageResultList<TInputList3,TPreviousItemCache3,TPreviousListCache3> inputList3, 
            
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 4)
                throw new ArgumentException($"This cache should have exactly 4 predecessor but had {cache.PreviousCache}");
            

            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputList3Result = await this.inputList3(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputList0Result,
            inputList1Result,
            inputList2Result,
            inputList3Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
            || inputList3Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle0List4StageBase<
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TInputList3, TPreviousItemCache3, TPreviousListCache3,
     TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
            private readonly bool updateOnRefresh;

        public OutputMultiInputSingle0List4StageBase(
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.inputList3 = inputList3;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
            StageResultList<TInputList3,TPreviousItemCache3,TPreviousListCache3> inputList3, 
           [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 4)
                throw new ArgumentException($"This cache should have exactly 4 predecessor but had {cache.PreviousCache}");
            

            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputList3Result = await this.inputList3(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputList0Result,
            inputList1Result,
            inputList2Result,
            inputList3Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
            || inputList3Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle1List0StageBase<
     TInputSingle0, TPreviousSingleCache0,
             TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
                private readonly bool updateOnRefresh;

        public OutputSingleInputSingle1List0StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
                    GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                    this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
                
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 1)
                throw new ArgumentException($"This cache should have exactly 1 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputSingle0Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle1List0StageBase<
     TInputSingle0, TPreviousSingleCache0,
             TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
                private readonly bool updateOnRefresh;

        public OutputMultiInputSingle1List0StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
                    GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                    this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
               [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 1)
                throw new ArgumentException($"This cache should have exactly 1 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputSingle0Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle1List1StageBase<
     TInputSingle0, TPreviousSingleCache0,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingle1List1StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                    this.inputList0 = inputList0;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 2)
                throw new ArgumentException($"This cache should have exactly 2 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputSingle0Result,
            inputList0Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputList0Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle1List1StageBase<
     TInputSingle0, TPreviousSingleCache0,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly bool updateOnRefresh;

        public OutputMultiInputSingle1List1StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                    this.inputList0 = inputList0;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
           [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 2)
                throw new ArgumentException($"This cache should have exactly 2 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputSingle0Result,
            inputList0Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputList0Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle1List2StageBase<
     TInputSingle0, TPreviousSingleCache0,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingle1List2StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 3)
                throw new ArgumentException($"This cache should have exactly 3 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputSingle0Result,
            inputList0Result,
            inputList1Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle1List2StageBase<
     TInputSingle0, TPreviousSingleCache0,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly bool updateOnRefresh;

        public OutputMultiInputSingle1List2StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
           [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 3)
                throw new ArgumentException($"This cache should have exactly 3 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputSingle0Result,
            inputList0Result,
            inputList1Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle1List3StageBase<
     TInputSingle0, TPreviousSingleCache0,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingle1List3StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
            
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 4)
                throw new ArgumentException($"This cache should have exactly 4 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputSingle0Result,
            inputList0Result,
            inputList1Result,
            inputList2Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle1List3StageBase<
     TInputSingle0, TPreviousSingleCache0,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly bool updateOnRefresh;

        public OutputMultiInputSingle1List3StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
           [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 4)
                throw new ArgumentException($"This cache should have exactly 4 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputSingle0Result,
            inputList0Result,
            inputList1Result,
            inputList2Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle1List4StageBase<
     TInputSingle0, TPreviousSingleCache0,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TInputList3, TPreviousItemCache3, TPreviousListCache3,
     TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingle1List4StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.inputList3 = inputList3;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
            StageResultList<TInputList3,TPreviousItemCache3,TPreviousListCache3> inputList3, 
            
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 5)
                throw new ArgumentException($"This cache should have exactly 5 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
            var inputList3Result = await this.inputList3(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputSingle0Result,
            inputList0Result,
            inputList1Result,
            inputList2Result,
            inputList3Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
            || inputList3Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle1List4StageBase<
     TInputSingle0, TPreviousSingleCache0,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TInputList3, TPreviousItemCache3, TPreviousListCache3,
     TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
            private readonly bool updateOnRefresh;

        public OutputMultiInputSingle1List4StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.inputList3 = inputList3;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
            StageResultList<TInputList3,TPreviousItemCache3,TPreviousListCache3> inputList3, 
           [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 5)
                throw new ArgumentException($"This cache should have exactly 5 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
            var inputList3Result = await this.inputList3(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputSingle0Result,
            inputList0Result,
            inputList1Result,
            inputList2Result,
            inputList3Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
            || inputList3Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle2List0StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
             TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
                private readonly bool updateOnRefresh;

        public OutputSingleInputSingle2List0StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
                    GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                    this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
                
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 2)
                throw new ArgumentException($"This cache should have exactly 2 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle2List0StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
             TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
                private readonly bool updateOnRefresh;

        public OutputMultiInputSingle2List0StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
                    GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                    this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
               [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 2)
                throw new ArgumentException($"This cache should have exactly 2 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle2List1StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingle2List1StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                    this.inputList0 = inputList0;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 3)
                throw new ArgumentException($"This cache should have exactly 3 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputList0Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputList0Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle2List1StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly bool updateOnRefresh;

        public OutputMultiInputSingle2List1StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                    this.inputList0 = inputList0;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
           [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 3)
                throw new ArgumentException($"This cache should have exactly 3 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputList0Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputList0Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle2List2StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingle2List2StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 4)
                throw new ArgumentException($"This cache should have exactly 4 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputList0Result,
            inputList1Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle2List2StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly bool updateOnRefresh;

        public OutputMultiInputSingle2List2StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
           [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 4)
                throw new ArgumentException($"This cache should have exactly 4 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputList0Result,
            inputList1Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle2List3StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingle2List3StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
            
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 5)
                throw new ArgumentException($"This cache should have exactly 5 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputList0Result,
            inputList1Result,
            inputList2Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle2List3StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly bool updateOnRefresh;

        public OutputMultiInputSingle2List3StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
           [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 5)
                throw new ArgumentException($"This cache should have exactly 5 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputList0Result,
            inputList1Result,
            inputList2Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle2List4StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TInputList3, TPreviousItemCache3, TPreviousListCache3,
     TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingle2List4StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.inputList3 = inputList3;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
            StageResultList<TInputList3,TPreviousItemCache3,TPreviousListCache3> inputList3, 
            
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 6)
                throw new ArgumentException($"This cache should have exactly 6 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);
            var inputList3Result = await this.inputList3(cache?.PreviousCache.Span[5], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputList0Result,
            inputList1Result,
            inputList2Result,
            inputList3Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
            || inputList3Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle2List4StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TInputList3, TPreviousItemCache3, TPreviousListCache3,
     TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
            private readonly bool updateOnRefresh;

        public OutputMultiInputSingle2List4StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.inputList3 = inputList3;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
            StageResultList<TInputList3,TPreviousItemCache3,TPreviousListCache3> inputList3, 
           [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 6)
                throw new ArgumentException($"This cache should have exactly 6 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);
            var inputList3Result = await this.inputList3(cache?.PreviousCache.Span[5], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputList0Result,
            inputList1Result,
            inputList2Result,
            inputList3Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
            || inputList3Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle3List0StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
             TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
                private readonly bool updateOnRefresh;

        public OutputSingleInputSingle3List0StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
                    GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                    this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
            StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
                
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 3)
                throw new ArgumentException($"This cache should have exactly 3 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputSingle2Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle3List0StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
             TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
                private readonly bool updateOnRefresh;

        public OutputMultiInputSingle3List0StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
                    GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                    this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
            StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
               [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 3)
                throw new ArgumentException($"This cache should have exactly 3 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputSingle2Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle3List1StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingle3List1StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                    this.inputList0 = inputList0;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
            StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 4)
                throw new ArgumentException($"This cache should have exactly 4 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputSingle2Result,
            inputList0Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputList0Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle3List1StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly bool updateOnRefresh;

        public OutputMultiInputSingle3List1StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                    this.inputList0 = inputList0;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
            StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
           [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 4)
                throw new ArgumentException($"This cache should have exactly 4 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputSingle2Result,
            inputList0Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputList0Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle3List2StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingle3List2StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
            StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 5)
                throw new ArgumentException($"This cache should have exactly 5 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputSingle2Result,
            inputList0Result,
            inputList1Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle3List2StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly bool updateOnRefresh;

        public OutputMultiInputSingle3List2StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
            StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
           [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 5)
                throw new ArgumentException($"This cache should have exactly 5 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputSingle2Result,
            inputList0Result,
            inputList1Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle3List3StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingle3List3StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
            StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
            
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 6)
                throw new ArgumentException($"This cache should have exactly 6 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[5], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputSingle2Result,
            inputList0Result,
            inputList1Result,
            inputList2Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle3List3StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly bool updateOnRefresh;

        public OutputMultiInputSingle3List3StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
            StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
           [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 6)
                throw new ArgumentException($"This cache should have exactly 6 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[5], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputSingle2Result,
            inputList0Result,
            inputList1Result,
            inputList2Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle3List4StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TInputList3, TPreviousItemCache3, TPreviousListCache3,
     TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingle3List4StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.inputList3 = inputList3;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
            StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
            StageResultList<TInputList3,TPreviousItemCache3,TPreviousListCache3> inputList3, 
            
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 7)
                throw new ArgumentException($"This cache should have exactly 7 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[5], options).ConfigureAwait(false);
            var inputList3Result = await this.inputList3(cache?.PreviousCache.Span[6], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputSingle2Result,
            inputList0Result,
            inputList1Result,
            inputList2Result,
            inputList3Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
            || inputList3Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle3List4StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TInputList3, TPreviousItemCache3, TPreviousListCache3,
     TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
            private readonly bool updateOnRefresh;

        public OutputMultiInputSingle3List4StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.inputList3 = inputList3;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
            StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
            StageResultList<TInputList3,TPreviousItemCache3,TPreviousListCache3> inputList3, 
           [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 7)
                throw new ArgumentException($"This cache should have exactly 7 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[5], options).ConfigureAwait(false);
            var inputList3Result = await this.inputList3(cache?.PreviousCache.Span[6], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputSingle2Result,
            inputList0Result,
            inputList1Result,
            inputList2Result,
            inputList3Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
            || inputList3Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle4List0StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
     TInputSingle3, TPreviousSingleCache3,
             TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
            private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
                private readonly bool updateOnRefresh;

        public OutputSingleInputSingle4List0StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
            StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
                    GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                this.inputSingle3 = inputSingle3;
                    this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
            StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
            StageResult<TInputSingle3,TPreviousSingleCache3> inputSingle3, 
                
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 4)
                throw new ArgumentException($"This cache should have exactly 4 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputSingle2Result,
            inputSingle3Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputSingle3Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle4List0StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
     TInputSingle3, TPreviousSingleCache3,
             TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
            private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
                private readonly bool updateOnRefresh;

        public OutputMultiInputSingle4List0StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
            StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
                    GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                this.inputSingle3 = inputSingle3;
                    this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
            StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
            StageResult<TInputSingle3,TPreviousSingleCache3> inputSingle3, 
               [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 4)
                throw new ArgumentException($"This cache should have exactly 4 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputSingle2Result,
            inputSingle3Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputSingle3Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle4List1StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
     TInputSingle3, TPreviousSingleCache3,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
            private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingle4List1StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
            StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                this.inputSingle3 = inputSingle3;
                    this.inputList0 = inputList0;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
            StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
            StageResult<TInputSingle3,TPreviousSingleCache3> inputSingle3, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 5)
                throw new ArgumentException($"This cache should have exactly 5 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputSingle2Result,
            inputSingle3Result,
            inputList0Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputSingle3Result.HasChanges
            || inputList0Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle4List1StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
     TInputSingle3, TPreviousSingleCache3,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
            private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly bool updateOnRefresh;

        public OutputMultiInputSingle4List1StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
            StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                this.inputSingle3 = inputSingle3;
                    this.inputList0 = inputList0;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
            StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
            StageResult<TInputSingle3,TPreviousSingleCache3> inputSingle3, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
           [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 5)
                throw new ArgumentException($"This cache should have exactly 5 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputSingle2Result,
            inputSingle3Result,
            inputList0Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputSingle3Result.HasChanges
            || inputList0Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle4List2StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
     TInputSingle3, TPreviousSingleCache3,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
            private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingle4List2StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
            StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                this.inputSingle3 = inputSingle3;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
            StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
            StageResult<TInputSingle3,TPreviousSingleCache3> inputSingle3, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 6)
                throw new ArgumentException($"This cache should have exactly 6 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[5], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputSingle2Result,
            inputSingle3Result,
            inputList0Result,
            inputList1Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputSingle3Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle4List2StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
     TInputSingle3, TPreviousSingleCache3,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
            private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly bool updateOnRefresh;

        public OutputMultiInputSingle4List2StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
            StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                this.inputSingle3 = inputSingle3;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
            StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
            StageResult<TInputSingle3,TPreviousSingleCache3> inputSingle3, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
           [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 6)
                throw new ArgumentException($"This cache should have exactly 6 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[5], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputSingle2Result,
            inputSingle3Result,
            inputList0Result,
            inputList1Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputSingle3Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle4List3StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
     TInputSingle3, TPreviousSingleCache3,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
            private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingle4List3StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
            StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                this.inputSingle3 = inputSingle3;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
            StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
            StageResult<TInputSingle3,TPreviousSingleCache3> inputSingle3, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
            
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 7)
                throw new ArgumentException($"This cache should have exactly 7 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[5], options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[6], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputSingle2Result,
            inputSingle3Result,
            inputList0Result,
            inputList1Result,
            inputList2Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputSingle3Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle4List3StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
     TInputSingle3, TPreviousSingleCache3,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
            private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly bool updateOnRefresh;

        public OutputMultiInputSingle4List3StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
            StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                this.inputSingle3 = inputSingle3;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
            StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
            StageResult<TInputSingle3,TPreviousSingleCache3> inputSingle3, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
           [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 7)
                throw new ArgumentException($"This cache should have exactly 7 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[5], options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[6], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputSingle2Result,
            inputSingle3Result,
            inputList0Result,
            inputList1Result,
            inputList2Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputSingle3Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

 public abstract class OutputSingleInputSingle4List4StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
     TInputSingle3, TPreviousSingleCache3,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TInputList3, TPreviousItemCache3, TPreviousListCache3,
     TResult, TCache
 > : StageBase<TResult, CacheId<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
            private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingle4List4StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
            StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                this.inputSingle3 = inputSingle3;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.inputList3 = inputList3;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(IDocument<TResult> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
            StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
            StageResult<TInputSingle3,TPreviousSingleCache3> inputSingle3, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
            StageResultList<TInputList3,TPreviousItemCache3,TPreviousListCache3> inputList3, 
            
        OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<TCache>>> DoInternal([AllowNull] BaseCache<CacheId<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 8)
                throw new ArgumentException($"This cache should have exactly 8 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[5], options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[6], options).ConfigureAwait(false);
            var inputList3Result = await this.inputList3(cache?.PreviousCache.Span[7], options).ConfigureAwait(false);

            var currentCache = cache?.Item;


            var task = LazyTask.Create(async () =>
            {
                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputSingle2Result,
            inputSingle3Result,
            inputList0Result,
            inputList1Result,
            inputList2Result,
            inputList3Result,

                options).ConfigureAwait(false);
                if(cache != null)
                    System.Diagnostics.Debug.Assert(work.cache.PreviousCache.Length == cache.PreviousCache.Length, $"Lenth of new presuccseor of new cache and old cache should be the same {work.cache.PreviousCache.Length}(new) {cache.PreviousCache.Length}(old)");

                return (work.result, cache: BaseCache.Create(new CacheId<TCache>() { Data = work.cache.Item, Id = work.result.Id }, work.cache.PreviousCache));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputSingle3Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
            || inputList3Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
            }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Id;

            return StageResult.Create(task, hasChanges, theId);
        }

        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull] TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }


     public abstract class OutputMultiInputSingle4List4StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
     TInputSingle3, TPreviousSingleCache3,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TInputList3, TPreviousItemCache3, TPreviousListCache3,
     TResult, TResultCache, TCache
 > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
        where TCache : class
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
            private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
            private readonly bool updateOnRefresh;

        public OutputMultiInputSingle4List4StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
            StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
            GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                this.inputSingle3 = inputSingle3;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.inputList3 = inputList3;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
        
            StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
            StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
            StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
            StageResult<TInputSingle3,TPreviousSingleCache3> inputSingle3, 
                StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
            StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
            StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
            StageResultList<TInputList3,TPreviousItemCache3,TPreviousListCache3> inputList3, 
           [AllowNull] TCache cache,
        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>> childCaches,
        OptionToken options);

        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] BaseCache<CacheIds<TCache>>? cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 8)
                throw new ArgumentException($"This cache should have exactly 8 predecessor but had {cache.PreviousCache}");
            

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[5], options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[6], options).ConfigureAwait(false);
            var inputList3Result = await this.inputList3(cache?.PreviousCache.Span[7], options).ConfigureAwait(false);

            var currentCache = cache?.Item;

             
            var task = LazyTask.Create(async () =>
            {
                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);

                var work = await this.Work(
                            inputSingle0Result,
            inputSingle1Result,
            inputSingle2Result,
            inputSingle3Result,
            inputList0Result,
            inputList1Result,
            inputList2Result,
            inputList3Result,
cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
                             
                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
                var ids = new (string id, string hash)[work.result.Count];
                var childCaches = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (work.result[i].HasChanges)
                    {
                        var itemResult = await work.result[i].Perform;
                        ids[i] = (itemResult.result.Id, itemResult.result.Hash);
                        childCaches.Add(itemResult.result.Id, itemResult.cache);
                    }
                    else
                    {
                        if (cache is null || !cache.ChildCache.TryGetValue(work.result[i].Id, out var childCache))
                            throw this.Context.Exception("The previous cache should exist if we had no changes.");
                        childCaches.Add(work.result[i].Id, childCache);
                        ids[i] = (work.result[i].Id, oldHashLookup[work.result[i].Id]);
                    }
                }

                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


            bool hasChanges = false 
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputSingle3Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
            || inputList3Result.HasChanges
;
            System.Diagnostics.Debug.Assert(cache != null || hasChanges);

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                currentCache = result.cache.Item;
                // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
                // if we found that cache had no changes, maybe the childcaches where changed.
                if (!hasChanges)
                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
           }

            // if currentCache is null, hasChanges must be true and so currentCache will be set.
            var theId = currentCache!.Ids;

            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
        }

      protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }

        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
        {
            if (item1 is null && item2 is null)
                return true;

            if (item1 is null || item2 is null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
            return itemResults.All(x => x);
        }

        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

}