
using Stasistium.Documents;
using System;
using Stasistium.Core;
using Stasistium;
using Stasistium.Stages;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Stasistium.Stages.GeneratedHelper
{ 


    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple0List0StageBase<
             TResult
 > : StageBase<TResult, CacheId<string>>
        

                    
    {
                private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple0List0StageBase(
                    IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                    this.updateOnRefresh = updateOnRefresh;
        } 
        
        protected abstract Task<IDocument<TResult>> Work(OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string>>> DoInternal([AllowNull] CacheId<string>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));



            var task = LazyTask.Create(async () =>
            {

        




                var work = await this.Work(
                
                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
;

;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle0List0StageBase<
             TResult
 > : MultiStageBase<TResult, string,  CachelessIds>

 
                    
    {
                private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle0List0StageBase(
                    IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                    this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
                OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds>> DoInternal([AllowNull]  CachelessIds? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


             
            var task = LazyTask.Create(async () =>
            {

            




                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray()));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
;

;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle0List0StageBase<
//    //        // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle0List0StageBase(
//    //        //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 0)
//                throw new ArgumentException($"This cache should have exactly 0 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
//////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                ////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
////;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple0List1StageBase<
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TResult
 > : StageBase<TResult, CacheId<string, TPreviousListCache0>>
        

                     where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

    
    {
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple0List1StageBase(
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                    this.inputList0 = inputList0;
                this.updateOnRefresh = updateOnRefresh;
        } 
        
        protected abstract Task<IDocument<TResult>> Work(ImmutableList<IDocument<TInputList0>> inputList0, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousListCache0>>> DoInternal([AllowNull] CacheId<string, TPreviousListCache0>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            var inputList0Result = await this.inputList0(cache?.PreviousCache0, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));


            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);


                var work = await this.Work(
                            inputList0PerformedList.ToImmutableList(),

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputList0Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputList0Result.HasChanges
;

            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle0List1StageBase<
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousListCache0>>

 
                     where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

    
    {
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle0List1StageBase(
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                    this.inputList0 = inputList0;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
                ImmutableList<IDocument<TInputList0>> inputList0, 
            OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousListCache0>>> DoInternal([AllowNull]  CachelessIds<TPreviousListCache0>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            var inputList0Result = await this.inputList0(cache?.PreviousCache0, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));


            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);



                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputList0PerformedList.ToImmutableList(),
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputList0Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputList0Result.HasChanges
;

            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle0List1StageBase<
//    //        // TInputList0, TPreviousItemCache0, TPreviousListCache0,
//    // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //    //        private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
//    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle0List1StageBase(
//    //        //        StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
//    //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //    //            this.inputList0 = inputList0;
//    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //    //        StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
//    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 1)
//                throw new ArgumentException($"This cache should have exactly 1 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
//////            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                ////            inputList0Result,
////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
//////            || inputList0Result.HasChanges
//;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple0List2StageBase<
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TResult
 > : StageBase<TResult, CacheId<string, TPreviousListCache0, TPreviousListCache1>>
        

                     where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

    
    {
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple0List2StageBase(
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.updateOnRefresh = updateOnRefresh;
        } 
        
        protected abstract Task<IDocument<TResult>> Work(ImmutableList<IDocument<TInputList0>> inputList0, ImmutableList<IDocument<TInputList1>> inputList1, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousListCache0, TPreviousListCache1>>> DoInternal([AllowNull] CacheId<string, TPreviousListCache0, TPreviousListCache1>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Task.WhenAll(
                this.inputList0(cache?.PreviousCache0, options),
                this.inputList1(cache?.PreviousCache1, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputList0Result = await this.inputList0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache1, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            await Task.WhenAll(
                this.inputList0(cache?.PreviousCache0, options),
                this.inputList1(cache?.PreviousCache1, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);


                var work = await this.Work(
                            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputList0Performed.cache, inputList1Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
;

            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle0List2StageBase<
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousListCache0, TPreviousListCache1>>

 
                     where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

    
    {
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle0List2StageBase(
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
                ImmutableList<IDocument<TInputList0>> inputList0, 
            ImmutableList<IDocument<TInputList1>> inputList1, 
            OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousListCache0, TPreviousListCache1>>> DoInternal([AllowNull]  CachelessIds<TPreviousListCache0, TPreviousListCache1>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            await Task.WhenAll(
                this.inputList0(cache?.PreviousCache0, options),
                this.inputList1(cache?.PreviousCache1, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputList0Result = await this.inputList0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache1, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        await Task.WhenAll(
                inputList0Result.Perform.AsTask(),
                inputList1Result.Perform.AsTask(),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);



                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputList0Performed.cache, inputList1Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
;

            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle0List2StageBase<
//    //        // TInputList0, TPreviousItemCache0, TPreviousListCache0,
//    // TInputList1, TPreviousItemCache1, TPreviousListCache1,
//    // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //    //        private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
//    //        private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
//    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle0List2StageBase(
//    //        //        StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
//    //        StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
//    //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //    //            this.inputList0 = inputList0;
//    //            this.inputList1 = inputList1;
//    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //    //        StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
//    //        StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
//    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 2)
//                throw new ArgumentException($"This cache should have exactly 2 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
//////            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
////            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                ////            inputList0Result,
////            inputList1Result,
////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
//////            || inputList0Result.HasChanges
////            || inputList1Result.HasChanges
//;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple0List3StageBase<
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TResult
 > : StageBase<TResult, CacheId<string, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>>
        

                     where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

     where TPreviousListCache2 : class
 where TPreviousItemCache2 : class

    
    {
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple0List3StageBase(
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.updateOnRefresh = updateOnRefresh;
        } 
        
        protected abstract Task<IDocument<TResult>> Work(ImmutableList<IDocument<TInputList0>> inputList0, ImmutableList<IDocument<TInputList1>> inputList1, ImmutableList<IDocument<TInputList2>> inputList2, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>>> DoInternal([AllowNull] CacheId<string, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Task.WhenAll(
                this.inputList0(cache?.PreviousCache0, options),
                this.inputList1(cache?.PreviousCache1, options),
                this.inputList2(cache?.PreviousCache2, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputList0Result = await this.inputList0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache2, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            await Task.WhenAll(
                this.inputList0(cache?.PreviousCache0, options),
                this.inputList1(cache?.PreviousCache1, options),
                this.inputList2(cache?.PreviousCache2, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));
            var inputList2Performed = await inputList2Result.Perform;

            var inputList2PerformedListTask = Task.WhenAll(inputList2Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
,              inputList2PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);
            var inputList2PerformedList = await inputList2PerformedListTask.ConfigureAwait(false);


                var work = await this.Work(
                            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
            inputList2PerformedList.ToImmutableList(),

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputList0Performed.cache, inputList1Performed.cache, inputList2Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
;

            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

            if(inputList2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList2Result.Ids)}");

;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle0List3StageBase<
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>>

 
                     where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

     where TPreviousListCache2 : class
 where TPreviousItemCache2 : class

    
    {
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle0List3StageBase(
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
                ImmutableList<IDocument<TInputList0>> inputList0, 
            ImmutableList<IDocument<TInputList1>> inputList1, 
            ImmutableList<IDocument<TInputList2>> inputList2, 
            OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>>> DoInternal([AllowNull]  CachelessIds<TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            await Task.WhenAll(
                this.inputList0(cache?.PreviousCache0, options),
                this.inputList1(cache?.PreviousCache1, options),
                this.inputList2(cache?.PreviousCache2, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputList0Result = await this.inputList0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache2, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        await Task.WhenAll(
                inputList0Result.Perform.AsTask(),
                inputList1Result.Perform.AsTask(),
                inputList2Result.Perform.AsTask(),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));
            var inputList2Performed = await inputList2Result.Perform;

            var inputList2PerformedListTask = Task.WhenAll(inputList2Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
,              inputList2PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);
            var inputList2PerformedList = await inputList2PerformedListTask.ConfigureAwait(false);



                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
            inputList2PerformedList.ToImmutableList(),
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputList0Performed.cache, inputList1Performed.cache, inputList2Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
;

            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

            if(inputList2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList2Result.Ids)}");

;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle0List3StageBase<
//    //        // TInputList0, TPreviousItemCache0, TPreviousListCache0,
//    // TInputList1, TPreviousItemCache1, TPreviousListCache1,
//    // TInputList2, TPreviousItemCache2, TPreviousListCache2,
//    // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //    //        private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
//    //        private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
//    //        private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
//    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle0List3StageBase(
//    //        //        StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
//    //        StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
//    //        StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
//    //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //    //            this.inputList0 = inputList0;
//    //            this.inputList1 = inputList1;
//    //            this.inputList2 = inputList2;
//    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //    //        StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
//    //        StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
//    //        StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
//    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 3)
//                throw new ArgumentException($"This cache should have exactly 3 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
//////            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
////            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
////            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                ////            inputList0Result,
////            inputList1Result,
////            inputList2Result,
////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
//////            || inputList0Result.HasChanges
////            || inputList1Result.HasChanges
////            || inputList2Result.HasChanges
//;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple0List4StageBase<
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TInputList3, TPreviousItemCache3, TPreviousListCache3,
     TResult
 > : StageBase<TResult, CacheId<string, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>>
        

                     where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

     where TPreviousListCache2 : class
 where TPreviousItemCache2 : class

     where TPreviousListCache3 : class
 where TPreviousItemCache3 : class

    
    {
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple0List4StageBase(
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.inputList3 = inputList3;
                this.updateOnRefresh = updateOnRefresh;
        } 
        
        protected abstract Task<IDocument<TResult>> Work(ImmutableList<IDocument<TInputList0>> inputList0, ImmutableList<IDocument<TInputList1>> inputList1, ImmutableList<IDocument<TInputList2>> inputList2, ImmutableList<IDocument<TInputList3>> inputList3, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>>> DoInternal([AllowNull] CacheId<string, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Task.WhenAll(
                this.inputList0(cache?.PreviousCache0, options),
                this.inputList1(cache?.PreviousCache1, options),
                this.inputList2(cache?.PreviousCache2, options),
                this.inputList3(cache?.PreviousCache3, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputList0Result = await this.inputList0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputList3Result = await this.inputList3(cache?.PreviousCache3, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            await Task.WhenAll(
                this.inputList0(cache?.PreviousCache0, options),
                this.inputList1(cache?.PreviousCache1, options),
                this.inputList2(cache?.PreviousCache2, options),
                this.inputList3(cache?.PreviousCache3, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));
            var inputList2Performed = await inputList2Result.Perform;

            var inputList2PerformedListTask = Task.WhenAll(inputList2Performed.result.Select(async x => (await x.Perform).result));
            var inputList3Performed = await inputList3Result.Perform;

            var inputList3PerformedListTask = Task.WhenAll(inputList3Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
,              inputList2PerformedListTask
,              inputList3PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);
            var inputList2PerformedList = await inputList2PerformedListTask.ConfigureAwait(false);
            var inputList3PerformedList = await inputList3PerformedListTask.ConfigureAwait(false);


                var work = await this.Work(
                            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
            inputList2PerformedList.ToImmutableList(),
            inputList3PerformedList.ToImmutableList(),

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputList0Performed.cache, inputList1Performed.cache, inputList2Performed.cache, inputList3Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
            || inputList3Result.HasChanges
;

            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

            if(inputList2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList2Result.Ids)}");

            if(inputList3Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList3Result.Ids)}");

;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle0List4StageBase<
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TInputList3, TPreviousItemCache3, TPreviousListCache3,
     TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>>

 
                     where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

     where TPreviousListCache2 : class
 where TPreviousItemCache2 : class

     where TPreviousListCache3 : class
 where TPreviousItemCache3 : class

    
    {
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
            private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle0List4StageBase(
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.inputList3 = inputList3;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
                ImmutableList<IDocument<TInputList0>> inputList0, 
            ImmutableList<IDocument<TInputList1>> inputList1, 
            ImmutableList<IDocument<TInputList2>> inputList2, 
            ImmutableList<IDocument<TInputList3>> inputList3, 
            OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>>> DoInternal([AllowNull]  CachelessIds<TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            await Task.WhenAll(
                this.inputList0(cache?.PreviousCache0, options),
                this.inputList1(cache?.PreviousCache1, options),
                this.inputList2(cache?.PreviousCache2, options),
                this.inputList3(cache?.PreviousCache3, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputList0Result = await this.inputList0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputList3Result = await this.inputList3(cache?.PreviousCache3, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        await Task.WhenAll(
                inputList0Result.Perform.AsTask(),
                inputList1Result.Perform.AsTask(),
                inputList2Result.Perform.AsTask(),
                inputList3Result.Perform.AsTask(),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));
            var inputList2Performed = await inputList2Result.Perform;

            var inputList2PerformedListTask = Task.WhenAll(inputList2Performed.result.Select(async x => (await x.Perform).result));
            var inputList3Performed = await inputList3Result.Perform;

            var inputList3PerformedListTask = Task.WhenAll(inputList3Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
,              inputList2PerformedListTask
,              inputList3PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);
            var inputList2PerformedList = await inputList2PerformedListTask.ConfigureAwait(false);
            var inputList3PerformedList = await inputList3PerformedListTask.ConfigureAwait(false);



                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
            inputList2PerformedList.ToImmutableList(),
            inputList3PerformedList.ToImmutableList(),
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputList0Performed.cache, inputList1Performed.cache, inputList2Performed.cache, inputList3Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
            || inputList3Result.HasChanges
;

            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

            if(inputList2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList2Result.Ids)}");

            if(inputList3Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList3Result.Ids)}");

;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle0List4StageBase<
//    //        // TInputList0, TPreviousItemCache0, TPreviousListCache0,
//    // TInputList1, TPreviousItemCache1, TPreviousListCache1,
//    // TInputList2, TPreviousItemCache2, TPreviousListCache2,
//    // TInputList3, TPreviousItemCache3, TPreviousListCache3,
//    // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //    //        private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
//    //        private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
//    //        private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
//    //        private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
//    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle0List4StageBase(
//    //        //        StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
//    //        StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
//    //        StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
//    //        StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
//    //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //    //            this.inputList0 = inputList0;
//    //            this.inputList1 = inputList1;
//    //            this.inputList2 = inputList2;
//    //            this.inputList3 = inputList3;
//    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //    //        StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
//    //        StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
//    //        StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
//    //        StageResultList<TInputList3,TPreviousItemCache3,TPreviousListCache3> inputList3, 
//    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 4)
//                throw new ArgumentException($"This cache should have exactly 4 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
//////            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
////            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
////            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
////            var inputList3Result = await this.inputList3(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                ////            inputList0Result,
////            inputList1Result,
////            inputList2Result,
////            inputList3Result,
////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
//////            || inputList0Result.HasChanges
////            || inputList1Result.HasChanges
////            || inputList2Result.HasChanges
////            || inputList3Result.HasChanges
//;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple1List0StageBase<
     TInputSingle0, TPreviousSingleCache0,
             TResult
 > : StageBase<TResult, CacheId<string, TPreviousSingleCache0>>
        

             where TPreviousSingleCache0 : class
            
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
                private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple1List0StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
                    IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                    this.updateOnRefresh = updateOnRefresh;
        } 
        
        protected abstract Task<IDocument<TResult>> Work(IDocument<TInputSingle0> inputSingle0, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousSingleCache0>>> DoInternal([AllowNull] CacheId<string, TPreviousSingleCache0>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            var inputSingle0Performed = await inputSingle0Result.Perform;




                var work = await this.Work(
                            inputSingle0Performed.result,

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputSingle0Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputSingle0Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle1List0StageBase<
     TInputSingle0, TPreviousSingleCache0,
             TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousSingleCache0>>

 
             where TPreviousSingleCache0 : class
            
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
                private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle1List0StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
                    IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                    this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
            IDocument<TInputSingle0> inputSingle0, 
                OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousSingleCache0>>> DoInternal([AllowNull]  CachelessIds<TPreviousSingleCache0>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        var inputSingle0Performed = await inputSingle0Result.Perform;





                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputSingle0Performed.result,
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputSingle0Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputSingle0Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
;

            var idsHashs = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                idsHashs = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, idsHashs.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle1List0StageBase<
//    // TInputSingle0, TPreviousSingleCache0,
//    //        // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //        private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
//    //    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle1List0StageBase(
//    //        StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
//    //        //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //            this.inputSingle0 = inputSingle0;
//    //    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //        StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
//    //    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 1)
//                throw new ArgumentException($"This cache should have exactly 1 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
////            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
//////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                //            inputSingle0Result,
//////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
////            || inputSingle0Result.HasChanges
////;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple1List1StageBase<
     TInputSingle0, TPreviousSingleCache0,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TResult
 > : StageBase<TResult, CacheId<string, TPreviousSingleCache0, TPreviousListCache0>>
        

             where TPreviousSingleCache0 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple1List1StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                    this.inputList0 = inputList0;
                this.updateOnRefresh = updateOnRefresh;
        } 
        
        protected abstract Task<IDocument<TResult>> Work(IDocument<TInputSingle0> inputSingle0, ImmutableList<IDocument<TInputList0>> inputList0, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousSingleCache0, TPreviousListCache0>>> DoInternal([AllowNull] CacheId<string, TPreviousSingleCache0, TPreviousListCache0>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputList0(cache?.PreviousCache1, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache1, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputList0(cache?.PreviousCache1, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));


            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);


                var work = await this.Work(
                            inputSingle0Performed.result,
            inputList0PerformedList.ToImmutableList(),

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputSingle0Performed.cache, inputList0Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputSingle0Result.HasChanges
            || inputList0Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle1List1StageBase<
     TInputSingle0, TPreviousSingleCache0,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousSingleCache0, TPreviousListCache0>>

 
             where TPreviousSingleCache0 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle1List1StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                    this.inputList0 = inputList0;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
            IDocument<TInputSingle0> inputSingle0, 
                ImmutableList<IDocument<TInputList0>> inputList0, 
            OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousSingleCache0, TPreviousListCache0>>> DoInternal([AllowNull]  CachelessIds<TPreviousSingleCache0, TPreviousListCache0>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputList0(cache?.PreviousCache1, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache1, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        await Task.WhenAll(
                inputSingle0Result.Perform.AsTask(),
                inputList0Result.Perform.AsTask(),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));


            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);



                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputSingle0Performed.result,
            inputList0PerformedList.ToImmutableList(),
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputSingle0Performed.cache, inputList0Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputSingle0Result.HasChanges
            || inputList0Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle1List1StageBase<
//    // TInputSingle0, TPreviousSingleCache0,
//    //        // TInputList0, TPreviousItemCache0, TPreviousListCache0,
//    // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //        private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
//    //    //        private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
//    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle1List1StageBase(
//    //        StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
//    //        //        StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
//    //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //            this.inputSingle0 = inputSingle0;
//    //    //            this.inputList0 = inputList0;
//    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //        StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
//    //    //        StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
//    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 2)
//                throw new ArgumentException($"This cache should have exactly 2 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
////            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
//////            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                //            inputSingle0Result,
//////            inputList0Result,
////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
////            || inputSingle0Result.HasChanges
//////            || inputList0Result.HasChanges
//;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple1List2StageBase<
     TInputSingle0, TPreviousSingleCache0,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TResult
 > : StageBase<TResult, CacheId<string, TPreviousSingleCache0, TPreviousListCache0, TPreviousListCache1>>
        

             where TPreviousSingleCache0 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple1List2StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.updateOnRefresh = updateOnRefresh;
        } 
        
        protected abstract Task<IDocument<TResult>> Work(IDocument<TInputSingle0> inputSingle0, ImmutableList<IDocument<TInputList0>> inputList0, ImmutableList<IDocument<TInputList1>> inputList1, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousSingleCache0, TPreviousListCache0, TPreviousListCache1>>> DoInternal([AllowNull] CacheId<string, TPreviousSingleCache0, TPreviousListCache0, TPreviousListCache1>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputList0(cache?.PreviousCache1, options),
                this.inputList1(cache?.PreviousCache2, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache2, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputList0(cache?.PreviousCache1, options),
                this.inputList1(cache?.PreviousCache2, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);


                var work = await this.Work(
                            inputSingle0Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputSingle0Performed.cache, inputList0Performed.cache, inputList1Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputSingle0Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle1List2StageBase<
     TInputSingle0, TPreviousSingleCache0,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousSingleCache0, TPreviousListCache0, TPreviousListCache1>>

 
             where TPreviousSingleCache0 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle1List2StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
            IDocument<TInputSingle0> inputSingle0, 
                ImmutableList<IDocument<TInputList0>> inputList0, 
            ImmutableList<IDocument<TInputList1>> inputList1, 
            OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousSingleCache0, TPreviousListCache0, TPreviousListCache1>>> DoInternal([AllowNull]  CachelessIds<TPreviousSingleCache0, TPreviousListCache0, TPreviousListCache1>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputList0(cache?.PreviousCache1, options),
                this.inputList1(cache?.PreviousCache2, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache2, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        await Task.WhenAll(
                inputSingle0Result.Perform.AsTask(),
                inputList0Result.Perform.AsTask(),
                inputList1Result.Perform.AsTask(),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);



                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputSingle0Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputSingle0Performed.cache, inputList0Performed.cache, inputList1Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputSingle0Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle1List2StageBase<
//    // TInputSingle0, TPreviousSingleCache0,
//    //        // TInputList0, TPreviousItemCache0, TPreviousListCache0,
//    // TInputList1, TPreviousItemCache1, TPreviousListCache1,
//    // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //        private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
//    //    //        private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
//    //        private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
//    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle1List2StageBase(
//    //        StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
//    //        //        StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
//    //        StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
//    //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //            this.inputSingle0 = inputSingle0;
//    //    //            this.inputList0 = inputList0;
//    //            this.inputList1 = inputList1;
//    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //        StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
//    //    //        StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
//    //        StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
//    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 3)
//                throw new ArgumentException($"This cache should have exactly 3 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
////            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
//////            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
////            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                //            inputSingle0Result,
//////            inputList0Result,
////            inputList1Result,
////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
////            || inputSingle0Result.HasChanges
//////            || inputList0Result.HasChanges
////            || inputList1Result.HasChanges
//;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple1List3StageBase<
     TInputSingle0, TPreviousSingleCache0,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TResult
 > : StageBase<TResult, CacheId<string, TPreviousSingleCache0, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>>
        

             where TPreviousSingleCache0 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

     where TPreviousListCache2 : class
 where TPreviousItemCache2 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple1List3StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.updateOnRefresh = updateOnRefresh;
        } 
        
        protected abstract Task<IDocument<TResult>> Work(IDocument<TInputSingle0> inputSingle0, ImmutableList<IDocument<TInputList0>> inputList0, ImmutableList<IDocument<TInputList1>> inputList1, ImmutableList<IDocument<TInputList2>> inputList2, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousSingleCache0, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>>> DoInternal([AllowNull] CacheId<string, TPreviousSingleCache0, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputList0(cache?.PreviousCache1, options),
                this.inputList1(cache?.PreviousCache2, options),
                this.inputList2(cache?.PreviousCache3, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache3, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputList0(cache?.PreviousCache1, options),
                this.inputList1(cache?.PreviousCache2, options),
                this.inputList2(cache?.PreviousCache3, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));
            var inputList2Performed = await inputList2Result.Perform;

            var inputList2PerformedListTask = Task.WhenAll(inputList2Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
,              inputList2PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);
            var inputList2PerformedList = await inputList2PerformedListTask.ConfigureAwait(false);


                var work = await this.Work(
                            inputSingle0Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
            inputList2PerformedList.ToImmutableList(),

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputSingle0Performed.cache, inputList0Performed.cache, inputList1Performed.cache, inputList2Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputSingle0Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

            if(inputList2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList2Result.Ids)}");

;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle1List3StageBase<
     TInputSingle0, TPreviousSingleCache0,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousSingleCache0, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>>

 
             where TPreviousSingleCache0 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

     where TPreviousListCache2 : class
 where TPreviousItemCache2 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle1List3StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
            IDocument<TInputSingle0> inputSingle0, 
                ImmutableList<IDocument<TInputList0>> inputList0, 
            ImmutableList<IDocument<TInputList1>> inputList1, 
            ImmutableList<IDocument<TInputList2>> inputList2, 
            OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousSingleCache0, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>>> DoInternal([AllowNull]  CachelessIds<TPreviousSingleCache0, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputList0(cache?.PreviousCache1, options),
                this.inputList1(cache?.PreviousCache2, options),
                this.inputList2(cache?.PreviousCache3, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache3, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        await Task.WhenAll(
                inputSingle0Result.Perform.AsTask(),
                inputList0Result.Perform.AsTask(),
                inputList1Result.Perform.AsTask(),
                inputList2Result.Perform.AsTask(),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));
            var inputList2Performed = await inputList2Result.Perform;

            var inputList2PerformedListTask = Task.WhenAll(inputList2Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
,              inputList2PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);
            var inputList2PerformedList = await inputList2PerformedListTask.ConfigureAwait(false);



                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputSingle0Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
            inputList2PerformedList.ToImmutableList(),
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputSingle0Performed.cache, inputList0Performed.cache, inputList1Performed.cache, inputList2Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputSingle0Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

            if(inputList2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList2Result.Ids)}");

;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle1List3StageBase<
//    // TInputSingle0, TPreviousSingleCache0,
//    //        // TInputList0, TPreviousItemCache0, TPreviousListCache0,
//    // TInputList1, TPreviousItemCache1, TPreviousListCache1,
//    // TInputList2, TPreviousItemCache2, TPreviousListCache2,
//    // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //        private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
//    //    //        private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
//    //        private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
//    //        private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
//    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle1List3StageBase(
//    //        StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
//    //        //        StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
//    //        StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
//    //        StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
//    //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //            this.inputSingle0 = inputSingle0;
//    //    //            this.inputList0 = inputList0;
//    //            this.inputList1 = inputList1;
//    //            this.inputList2 = inputList2;
//    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //        StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
//    //    //        StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
//    //        StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
//    //        StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
//    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 4)
//                throw new ArgumentException($"This cache should have exactly 4 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
////            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
//////            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
////            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
////            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                //            inputSingle0Result,
//////            inputList0Result,
////            inputList1Result,
////            inputList2Result,
////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
////            || inputSingle0Result.HasChanges
//////            || inputList0Result.HasChanges
////            || inputList1Result.HasChanges
////            || inputList2Result.HasChanges
//;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple1List4StageBase<
     TInputSingle0, TPreviousSingleCache0,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TInputList3, TPreviousItemCache3, TPreviousListCache3,
     TResult
 > : StageBase<TResult, CacheId<string, TPreviousSingleCache0, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>>
        

             where TPreviousSingleCache0 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

     where TPreviousListCache2 : class
 where TPreviousItemCache2 : class

     where TPreviousListCache3 : class
 where TPreviousItemCache3 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple1List4StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.inputList3 = inputList3;
                this.updateOnRefresh = updateOnRefresh;
        } 
        
        protected abstract Task<IDocument<TResult>> Work(IDocument<TInputSingle0> inputSingle0, ImmutableList<IDocument<TInputList0>> inputList0, ImmutableList<IDocument<TInputList1>> inputList1, ImmutableList<IDocument<TInputList2>> inputList2, ImmutableList<IDocument<TInputList3>> inputList3, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousSingleCache0, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>>> DoInternal([AllowNull] CacheId<string, TPreviousSingleCache0, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputList0(cache?.PreviousCache1, options),
                this.inputList1(cache?.PreviousCache2, options),
                this.inputList2(cache?.PreviousCache3, options),
                this.inputList3(cache?.PreviousCache4, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache3, options).ConfigureAwait(false);
            var inputList3Result = await this.inputList3(cache?.PreviousCache4, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputList0(cache?.PreviousCache1, options),
                this.inputList1(cache?.PreviousCache2, options),
                this.inputList2(cache?.PreviousCache3, options),
                this.inputList3(cache?.PreviousCache4, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));
            var inputList2Performed = await inputList2Result.Perform;

            var inputList2PerformedListTask = Task.WhenAll(inputList2Performed.result.Select(async x => (await x.Perform).result));
            var inputList3Performed = await inputList3Result.Perform;

            var inputList3PerformedListTask = Task.WhenAll(inputList3Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
,              inputList2PerformedListTask
,              inputList3PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);
            var inputList2PerformedList = await inputList2PerformedListTask.ConfigureAwait(false);
            var inputList3PerformedList = await inputList3PerformedListTask.ConfigureAwait(false);


                var work = await this.Work(
                            inputSingle0Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
            inputList2PerformedList.ToImmutableList(),
            inputList3PerformedList.ToImmutableList(),

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputSingle0Performed.cache, inputList0Performed.cache, inputList1Performed.cache, inputList2Performed.cache, inputList3Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputSingle0Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
            || inputList3Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

            if(inputList2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList2Result.Ids)}");

            if(inputList3Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList3Result.Ids)}");

;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle1List4StageBase<
     TInputSingle0, TPreviousSingleCache0,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TInputList3, TPreviousItemCache3, TPreviousListCache3,
     TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousSingleCache0, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>>

 
             where TPreviousSingleCache0 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

     where TPreviousListCache2 : class
 where TPreviousItemCache2 : class

     where TPreviousListCache3 : class
 where TPreviousItemCache3 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
            private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle1List4StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.inputList3 = inputList3;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
            IDocument<TInputSingle0> inputSingle0, 
                ImmutableList<IDocument<TInputList0>> inputList0, 
            ImmutableList<IDocument<TInputList1>> inputList1, 
            ImmutableList<IDocument<TInputList2>> inputList2, 
            ImmutableList<IDocument<TInputList3>> inputList3, 
            OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousSingleCache0, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>>> DoInternal([AllowNull]  CachelessIds<TPreviousSingleCache0, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputList0(cache?.PreviousCache1, options),
                this.inputList1(cache?.PreviousCache2, options),
                this.inputList2(cache?.PreviousCache3, options),
                this.inputList3(cache?.PreviousCache4, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache3, options).ConfigureAwait(false);
            var inputList3Result = await this.inputList3(cache?.PreviousCache4, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        await Task.WhenAll(
                inputSingle0Result.Perform.AsTask(),
                inputList0Result.Perform.AsTask(),
                inputList1Result.Perform.AsTask(),
                inputList2Result.Perform.AsTask(),
                inputList3Result.Perform.AsTask(),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));
            var inputList2Performed = await inputList2Result.Perform;

            var inputList2PerformedListTask = Task.WhenAll(inputList2Performed.result.Select(async x => (await x.Perform).result));
            var inputList3Performed = await inputList3Result.Perform;

            var inputList3PerformedListTask = Task.WhenAll(inputList3Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
,              inputList2PerformedListTask
,              inputList3PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);
            var inputList2PerformedList = await inputList2PerformedListTask.ConfigureAwait(false);
            var inputList3PerformedList = await inputList3PerformedListTask.ConfigureAwait(false);



                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputSingle0Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
            inputList2PerformedList.ToImmutableList(),
            inputList3PerformedList.ToImmutableList(),
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputSingle0Performed.cache, inputList0Performed.cache, inputList1Performed.cache, inputList2Performed.cache, inputList3Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputSingle0Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
            || inputList3Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

            if(inputList2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList2Result.Ids)}");

            if(inputList3Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList3Result.Ids)}");

;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle1List4StageBase<
//    // TInputSingle0, TPreviousSingleCache0,
//    //        // TInputList0, TPreviousItemCache0, TPreviousListCache0,
//    // TInputList1, TPreviousItemCache1, TPreviousListCache1,
//    // TInputList2, TPreviousItemCache2, TPreviousListCache2,
//    // TInputList3, TPreviousItemCache3, TPreviousListCache3,
//    // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //        private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
//    //    //        private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
//    //        private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
//    //        private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
//    //        private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
//    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle1List4StageBase(
//    //        StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
//    //        //        StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
//    //        StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
//    //        StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
//    //        StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
//    //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //            this.inputSingle0 = inputSingle0;
//    //    //            this.inputList0 = inputList0;
//    //            this.inputList1 = inputList1;
//    //            this.inputList2 = inputList2;
//    //            this.inputList3 = inputList3;
//    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //        StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
//    //    //        StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
//    //        StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
//    //        StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
//    //        StageResultList<TInputList3,TPreviousItemCache3,TPreviousListCache3> inputList3, 
//    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 5)
//                throw new ArgumentException($"This cache should have exactly 5 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
////            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
//////            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
////            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
////            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
////            var inputList3Result = await this.inputList3(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);
////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                //            inputSingle0Result,
//////            inputList0Result,
////            inputList1Result,
////            inputList2Result,
////            inputList3Result,
////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
////            || inputSingle0Result.HasChanges
//////            || inputList0Result.HasChanges
////            || inputList1Result.HasChanges
////            || inputList2Result.HasChanges
////            || inputList3Result.HasChanges
//;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple2List0StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
             TResult
 > : StageBase<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1>>
        

             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
            
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
                private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple2List0StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
                    IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                    this.updateOnRefresh = updateOnRefresh;
        } 
        
        protected abstract Task<IDocument<TResult>> Work(IDocument<TInputSingle0> inputSingle0, IDocument<TInputSingle1> inputSingle1, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1>>> DoInternal([AllowNull] CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;




                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputSingle0Performed.cache, inputSingle1Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle2List0StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
             TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1>>

 
             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
            
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
                private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle2List0StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
                    IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                    this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
            IDocument<TInputSingle0> inputSingle0, 
            IDocument<TInputSingle1> inputSingle1, 
                OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1>>> DoInternal([AllowNull]  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        await Task.WhenAll(
                inputSingle0Result.Perform.AsTask(),
                inputSingle1Result.Perform.AsTask(),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;





                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputSingle0Performed.cache, inputSingle1Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle2List0StageBase<
//    // TInputSingle0, TPreviousSingleCache0,
//    // TInputSingle1, TPreviousSingleCache1,
//    //        // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //        private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
//    //        private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
//    //    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle2List0StageBase(
//    //        StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
//    //        StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
//    //        //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //            this.inputSingle0 = inputSingle0;
//    //            this.inputSingle1 = inputSingle1;
//    //    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //        StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
//    //        StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
//    //    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 2)
//                throw new ArgumentException($"This cache should have exactly 2 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
////            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
////            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
//////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                //            inputSingle0Result,
////            inputSingle1Result,
//////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
////            || inputSingle0Result.HasChanges
////            || inputSingle1Result.HasChanges
////;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple2List1StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TResult
 > : StageBase<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0>>
        

             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple2List1StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                    this.inputList0 = inputList0;
                this.updateOnRefresh = updateOnRefresh;
        } 
        
        protected abstract Task<IDocument<TResult>> Work(IDocument<TInputSingle0> inputSingle0, IDocument<TInputSingle1> inputSingle1, ImmutableList<IDocument<TInputList0>> inputList0, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0>>> DoInternal([AllowNull] CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputList0(cache?.PreviousCache2, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache2, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputList0(cache?.PreviousCache2, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));


            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);


                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputList0PerformedList.ToImmutableList(),

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputSingle0Performed.cache, inputSingle1Performed.cache, inputList0Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputList0Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle2List1StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0>>

 
             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle2List1StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                    this.inputList0 = inputList0;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
            IDocument<TInputSingle0> inputSingle0, 
            IDocument<TInputSingle1> inputSingle1, 
                ImmutableList<IDocument<TInputList0>> inputList0, 
            OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0>>> DoInternal([AllowNull]  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputList0(cache?.PreviousCache2, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache2, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        await Task.WhenAll(
                inputSingle0Result.Perform.AsTask(),
                inputSingle1Result.Perform.AsTask(),
                inputList0Result.Perform.AsTask(),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));


            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);



                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputList0PerformedList.ToImmutableList(),
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputSingle0Performed.cache, inputSingle1Performed.cache, inputList0Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputList0Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle2List1StageBase<
//    // TInputSingle0, TPreviousSingleCache0,
//    // TInputSingle1, TPreviousSingleCache1,
//    //        // TInputList0, TPreviousItemCache0, TPreviousListCache0,
//    // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //        private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
//    //        private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
//    //    //        private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
//    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle2List1StageBase(
//    //        StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
//    //        StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
//    //        //        StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
//    //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //            this.inputSingle0 = inputSingle0;
//    //            this.inputSingle1 = inputSingle1;
//    //    //            this.inputList0 = inputList0;
//    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //        StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
//    //        StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
//    //    //        StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
//    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 3)
//                throw new ArgumentException($"This cache should have exactly 3 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
////            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
////            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
//////            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                //            inputSingle0Result,
////            inputSingle1Result,
//////            inputList0Result,
////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
////            || inputSingle0Result.HasChanges
////            || inputSingle1Result.HasChanges
//////            || inputList0Result.HasChanges
//;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple2List2StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TResult
 > : StageBase<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0, TPreviousListCache1>>
        

             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple2List2StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.updateOnRefresh = updateOnRefresh;
        } 
        
        protected abstract Task<IDocument<TResult>> Work(IDocument<TInputSingle0> inputSingle0, IDocument<TInputSingle1> inputSingle1, ImmutableList<IDocument<TInputList0>> inputList0, ImmutableList<IDocument<TInputList1>> inputList1, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0, TPreviousListCache1>>> DoInternal([AllowNull] CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0, TPreviousListCache1>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputList0(cache?.PreviousCache2, options),
                this.inputList1(cache?.PreviousCache3, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache3, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputList0(cache?.PreviousCache2, options),
                this.inputList1(cache?.PreviousCache3, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);


                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputSingle0Performed.cache, inputSingle1Performed.cache, inputList0Performed.cache, inputList1Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle2List2StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0, TPreviousListCache1>>

 
             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle2List2StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
            IDocument<TInputSingle0> inputSingle0, 
            IDocument<TInputSingle1> inputSingle1, 
                ImmutableList<IDocument<TInputList0>> inputList0, 
            ImmutableList<IDocument<TInputList1>> inputList1, 
            OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0, TPreviousListCache1>>> DoInternal([AllowNull]  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0, TPreviousListCache1>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputList0(cache?.PreviousCache2, options),
                this.inputList1(cache?.PreviousCache3, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache3, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        await Task.WhenAll(
                inputSingle0Result.Perform.AsTask(),
                inputSingle1Result.Perform.AsTask(),
                inputList0Result.Perform.AsTask(),
                inputList1Result.Perform.AsTask(),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);



                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputSingle0Performed.cache, inputSingle1Performed.cache, inputList0Performed.cache, inputList1Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle2List2StageBase<
//    // TInputSingle0, TPreviousSingleCache0,
//    // TInputSingle1, TPreviousSingleCache1,
//    //        // TInputList0, TPreviousItemCache0, TPreviousListCache0,
//    // TInputList1, TPreviousItemCache1, TPreviousListCache1,
//    // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //        private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
//    //        private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
//    //    //        private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
//    //        private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
//    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle2List2StageBase(
//    //        StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
//    //        StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
//    //        //        StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
//    //        StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
//    //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //            this.inputSingle0 = inputSingle0;
//    //            this.inputSingle1 = inputSingle1;
//    //    //            this.inputList0 = inputList0;
//    //            this.inputList1 = inputList1;
//    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //        StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
//    //        StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
//    //    //        StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
//    //        StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
//    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 4)
//                throw new ArgumentException($"This cache should have exactly 4 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
////            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
////            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
//////            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
////            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                //            inputSingle0Result,
////            inputSingle1Result,
//////            inputList0Result,
////            inputList1Result,
////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
////            || inputSingle0Result.HasChanges
////            || inputSingle1Result.HasChanges
//////            || inputList0Result.HasChanges
////            || inputList1Result.HasChanges
//;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple2List3StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TResult
 > : StageBase<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>>
        

             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

     where TPreviousListCache2 : class
 where TPreviousItemCache2 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple2List3StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.updateOnRefresh = updateOnRefresh;
        } 
        
        protected abstract Task<IDocument<TResult>> Work(IDocument<TInputSingle0> inputSingle0, IDocument<TInputSingle1> inputSingle1, ImmutableList<IDocument<TInputList0>> inputList0, ImmutableList<IDocument<TInputList1>> inputList1, ImmutableList<IDocument<TInputList2>> inputList2, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>>> DoInternal([AllowNull] CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputList0(cache?.PreviousCache2, options),
                this.inputList1(cache?.PreviousCache3, options),
                this.inputList2(cache?.PreviousCache4, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache3, options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache4, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputList0(cache?.PreviousCache2, options),
                this.inputList1(cache?.PreviousCache3, options),
                this.inputList2(cache?.PreviousCache4, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));
            var inputList2Performed = await inputList2Result.Perform;

            var inputList2PerformedListTask = Task.WhenAll(inputList2Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
,              inputList2PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);
            var inputList2PerformedList = await inputList2PerformedListTask.ConfigureAwait(false);


                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
            inputList2PerformedList.ToImmutableList(),

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputSingle0Performed.cache, inputSingle1Performed.cache, inputList0Performed.cache, inputList1Performed.cache, inputList2Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

            if(inputList2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList2Result.Ids)}");

;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle2List3StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>>

 
             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

     where TPreviousListCache2 : class
 where TPreviousItemCache2 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle2List3StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
            IDocument<TInputSingle0> inputSingle0, 
            IDocument<TInputSingle1> inputSingle1, 
                ImmutableList<IDocument<TInputList0>> inputList0, 
            ImmutableList<IDocument<TInputList1>> inputList1, 
            ImmutableList<IDocument<TInputList2>> inputList2, 
            OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>>> DoInternal([AllowNull]  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputList0(cache?.PreviousCache2, options),
                this.inputList1(cache?.PreviousCache3, options),
                this.inputList2(cache?.PreviousCache4, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache3, options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache4, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        await Task.WhenAll(
                inputSingle0Result.Perform.AsTask(),
                inputSingle1Result.Perform.AsTask(),
                inputList0Result.Perform.AsTask(),
                inputList1Result.Perform.AsTask(),
                inputList2Result.Perform.AsTask(),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));
            var inputList2Performed = await inputList2Result.Perform;

            var inputList2PerformedListTask = Task.WhenAll(inputList2Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
,              inputList2PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);
            var inputList2PerformedList = await inputList2PerformedListTask.ConfigureAwait(false);



                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
            inputList2PerformedList.ToImmutableList(),
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputSingle0Performed.cache, inputSingle1Performed.cache, inputList0Performed.cache, inputList1Performed.cache, inputList2Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

            if(inputList2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList2Result.Ids)}");

;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle2List3StageBase<
//    // TInputSingle0, TPreviousSingleCache0,
//    // TInputSingle1, TPreviousSingleCache1,
//    //        // TInputList0, TPreviousItemCache0, TPreviousListCache0,
//    // TInputList1, TPreviousItemCache1, TPreviousListCache1,
//    // TInputList2, TPreviousItemCache2, TPreviousListCache2,
//    // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //        private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
//    //        private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
//    //    //        private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
//    //        private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
//    //        private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
//    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle2List3StageBase(
//    //        StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
//    //        StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
//    //        //        StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
//    //        StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
//    //        StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
//    //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //            this.inputSingle0 = inputSingle0;
//    //            this.inputSingle1 = inputSingle1;
//    //    //            this.inputList0 = inputList0;
//    //            this.inputList1 = inputList1;
//    //            this.inputList2 = inputList2;
//    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //        StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
//    //        StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
//    //    //        StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
//    //        StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
//    //        StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
//    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 5)
//                throw new ArgumentException($"This cache should have exactly 5 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
////            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
////            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
//////            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
////            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
////            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);
////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                //            inputSingle0Result,
////            inputSingle1Result,
//////            inputList0Result,
////            inputList1Result,
////            inputList2Result,
////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
////            || inputSingle0Result.HasChanges
////            || inputSingle1Result.HasChanges
//////            || inputList0Result.HasChanges
////            || inputList1Result.HasChanges
////            || inputList2Result.HasChanges
//;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple2List4StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TInputList3, TPreviousItemCache3, TPreviousListCache3,
     TResult
 > : StageBase<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>>
        

             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

     where TPreviousListCache2 : class
 where TPreviousItemCache2 : class

     where TPreviousListCache3 : class
 where TPreviousItemCache3 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple2List4StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.inputList3 = inputList3;
                this.updateOnRefresh = updateOnRefresh;
        } 
        
        protected abstract Task<IDocument<TResult>> Work(IDocument<TInputSingle0> inputSingle0, IDocument<TInputSingle1> inputSingle1, ImmutableList<IDocument<TInputList0>> inputList0, ImmutableList<IDocument<TInputList1>> inputList1, ImmutableList<IDocument<TInputList2>> inputList2, ImmutableList<IDocument<TInputList3>> inputList3, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>>> DoInternal([AllowNull] CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputList0(cache?.PreviousCache2, options),
                this.inputList1(cache?.PreviousCache3, options),
                this.inputList2(cache?.PreviousCache4, options),
                this.inputList3(cache?.PreviousCache5, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache3, options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache4, options).ConfigureAwait(false);
            var inputList3Result = await this.inputList3(cache?.PreviousCache5, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputList0(cache?.PreviousCache2, options),
                this.inputList1(cache?.PreviousCache3, options),
                this.inputList2(cache?.PreviousCache4, options),
                this.inputList3(cache?.PreviousCache5, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));
            var inputList2Performed = await inputList2Result.Perform;

            var inputList2PerformedListTask = Task.WhenAll(inputList2Performed.result.Select(async x => (await x.Perform).result));
            var inputList3Performed = await inputList3Result.Perform;

            var inputList3PerformedListTask = Task.WhenAll(inputList3Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
,              inputList2PerformedListTask
,              inputList3PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);
            var inputList2PerformedList = await inputList2PerformedListTask.ConfigureAwait(false);
            var inputList3PerformedList = await inputList3PerformedListTask.ConfigureAwait(false);


                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
            inputList2PerformedList.ToImmutableList(),
            inputList3PerformedList.ToImmutableList(),

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputSingle0Performed.cache, inputSingle1Performed.cache, inputList0Performed.cache, inputList1Performed.cache, inputList2Performed.cache, inputList3Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
            || inputList3Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

            if(inputList2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList2Result.Ids)}");

            if(inputList3Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList3Result.Ids)}");

;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle2List4StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TInputList3, TPreviousItemCache3, TPreviousListCache3,
     TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>>

 
             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

     where TPreviousListCache2 : class
 where TPreviousItemCache2 : class

     where TPreviousListCache3 : class
 where TPreviousItemCache3 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
            private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle2List4StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.inputList3 = inputList3;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
            IDocument<TInputSingle0> inputSingle0, 
            IDocument<TInputSingle1> inputSingle1, 
                ImmutableList<IDocument<TInputList0>> inputList0, 
            ImmutableList<IDocument<TInputList1>> inputList1, 
            ImmutableList<IDocument<TInputList2>> inputList2, 
            ImmutableList<IDocument<TInputList3>> inputList3, 
            OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>>> DoInternal([AllowNull]  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputList0(cache?.PreviousCache2, options),
                this.inputList1(cache?.PreviousCache3, options),
                this.inputList2(cache?.PreviousCache4, options),
                this.inputList3(cache?.PreviousCache5, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache3, options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache4, options).ConfigureAwait(false);
            var inputList3Result = await this.inputList3(cache?.PreviousCache5, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        await Task.WhenAll(
                inputSingle0Result.Perform.AsTask(),
                inputSingle1Result.Perform.AsTask(),
                inputList0Result.Perform.AsTask(),
                inputList1Result.Perform.AsTask(),
                inputList2Result.Perform.AsTask(),
                inputList3Result.Perform.AsTask(),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));
            var inputList2Performed = await inputList2Result.Perform;

            var inputList2PerformedListTask = Task.WhenAll(inputList2Performed.result.Select(async x => (await x.Perform).result));
            var inputList3Performed = await inputList3Result.Perform;

            var inputList3PerformedListTask = Task.WhenAll(inputList3Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
,              inputList2PerformedListTask
,              inputList3PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);
            var inputList2PerformedList = await inputList2PerformedListTask.ConfigureAwait(false);
            var inputList3PerformedList = await inputList3PerformedListTask.ConfigureAwait(false);



                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
            inputList2PerformedList.ToImmutableList(),
            inputList3PerformedList.ToImmutableList(),
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputSingle0Performed.cache, inputSingle1Performed.cache, inputList0Performed.cache, inputList1Performed.cache, inputList2Performed.cache, inputList3Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
            || inputList3Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

            if(inputList2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList2Result.Ids)}");

            if(inputList3Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList3Result.Ids)}");

;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle2List4StageBase<
//    // TInputSingle0, TPreviousSingleCache0,
//    // TInputSingle1, TPreviousSingleCache1,
//    //        // TInputList0, TPreviousItemCache0, TPreviousListCache0,
//    // TInputList1, TPreviousItemCache1, TPreviousListCache1,
//    // TInputList2, TPreviousItemCache2, TPreviousListCache2,
//    // TInputList3, TPreviousItemCache3, TPreviousListCache3,
//    // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //        private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
//    //        private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
//    //    //        private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
//    //        private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
//    //        private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
//    //        private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
//    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle2List4StageBase(
//    //        StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
//    //        StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
//    //        //        StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
//    //        StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
//    //        StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
//    //        StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
//    //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //            this.inputSingle0 = inputSingle0;
//    //            this.inputSingle1 = inputSingle1;
//    //    //            this.inputList0 = inputList0;
//    //            this.inputList1 = inputList1;
//    //            this.inputList2 = inputList2;
//    //            this.inputList3 = inputList3;
//    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //        StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
//    //        StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
//    //    //        StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
//    //        StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
//    //        StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
//    //        StageResultList<TInputList3,TPreviousItemCache3,TPreviousListCache3> inputList3, 
//    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 6)
//                throw new ArgumentException($"This cache should have exactly 6 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
////            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
////            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
//////            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
////            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
////            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);
////            var inputList3Result = await this.inputList3(cache?.PreviousCache.Span[5], options).ConfigureAwait(false);
////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                //            inputSingle0Result,
////            inputSingle1Result,
//////            inputList0Result,
////            inputList1Result,
////            inputList2Result,
////            inputList3Result,
////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
////            || inputSingle0Result.HasChanges
////            || inputSingle1Result.HasChanges
//////            || inputList0Result.HasChanges
////            || inputList1Result.HasChanges
////            || inputList2Result.HasChanges
////            || inputList3Result.HasChanges
//;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple3List0StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
             TResult
 > : StageBase<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2>>
        

             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
     where TPreviousSingleCache2 : class
            
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
                private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple3List0StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
                    IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                    this.updateOnRefresh = updateOnRefresh;
        } 
        
        protected abstract Task<IDocument<TResult>> Work(IDocument<TInputSingle0> inputSingle0, IDocument<TInputSingle1> inputSingle1, IDocument<TInputSingle2> inputSingle2, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2>>> DoInternal([AllowNull] CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache2, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputSingle2Performed = await inputSingle2Result.Perform;




                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputSingle2Performed.result,

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputSingle0Performed.cache, inputSingle1Performed.cache, inputSingle2Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputSingle2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle2Result.Id}");
;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle3List0StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
             TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2>>

 
             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
     where TPreviousSingleCache2 : class
            
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
                private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle3List0StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
                    IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                    this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
            IDocument<TInputSingle0> inputSingle0, 
            IDocument<TInputSingle1> inputSingle1, 
            IDocument<TInputSingle2> inputSingle2, 
                OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2>>> DoInternal([AllowNull]  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache2, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        await Task.WhenAll(
                inputSingle0Result.Perform.AsTask(),
                inputSingle1Result.Perform.AsTask(),
                inputSingle2Result.Perform.AsTask(),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputSingle2Performed = await inputSingle2Result.Perform;





                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputSingle2Performed.result,
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputSingle0Performed.cache, inputSingle1Performed.cache, inputSingle2Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputSingle2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle2Result.Id}");
;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle3List0StageBase<
//    // TInputSingle0, TPreviousSingleCache0,
//    // TInputSingle1, TPreviousSingleCache1,
//    // TInputSingle2, TPreviousSingleCache2,
//    //        // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //        private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
//    //        private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
//    //        private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
//    //    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle3List0StageBase(
//    //        StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
//    //        StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
//    //        StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
//    //        //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //            this.inputSingle0 = inputSingle0;
//    //            this.inputSingle1 = inputSingle1;
//    //            this.inputSingle2 = inputSingle2;
//    //    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //        StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
//    //        StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
//    //        StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
//    //    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 3)
//                throw new ArgumentException($"This cache should have exactly 3 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
////            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
////            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
////            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
//////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                //            inputSingle0Result,
////            inputSingle1Result,
////            inputSingle2Result,
//////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
////            || inputSingle0Result.HasChanges
////            || inputSingle1Result.HasChanges
////            || inputSingle2Result.HasChanges
////;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple3List1StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TResult
 > : StageBase<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0>>
        

             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
     where TPreviousSingleCache2 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple3List1StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                    this.inputList0 = inputList0;
                this.updateOnRefresh = updateOnRefresh;
        } 
        
        protected abstract Task<IDocument<TResult>> Work(IDocument<TInputSingle0> inputSingle0, IDocument<TInputSingle1> inputSingle1, IDocument<TInputSingle2> inputSingle2, ImmutableList<IDocument<TInputList0>> inputList0, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0>>> DoInternal([AllowNull] CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputList0(cache?.PreviousCache3, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache3, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputList0(cache?.PreviousCache3, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputSingle2Performed = await inputSingle2Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));


            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);


                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputSingle2Performed.result,
            inputList0PerformedList.ToImmutableList(),

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputSingle0Performed.cache, inputSingle1Performed.cache, inputSingle2Performed.cache, inputList0Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputList0Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputSingle2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle2Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle3List1StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0>>

 
             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
     where TPreviousSingleCache2 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle3List1StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                    this.inputList0 = inputList0;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
            IDocument<TInputSingle0> inputSingle0, 
            IDocument<TInputSingle1> inputSingle1, 
            IDocument<TInputSingle2> inputSingle2, 
                ImmutableList<IDocument<TInputList0>> inputList0, 
            OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0>>> DoInternal([AllowNull]  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputList0(cache?.PreviousCache3, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache3, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        await Task.WhenAll(
                inputSingle0Result.Perform.AsTask(),
                inputSingle1Result.Perform.AsTask(),
                inputSingle2Result.Perform.AsTask(),
                inputList0Result.Perform.AsTask(),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputSingle2Performed = await inputSingle2Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));


            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);



                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputSingle2Performed.result,
            inputList0PerformedList.ToImmutableList(),
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputSingle0Performed.cache, inputSingle1Performed.cache, inputSingle2Performed.cache, inputList0Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputList0Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputSingle2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle2Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle3List1StageBase<
//    // TInputSingle0, TPreviousSingleCache0,
//    // TInputSingle1, TPreviousSingleCache1,
//    // TInputSingle2, TPreviousSingleCache2,
//    //        // TInputList0, TPreviousItemCache0, TPreviousListCache0,
//    // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //        private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
//    //        private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
//    //        private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
//    //    //        private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
//    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle3List1StageBase(
//    //        StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
//    //        StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
//    //        StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
//    //        //        StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
//    //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //            this.inputSingle0 = inputSingle0;
//    //            this.inputSingle1 = inputSingle1;
//    //            this.inputSingle2 = inputSingle2;
//    //    //            this.inputList0 = inputList0;
//    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //        StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
//    //        StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
//    //        StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
//    //    //        StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
//    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 4)
//                throw new ArgumentException($"This cache should have exactly 4 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
////            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
////            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
////            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
//////            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                //            inputSingle0Result,
////            inputSingle1Result,
////            inputSingle2Result,
//////            inputList0Result,
////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
////            || inputSingle0Result.HasChanges
////            || inputSingle1Result.HasChanges
////            || inputSingle2Result.HasChanges
//////            || inputList0Result.HasChanges
//;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple3List2StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TResult
 > : StageBase<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0, TPreviousListCache1>>
        

             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
     where TPreviousSingleCache2 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple3List2StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.updateOnRefresh = updateOnRefresh;
        } 
        
        protected abstract Task<IDocument<TResult>> Work(IDocument<TInputSingle0> inputSingle0, IDocument<TInputSingle1> inputSingle1, IDocument<TInputSingle2> inputSingle2, ImmutableList<IDocument<TInputList0>> inputList0, ImmutableList<IDocument<TInputList1>> inputList1, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0, TPreviousListCache1>>> DoInternal([AllowNull] CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0, TPreviousListCache1>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputList0(cache?.PreviousCache3, options),
                this.inputList1(cache?.PreviousCache4, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache3, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache4, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputList0(cache?.PreviousCache3, options),
                this.inputList1(cache?.PreviousCache4, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputSingle2Performed = await inputSingle2Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);


                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputSingle2Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputSingle0Performed.cache, inputSingle1Performed.cache, inputSingle2Performed.cache, inputList0Performed.cache, inputList1Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputSingle2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle2Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle3List2StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0, TPreviousListCache1>>

 
             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
     where TPreviousSingleCache2 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle3List2StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
            IDocument<TInputSingle0> inputSingle0, 
            IDocument<TInputSingle1> inputSingle1, 
            IDocument<TInputSingle2> inputSingle2, 
                ImmutableList<IDocument<TInputList0>> inputList0, 
            ImmutableList<IDocument<TInputList1>> inputList1, 
            OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0, TPreviousListCache1>>> DoInternal([AllowNull]  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0, TPreviousListCache1>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputList0(cache?.PreviousCache3, options),
                this.inputList1(cache?.PreviousCache4, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache3, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache4, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        await Task.WhenAll(
                inputSingle0Result.Perform.AsTask(),
                inputSingle1Result.Perform.AsTask(),
                inputSingle2Result.Perform.AsTask(),
                inputList0Result.Perform.AsTask(),
                inputList1Result.Perform.AsTask(),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputSingle2Performed = await inputSingle2Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);



                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputSingle2Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputSingle0Performed.cache, inputSingle1Performed.cache, inputSingle2Performed.cache, inputList0Performed.cache, inputList1Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputSingle2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle2Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle3List2StageBase<
//    // TInputSingle0, TPreviousSingleCache0,
//    // TInputSingle1, TPreviousSingleCache1,
//    // TInputSingle2, TPreviousSingleCache2,
//    //        // TInputList0, TPreviousItemCache0, TPreviousListCache0,
//    // TInputList1, TPreviousItemCache1, TPreviousListCache1,
//    // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //        private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
//    //        private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
//    //        private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
//    //    //        private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
//    //        private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
//    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle3List2StageBase(
//    //        StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
//    //        StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
//    //        StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
//    //        //        StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
//    //        StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
//    //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //            this.inputSingle0 = inputSingle0;
//    //            this.inputSingle1 = inputSingle1;
//    //            this.inputSingle2 = inputSingle2;
//    //    //            this.inputList0 = inputList0;
//    //            this.inputList1 = inputList1;
//    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //        StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
//    //        StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
//    //        StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
//    //    //        StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
//    //        StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
//    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 5)
//                throw new ArgumentException($"This cache should have exactly 5 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
////            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
////            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
////            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
//////            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
////            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);
////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                //            inputSingle0Result,
////            inputSingle1Result,
////            inputSingle2Result,
//////            inputList0Result,
////            inputList1Result,
////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
////            || inputSingle0Result.HasChanges
////            || inputSingle1Result.HasChanges
////            || inputSingle2Result.HasChanges
//////            || inputList0Result.HasChanges
////            || inputList1Result.HasChanges
//;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple3List3StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TResult
 > : StageBase<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>>
        

             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
     where TPreviousSingleCache2 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

     where TPreviousListCache2 : class
 where TPreviousItemCache2 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple3List3StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.updateOnRefresh = updateOnRefresh;
        } 
        
        protected abstract Task<IDocument<TResult>> Work(IDocument<TInputSingle0> inputSingle0, IDocument<TInputSingle1> inputSingle1, IDocument<TInputSingle2> inputSingle2, ImmutableList<IDocument<TInputList0>> inputList0, ImmutableList<IDocument<TInputList1>> inputList1, ImmutableList<IDocument<TInputList2>> inputList2, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>>> DoInternal([AllowNull] CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputList0(cache?.PreviousCache3, options),
                this.inputList1(cache?.PreviousCache4, options),
                this.inputList2(cache?.PreviousCache5, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache3, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache4, options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache5, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputList0(cache?.PreviousCache3, options),
                this.inputList1(cache?.PreviousCache4, options),
                this.inputList2(cache?.PreviousCache5, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputSingle2Performed = await inputSingle2Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));
            var inputList2Performed = await inputList2Result.Perform;

            var inputList2PerformedListTask = Task.WhenAll(inputList2Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
,              inputList2PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);
            var inputList2PerformedList = await inputList2PerformedListTask.ConfigureAwait(false);


                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputSingle2Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
            inputList2PerformedList.ToImmutableList(),

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputSingle0Performed.cache, inputSingle1Performed.cache, inputSingle2Performed.cache, inputList0Performed.cache, inputList1Performed.cache, inputList2Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputSingle2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle2Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

            if(inputList2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList2Result.Ids)}");

;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle3List3StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>>

 
             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
     where TPreviousSingleCache2 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

     where TPreviousListCache2 : class
 where TPreviousItemCache2 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle3List3StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.inputList2 = inputList2;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
            IDocument<TInputSingle0> inputSingle0, 
            IDocument<TInputSingle1> inputSingle1, 
            IDocument<TInputSingle2> inputSingle2, 
                ImmutableList<IDocument<TInputList0>> inputList0, 
            ImmutableList<IDocument<TInputList1>> inputList1, 
            ImmutableList<IDocument<TInputList2>> inputList2, 
            OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>>> DoInternal([AllowNull]  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputList0(cache?.PreviousCache3, options),
                this.inputList1(cache?.PreviousCache4, options),
                this.inputList2(cache?.PreviousCache5, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache3, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache4, options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache5, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        await Task.WhenAll(
                inputSingle0Result.Perform.AsTask(),
                inputSingle1Result.Perform.AsTask(),
                inputSingle2Result.Perform.AsTask(),
                inputList0Result.Perform.AsTask(),
                inputList1Result.Perform.AsTask(),
                inputList2Result.Perform.AsTask(),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputSingle2Performed = await inputSingle2Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));
            var inputList2Performed = await inputList2Result.Perform;

            var inputList2PerformedListTask = Task.WhenAll(inputList2Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
,              inputList2PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);
            var inputList2PerformedList = await inputList2PerformedListTask.ConfigureAwait(false);



                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputSingle2Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
            inputList2PerformedList.ToImmutableList(),
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputSingle0Performed.cache, inputSingle1Performed.cache, inputSingle2Performed.cache, inputList0Performed.cache, inputList1Performed.cache, inputList2Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputSingle2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle2Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

            if(inputList2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList2Result.Ids)}");

;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle3List3StageBase<
//    // TInputSingle0, TPreviousSingleCache0,
//    // TInputSingle1, TPreviousSingleCache1,
//    // TInputSingle2, TPreviousSingleCache2,
//    //        // TInputList0, TPreviousItemCache0, TPreviousListCache0,
//    // TInputList1, TPreviousItemCache1, TPreviousListCache1,
//    // TInputList2, TPreviousItemCache2, TPreviousListCache2,
//    // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //        private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
//    //        private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
//    //        private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
//    //    //        private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
//    //        private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
//    //        private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
//    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle3List3StageBase(
//    //        StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
//    //        StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
//    //        StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
//    //        //        StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
//    //        StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
//    //        StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
//    //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //            this.inputSingle0 = inputSingle0;
//    //            this.inputSingle1 = inputSingle1;
//    //            this.inputSingle2 = inputSingle2;
//    //    //            this.inputList0 = inputList0;
//    //            this.inputList1 = inputList1;
//    //            this.inputList2 = inputList2;
//    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //        StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
//    //        StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
//    //        StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
//    //    //        StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
//    //        StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
//    //        StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
//    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 6)
//                throw new ArgumentException($"This cache should have exactly 6 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
////            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
////            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
////            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
//////            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
////            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);
////            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[5], options).ConfigureAwait(false);
////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                //            inputSingle0Result,
////            inputSingle1Result,
////            inputSingle2Result,
//////            inputList0Result,
////            inputList1Result,
////            inputList2Result,
////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
////            || inputSingle0Result.HasChanges
////            || inputSingle1Result.HasChanges
////            || inputSingle2Result.HasChanges
//////            || inputList0Result.HasChanges
////            || inputList1Result.HasChanges
////            || inputList2Result.HasChanges
//;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple3List4StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TInputList3, TPreviousItemCache3, TPreviousListCache3,
     TResult
 > : StageBase<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>>
        

             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
     where TPreviousSingleCache2 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

     where TPreviousListCache2 : class
 where TPreviousItemCache2 : class

     where TPreviousListCache3 : class
 where TPreviousItemCache3 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple3List4StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
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
        
        protected abstract Task<IDocument<TResult>> Work(IDocument<TInputSingle0> inputSingle0, IDocument<TInputSingle1> inputSingle1, IDocument<TInputSingle2> inputSingle2, ImmutableList<IDocument<TInputList0>> inputList0, ImmutableList<IDocument<TInputList1>> inputList1, ImmutableList<IDocument<TInputList2>> inputList2, ImmutableList<IDocument<TInputList3>> inputList3, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>>> DoInternal([AllowNull] CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputList0(cache?.PreviousCache3, options),
                this.inputList1(cache?.PreviousCache4, options),
                this.inputList2(cache?.PreviousCache5, options),
                this.inputList3(cache?.PreviousCache6, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache3, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache4, options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache5, options).ConfigureAwait(false);
            var inputList3Result = await this.inputList3(cache?.PreviousCache6, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputList0(cache?.PreviousCache3, options),
                this.inputList1(cache?.PreviousCache4, options),
                this.inputList2(cache?.PreviousCache5, options),
                this.inputList3(cache?.PreviousCache6, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputSingle2Performed = await inputSingle2Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));
            var inputList2Performed = await inputList2Result.Perform;

            var inputList2PerformedListTask = Task.WhenAll(inputList2Performed.result.Select(async x => (await x.Perform).result));
            var inputList3Performed = await inputList3Result.Perform;

            var inputList3PerformedListTask = Task.WhenAll(inputList3Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
,              inputList2PerformedListTask
,              inputList3PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);
            var inputList2PerformedList = await inputList2PerformedListTask.ConfigureAwait(false);
            var inputList3PerformedList = await inputList3PerformedListTask.ConfigureAwait(false);


                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputSingle2Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
            inputList2PerformedList.ToImmutableList(),
            inputList3PerformedList.ToImmutableList(),

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputSingle0Performed.cache, inputSingle1Performed.cache, inputSingle2Performed.cache, inputList0Performed.cache, inputList1Performed.cache, inputList2Performed.cache, inputList3Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
            || inputList3Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputSingle2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle2Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

            if(inputList2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList2Result.Ids)}");

            if(inputList3Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList3Result.Ids)}");

;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle3List4StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TInputList3, TPreviousItemCache3, TPreviousListCache3,
     TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>>

 
             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
     where TPreviousSingleCache2 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

     where TPreviousListCache2 : class
 where TPreviousItemCache2 : class

     where TPreviousListCache3 : class
 where TPreviousItemCache3 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
            private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle3List4StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
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

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
            IDocument<TInputSingle0> inputSingle0, 
            IDocument<TInputSingle1> inputSingle1, 
            IDocument<TInputSingle2> inputSingle2, 
                ImmutableList<IDocument<TInputList0>> inputList0, 
            ImmutableList<IDocument<TInputList1>> inputList1, 
            ImmutableList<IDocument<TInputList2>> inputList2, 
            ImmutableList<IDocument<TInputList3>> inputList3, 
            OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>>> DoInternal([AllowNull]  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputList0(cache?.PreviousCache3, options),
                this.inputList1(cache?.PreviousCache4, options),
                this.inputList2(cache?.PreviousCache5, options),
                this.inputList3(cache?.PreviousCache6, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache3, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache4, options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache5, options).ConfigureAwait(false);
            var inputList3Result = await this.inputList3(cache?.PreviousCache6, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        await Task.WhenAll(
                inputSingle0Result.Perform.AsTask(),
                inputSingle1Result.Perform.AsTask(),
                inputSingle2Result.Perform.AsTask(),
                inputList0Result.Perform.AsTask(),
                inputList1Result.Perform.AsTask(),
                inputList2Result.Perform.AsTask(),
                inputList3Result.Perform.AsTask(),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputSingle2Performed = await inputSingle2Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));
            var inputList2Performed = await inputList2Result.Perform;

            var inputList2PerformedListTask = Task.WhenAll(inputList2Performed.result.Select(async x => (await x.Perform).result));
            var inputList3Performed = await inputList3Result.Perform;

            var inputList3PerformedListTask = Task.WhenAll(inputList3Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
,              inputList2PerformedListTask
,              inputList3PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);
            var inputList2PerformedList = await inputList2PerformedListTask.ConfigureAwait(false);
            var inputList3PerformedList = await inputList3PerformedListTask.ConfigureAwait(false);



                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputSingle2Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
            inputList2PerformedList.ToImmutableList(),
            inputList3PerformedList.ToImmutableList(),
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputSingle0Performed.cache, inputSingle1Performed.cache, inputSingle2Performed.cache, inputList0Performed.cache, inputList1Performed.cache, inputList2Performed.cache, inputList3Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
            || inputList3Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputSingle2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle2Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

            if(inputList2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList2Result.Ids)}");

            if(inputList3Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList3Result.Ids)}");

;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle3List4StageBase<
//    // TInputSingle0, TPreviousSingleCache0,
//    // TInputSingle1, TPreviousSingleCache1,
//    // TInputSingle2, TPreviousSingleCache2,
//    //        // TInputList0, TPreviousItemCache0, TPreviousListCache0,
//    // TInputList1, TPreviousItemCache1, TPreviousListCache1,
//    // TInputList2, TPreviousItemCache2, TPreviousListCache2,
//    // TInputList3, TPreviousItemCache3, TPreviousListCache3,
//    // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //        private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
//    //        private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
//    //        private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
//    //    //        private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
//    //        private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
//    //        private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
//    //        private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
//    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle3List4StageBase(
//    //        StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
//    //        StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
//    //        StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
//    //        //        StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
//    //        StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
//    //        StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
//    //        StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
//    //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //            this.inputSingle0 = inputSingle0;
//    //            this.inputSingle1 = inputSingle1;
//    //            this.inputSingle2 = inputSingle2;
//    //    //            this.inputList0 = inputList0;
//    //            this.inputList1 = inputList1;
//    //            this.inputList2 = inputList2;
//    //            this.inputList3 = inputList3;
//    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //        StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
//    //        StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
//    //        StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
//    //    //        StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
//    //        StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
//    //        StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
//    //        StageResultList<TInputList3,TPreviousItemCache3,TPreviousListCache3> inputList3, 
//    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 7)
//                throw new ArgumentException($"This cache should have exactly 7 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
////            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
////            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
////            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
//////            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
////            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);
////            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[5], options).ConfigureAwait(false);
////            var inputList3Result = await this.inputList3(cache?.PreviousCache.Span[6], options).ConfigureAwait(false);
////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                //            inputSingle0Result,
////            inputSingle1Result,
////            inputSingle2Result,
//////            inputList0Result,
////            inputList1Result,
////            inputList2Result,
////            inputList3Result,
////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
////            || inputSingle0Result.HasChanges
////            || inputSingle1Result.HasChanges
////            || inputSingle2Result.HasChanges
//////            || inputList0Result.HasChanges
////            || inputList1Result.HasChanges
////            || inputList2Result.HasChanges
////            || inputList3Result.HasChanges
//;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple4List0StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
     TInputSingle3, TPreviousSingleCache3,
             TResult
 > : StageBase<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3>>
        

             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
     where TPreviousSingleCache2 : class
     where TPreviousSingleCache3 : class
            
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
            private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
                private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple4List0StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
            StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
                    IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                this.inputSingle3 = inputSingle3;
                    this.updateOnRefresh = updateOnRefresh;
        } 
        
        protected abstract Task<IDocument<TResult>> Work(IDocument<TInputSingle0> inputSingle0, IDocument<TInputSingle1> inputSingle1, IDocument<TInputSingle2> inputSingle2, IDocument<TInputSingle3> inputSingle3, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3>>> DoInternal([AllowNull] CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputSingle3(cache?.PreviousCache3, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache3, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputSingle3(cache?.PreviousCache3, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputSingle2Performed = await inputSingle2Result.Perform;
            var inputSingle3Performed = await inputSingle3Result.Perform;




                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputSingle2Performed.result,
            inputSingle3Performed.result,

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputSingle0Performed.cache, inputSingle1Performed.cache, inputSingle2Performed.cache, inputSingle3Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputSingle3Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputSingle2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle2Result.Id}");
            if(inputSingle3Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle3Result.Id}");
;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle4List0StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
     TInputSingle3, TPreviousSingleCache3,
             TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3>>

 
             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
     where TPreviousSingleCache2 : class
     where TPreviousSingleCache3 : class
            
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
            private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
                private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle4List0StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
            StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
                    IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                this.inputSingle3 = inputSingle3;
                    this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
            IDocument<TInputSingle0> inputSingle0, 
            IDocument<TInputSingle1> inputSingle1, 
            IDocument<TInputSingle2> inputSingle2, 
            IDocument<TInputSingle3> inputSingle3, 
                OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3>>> DoInternal([AllowNull]  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputSingle3(cache?.PreviousCache3, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache3, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        await Task.WhenAll(
                inputSingle0Result.Perform.AsTask(),
                inputSingle1Result.Perform.AsTask(),
                inputSingle2Result.Perform.AsTask(),
                inputSingle3Result.Perform.AsTask(),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputSingle2Performed = await inputSingle2Result.Perform;
            var inputSingle3Performed = await inputSingle3Result.Perform;





                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputSingle2Performed.result,
            inputSingle3Performed.result,
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputSingle0Performed.cache, inputSingle1Performed.cache, inputSingle2Performed.cache, inputSingle3Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputSingle3Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputSingle2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle2Result.Id}");
            if(inputSingle3Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle3Result.Id}");
;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle4List0StageBase<
//    // TInputSingle0, TPreviousSingleCache0,
//    // TInputSingle1, TPreviousSingleCache1,
//    // TInputSingle2, TPreviousSingleCache2,
//    // TInputSingle3, TPreviousSingleCache3,
//    //        // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //        private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
//    //        private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
//    //        private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
//    //        private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
//    //    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle4List0StageBase(
//    //        StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
//    //        StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
//    //        StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
//    //        StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
//    //        //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //            this.inputSingle0 = inputSingle0;
//    //            this.inputSingle1 = inputSingle1;
//    //            this.inputSingle2 = inputSingle2;
//    //            this.inputSingle3 = inputSingle3;
//    //    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //        StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
//    //        StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
//    //        StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
//    //        StageResult<TInputSingle3,TPreviousSingleCache3> inputSingle3, 
//    //    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 4)
//                throw new ArgumentException($"This cache should have exactly 4 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
////            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
////            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
////            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
////            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
//////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                //            inputSingle0Result,
////            inputSingle1Result,
////            inputSingle2Result,
////            inputSingle3Result,
//////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
////            || inputSingle0Result.HasChanges
////            || inputSingle1Result.HasChanges
////            || inputSingle2Result.HasChanges
////            || inputSingle3Result.HasChanges
////;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple4List1StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
     TInputSingle3, TPreviousSingleCache3,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TResult
 > : StageBase<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0>>
        

             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
     where TPreviousSingleCache2 : class
     where TPreviousSingleCache3 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
            private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple4List1StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
            StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                this.inputSingle3 = inputSingle3;
                    this.inputList0 = inputList0;
                this.updateOnRefresh = updateOnRefresh;
        } 
        
        protected abstract Task<IDocument<TResult>> Work(IDocument<TInputSingle0> inputSingle0, IDocument<TInputSingle1> inputSingle1, IDocument<TInputSingle2> inputSingle2, IDocument<TInputSingle3> inputSingle3, ImmutableList<IDocument<TInputList0>> inputList0, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0>>> DoInternal([AllowNull] CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputSingle3(cache?.PreviousCache3, options),
                this.inputList0(cache?.PreviousCache4, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache3, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache4, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputSingle3(cache?.PreviousCache3, options),
                this.inputList0(cache?.PreviousCache4, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputSingle2Performed = await inputSingle2Result.Perform;
            var inputSingle3Performed = await inputSingle3Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));


            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);


                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputSingle2Performed.result,
            inputSingle3Performed.result,
            inputList0PerformedList.ToImmutableList(),

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputSingle0Performed.cache, inputSingle1Performed.cache, inputSingle2Performed.cache, inputSingle3Performed.cache, inputList0Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputSingle3Result.HasChanges
            || inputList0Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputSingle2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle2Result.Id}");
            if(inputSingle3Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle3Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle4List1StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
     TInputSingle3, TPreviousSingleCache3,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0>>

 
             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
     where TPreviousSingleCache2 : class
     where TPreviousSingleCache3 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
            private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle4List1StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
            StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                this.inputSingle3 = inputSingle3;
                    this.inputList0 = inputList0;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
            IDocument<TInputSingle0> inputSingle0, 
            IDocument<TInputSingle1> inputSingle1, 
            IDocument<TInputSingle2> inputSingle2, 
            IDocument<TInputSingle3> inputSingle3, 
                ImmutableList<IDocument<TInputList0>> inputList0, 
            OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0>>> DoInternal([AllowNull]  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputSingle3(cache?.PreviousCache3, options),
                this.inputList0(cache?.PreviousCache4, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache3, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache4, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        await Task.WhenAll(
                inputSingle0Result.Perform.AsTask(),
                inputSingle1Result.Perform.AsTask(),
                inputSingle2Result.Perform.AsTask(),
                inputSingle3Result.Perform.AsTask(),
                inputList0Result.Perform.AsTask(),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputSingle2Performed = await inputSingle2Result.Perform;
            var inputSingle3Performed = await inputSingle3Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));


            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);



                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputSingle2Performed.result,
            inputSingle3Performed.result,
            inputList0PerformedList.ToImmutableList(),
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputSingle0Performed.cache, inputSingle1Performed.cache, inputSingle2Performed.cache, inputSingle3Performed.cache, inputList0Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputSingle3Result.HasChanges
            || inputList0Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputSingle2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle2Result.Id}");
            if(inputSingle3Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle3Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle4List1StageBase<
//    // TInputSingle0, TPreviousSingleCache0,
//    // TInputSingle1, TPreviousSingleCache1,
//    // TInputSingle2, TPreviousSingleCache2,
//    // TInputSingle3, TPreviousSingleCache3,
//    //        // TInputList0, TPreviousItemCache0, TPreviousListCache0,
//    // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //        private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
//    //        private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
//    //        private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
//    //        private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
//    //    //        private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
//    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle4List1StageBase(
//    //        StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
//    //        StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
//    //        StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
//    //        StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
//    //        //        StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
//    //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //            this.inputSingle0 = inputSingle0;
//    //            this.inputSingle1 = inputSingle1;
//    //            this.inputSingle2 = inputSingle2;
//    //            this.inputSingle3 = inputSingle3;
//    //    //            this.inputList0 = inputList0;
//    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //        StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
//    //        StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
//    //        StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
//    //        StageResult<TInputSingle3,TPreviousSingleCache3> inputSingle3, 
//    //    //        StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
//    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 5)
//                throw new ArgumentException($"This cache should have exactly 5 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
////            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
////            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
////            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
////            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
//////            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);
////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                //            inputSingle0Result,
////            inputSingle1Result,
////            inputSingle2Result,
////            inputSingle3Result,
//////            inputList0Result,
////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
////            || inputSingle0Result.HasChanges
////            || inputSingle1Result.HasChanges
////            || inputSingle2Result.HasChanges
////            || inputSingle3Result.HasChanges
//////            || inputList0Result.HasChanges
//;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple4List2StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
     TInputSingle3, TPreviousSingleCache3,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TResult
 > : StageBase<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0, TPreviousListCache1>>
        

             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
     where TPreviousSingleCache2 : class
     where TPreviousSingleCache3 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
            private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple4List2StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
            StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                this.inputSingle3 = inputSingle3;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.updateOnRefresh = updateOnRefresh;
        } 
        
        protected abstract Task<IDocument<TResult>> Work(IDocument<TInputSingle0> inputSingle0, IDocument<TInputSingle1> inputSingle1, IDocument<TInputSingle2> inputSingle2, IDocument<TInputSingle3> inputSingle3, ImmutableList<IDocument<TInputList0>> inputList0, ImmutableList<IDocument<TInputList1>> inputList1, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0, TPreviousListCache1>>> DoInternal([AllowNull] CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0, TPreviousListCache1>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputSingle3(cache?.PreviousCache3, options),
                this.inputList0(cache?.PreviousCache4, options),
                this.inputList1(cache?.PreviousCache5, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache3, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache4, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache5, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputSingle3(cache?.PreviousCache3, options),
                this.inputList0(cache?.PreviousCache4, options),
                this.inputList1(cache?.PreviousCache5, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputSingle2Performed = await inputSingle2Result.Perform;
            var inputSingle3Performed = await inputSingle3Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);


                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputSingle2Performed.result,
            inputSingle3Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputSingle0Performed.cache, inputSingle1Performed.cache, inputSingle2Performed.cache, inputSingle3Performed.cache, inputList0Performed.cache, inputList1Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputSingle3Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputSingle2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle2Result.Id}");
            if(inputSingle3Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle3Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle4List2StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
     TInputSingle3, TPreviousSingleCache3,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0, TPreviousListCache1>>

 
             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
     where TPreviousSingleCache2 : class
     where TPreviousSingleCache3 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
            private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle4List2StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
            StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
        {
                this.inputSingle0 = inputSingle0;
                this.inputSingle1 = inputSingle1;
                this.inputSingle2 = inputSingle2;
                this.inputSingle3 = inputSingle3;
                    this.inputList0 = inputList0;
                this.inputList1 = inputList1;
                this.updateOnRefresh = updateOnRefresh;
        } 

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
            IDocument<TInputSingle0> inputSingle0, 
            IDocument<TInputSingle1> inputSingle1, 
            IDocument<TInputSingle2> inputSingle2, 
            IDocument<TInputSingle3> inputSingle3, 
                ImmutableList<IDocument<TInputList0>> inputList0, 
            ImmutableList<IDocument<TInputList1>> inputList1, 
            OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0, TPreviousListCache1>>> DoInternal([AllowNull]  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0, TPreviousListCache1>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputSingle3(cache?.PreviousCache3, options),
                this.inputList0(cache?.PreviousCache4, options),
                this.inputList1(cache?.PreviousCache5, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache3, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache4, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache5, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        await Task.WhenAll(
                inputSingle0Result.Perform.AsTask(),
                inputSingle1Result.Perform.AsTask(),
                inputSingle2Result.Perform.AsTask(),
                inputSingle3Result.Perform.AsTask(),
                inputList0Result.Perform.AsTask(),
                inputList1Result.Perform.AsTask(),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputSingle2Performed = await inputSingle2Result.Perform;
            var inputSingle3Performed = await inputSingle3Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);



                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputSingle2Performed.result,
            inputSingle3Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputSingle0Performed.cache, inputSingle1Performed.cache, inputSingle2Performed.cache, inputSingle3Performed.cache, inputList0Performed.cache, inputList1Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputSingle3Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputSingle2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle2Result.Id}");
            if(inputSingle3Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle3Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle4List2StageBase<
//    // TInputSingle0, TPreviousSingleCache0,
//    // TInputSingle1, TPreviousSingleCache1,
//    // TInputSingle2, TPreviousSingleCache2,
//    // TInputSingle3, TPreviousSingleCache3,
//    //        // TInputList0, TPreviousItemCache0, TPreviousListCache0,
//    // TInputList1, TPreviousItemCache1, TPreviousListCache1,
//    // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //        private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
//    //        private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
//    //        private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
//    //        private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
//    //    //        private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
//    //        private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
//    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle4List2StageBase(
//    //        StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
//    //        StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
//    //        StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
//    //        StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
//    //        //        StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
//    //        StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
//    //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //            this.inputSingle0 = inputSingle0;
//    //            this.inputSingle1 = inputSingle1;
//    //            this.inputSingle2 = inputSingle2;
//    //            this.inputSingle3 = inputSingle3;
//    //    //            this.inputList0 = inputList0;
//    //            this.inputList1 = inputList1;
//    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //        StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
//    //        StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
//    //        StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
//    //        StageResult<TInputSingle3,TPreviousSingleCache3> inputSingle3, 
//    //    //        StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
//    //        StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
//    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 6)
//                throw new ArgumentException($"This cache should have exactly 6 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
////            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
////            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
////            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
////            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
//////            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);
////            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[5], options).ConfigureAwait(false);
////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                //            inputSingle0Result,
////            inputSingle1Result,
////            inputSingle2Result,
////            inputSingle3Result,
//////            inputList0Result,
////            inputList1Result,
////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
////            || inputSingle0Result.HasChanges
////            || inputSingle1Result.HasChanges
////            || inputSingle2Result.HasChanges
////            || inputSingle3Result.HasChanges
//////            || inputList0Result.HasChanges
////            || inputList1Result.HasChanges
//;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple4List3StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
     TInputSingle3, TPreviousSingleCache3,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TResult
 > : StageBase<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>>
        

             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
     where TPreviousSingleCache2 : class
     where TPreviousSingleCache3 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

     where TPreviousListCache2 : class
 where TPreviousItemCache2 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
            private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly bool updateOnRefresh;

        public OutputSingleInputSingleSimple4List3StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
            StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
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
        
        protected abstract Task<IDocument<TResult>> Work(IDocument<TInputSingle0> inputSingle0, IDocument<TInputSingle1> inputSingle1, IDocument<TInputSingle2> inputSingle2, IDocument<TInputSingle3> inputSingle3, ImmutableList<IDocument<TInputList0>> inputList0, ImmutableList<IDocument<TInputList1>> inputList1, ImmutableList<IDocument<TInputList2>> inputList2, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>>> DoInternal([AllowNull] CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputSingle3(cache?.PreviousCache3, options),
                this.inputList0(cache?.PreviousCache4, options),
                this.inputList1(cache?.PreviousCache5, options),
                this.inputList2(cache?.PreviousCache6, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache3, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache4, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache5, options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache6, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputSingle3(cache?.PreviousCache3, options),
                this.inputList0(cache?.PreviousCache4, options),
                this.inputList1(cache?.PreviousCache5, options),
                this.inputList2(cache?.PreviousCache6, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputSingle2Performed = await inputSingle2Result.Perform;
            var inputSingle3Performed = await inputSingle3Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));
            var inputList2Performed = await inputList2Result.Perform;

            var inputList2PerformedListTask = Task.WhenAll(inputList2Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
,              inputList2PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);
            var inputList2PerformedList = await inputList2PerformedListTask.ConfigureAwait(false);


                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputSingle2Performed.result,
            inputSingle3Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
            inputList2PerformedList.ToImmutableList(),

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputSingle0Performed.cache, inputSingle1Performed.cache, inputSingle2Performed.cache, inputSingle3Performed.cache, inputList0Performed.cache, inputList1Performed.cache, inputList2Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputSingle3Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputSingle2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle2Result.Id}");
            if(inputSingle3Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle3Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

            if(inputList2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList2Result.Ids)}");

;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle4List3StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
     TInputSingle3, TPreviousSingleCache3,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>>

 
             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
     where TPreviousSingleCache2 : class
     where TPreviousSingleCache3 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

     where TPreviousListCache2 : class
 where TPreviousItemCache2 : class

    
    {
            private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
            private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
            private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
            private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
                private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
            private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
            private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
            private readonly bool updateOnRefresh;

        public OutputMultiSimpleInputSingle4List3StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
            StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
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

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
            IDocument<TInputSingle0> inputSingle0, 
            IDocument<TInputSingle1> inputSingle1, 
            IDocument<TInputSingle2> inputSingle2, 
            IDocument<TInputSingle3> inputSingle3, 
                ImmutableList<IDocument<TInputList0>> inputList0, 
            ImmutableList<IDocument<TInputList1>> inputList1, 
            ImmutableList<IDocument<TInputList2>> inputList2, 
            OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>>> DoInternal([AllowNull]  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputSingle3(cache?.PreviousCache3, options),
                this.inputList0(cache?.PreviousCache4, options),
                this.inputList1(cache?.PreviousCache5, options),
                this.inputList2(cache?.PreviousCache6, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache3, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache4, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache5, options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache6, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        await Task.WhenAll(
                inputSingle0Result.Perform.AsTask(),
                inputSingle1Result.Perform.AsTask(),
                inputSingle2Result.Perform.AsTask(),
                inputSingle3Result.Perform.AsTask(),
                inputList0Result.Perform.AsTask(),
                inputList1Result.Perform.AsTask(),
                inputList2Result.Perform.AsTask(),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputSingle2Performed = await inputSingle2Result.Perform;
            var inputSingle3Performed = await inputSingle3Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));
            var inputList2Performed = await inputList2Result.Perform;

            var inputList2PerformedListTask = Task.WhenAll(inputList2Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
,              inputList2PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);
            var inputList2PerformedList = await inputList2PerformedListTask.ConfigureAwait(false);



                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputSingle2Performed.result,
            inputSingle3Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
            inputList2PerformedList.ToImmutableList(),
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputSingle0Performed.cache, inputSingle1Performed.cache, inputSingle2Performed.cache, inputSingle3Performed.cache, inputList0Performed.cache, inputList1Performed.cache, inputList2Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputSingle3Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputSingle2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle2Result.Id}");
            if(inputSingle3Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle3Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

            if(inputList2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList2Result.Ids)}");

;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle4List3StageBase<
//    // TInputSingle0, TPreviousSingleCache0,
//    // TInputSingle1, TPreviousSingleCache1,
//    // TInputSingle2, TPreviousSingleCache2,
//    // TInputSingle3, TPreviousSingleCache3,
//    //        // TInputList0, TPreviousItemCache0, TPreviousListCache0,
//    // TInputList1, TPreviousItemCache1, TPreviousListCache1,
//    // TInputList2, TPreviousItemCache2, TPreviousListCache2,
//    // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //        private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
//    //        private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
//    //        private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
//    //        private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
//    //    //        private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
//    //        private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
//    //        private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
//    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle4List3StageBase(
//    //        StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
//    //        StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
//    //        StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
//    //        StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
//    //        //        StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
//    //        StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
//    //        StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
//    //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //            this.inputSingle0 = inputSingle0;
//    //            this.inputSingle1 = inputSingle1;
//    //            this.inputSingle2 = inputSingle2;
//    //            this.inputSingle3 = inputSingle3;
//    //    //            this.inputList0 = inputList0;
//    //            this.inputList1 = inputList1;
//    //            this.inputList2 = inputList2;
//    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //        StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
//    //        StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
//    //        StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
//    //        StageResult<TInputSingle3,TPreviousSingleCache3> inputSingle3, 
//    //    //        StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
//    //        StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
//    //        StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
//    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 7)
//                throw new ArgumentException($"This cache should have exactly 7 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
////            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
////            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
////            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
////            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
//////            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);
////            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[5], options).ConfigureAwait(false);
////            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[6], options).ConfigureAwait(false);
////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                //            inputSingle0Result,
////            inputSingle1Result,
////            inputSingle2Result,
////            inputSingle3Result,
//////            inputList0Result,
////            inputList1Result,
////            inputList2Result,
////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
////            || inputSingle0Result.HasChanges
////            || inputSingle1Result.HasChanges
////            || inputSingle2Result.HasChanges
////            || inputSingle3Result.HasChanges
//////            || inputList0Result.HasChanges
////            || inputList1Result.HasChanges
////            || inputList2Result.HasChanges
//;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//



    //////////////// SINGLE SIMPLE //////////////// 

namespace Single.Simple {
     public abstract class OutputSingleInputSingleSimple4List4StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
     TInputSingle3, TPreviousSingleCache3,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TInputList3, TPreviousItemCache3, TPreviousListCache3,
     TResult
 > : StageBase<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>>
        

             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
     where TPreviousSingleCache2 : class
     where TPreviousSingleCache3 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

     where TPreviousListCache2 : class
 where TPreviousItemCache2 : class

     where TPreviousListCache3 : class
 where TPreviousItemCache3 : class

    
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

        public OutputSingleInputSingleSimple4List4StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
            StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
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
        
        protected abstract Task<IDocument<TResult>> Work(IDocument<TInputSingle0> inputSingle0, IDocument<TInputSingle1> inputSingle1, IDocument<TInputSingle2> inputSingle2, IDocument<TInputSingle3> inputSingle3, ImmutableList<IDocument<TInputList0>> inputList0, ImmutableList<IDocument<TInputList1>> inputList1, ImmutableList<IDocument<TInputList2>> inputList2, ImmutableList<IDocument<TInputList3>> inputList3, OptionToken options);

        protected sealed override async Task<StageResult<TResult, CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>>> DoInternal([AllowNull] CacheId<string, TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputSingle3(cache?.PreviousCache3, options),
                this.inputList0(cache?.PreviousCache4, options),
                this.inputList1(cache?.PreviousCache5, options),
                this.inputList2(cache?.PreviousCache6, options),
                this.inputList3(cache?.PreviousCache7, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache3, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache4, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache5, options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache6, options).ConfigureAwait(false);
            var inputList3Result = await this.inputList3(cache?.PreviousCache7, options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {

        
            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputSingle3(cache?.PreviousCache3, options),
                this.inputList0(cache?.PreviousCache4, options),
                this.inputList1(cache?.PreviousCache5, options),
                this.inputList2(cache?.PreviousCache6, options),
                this.inputList3(cache?.PreviousCache7, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputSingle2Performed = await inputSingle2Result.Perform;
            var inputSingle3Performed = await inputSingle3Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));
            var inputList2Performed = await inputList2Result.Perform;

            var inputList2PerformedListTask = Task.WhenAll(inputList2Performed.result.Select(async x => (await x.Perform).result));
            var inputList3Performed = await inputList3Result.Perform;

            var inputList3PerformedListTask = Task.WhenAll(inputList3Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
,              inputList2PerformedListTask
,              inputList3PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);
            var inputList2PerformedList = await inputList2PerformedListTask.ConfigureAwait(false);
            var inputList3PerformedList = await inputList3PerformedListTask.ConfigureAwait(false);


                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputSingle2Performed.result,
            inputSingle3Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
            inputList2PerformedList.ToImmutableList(),
            inputList3PerformedList.ToImmutableList(),

                options).ConfigureAwait(false);
                
                return (work, cache: CacheId.Create(work.Id,work.Hash, inputSingle0Performed.cache, inputSingle1Performed.cache, inputSingle2Performed.cache, inputSingle3Performed.cache, inputList0Performed.cache, inputList1Performed.cache, inputList2Performed.cache, inputList3Performed.cache));
            });


            bool hasChanges = (await this.ForceUpdate(cache?.Id, cache?.Data, options).ConfigureAwait(false) )??false
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputSingle3Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
            || inputList3Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputSingle2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle2Result.Id}");
            if(inputSingle3Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle3Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

            if(inputList2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList2Result.Ids)}");

            if(inputList3Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList3Result.Ids)}");

;

            var id = cache?.Id;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null || id is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                id = result.work.Id;
                hasChanges = !await this.CacheEquals(cache?.Data, result.cache.Data).ConfigureAwait(false);

                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResult.Create(task, hasChanges, id);
        }

        protected virtual Task<bool?> ForceUpdate(string? id, string? hash, OptionToken options) => Task.FromResult<bool?>(null);

        protected Task<bool> CacheEquals([AllowNull]string item1, [AllowNull] string item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }
}


    //////////////// MULTI SIMPLE //////////////// 
namespace Multiple.Simple {

    public abstract class OutputMultiSimpleInputSingle4List4StageBase<
     TInputSingle0, TPreviousSingleCache0,
     TInputSingle1, TPreviousSingleCache1,
     TInputSingle2, TPreviousSingleCache2,
     TInputSingle3, TPreviousSingleCache3,
             TInputList0, TPreviousItemCache0, TPreviousListCache0,
     TInputList1, TPreviousItemCache1, TPreviousListCache1,
     TInputList2, TPreviousItemCache2, TPreviousListCache2,
     TInputList3, TPreviousItemCache3, TPreviousListCache3,
     TResult
 > : MultiStageBase<TResult, string,  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>>

 
             where TPreviousSingleCache0 : class
     where TPreviousSingleCache1 : class
     where TPreviousSingleCache2 : class
     where TPreviousSingleCache3 : class
             where TPreviousListCache0 : class
 where TPreviousItemCache0 : class

     where TPreviousListCache1 : class
 where TPreviousItemCache1 : class

     where TPreviousListCache2 : class
 where TPreviousItemCache2 : class

     where TPreviousListCache3 : class
 where TPreviousItemCache3 : class

    
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

        public OutputMultiSimpleInputSingle4List4StageBase(
            StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
            StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
            StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
            StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
                    StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
            StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
            StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
            StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
            IGeneratorContext context, string? name, bool updateOnRefresh = false) : base(context, name)
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

        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(
        
            IDocument<TInputSingle0> inputSingle0, 
            IDocument<TInputSingle1> inputSingle1, 
            IDocument<TInputSingle2> inputSingle2, 
            IDocument<TInputSingle3> inputSingle3, 
                ImmutableList<IDocument<TInputList0>> inputList0, 
            ImmutableList<IDocument<TInputList1>> inputList1, 
            ImmutableList<IDocument<TInputList2>> inputList2, 
            ImmutableList<IDocument<TInputList3>> inputList3, 
            OptionToken options);

        protected sealed override async Task<StageResultList<TResult, string, CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>>> DoInternal([AllowNull]  CachelessIds<TPreviousSingleCache0, TPreviousSingleCache1, TPreviousSingleCache2, TPreviousSingleCache3, TPreviousListCache0, TPreviousListCache1, TPreviousListCache2, TPreviousListCache3>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));


            await Task.WhenAll(
                this.inputSingle0(cache?.PreviousCache0, options),
                this.inputSingle1(cache?.PreviousCache1, options),
                this.inputSingle2(cache?.PreviousCache2, options),
                this.inputSingle3(cache?.PreviousCache3, options),
                this.inputList0(cache?.PreviousCache4, options),
                this.inputList1(cache?.PreviousCache5, options),
                this.inputList2(cache?.PreviousCache6, options),
                this.inputList3(cache?.PreviousCache7, options),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache0, options).ConfigureAwait(false);
            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache1, options).ConfigureAwait(false);
            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache2, options).ConfigureAwait(false);
            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache3, options).ConfigureAwait(false);
            var inputList0Result = await this.inputList0(cache?.PreviousCache4, options).ConfigureAwait(false);
            var inputList1Result = await this.inputList1(cache?.PreviousCache5, options).ConfigureAwait(false);
            var inputList2Result = await this.inputList2(cache?.PreviousCache6, options).ConfigureAwait(false);
            var inputList3Result = await this.inputList3(cache?.PreviousCache7, options).ConfigureAwait(false);
             
            var task = LazyTask.Create(async () =>
            {

                        await Task.WhenAll(
                inputSingle0Result.Perform.AsTask(),
                inputSingle1Result.Perform.AsTask(),
                inputSingle2Result.Perform.AsTask(),
                inputSingle3Result.Perform.AsTask(),
                inputList0Result.Perform.AsTask(),
                inputList1Result.Perform.AsTask(),
                inputList2Result.Perform.AsTask(),
                inputList3Result.Perform.AsTask(),
                Task.CompletedTask
            ).ConfigureAwait(false);
                            var inputSingle0Performed = await inputSingle0Result.Perform;
            var inputSingle1Performed = await inputSingle1Result.Perform;
            var inputSingle2Performed = await inputSingle2Result.Perform;
            var inputSingle3Performed = await inputSingle3Result.Perform;
            var inputList0Performed = await inputList0Result.Perform;

            var inputList0PerformedListTask = Task.WhenAll(inputList0Performed.result.Select(async x => (await x.Perform).result));
            var inputList1Performed = await inputList1Result.Perform;

            var inputList1PerformedListTask = Task.WhenAll(inputList1Performed.result.Select(async x => (await x.Perform).result));
            var inputList2Performed = await inputList2Result.Perform;

            var inputList2PerformedListTask = Task.WhenAll(inputList2Performed.result.Select(async x => (await x.Perform).result));
            var inputList3Performed = await inputList3Result.Perform;

            var inputList3PerformedListTask = Task.WhenAll(inputList3Performed.result.Select(async x => (await x.Perform).result));

await Task.WhenAll(
             inputList0PerformedListTask
,              inputList1PerformedListTask
,              inputList2PerformedListTask
,              inputList3PerformedListTask
).ConfigureAwait(false);

            var inputList0PerformedList = await inputList0PerformedListTask.ConfigureAwait(false);
            var inputList1PerformedList = await inputList1PerformedListTask.ConfigureAwait(false);
            var inputList2PerformedList = await inputList2PerformedListTask.ConfigureAwait(false);
            var inputList3PerformedList = await inputList3PerformedListTask.ConfigureAwait(false);



                var oldChildCaches = cache?.Ids.ToImmutableDictionary(x => x.id, x => x.hash);

                var work = await this.Work(
                            inputSingle0Performed.result,
            inputSingle1Performed.result,
            inputSingle2Performed.result,
            inputSingle3Performed.result,
            inputList0PerformedList.ToImmutableList(),
            inputList1PerformedList.ToImmutableList(),
            inputList2PerformedList.ToImmutableList(),
            inputList3PerformedList.ToImmutableList(),
 options).ConfigureAwait(false);
                             
                
                var list = work.Select(x=>
                {
                    var hasChanges =true;
                    if(oldChildCaches !=null && oldChildCaches.TryGetValue(x.Id, out var oldHash))
                        hasChanges = x.Hash != oldHash;
                    return (result: StageResult.Create( x,x.Hash,hasChanges,x.Id), hash: x.Hash);
                
                }).ToArray();


                return (list.Select(x=>x.result).ToImmutableList(), cache: CachelessIds.Create(list.Select(x=>(x.result.Id, x.hash)).ToArray(), inputSingle0Performed.cache, inputSingle1Performed.cache, inputSingle2Performed.cache, inputSingle3Performed.cache, inputList0Performed.cache, inputList1Performed.cache, inputList2Performed.cache, inputList3Performed.cache));// { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
            });


                            bool hasChanges = (await this.ForceUpdate(cache?.Ids, options).ConfigureAwait(false) ?? false)
            || inputSingle0Result.HasChanges
            || inputSingle1Result.HasChanges
            || inputSingle2Result.HasChanges
            || inputSingle3Result.HasChanges
            || inputList0Result.HasChanges
            || inputList1Result.HasChanges
            || inputList2Result.HasChanges
            || inputList3Result.HasChanges
;

            if(inputSingle0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle0Result.Id}");
            if(inputSingle1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle1Result.Id}");
            if(inputSingle2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle2Result.Id}");
            if(inputSingle3Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for input with id: {inputSingle3Result.Id}");
            if(inputList0Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList0Result.Ids)}");

            if(inputList1Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList1Result.Ids)}");

            if(inputList2Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList2Result.Ids)}");

            if(inputList3Result.HasChanges)
                this.Context.Logger.Info($"Found Changes for list with ids: {string.Join(", ", inputList3Result.Ids)}");

;

            var ids = cache?.Ids;
            if (hasChanges || (this.updateOnRefresh && options.Refresh) || cache is null)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                ids = await Task.WhenAll(result.Item1.Select(async x => ((await x.Perform).result.Id, (await x.Perform).result.Hash))).ConfigureAwait(false); // we want to make sure thate there are actually changes, so we compare the caches.
                hasChanges = !this.CacheEquals(cache?.Ids, result.cache.Ids);
                if(!hasChanges)
                    this.Context.Logger.Info($"Output will not have changes.");

            }

            return StageResultList.Create(task, hasChanges, ids.Select(x=>x.id).ToImmutableList());
        }

        protected virtual Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options) => Task.FromResult<bool?>(null);


        private bool CacheEquals((string id, string hash)[]? item1, (string id, string hash)[]? item2)
        {
            if (item1 is null && item2 is null)
                return true;
            if (item1 is null || item2 is null)
                return false;

            return item1.SequenceEqual(item2);
        }

    
    }
}


//    //////////////// MULTI ADVANCED //////////////// 
//    //
//     public abstract class OutputMultiInputSingle4List4StageBase<
//    // TInputSingle0, TPreviousSingleCache0,
//    // TInputSingle1, TPreviousSingleCache1,
//    // TInputSingle2, TPreviousSingleCache2,
//    // TInputSingle3, TPreviousSingleCache3,
//    //        // TInputList0, TPreviousItemCache0, TPreviousListCache0,
//    // TInputList1, TPreviousItemCache1, TPreviousListCache1,
//    // TInputList2, TPreviousItemCache2, TPreviousListCache2,
//    // TInputList3, TPreviousItemCache3, TPreviousListCache3,
//    // TResult, TResultCache, TCache
// > : MultiStageBase<TResult, TResultCache, CacheIds<TCache>>
//        where TCache : class
//    {
//    //        private readonly StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0;
//    //        private readonly StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1;
//    //        private readonly StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2;
//    //        private readonly StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3;
//    //    //        private readonly StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0;
//    //        private readonly StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1;
//    //        private readonly StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2;
//    //        private readonly StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3;
//    //        private readonly bool updateOnRefresh;
//
//        public OutputMultiInputSingle4List4StageBase(
//    //        StagePerformHandler<TInputSingle0, TPreviousSingleCache0> inputSingle0,
//    //        StagePerformHandler<TInputSingle1, TPreviousSingleCache1> inputSingle1,
//    //        StagePerformHandler<TInputSingle2, TPreviousSingleCache2> inputSingle2,
//    //        StagePerformHandler<TInputSingle3, TPreviousSingleCache3> inputSingle3,
//    //        //        StagePerformHandler<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0,
//    //        StagePerformHandler<TInputList1, TPreviousItemCache1, TPreviousListCache1> inputList1,
//    //        StagePerformHandler<TInputList2, TPreviousItemCache2, TPreviousListCache2> inputList2,
//    //        StagePerformHandler<TInputList3, TPreviousItemCache3, TPreviousListCache3> inputList3,
//    //        GeneratorContext context, bool updateOnRefresh = false) : base(context)
//        {
//    //            this.inputSingle0 = inputSingle0;
//    //            this.inputSingle1 = inputSingle1;
//    //            this.inputSingle2 = inputSingle2;
//    //            this.inputSingle3 = inputSingle3;
//    //    //            this.inputList0 = inputList0;
//    //            this.inputList1 = inputList1;
//    //            this.inputList2 = inputList2;
//    //            this.inputList3 = inputList3;
//    //            this.updateOnRefresh = updateOnRefresh;
//        } 
//
//        protected abstract Task<(ImmutableList<StageResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> Work(
//        
//    //        StageResult<TInputSingle0,TPreviousSingleCache0> inputSingle0, 
//    //        StageResult<TInputSingle1,TPreviousSingleCache1> inputSingle1, 
//    //        StageResult<TInputSingle2,TPreviousSingleCache2> inputSingle2, 
//    //        StageResult<TInputSingle3,TPreviousSingleCache3> inputSingle3, 
//    //    //        StageResultList<TInputList0,TPreviousItemCache0,TPreviousListCache0> inputList0, 
//    //        StageResultList<TInputList1,TPreviousItemCache1,TPreviousListCache1> inputList1, 
//    //        StageResultList<TInputList2,TPreviousItemCache2,TPreviousListCache2> inputList2, 
//    //        StageResultList<TInputList3,TPreviousItemCache3,TPreviousListCache3> inputList3, 
//    //       [AllowNull] TCache cache,
//        [AllowNull] ImmutableDictionary<string, BaseCache<TResultCache>>? childCaches,
//        OptionToken options);
//
//        protected sealed override async Task<StageResultList<TResult, TResultCache, CacheIds<TCache>>> DoInternal([AllowNull] CacheIds<TCache>? cache, OptionToken options)
//        {
//            if (cache != null && cache.PreviousCache.Length != 8)
//                throw new ArgumentException($"This cache should have exactly 8 predecessor but had {cache.PreviousCache}");
//            if (options is null)
//                throw new ArgumentNullException(nameof(options));
//
//
////            var inputSingle0Result = await this.inputSingle0(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);
////            var inputSingle1Result = await this.inputSingle1(cache?.PreviousCache.Span[1], options).ConfigureAwait(false);
////            var inputSingle2Result = await this.inputSingle2(cache?.PreviousCache.Span[2], options).ConfigureAwait(false);
////            var inputSingle3Result = await this.inputSingle3(cache?.PreviousCache.Span[3], options).ConfigureAwait(false);
//////            var inputList0Result = await this.inputList0(cache?.PreviousCache.Span[4], options).ConfigureAwait(false);
////            var inputList1Result = await this.inputList1(cache?.PreviousCache.Span[5], options).ConfigureAwait(false);
////            var inputList2Result = await this.inputList2(cache?.PreviousCache.Span[6], options).ConfigureAwait(false);
////            var inputList3Result = await this.inputList3(cache?.PreviousCache.Span[7], options).ConfigureAwait(false);
////
//            var currentCache = cache?.Item;
//
//             
//            var task = LazyTask.Create(async () =>
//            {
//                var oldChildCaches = cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => (BaseCache<TResultCache>)x.Value);
//
//                var work = await this.Work(
//                //            inputSingle0Result,
////            inputSingle1Result,
////            inputSingle2Result,
////            inputSingle3Result,
//////            inputList0Result,
////            inputList1Result,
////            inputList2Result,
////            inputList3Result,
////cache?.Item.Data, oldChildCaches, options).ConfigureAwait(false);
//                             
//                             var oldHashLookup = currentCache?.Ids.ToDictionary(x => x.id, x => x.hash) ?? new System.Collections.Generic.Dictionary<string, string>();
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
//
//                return (work.result, cache: BaseCache.Create(new CacheIds<TCache>() { Data = work.cache.Item, Ids = ids }, work.cache.PreviousCache, childCaches.ToImmutable()));
//            });
//
//
//            bool hasChanges = this.ForceUpdate(cache?.Item.Data, options) 
////            || inputSingle0Result.HasChanges
////            || inputSingle1Result.HasChanges
////            || inputSingle2Result.HasChanges
////            || inputSingle3Result.HasChanges
//////            || inputList0Result.HasChanges
////            || inputList1Result.HasChanges
////            || inputList2Result.HasChanges
////            || inputList3Result.HasChanges
//;
//            System.Diagnostics.Debug.Assert(cache != null || hasChanges);
//
//            if (hasChanges || (this.updateOnRefresh && options.Refresh))
//            {
//                // if we should refresh we need to update the repo or if the previous input was different
//                // we need to perform the network operation to ensure we have no changes
//
//                var result = await task;
//                currentCache = result.cache.Item;
//                // we want to make sure thate there are actually changes, so we compare the caches.
//                hasChanges = !await this.CacheEquals(cache?.Item.Data, result.cache.Item.Data).ConfigureAwait(false);
//                // if we found that cache had no changes, maybe the childcaches where changed.
//                if (!hasChanges)
//                    hasChanges = !await this.ChildCacheEquals(cache?.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item), result.cache.ChildCache.ToImmutableDictionary(x => x.Key, x => ((BaseCache<TResultCache>)x.Value).Item)).ConfigureAwait(false);
//           }
//
//            // if currentCache is null, hasChanges must be true and so currentCache will be set.
//            var theId = currentCache!.Ids;
//
//            return StageResult.Create(task, hasChanges, theId.Select(x => x.id).ToImmutableList());
//        }
//
//        protected virtual bool ForceUpdate([AllowNull]TCache cache, OptionToken options) => true;
//
//
//        protected virtual Task<bool> CacheEquals([AllowNull]TCache item1, [AllowNull]TCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//
//        protected virtual async Task<bool> ChildCacheEquals([AllowNull]ImmutableDictionary<string, TResultCache> item1, [AllowNull]ImmutableDictionary<string, TResultCache> item2)
//        {
//            if (item1 is null && item2 is null)
//                return true;
//
//            if (item1 is null || item2 is null)
//                return false;
//
//            if (item1.Count != item2.Count)
//                return false;
//
//            var itemResults = await Task.WhenAll(item1.Select(async pair => item2.TryGetValue(pair.Key, out var entry) && await this.ChildCacheElementRquals(pair.Value, entry).ConfigureAwait(false))).ConfigureAwait(false);
//            return itemResults.All(x => x);
//        }
//
//        protected virtual Task<bool> ChildCacheElementRquals([AllowNull] TResultCache item1, [AllowNull] TResultCache item2)
//        {
//            return Task.FromResult(Equals(item1, item2));
//        }
//    }
//
//
////
//




    #pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning disable CA1819 // Properties should not return arrays



public static partial class CacheId{

    public static CacheId<TCache> Create<TCache>(string id, TCache cache) 
        where TCache : class
 => new CacheId<TCache>(){ Id = id, Data = cache};
}

public static  partial class CacheIds{

    public static CacheIds<TCache> Create<TCache>((string id, string hash)[] ids, TCache cache) 
        where TCache : class
 => new CacheIds<TCache>(){ Ids = ids, Data = cache};
}



    public class CacheId<TCache>
        where TCache : class
    {


        public TCache? Data { get; set; }

        public string Id { get; set; }
    }

        public class CacheIds<TCache>
        where TCache : class
    {
    
        public TCache? Data { get; set; }

        public (string id, string hash)[] Ids { get; set; }
    }


public  partial class CachelessId{

    public static CachelessId Create(string id) 
 => new CachelessId(){ Id = id};
}

public   partial class CachelessIds{

    public static CachelessIds Create((string id, string hash)[] ids) 
 => new CachelessIds(){ Ids = ids};
}



    public partial class CachelessId    {



        public string Id { get; set; }
    }

        public partial class CachelessIds    {
    

        public (string id, string hash)[] Ids { get; set; }
    }


  


public static partial class CacheId{

    public static CacheId<TCache, TPreview0> Create<TCache, TPreview0>(string id, TCache cache, TPreview0 preview0) 
        where TCache : class
        where TPreview0 : class
 => new CacheId<TCache, TPreview0>(){ Id = id, Data = cache, PreviousCache0 = preview0};
}

public static  partial class CacheIds{

    public static CacheIds<TCache, TPreview0> Create<TCache, TPreview0>((string id, string hash)[] ids, TCache cache, TPreview0 preview0) 
        where TCache : class
        where TPreview0 : class
 => new CacheIds<TCache, TPreview0>(){ Ids = ids, Data = cache, PreviousCache0 = preview0};
}



    public class CacheId<TCache, TPreview0>
        where TCache : class
        where TPreview0 : class
    {

        public TPreview0? PreviousCache0 {get;set;}

        public TCache? Data { get; set; }

        public string Id { get; set; }
    }

        public class CacheIds<TCache, TPreview0>
        where TCache : class
        where TPreview0 : class
    {
            public TPreview0? PreviousCache0 {get;set;}

        public TCache? Data { get; set; }

        public (string id, string hash)[] Ids { get; set; }
    }


public  partial class CachelessId{

    public static CachelessId< TPreview0> Create< TPreview0>(string id, TPreview0 preview0) 
        where TPreview0 : class
 => new CachelessId< TPreview0>(){ Id = id, PreviousCache0 = preview0};
}

public   partial class CachelessIds{

    public static CachelessIds< TPreview0> Create< TPreview0>((string id, string hash)[] ids, TPreview0 preview0) 
        where TPreview0 : class
 => new CachelessIds< TPreview0>(){ Ids = ids, PreviousCache0 = preview0};
}



    public partial class CachelessId< TPreview0>        where TPreview0 : class
    {

        public TPreview0? PreviousCache0 {get;set;}


        public string Id { get; set; }
    }

        public partial class CachelessIds< TPreview0>        where TPreview0 : class
    {
            public TPreview0? PreviousCache0 {get;set;}


        public (string id, string hash)[] Ids { get; set; }
    }


  


public static partial class CacheId{

    public static CacheId<TCache, TPreview0, TPreview1> Create<TCache, TPreview0, TPreview1>(string id, TCache cache, TPreview0 preview0, TPreview1 preview1) 
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
 => new CacheId<TCache, TPreview0, TPreview1>(){ Id = id, Data = cache, PreviousCache0 = preview0, PreviousCache1 = preview1};
}

public static  partial class CacheIds{

    public static CacheIds<TCache, TPreview0, TPreview1> Create<TCache, TPreview0, TPreview1>((string id, string hash)[] ids, TCache cache, TPreview0 preview0, TPreview1 preview1) 
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
 => new CacheIds<TCache, TPreview0, TPreview1>(){ Ids = ids, Data = cache, PreviousCache0 = preview0, PreviousCache1 = preview1};
}



    public class CacheId<TCache, TPreview0, TPreview1>
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
    {

        public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}

        public TCache? Data { get; set; }

        public string Id { get; set; }
    }

        public class CacheIds<TCache, TPreview0, TPreview1>
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
    {
            public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}

        public TCache? Data { get; set; }

        public (string id, string hash)[] Ids { get; set; }
    }


public  partial class CachelessId{

    public static CachelessId< TPreview0 , TPreview1> Create< TPreview0 , TPreview1>(string id, TPreview0 preview0, TPreview1 preview1) 
        where TPreview0 : class
        where TPreview1 : class
 => new CachelessId< TPreview0 , TPreview1>(){ Id = id, PreviousCache0 = preview0, PreviousCache1 = preview1};
}

public   partial class CachelessIds{

    public static CachelessIds< TPreview0 , TPreview1> Create< TPreview0 , TPreview1>((string id, string hash)[] ids, TPreview0 preview0, TPreview1 preview1) 
        where TPreview0 : class
        where TPreview1 : class
 => new CachelessIds< TPreview0 , TPreview1>(){ Ids = ids, PreviousCache0 = preview0, PreviousCache1 = preview1};
}



    public partial class CachelessId< TPreview0 , TPreview1>        where TPreview0 : class
        where TPreview1 : class
    {

        public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}


        public string Id { get; set; }
    }

        public partial class CachelessIds< TPreview0 , TPreview1>        where TPreview0 : class
        where TPreview1 : class
    {
            public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}


        public (string id, string hash)[] Ids { get; set; }
    }


  


public static partial class CacheId{

    public static CacheId<TCache, TPreview0, TPreview1, TPreview2> Create<TCache, TPreview0, TPreview1, TPreview2>(string id, TCache cache, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2) 
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
 => new CacheId<TCache, TPreview0, TPreview1, TPreview2>(){ Id = id, Data = cache, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2};
}

public static  partial class CacheIds{

    public static CacheIds<TCache, TPreview0, TPreview1, TPreview2> Create<TCache, TPreview0, TPreview1, TPreview2>((string id, string hash)[] ids, TCache cache, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2) 
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
 => new CacheIds<TCache, TPreview0, TPreview1, TPreview2>(){ Ids = ids, Data = cache, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2};
}



    public class CacheId<TCache, TPreview0, TPreview1, TPreview2>
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
    {

        public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}

        public TCache? Data { get; set; }

        public string Id { get; set; }
    }

        public class CacheIds<TCache, TPreview0, TPreview1, TPreview2>
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
    {
            public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}

        public TCache? Data { get; set; }

        public (string id, string hash)[] Ids { get; set; }
    }


public  partial class CachelessId{

    public static CachelessId< TPreview0 , TPreview1 , TPreview2> Create< TPreview0 , TPreview1 , TPreview2>(string id, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2) 
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
 => new CachelessId< TPreview0 , TPreview1 , TPreview2>(){ Id = id, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2};
}

public   partial class CachelessIds{

    public static CachelessIds< TPreview0 , TPreview1 , TPreview2> Create< TPreview0 , TPreview1 , TPreview2>((string id, string hash)[] ids, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2) 
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
 => new CachelessIds< TPreview0 , TPreview1 , TPreview2>(){ Ids = ids, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2};
}



    public partial class CachelessId< TPreview0 , TPreview1 , TPreview2>        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
    {

        public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}


        public string Id { get; set; }
    }

        public partial class CachelessIds< TPreview0 , TPreview1 , TPreview2>        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
    {
            public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}


        public (string id, string hash)[] Ids { get; set; }
    }


  


public static partial class CacheId{

    public static CacheId<TCache, TPreview0, TPreview1, TPreview2, TPreview3> Create<TCache, TPreview0, TPreview1, TPreview2, TPreview3>(string id, TCache cache, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2, TPreview3 preview3) 
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
 => new CacheId<TCache, TPreview0, TPreview1, TPreview2, TPreview3>(){ Id = id, Data = cache, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2, PreviousCache3 = preview3};
}

public static  partial class CacheIds{

    public static CacheIds<TCache, TPreview0, TPreview1, TPreview2, TPreview3> Create<TCache, TPreview0, TPreview1, TPreview2, TPreview3>((string id, string hash)[] ids, TCache cache, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2, TPreview3 preview3) 
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
 => new CacheIds<TCache, TPreview0, TPreview1, TPreview2, TPreview3>(){ Ids = ids, Data = cache, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2, PreviousCache3 = preview3};
}



    public class CacheId<TCache, TPreview0, TPreview1, TPreview2, TPreview3>
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
    {

        public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}
        public TPreview3? PreviousCache3 {get;set;}

        public TCache? Data { get; set; }

        public string Id { get; set; }
    }

        public class CacheIds<TCache, TPreview0, TPreview1, TPreview2, TPreview3>
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
    {
            public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}
        public TPreview3? PreviousCache3 {get;set;}

        public TCache? Data { get; set; }

        public (string id, string hash)[] Ids { get; set; }
    }


public  partial class CachelessId{

    public static CachelessId< TPreview0 , TPreview1 , TPreview2 , TPreview3> Create< TPreview0 , TPreview1 , TPreview2 , TPreview3>(string id, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2, TPreview3 preview3) 
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
 => new CachelessId< TPreview0 , TPreview1 , TPreview2 , TPreview3>(){ Id = id, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2, PreviousCache3 = preview3};
}

public   partial class CachelessIds{

    public static CachelessIds< TPreview0 , TPreview1 , TPreview2 , TPreview3> Create< TPreview0 , TPreview1 , TPreview2 , TPreview3>((string id, string hash)[] ids, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2, TPreview3 preview3) 
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
 => new CachelessIds< TPreview0 , TPreview1 , TPreview2 , TPreview3>(){ Ids = ids, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2, PreviousCache3 = preview3};
}



    public partial class CachelessId< TPreview0 , TPreview1 , TPreview2 , TPreview3>        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
    {

        public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}
        public TPreview3? PreviousCache3 {get;set;}


        public string Id { get; set; }
    }

        public partial class CachelessIds< TPreview0 , TPreview1 , TPreview2 , TPreview3>        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
    {
            public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}
        public TPreview3? PreviousCache3 {get;set;}


        public (string id, string hash)[] Ids { get; set; }
    }


  


public static partial class CacheId{

    public static CacheId<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4> Create<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4>(string id, TCache cache, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2, TPreview3 preview3, TPreview4 preview4) 
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
 => new CacheId<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4>(){ Id = id, Data = cache, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2, PreviousCache3 = preview3, PreviousCache4 = preview4};
}

public static  partial class CacheIds{

    public static CacheIds<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4> Create<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4>((string id, string hash)[] ids, TCache cache, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2, TPreview3 preview3, TPreview4 preview4) 
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
 => new CacheIds<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4>(){ Ids = ids, Data = cache, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2, PreviousCache3 = preview3, PreviousCache4 = preview4};
}



    public class CacheId<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4>
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
    {

        public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}
        public TPreview3? PreviousCache3 {get;set;}
        public TPreview4? PreviousCache4 {get;set;}

        public TCache? Data { get; set; }

        public string Id { get; set; }
    }

        public class CacheIds<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4>
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
    {
            public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}
        public TPreview3? PreviousCache3 {get;set;}
        public TPreview4? PreviousCache4 {get;set;}

        public TCache? Data { get; set; }

        public (string id, string hash)[] Ids { get; set; }
    }


public  partial class CachelessId{

    public static CachelessId< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4> Create< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4>(string id, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2, TPreview3 preview3, TPreview4 preview4) 
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
 => new CachelessId< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4>(){ Id = id, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2, PreviousCache3 = preview3, PreviousCache4 = preview4};
}

public   partial class CachelessIds{

    public static CachelessIds< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4> Create< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4>((string id, string hash)[] ids, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2, TPreview3 preview3, TPreview4 preview4) 
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
 => new CachelessIds< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4>(){ Ids = ids, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2, PreviousCache3 = preview3, PreviousCache4 = preview4};
}



    public partial class CachelessId< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4>        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
    {

        public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}
        public TPreview3? PreviousCache3 {get;set;}
        public TPreview4? PreviousCache4 {get;set;}


        public string Id { get; set; }
    }

        public partial class CachelessIds< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4>        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
    {
            public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}
        public TPreview3? PreviousCache3 {get;set;}
        public TPreview4? PreviousCache4 {get;set;}


        public (string id, string hash)[] Ids { get; set; }
    }


  


public static partial class CacheId{

    public static CacheId<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5> Create<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5>(string id, TCache cache, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2, TPreview3 preview3, TPreview4 preview4, TPreview5 preview5) 
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
 => new CacheId<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5>(){ Id = id, Data = cache, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2, PreviousCache3 = preview3, PreviousCache4 = preview4, PreviousCache5 = preview5};
}

public static  partial class CacheIds{

    public static CacheIds<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5> Create<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5>((string id, string hash)[] ids, TCache cache, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2, TPreview3 preview3, TPreview4 preview4, TPreview5 preview5) 
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
 => new CacheIds<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5>(){ Ids = ids, Data = cache, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2, PreviousCache3 = preview3, PreviousCache4 = preview4, PreviousCache5 = preview5};
}



    public class CacheId<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5>
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
    {

        public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}
        public TPreview3? PreviousCache3 {get;set;}
        public TPreview4? PreviousCache4 {get;set;}
        public TPreview5? PreviousCache5 {get;set;}

        public TCache? Data { get; set; }

        public string Id { get; set; }
    }

        public class CacheIds<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5>
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
    {
            public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}
        public TPreview3? PreviousCache3 {get;set;}
        public TPreview4? PreviousCache4 {get;set;}
        public TPreview5? PreviousCache5 {get;set;}

        public TCache? Data { get; set; }

        public (string id, string hash)[] Ids { get; set; }
    }


public  partial class CachelessId{

    public static CachelessId< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5> Create< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5>(string id, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2, TPreview3 preview3, TPreview4 preview4, TPreview5 preview5) 
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
 => new CachelessId< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5>(){ Id = id, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2, PreviousCache3 = preview3, PreviousCache4 = preview4, PreviousCache5 = preview5};
}

public   partial class CachelessIds{

    public static CachelessIds< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5> Create< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5>((string id, string hash)[] ids, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2, TPreview3 preview3, TPreview4 preview4, TPreview5 preview5) 
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
 => new CachelessIds< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5>(){ Ids = ids, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2, PreviousCache3 = preview3, PreviousCache4 = preview4, PreviousCache5 = preview5};
}



    public partial class CachelessId< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5>        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
    {

        public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}
        public TPreview3? PreviousCache3 {get;set;}
        public TPreview4? PreviousCache4 {get;set;}
        public TPreview5? PreviousCache5 {get;set;}


        public string Id { get; set; }
    }

        public partial class CachelessIds< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5>        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
    {
            public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}
        public TPreview3? PreviousCache3 {get;set;}
        public TPreview4? PreviousCache4 {get;set;}
        public TPreview5? PreviousCache5 {get;set;}


        public (string id, string hash)[] Ids { get; set; }
    }


  


public static partial class CacheId{

    public static CacheId<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5, TPreview6> Create<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5, TPreview6>(string id, TCache cache, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2, TPreview3 preview3, TPreview4 preview4, TPreview5 preview5, TPreview6 preview6) 
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
        where TPreview6 : class
 => new CacheId<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5, TPreview6>(){ Id = id, Data = cache, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2, PreviousCache3 = preview3, PreviousCache4 = preview4, PreviousCache5 = preview5, PreviousCache6 = preview6};
}

public static  partial class CacheIds{

    public static CacheIds<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5, TPreview6> Create<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5, TPreview6>((string id, string hash)[] ids, TCache cache, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2, TPreview3 preview3, TPreview4 preview4, TPreview5 preview5, TPreview6 preview6) 
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
        where TPreview6 : class
 => new CacheIds<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5, TPreview6>(){ Ids = ids, Data = cache, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2, PreviousCache3 = preview3, PreviousCache4 = preview4, PreviousCache5 = preview5, PreviousCache6 = preview6};
}



    public class CacheId<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5, TPreview6>
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
        where TPreview6 : class
    {

        public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}
        public TPreview3? PreviousCache3 {get;set;}
        public TPreview4? PreviousCache4 {get;set;}
        public TPreview5? PreviousCache5 {get;set;}
        public TPreview6? PreviousCache6 {get;set;}

        public TCache? Data { get; set; }

        public string Id { get; set; }
    }

        public class CacheIds<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5, TPreview6>
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
        where TPreview6 : class
    {
            public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}
        public TPreview3? PreviousCache3 {get;set;}
        public TPreview4? PreviousCache4 {get;set;}
        public TPreview5? PreviousCache5 {get;set;}
        public TPreview6? PreviousCache6 {get;set;}

        public TCache? Data { get; set; }

        public (string id, string hash)[] Ids { get; set; }
    }


public  partial class CachelessId{

    public static CachelessId< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5 , TPreview6> Create< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5 , TPreview6>(string id, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2, TPreview3 preview3, TPreview4 preview4, TPreview5 preview5, TPreview6 preview6) 
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
        where TPreview6 : class
 => new CachelessId< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5 , TPreview6>(){ Id = id, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2, PreviousCache3 = preview3, PreviousCache4 = preview4, PreviousCache5 = preview5, PreviousCache6 = preview6};
}

public   partial class CachelessIds{

    public static CachelessIds< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5 , TPreview6> Create< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5 , TPreview6>((string id, string hash)[] ids, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2, TPreview3 preview3, TPreview4 preview4, TPreview5 preview5, TPreview6 preview6) 
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
        where TPreview6 : class
 => new CachelessIds< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5 , TPreview6>(){ Ids = ids, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2, PreviousCache3 = preview3, PreviousCache4 = preview4, PreviousCache5 = preview5, PreviousCache6 = preview6};
}



    public partial class CachelessId< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5 , TPreview6>        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
        where TPreview6 : class
    {

        public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}
        public TPreview3? PreviousCache3 {get;set;}
        public TPreview4? PreviousCache4 {get;set;}
        public TPreview5? PreviousCache5 {get;set;}
        public TPreview6? PreviousCache6 {get;set;}


        public string Id { get; set; }
    }

        public partial class CachelessIds< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5 , TPreview6>        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
        where TPreview6 : class
    {
            public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}
        public TPreview3? PreviousCache3 {get;set;}
        public TPreview4? PreviousCache4 {get;set;}
        public TPreview5? PreviousCache5 {get;set;}
        public TPreview6? PreviousCache6 {get;set;}


        public (string id, string hash)[] Ids { get; set; }
    }


  


public static partial class CacheId{

    public static CacheId<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5, TPreview6, TPreview7> Create<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5, TPreview6, TPreview7>(string id, TCache cache, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2, TPreview3 preview3, TPreview4 preview4, TPreview5 preview5, TPreview6 preview6, TPreview7 preview7) 
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
        where TPreview6 : class
        where TPreview7 : class
 => new CacheId<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5, TPreview6, TPreview7>(){ Id = id, Data = cache, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2, PreviousCache3 = preview3, PreviousCache4 = preview4, PreviousCache5 = preview5, PreviousCache6 = preview6, PreviousCache7 = preview7};
}

public static  partial class CacheIds{

    public static CacheIds<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5, TPreview6, TPreview7> Create<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5, TPreview6, TPreview7>((string id, string hash)[] ids, TCache cache, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2, TPreview3 preview3, TPreview4 preview4, TPreview5 preview5, TPreview6 preview6, TPreview7 preview7) 
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
        where TPreview6 : class
        where TPreview7 : class
 => new CacheIds<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5, TPreview6, TPreview7>(){ Ids = ids, Data = cache, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2, PreviousCache3 = preview3, PreviousCache4 = preview4, PreviousCache5 = preview5, PreviousCache6 = preview6, PreviousCache7 = preview7};
}



    public class CacheId<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5, TPreview6, TPreview7>
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
        where TPreview6 : class
        where TPreview7 : class
    {

        public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}
        public TPreview3? PreviousCache3 {get;set;}
        public TPreview4? PreviousCache4 {get;set;}
        public TPreview5? PreviousCache5 {get;set;}
        public TPreview6? PreviousCache6 {get;set;}
        public TPreview7? PreviousCache7 {get;set;}

        public TCache? Data { get; set; }

        public string Id { get; set; }
    }

        public class CacheIds<TCache, TPreview0, TPreview1, TPreview2, TPreview3, TPreview4, TPreview5, TPreview6, TPreview7>
        where TCache : class
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
        where TPreview6 : class
        where TPreview7 : class
    {
            public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}
        public TPreview3? PreviousCache3 {get;set;}
        public TPreview4? PreviousCache4 {get;set;}
        public TPreview5? PreviousCache5 {get;set;}
        public TPreview6? PreviousCache6 {get;set;}
        public TPreview7? PreviousCache7 {get;set;}

        public TCache? Data { get; set; }

        public (string id, string hash)[] Ids { get; set; }
    }


public  partial class CachelessId{

    public static CachelessId< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5 , TPreview6 , TPreview7> Create< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5 , TPreview6 , TPreview7>(string id, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2, TPreview3 preview3, TPreview4 preview4, TPreview5 preview5, TPreview6 preview6, TPreview7 preview7) 
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
        where TPreview6 : class
        where TPreview7 : class
 => new CachelessId< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5 , TPreview6 , TPreview7>(){ Id = id, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2, PreviousCache3 = preview3, PreviousCache4 = preview4, PreviousCache5 = preview5, PreviousCache6 = preview6, PreviousCache7 = preview7};
}

public   partial class CachelessIds{

    public static CachelessIds< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5 , TPreview6 , TPreview7> Create< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5 , TPreview6 , TPreview7>((string id, string hash)[] ids, TPreview0 preview0, TPreview1 preview1, TPreview2 preview2, TPreview3 preview3, TPreview4 preview4, TPreview5 preview5, TPreview6 preview6, TPreview7 preview7) 
        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
        where TPreview6 : class
        where TPreview7 : class
 => new CachelessIds< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5 , TPreview6 , TPreview7>(){ Ids = ids, PreviousCache0 = preview0, PreviousCache1 = preview1, PreviousCache2 = preview2, PreviousCache3 = preview3, PreviousCache4 = preview4, PreviousCache5 = preview5, PreviousCache6 = preview6, PreviousCache7 = preview7};
}



    public partial class CachelessId< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5 , TPreview6 , TPreview7>        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
        where TPreview6 : class
        where TPreview7 : class
    {

        public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}
        public TPreview3? PreviousCache3 {get;set;}
        public TPreview4? PreviousCache4 {get;set;}
        public TPreview5? PreviousCache5 {get;set;}
        public TPreview6? PreviousCache6 {get;set;}
        public TPreview7? PreviousCache7 {get;set;}


        public string Id { get; set; }
    }

        public partial class CachelessIds< TPreview0 , TPreview1 , TPreview2 , TPreview3 , TPreview4 , TPreview5 , TPreview6 , TPreview7>        where TPreview0 : class
        where TPreview1 : class
        where TPreview2 : class
        where TPreview3 : class
        where TPreview4 : class
        where TPreview5 : class
        where TPreview6 : class
        where TPreview7 : class
    {
            public TPreview0? PreviousCache0 {get;set;}
        public TPreview1? PreviousCache1 {get;set;}
        public TPreview2? PreviousCache2 {get;set;}
        public TPreview3? PreviousCache3 {get;set;}
        public TPreview4? PreviousCache4 {get;set;}
        public TPreview5? PreviousCache5 {get;set;}
        public TPreview6? PreviousCache6 {get;set;}
        public TPreview7? PreviousCache7 {get;set;}


        public (string id, string hash)[] Ids { get; set; }
    }


  


#pragma warning restore CA1819 // Properties should not return arrays
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

}