
using Stasistium.Documents;
using System.Collections.Immutable; 
using System.Threading.Tasks;
using Stasistium.Stages;
using System;
using Stasistium.Core;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Stasistium.Stages
{


    public class ConcatStage<T, TCache1> : GeneratedHelper.Multiple.Simple.OutputMultiSimpleInputSingle1List0StageBase<T, TCache1, T>
        where TCache1 : class
    {
        public ConcatStage(StagePerformHandler<T, TCache1> input1, IGeneratorContext context, string? name) : base(input1, context, name)
        {
        }

        protected override Task<ImmutableList<IDocument<T>>> Work(IDocument<T> input1, OptionToken options)
        {
            return Task.FromResult(ImmutableList.Create(input1));
        }
    }



    public class ConcatStage<T, TCache1, TCache2> : GeneratedHelper.Multiple.Simple.OutputMultiSimpleInputSingle2List0StageBase<T, TCache1, T, TCache2, T>
        where TCache1 : class
        where TCache2 : class
    {
        public ConcatStage(StagePerformHandler<T, TCache1> input1, StagePerformHandler<T, TCache2> input2, IGeneratorContext context, string? name) : base(input1, input2, context, name)
        {
        }

        protected override Task<ImmutableList<IDocument<T>>> Work(IDocument<T> input1, IDocument<T> input2, OptionToken options)
        {
            return Task.FromResult(ImmutableList.Create(input1, input2));
        }
    }



    public class ConcatStage<T, TCache1, TCache2, TCache3> : GeneratedHelper.Multiple.Simple.OutputMultiSimpleInputSingle3List0StageBase<T, TCache1, T, TCache2, T, TCache3, T>
        where TCache1 : class
        where TCache2 : class
        where TCache3 : class
    {
        public ConcatStage(StagePerformHandler<T, TCache1> input1, StagePerformHandler<T, TCache2> input2, StagePerformHandler<T, TCache3> input3, IGeneratorContext context, string? name) : base(input1, input2, input3, context, name)
        {
        }

        protected override Task<ImmutableList<IDocument<T>>> Work(IDocument<T> input1, IDocument<T> input2, IDocument<T> input3, OptionToken options)
        {
            return Task.FromResult(ImmutableList.Create(input1, input2, input3));
        }
    }



    public class ConcatStage<T, TCache1, TCache2, TCache3, TCache4> : GeneratedHelper.Multiple.Simple.OutputMultiSimpleInputSingle4List0StageBase<T, TCache1, T, TCache2, T, TCache3, T, TCache4, T>
        where TCache1 : class
        where TCache2 : class
        where TCache3 : class
        where TCache4 : class
    {
        public ConcatStage(StagePerformHandler<T, TCache1> input1, StagePerformHandler<T, TCache2> input2, StagePerformHandler<T, TCache3> input3, StagePerformHandler<T, TCache4> input4, IGeneratorContext context, string? name) : base(input1, input2, input3, input4, context, name)
        {
        }

        protected override Task<ImmutableList<IDocument<T>>> Work(IDocument<T> input1, IDocument<T> input2, IDocument<T> input3, IDocument<T> input4, OptionToken options)
        {
            return Task.FromResult(ImmutableList.Create(input1, input2, input3, input4));
        }
    }




    public class ConcatManyStage<T, TItemCache1, TCache1> : MultiStageBase<T, string, ConcatStageManyCache<TCache1>>
        where TItemCache1 : class
        where TCache1 : class
    {

        private readonly StagePerformHandler<T, TItemCache1, TCache1> input1;
        public ConcatManyStage(StagePerformHandler<T, TItemCache1, TCache1> input1, IGeneratorContext context, string? name = null) : base(context, name)
        {
            this.input1 = input1;
        }

        protected override async Task<StageResultList<T, string, ConcatStageManyCache<TCache1>>> DoInternal([AllowNull] ConcatStageManyCache<TCache1>? cache, OptionToken options)
        {
            var resultTask1 = this.input1(cache?.PreviouseCache1, options);
await Task.WhenAll(
             resultTask1
).ConfigureAwait(false);
            var result1 = await resultTask1.ConfigureAwait(false);
            var task = LazyTask.Create(async () =>
            {
                var list = ImmutableList<StageResult<T, string>>.Empty.ToBuilder();
                var newCache = new ConcatStageManyCache<TCache1>();


                if (result1.HasChanges)
                {
                    var performed = await result1.Perform;
                    newCache.Ids1 = new string[performed.Count];
                    newCache.PreviouseCache1 = result1.Cache;

                    for (int i = 0; i < performed.Count; i++)
                    {
                        var child = performed[i];

                        if (child.HasChanges)
                        {
                            var childPerformed = await child.Perform;

                            
                            if (cache == null || !cache.IdToHash.TryGetValue(childPerformed.Id, out string? oldHash))
                                oldHash = null;
                            var childHashChanges = oldHash != childPerformed.Hash;

                            list.Add(StageResult.Create(childPerformed, childHashChanges, childPerformed.Id, childPerformed.Hash));
                            newCache.IdToHash.Add(child.Id, childPerformed.Id);

                        }
                        else
                        {

                            var childTask = LazyTask.Create(async () =>
                            {
                                var childPerform = await child.Perform;
                                return childPerform;
                            });
                            if (cache is null || !cache.IdToHash.TryGetValue(child.Id, out var oldHash))
                                throw this.Context.Exception("Should Not Happen");
                            list.Add(StageResult.Create(childTask, false, child.Id, oldHash));
                            newCache.IdToHash.Add(child.Id, oldHash);

                        }
                        newCache.Ids1[i] = child.Id;
                    }

                }
                else
                {
                    if (cache is null)
                        throw this.Context.Exception("Should Not Happen");
                    for (int i = 0; i < cache.Ids1.Length; i++)
                    {
                        var currentIndex = i;
                        var childTask = LazyTask.Create(async () =>
                        {
                            var performed = await result1.Perform;
                            var chiledIndex = performed[currentIndex];
                            var childPerform = await chiledIndex.Perform;
                            // We are in the no changes part. So ther must be no changes.
                            System.Diagnostics.Debug.Assert(!chiledIndex.HasChanges);

                            return childPerform;
                        });
                        if (cache is null || !cache.IdToHash.TryGetValue(cache.Ids1[currentIndex], out var oldHash))
                            throw this.Context.Exception("Should Not Happen");

                        list.Add(StageResult.Create(childTask, false, cache.Ids1[currentIndex], oldHash));
                    }
                    newCache.PreviouseCache1 = cache.PreviouseCache1;
                    newCache.Ids1 = cache.Ids1;
                    foreach (var id in newCache.Ids1)
                    {
                        if (!cache.IdToHash.TryGetValue(id, out var oldHash))
                            throw this.Context.Exception("Should Not Happen");
                        newCache.IdToHash.Add(id, oldHash);
                    }
                }


                return (result:list.ToImmutable(),cache: newCache);
            });

            var hasChanges = false
                ||result1.HasChanges
                ;
            var ids = ImmutableList<string>.Empty.ToBuilder();

            if (hasChanges || cache is null)
            {
                var performed = await task;
                hasChanges = performed.result.Any(x => x.HasChanges);
                if (!hasChanges && cache != null)
                    hasChanges = !performed.cache.Ids1.SequenceEqual(cache.Ids1);
                ids.AddRange(performed.cache.Ids1);
                return StageResultList.Create(performed.result, hasChanges, ids.ToImmutable(), performed.cache);
            }
            else
            {
                ids.AddRange(cache.Ids1);
                var actualTask = LazyTask.Create(async ()=>{
                    var temp = await task;
                return temp.result;
            });
            return StageResultList.Create(actualTask, hasChanges, ids.ToImmutable(), cache);
            }
        }
    }
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning disable CA1819 // Properties should not return arrays
#pragma warning disable CA2227 // Collection properties should be read only

    public class ConcatStageManyCache<TCache1>
    {
        public string[] Ids1 { get; set; }
        public TCache1 PreviouseCache1 { get; set; }

        public Dictionary<string, string> IdToHash { get; set; }

        public ConcatStageManyCache()
        {
            this.IdToHash = new Dictionary<string, string>();
        }
    }
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning restore CA1819 // Properties should not return arrays

    public class ConcatManyStage<T, TItemCache1, TCache1, TItemCache2, TCache2> : MultiStageBase<T, string, ConcatStageManyCache<TCache1, TCache2>>
        where TItemCache1 : class
        where TCache1 : class
        where TItemCache2 : class
        where TCache2 : class
    {

        private readonly StagePerformHandler<T, TItemCache1, TCache1> input1;
        private readonly StagePerformHandler<T, TItemCache2, TCache2> input2;
        public ConcatManyStage(StagePerformHandler<T, TItemCache1, TCache1> input1, StagePerformHandler<T, TItemCache2, TCache2> input2, IGeneratorContext context, string? name = null) : base(context, name)
        {
            this.input1 = input1;
            this.input2 = input2;
        }

        protected override async Task<StageResultList<T, string, ConcatStageManyCache<TCache1, TCache2>>> DoInternal([AllowNull] ConcatStageManyCache<TCache1, TCache2>? cache, OptionToken options)
        {
            var resultTask1 = this.input1(cache?.PreviouseCache1, options);
            var resultTask2 = this.input2(cache?.PreviouseCache2, options);
await Task.WhenAll(
             resultTask1
             ,resultTask2
).ConfigureAwait(false);
            var result1 = await resultTask1.ConfigureAwait(false);
            var result2 = await resultTask2.ConfigureAwait(false);
            var task = LazyTask.Create(async () =>
            {
                var list = ImmutableList<StageResult<T, string>>.Empty.ToBuilder();
                var newCache = new ConcatStageManyCache<TCache1, TCache2>();


                if (result1.HasChanges)
                {
                    var performed = await result1.Perform;
                    newCache.Ids1 = new string[performed.Count];
                    newCache.PreviouseCache1 = result1.Cache;

                    for (int i = 0; i < performed.Count; i++)
                    {
                        var child = performed[i];

                        if (child.HasChanges)
                        {
                            var childPerformed = await child.Perform;

                            
                            if (cache == null || !cache.IdToHash.TryGetValue(childPerformed.Id, out string? oldHash))
                                oldHash = null;
                            var childHashChanges = oldHash != childPerformed.Hash;

                            list.Add(StageResult.Create(childPerformed, childHashChanges, childPerformed.Id, childPerformed.Hash));
                            newCache.IdToHash.Add(child.Id, childPerformed.Id);

                        }
                        else
                        {

                            var childTask = LazyTask.Create(async () =>
                            {
                                var childPerform = await child.Perform;
                                return childPerform;
                            });
                            if (cache is null || !cache.IdToHash.TryGetValue(child.Id, out var oldHash))
                                throw this.Context.Exception("Should Not Happen");
                            list.Add(StageResult.Create(childTask, false, child.Id, oldHash));
                            newCache.IdToHash.Add(child.Id, oldHash);

                        }
                        newCache.Ids1[i] = child.Id;
                    }

                }
                else
                {
                    if (cache is null)
                        throw this.Context.Exception("Should Not Happen");
                    for (int i = 0; i < cache.Ids1.Length; i++)
                    {
                        var currentIndex = i;
                        var childTask = LazyTask.Create(async () =>
                        {
                            var performed = await result1.Perform;
                            var chiledIndex = performed[currentIndex];
                            var childPerform = await chiledIndex.Perform;
                            // We are in the no changes part. So ther must be no changes.
                            System.Diagnostics.Debug.Assert(!chiledIndex.HasChanges);

                            return childPerform;
                        });
                        if (cache is null || !cache.IdToHash.TryGetValue(cache.Ids1[currentIndex], out var oldHash))
                            throw this.Context.Exception("Should Not Happen");

                        list.Add(StageResult.Create(childTask, false, cache.Ids1[currentIndex], oldHash));
                    }
                    newCache.PreviouseCache1 = cache.PreviouseCache1;
                    newCache.Ids1 = cache.Ids1;
                    foreach (var id in newCache.Ids1)
                    {
                        if (!cache.IdToHash.TryGetValue(id, out var oldHash))
                            throw this.Context.Exception("Should Not Happen");
                        newCache.IdToHash.Add(id, oldHash);
                    }
                }


                if (result2.HasChanges)
                {
                    var performed = await result2.Perform;
                    newCache.Ids2 = new string[performed.Count];
                    newCache.PreviouseCache2 = result2.Cache;

                    for (int i = 0; i < performed.Count; i++)
                    {
                        var child = performed[i];

                        if (child.HasChanges)
                        {
                            var childPerformed = await child.Perform;

                            
                            if (cache == null || !cache.IdToHash.TryGetValue(childPerformed.Id, out string? oldHash))
                                oldHash = null;
                            var childHashChanges = oldHash != childPerformed.Hash;

                            list.Add(StageResult.Create(childPerformed, childHashChanges, childPerformed.Id, childPerformed.Hash));
                            newCache.IdToHash.Add(child.Id, childPerformed.Id);

                        }
                        else
                        {

                            var childTask = LazyTask.Create(async () =>
                            {
                                var childPerform = await child.Perform;
                                return childPerform;
                            });
                            if (cache is null || !cache.IdToHash.TryGetValue(child.Id, out var oldHash))
                                throw this.Context.Exception("Should Not Happen");
                            list.Add(StageResult.Create(childTask, false, child.Id, oldHash));
                            newCache.IdToHash.Add(child.Id, oldHash);

                        }
                        newCache.Ids2[i] = child.Id;
                    }

                }
                else
                {
                    if (cache is null)
                        throw this.Context.Exception("Should Not Happen");
                    for (int i = 0; i < cache.Ids2.Length; i++)
                    {
                        var currentIndex = i;
                        var childTask = LazyTask.Create(async () =>
                        {
                            var performed = await result2.Perform;
                            var chiledIndex = performed[currentIndex];
                            var childPerform = await chiledIndex.Perform;
                            // We are in the no changes part. So ther must be no changes.
                            System.Diagnostics.Debug.Assert(!chiledIndex.HasChanges);

                            return childPerform;
                        });
                        if (cache is null || !cache.IdToHash.TryGetValue(cache.Ids2[currentIndex], out var oldHash))
                            throw this.Context.Exception("Should Not Happen");

                        list.Add(StageResult.Create(childTask, false, cache.Ids2[currentIndex], oldHash));
                    }
                    newCache.PreviouseCache2 = cache.PreviouseCache2;
                    newCache.Ids2 = cache.Ids2;
                    foreach (var id in newCache.Ids2)
                    {
                        if (!cache.IdToHash.TryGetValue(id, out var oldHash))
                            throw this.Context.Exception("Should Not Happen");
                        newCache.IdToHash.Add(id, oldHash);
                    }
                }


                return (result:list.ToImmutable(),cache: newCache);
            });

            var hasChanges = false
                ||result1.HasChanges
                ||result2.HasChanges
                ;
            var ids = ImmutableList<string>.Empty.ToBuilder();

            if (hasChanges || cache is null)
            {
                var performed = await task;
                hasChanges = performed.result.Any(x => x.HasChanges);
                if (!hasChanges && cache != null)
                    hasChanges = !performed.cache.Ids1.SequenceEqual(cache.Ids1);
                if (!hasChanges && cache != null)
                    hasChanges = !performed.cache.Ids2.SequenceEqual(cache.Ids2);
                ids.AddRange(performed.cache.Ids1);
                ids.AddRange(performed.cache.Ids2);
                return StageResultList.Create(performed.result, hasChanges, ids.ToImmutable(), performed.cache);
            }
            else
            {
                ids.AddRange(cache.Ids1);
                ids.AddRange(cache.Ids2);
                var actualTask = LazyTask.Create(async ()=>{
                    var temp = await task;
                return temp.result;
            });
            return StageResultList.Create(actualTask, hasChanges, ids.ToImmutable(), cache);
            }
        }
    }
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning disable CA1819 // Properties should not return arrays
#pragma warning disable CA2227 // Collection properties should be read only

    public class ConcatStageManyCache<TCache1, TCache2>
    {
        public string[] Ids1 { get; set; }
        public TCache1 PreviouseCache1 { get; set; }

        public string[] Ids2 { get; set; }
        public TCache2 PreviouseCache2 { get; set; }

        public Dictionary<string, string> IdToHash { get; set; }

        public ConcatStageManyCache()
        {
            this.IdToHash = new Dictionary<string, string>();
        }
    }
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning restore CA1819 // Properties should not return arrays

    public class ConcatManyStage<T, TItemCache1, TCache1, TItemCache2, TCache2, TItemCache3, TCache3> : MultiStageBase<T, string, ConcatStageManyCache<TCache1, TCache2, TCache3>>
        where TItemCache1 : class
        where TCache1 : class
        where TItemCache2 : class
        where TCache2 : class
        where TItemCache3 : class
        where TCache3 : class
    {

        private readonly StagePerformHandler<T, TItemCache1, TCache1> input1;
        private readonly StagePerformHandler<T, TItemCache2, TCache2> input2;
        private readonly StagePerformHandler<T, TItemCache3, TCache3> input3;
        public ConcatManyStage(StagePerformHandler<T, TItemCache1, TCache1> input1, StagePerformHandler<T, TItemCache2, TCache2> input2, StagePerformHandler<T, TItemCache3, TCache3> input3, IGeneratorContext context, string? name = null) : base(context, name)
        {
            this.input1 = input1;
            this.input2 = input2;
            this.input3 = input3;
        }

        protected override async Task<StageResultList<T, string, ConcatStageManyCache<TCache1, TCache2, TCache3>>> DoInternal([AllowNull] ConcatStageManyCache<TCache1, TCache2, TCache3>? cache, OptionToken options)
        {
            var resultTask1 = this.input1(cache?.PreviouseCache1, options);
            var resultTask2 = this.input2(cache?.PreviouseCache2, options);
            var resultTask3 = this.input3(cache?.PreviouseCache3, options);
await Task.WhenAll(
             resultTask1
             ,resultTask2
             ,resultTask3
).ConfigureAwait(false);
            var result1 = await resultTask1.ConfigureAwait(false);
            var result2 = await resultTask2.ConfigureAwait(false);
            var result3 = await resultTask3.ConfigureAwait(false);
            var task = LazyTask.Create(async () =>
            {
                var list = ImmutableList<StageResult<T, string>>.Empty.ToBuilder();
                var newCache = new ConcatStageManyCache<TCache1, TCache2, TCache3>();


                if (result1.HasChanges)
                {
                    var performed = await result1.Perform;
                    newCache.Ids1 = new string[performed.Count];
                    newCache.PreviouseCache1 = result1.Cache;

                    for (int i = 0; i < performed.Count; i++)
                    {
                        var child = performed[i];

                        if (child.HasChanges)
                        {
                            var childPerformed = await child.Perform;

                            
                            if (cache == null || !cache.IdToHash.TryGetValue(childPerformed.Id, out string? oldHash))
                                oldHash = null;
                            var childHashChanges = oldHash != childPerformed.Hash;

                            list.Add(StageResult.Create(childPerformed, childHashChanges, childPerformed.Id, childPerformed.Hash));
                            newCache.IdToHash.Add(child.Id, childPerformed.Id);

                        }
                        else
                        {

                            var childTask = LazyTask.Create(async () =>
                            {
                                var childPerform = await child.Perform;
                                return childPerform;
                            });
                            if (cache is null || !cache.IdToHash.TryGetValue(child.Id, out var oldHash))
                                throw this.Context.Exception("Should Not Happen");
                            list.Add(StageResult.Create(childTask, false, child.Id, oldHash));
                            newCache.IdToHash.Add(child.Id, oldHash);

                        }
                        newCache.Ids1[i] = child.Id;
                    }

                }
                else
                {
                    if (cache is null)
                        throw this.Context.Exception("Should Not Happen");
                    for (int i = 0; i < cache.Ids1.Length; i++)
                    {
                        var currentIndex = i;
                        var childTask = LazyTask.Create(async () =>
                        {
                            var performed = await result1.Perform;
                            var chiledIndex = performed[currentIndex];
                            var childPerform = await chiledIndex.Perform;
                            // We are in the no changes part. So ther must be no changes.
                            System.Diagnostics.Debug.Assert(!chiledIndex.HasChanges);

                            return childPerform;
                        });
                        if (cache is null || !cache.IdToHash.TryGetValue(cache.Ids1[currentIndex], out var oldHash))
                            throw this.Context.Exception("Should Not Happen");

                        list.Add(StageResult.Create(childTask, false, cache.Ids1[currentIndex], oldHash));
                    }
                    newCache.PreviouseCache1 = cache.PreviouseCache1;
                    newCache.Ids1 = cache.Ids1;
                    foreach (var id in newCache.Ids1)
                    {
                        if (!cache.IdToHash.TryGetValue(id, out var oldHash))
                            throw this.Context.Exception("Should Not Happen");
                        newCache.IdToHash.Add(id, oldHash);
                    }
                }


                if (result2.HasChanges)
                {
                    var performed = await result2.Perform;
                    newCache.Ids2 = new string[performed.Count];
                    newCache.PreviouseCache2 = result2.Cache;

                    for (int i = 0; i < performed.Count; i++)
                    {
                        var child = performed[i];

                        if (child.HasChanges)
                        {
                            var childPerformed = await child.Perform;

                            
                            if (cache == null || !cache.IdToHash.TryGetValue(childPerformed.Id, out string? oldHash))
                                oldHash = null;
                            var childHashChanges = oldHash != childPerformed.Hash;

                            list.Add(StageResult.Create(childPerformed, childHashChanges, childPerformed.Id, childPerformed.Hash));
                            newCache.IdToHash.Add(child.Id, childPerformed.Id);

                        }
                        else
                        {

                            var childTask = LazyTask.Create(async () =>
                            {
                                var childPerform = await child.Perform;
                                return childPerform;
                            });
                            if (cache is null || !cache.IdToHash.TryGetValue(child.Id, out var oldHash))
                                throw this.Context.Exception("Should Not Happen");
                            list.Add(StageResult.Create(childTask, false, child.Id, oldHash));
                            newCache.IdToHash.Add(child.Id, oldHash);

                        }
                        newCache.Ids2[i] = child.Id;
                    }

                }
                else
                {
                    if (cache is null)
                        throw this.Context.Exception("Should Not Happen");
                    for (int i = 0; i < cache.Ids2.Length; i++)
                    {
                        var currentIndex = i;
                        var childTask = LazyTask.Create(async () =>
                        {
                            var performed = await result2.Perform;
                            var chiledIndex = performed[currentIndex];
                            var childPerform = await chiledIndex.Perform;
                            // We are in the no changes part. So ther must be no changes.
                            System.Diagnostics.Debug.Assert(!chiledIndex.HasChanges);

                            return childPerform;
                        });
                        if (cache is null || !cache.IdToHash.TryGetValue(cache.Ids2[currentIndex], out var oldHash))
                            throw this.Context.Exception("Should Not Happen");

                        list.Add(StageResult.Create(childTask, false, cache.Ids2[currentIndex], oldHash));
                    }
                    newCache.PreviouseCache2 = cache.PreviouseCache2;
                    newCache.Ids2 = cache.Ids2;
                    foreach (var id in newCache.Ids2)
                    {
                        if (!cache.IdToHash.TryGetValue(id, out var oldHash))
                            throw this.Context.Exception("Should Not Happen");
                        newCache.IdToHash.Add(id, oldHash);
                    }
                }


                if (result3.HasChanges)
                {
                    var performed = await result3.Perform;
                    newCache.Ids3 = new string[performed.Count];
                    newCache.PreviouseCache3 = result3.Cache;

                    for (int i = 0; i < performed.Count; i++)
                    {
                        var child = performed[i];

                        if (child.HasChanges)
                        {
                            var childPerformed = await child.Perform;

                            
                            if (cache == null || !cache.IdToHash.TryGetValue(childPerformed.Id, out string? oldHash))
                                oldHash = null;
                            var childHashChanges = oldHash != childPerformed.Hash;

                            list.Add(StageResult.Create(childPerformed, childHashChanges, childPerformed.Id, childPerformed.Hash));
                            newCache.IdToHash.Add(child.Id, childPerformed.Id);

                        }
                        else
                        {

                            var childTask = LazyTask.Create(async () =>
                            {
                                var childPerform = await child.Perform;
                                return childPerform;
                            });
                            if (cache is null || !cache.IdToHash.TryGetValue(child.Id, out var oldHash))
                                throw this.Context.Exception("Should Not Happen");
                            list.Add(StageResult.Create(childTask, false, child.Id, oldHash));
                            newCache.IdToHash.Add(child.Id, oldHash);

                        }
                        newCache.Ids3[i] = child.Id;
                    }

                }
                else
                {
                    if (cache is null)
                        throw this.Context.Exception("Should Not Happen");
                    for (int i = 0; i < cache.Ids3.Length; i++)
                    {
                        var currentIndex = i;
                        var childTask = LazyTask.Create(async () =>
                        {
                            var performed = await result3.Perform;
                            var chiledIndex = performed[currentIndex];
                            var childPerform = await chiledIndex.Perform;
                            // We are in the no changes part. So ther must be no changes.
                            System.Diagnostics.Debug.Assert(!chiledIndex.HasChanges);

                            return childPerform;
                        });
                        if (cache is null || !cache.IdToHash.TryGetValue(cache.Ids3[currentIndex], out var oldHash))
                            throw this.Context.Exception("Should Not Happen");

                        list.Add(StageResult.Create(childTask, false, cache.Ids3[currentIndex], oldHash));
                    }
                    newCache.PreviouseCache3 = cache.PreviouseCache3;
                    newCache.Ids3 = cache.Ids3;
                    foreach (var id in newCache.Ids3)
                    {
                        if (!cache.IdToHash.TryGetValue(id, out var oldHash))
                            throw this.Context.Exception("Should Not Happen");
                        newCache.IdToHash.Add(id, oldHash);
                    }
                }


                return (result:list.ToImmutable(),cache: newCache);
            });

            var hasChanges = false
                ||result1.HasChanges
                ||result2.HasChanges
                ||result3.HasChanges
                ;
            var ids = ImmutableList<string>.Empty.ToBuilder();

            if (hasChanges || cache is null)
            {
                var performed = await task;
                hasChanges = performed.result.Any(x => x.HasChanges);
                if (!hasChanges && cache != null)
                    hasChanges = !performed.cache.Ids1.SequenceEqual(cache.Ids1);
                if (!hasChanges && cache != null)
                    hasChanges = !performed.cache.Ids2.SequenceEqual(cache.Ids2);
                if (!hasChanges && cache != null)
                    hasChanges = !performed.cache.Ids3.SequenceEqual(cache.Ids3);
                ids.AddRange(performed.cache.Ids1);
                ids.AddRange(performed.cache.Ids2);
                ids.AddRange(performed.cache.Ids3);
                return StageResultList.Create(performed.result, hasChanges, ids.ToImmutable(), performed.cache);
            }
            else
            {
                ids.AddRange(cache.Ids1);
                ids.AddRange(cache.Ids2);
                ids.AddRange(cache.Ids3);
                var actualTask = LazyTask.Create(async ()=>{
                    var temp = await task;
                return temp.result;
            });
            return StageResultList.Create(actualTask, hasChanges, ids.ToImmutable(), cache);
            }
        }
    }
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning disable CA1819 // Properties should not return arrays
#pragma warning disable CA2227 // Collection properties should be read only

    public class ConcatStageManyCache<TCache1, TCache2, TCache3>
    {
        public string[] Ids1 { get; set; }
        public TCache1 PreviouseCache1 { get; set; }

        public string[] Ids2 { get; set; }
        public TCache2 PreviouseCache2 { get; set; }

        public string[] Ids3 { get; set; }
        public TCache3 PreviouseCache3 { get; set; }

        public Dictionary<string, string> IdToHash { get; set; }

        public ConcatStageManyCache()
        {
            this.IdToHash = new Dictionary<string, string>();
        }
    }
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning restore CA1819 // Properties should not return arrays

    public class ConcatManyStage<T, TItemCache1, TCache1, TItemCache2, TCache2, TItemCache3, TCache3, TItemCache4, TCache4> : MultiStageBase<T, string, ConcatStageManyCache<TCache1, TCache2, TCache3, TCache4>>
        where TItemCache1 : class
        where TCache1 : class
        where TItemCache2 : class
        where TCache2 : class
        where TItemCache3 : class
        where TCache3 : class
        where TItemCache4 : class
        where TCache4 : class
    {

        private readonly StagePerformHandler<T, TItemCache1, TCache1> input1;
        private readonly StagePerformHandler<T, TItemCache2, TCache2> input2;
        private readonly StagePerformHandler<T, TItemCache3, TCache3> input3;
        private readonly StagePerformHandler<T, TItemCache4, TCache4> input4;
        public ConcatManyStage(StagePerformHandler<T, TItemCache1, TCache1> input1, StagePerformHandler<T, TItemCache2, TCache2> input2, StagePerformHandler<T, TItemCache3, TCache3> input3, StagePerformHandler<T, TItemCache4, TCache4> input4, IGeneratorContext context, string? name = null) : base(context, name)
        {
            this.input1 = input1;
            this.input2 = input2;
            this.input3 = input3;
            this.input4 = input4;
        }

        protected override async Task<StageResultList<T, string, ConcatStageManyCache<TCache1, TCache2, TCache3, TCache4>>> DoInternal([AllowNull] ConcatStageManyCache<TCache1, TCache2, TCache3, TCache4>? cache, OptionToken options)
        {
            var resultTask1 = this.input1(cache?.PreviouseCache1, options);
            var resultTask2 = this.input2(cache?.PreviouseCache2, options);
            var resultTask3 = this.input3(cache?.PreviouseCache3, options);
            var resultTask4 = this.input4(cache?.PreviouseCache4, options);
await Task.WhenAll(
             resultTask1
             ,resultTask2
             ,resultTask3
             ,resultTask4
).ConfigureAwait(false);
            var result1 = await resultTask1.ConfigureAwait(false);
            var result2 = await resultTask2.ConfigureAwait(false);
            var result3 = await resultTask3.ConfigureAwait(false);
            var result4 = await resultTask4.ConfigureAwait(false);
            var task = LazyTask.Create(async () =>
            {
                var list = ImmutableList<StageResult<T, string>>.Empty.ToBuilder();
                var newCache = new ConcatStageManyCache<TCache1, TCache2, TCache3, TCache4>();


                if (result1.HasChanges)
                {
                    var performed = await result1.Perform;
                    newCache.Ids1 = new string[performed.Count];
                    newCache.PreviouseCache1 = result1.Cache;

                    for (int i = 0; i < performed.Count; i++)
                    {
                        var child = performed[i];

                        if (child.HasChanges)
                        {
                            var childPerformed = await child.Perform;

                            
                            if (cache == null || !cache.IdToHash.TryGetValue(childPerformed.Id, out string? oldHash))
                                oldHash = null;
                            var childHashChanges = oldHash != childPerformed.Hash;

                            list.Add(StageResult.Create(childPerformed, childHashChanges, childPerformed.Id, childPerformed.Hash));
                            newCache.IdToHash.Add(child.Id, childPerformed.Id);

                        }
                        else
                        {

                            var childTask = LazyTask.Create(async () =>
                            {
                                var childPerform = await child.Perform;
                                return childPerform;
                            });
                            if (cache is null || !cache.IdToHash.TryGetValue(child.Id, out var oldHash))
                                throw this.Context.Exception("Should Not Happen");
                            list.Add(StageResult.Create(childTask, false, child.Id, oldHash));
                            newCache.IdToHash.Add(child.Id, oldHash);

                        }
                        newCache.Ids1[i] = child.Id;
                    }

                }
                else
                {
                    if (cache is null)
                        throw this.Context.Exception("Should Not Happen");
                    for (int i = 0; i < cache.Ids1.Length; i++)
                    {
                        var currentIndex = i;
                        var childTask = LazyTask.Create(async () =>
                        {
                            var performed = await result1.Perform;
                            var chiledIndex = performed[currentIndex];
                            var childPerform = await chiledIndex.Perform;
                            // We are in the no changes part. So ther must be no changes.
                            System.Diagnostics.Debug.Assert(!chiledIndex.HasChanges);

                            return childPerform;
                        });
                        if (cache is null || !cache.IdToHash.TryGetValue(cache.Ids1[currentIndex], out var oldHash))
                            throw this.Context.Exception("Should Not Happen");

                        list.Add(StageResult.Create(childTask, false, cache.Ids1[currentIndex], oldHash));
                    }
                    newCache.PreviouseCache1 = cache.PreviouseCache1;
                    newCache.Ids1 = cache.Ids1;
                    foreach (var id in newCache.Ids1)
                    {
                        if (!cache.IdToHash.TryGetValue(id, out var oldHash))
                            throw this.Context.Exception("Should Not Happen");
                        newCache.IdToHash.Add(id, oldHash);
                    }
                }


                if (result2.HasChanges)
                {
                    var performed = await result2.Perform;
                    newCache.Ids2 = new string[performed.Count];
                    newCache.PreviouseCache2 = result2.Cache;

                    for (int i = 0; i < performed.Count; i++)
                    {
                        var child = performed[i];

                        if (child.HasChanges)
                        {
                            var childPerformed = await child.Perform;

                            
                            if (cache == null || !cache.IdToHash.TryGetValue(childPerformed.Id, out string? oldHash))
                                oldHash = null;
                            var childHashChanges = oldHash != childPerformed.Hash;

                            list.Add(StageResult.Create(childPerformed, childHashChanges, childPerformed.Id, childPerformed.Hash));
                            newCache.IdToHash.Add(child.Id, childPerformed.Id);

                        }
                        else
                        {

                            var childTask = LazyTask.Create(async () =>
                            {
                                var childPerform = await child.Perform;
                                return childPerform;
                            });
                            if (cache is null || !cache.IdToHash.TryGetValue(child.Id, out var oldHash))
                                throw this.Context.Exception("Should Not Happen");
                            list.Add(StageResult.Create(childTask, false, child.Id, oldHash));
                            newCache.IdToHash.Add(child.Id, oldHash);

                        }
                        newCache.Ids2[i] = child.Id;
                    }

                }
                else
                {
                    if (cache is null)
                        throw this.Context.Exception("Should Not Happen");
                    for (int i = 0; i < cache.Ids2.Length; i++)
                    {
                        var currentIndex = i;
                        var childTask = LazyTask.Create(async () =>
                        {
                            var performed = await result2.Perform;
                            var chiledIndex = performed[currentIndex];
                            var childPerform = await chiledIndex.Perform;
                            // We are in the no changes part. So ther must be no changes.
                            System.Diagnostics.Debug.Assert(!chiledIndex.HasChanges);

                            return childPerform;
                        });
                        if (cache is null || !cache.IdToHash.TryGetValue(cache.Ids2[currentIndex], out var oldHash))
                            throw this.Context.Exception("Should Not Happen");

                        list.Add(StageResult.Create(childTask, false, cache.Ids2[currentIndex], oldHash));
                    }
                    newCache.PreviouseCache2 = cache.PreviouseCache2;
                    newCache.Ids2 = cache.Ids2;
                    foreach (var id in newCache.Ids2)
                    {
                        if (!cache.IdToHash.TryGetValue(id, out var oldHash))
                            throw this.Context.Exception("Should Not Happen");
                        newCache.IdToHash.Add(id, oldHash);
                    }
                }


                if (result3.HasChanges)
                {
                    var performed = await result3.Perform;
                    newCache.Ids3 = new string[performed.Count];
                    newCache.PreviouseCache3 = result3.Cache;

                    for (int i = 0; i < performed.Count; i++)
                    {
                        var child = performed[i];

                        if (child.HasChanges)
                        {
                            var childPerformed = await child.Perform;

                            
                            if (cache == null || !cache.IdToHash.TryGetValue(childPerformed.Id, out string? oldHash))
                                oldHash = null;
                            var childHashChanges = oldHash != childPerformed.Hash;

                            list.Add(StageResult.Create(childPerformed, childHashChanges, childPerformed.Id, childPerformed.Hash));
                            newCache.IdToHash.Add(child.Id, childPerformed.Id);

                        }
                        else
                        {

                            var childTask = LazyTask.Create(async () =>
                            {
                                var childPerform = await child.Perform;
                                return childPerform;
                            });
                            if (cache is null || !cache.IdToHash.TryGetValue(child.Id, out var oldHash))
                                throw this.Context.Exception("Should Not Happen");
                            list.Add(StageResult.Create(childTask, false, child.Id, oldHash));
                            newCache.IdToHash.Add(child.Id, oldHash);

                        }
                        newCache.Ids3[i] = child.Id;
                    }

                }
                else
                {
                    if (cache is null)
                        throw this.Context.Exception("Should Not Happen");
                    for (int i = 0; i < cache.Ids3.Length; i++)
                    {
                        var currentIndex = i;
                        var childTask = LazyTask.Create(async () =>
                        {
                            var performed = await result3.Perform;
                            var chiledIndex = performed[currentIndex];
                            var childPerform = await chiledIndex.Perform;
                            // We are in the no changes part. So ther must be no changes.
                            System.Diagnostics.Debug.Assert(!chiledIndex.HasChanges);

                            return childPerform;
                        });
                        if (cache is null || !cache.IdToHash.TryGetValue(cache.Ids3[currentIndex], out var oldHash))
                            throw this.Context.Exception("Should Not Happen");

                        list.Add(StageResult.Create(childTask, false, cache.Ids3[currentIndex], oldHash));
                    }
                    newCache.PreviouseCache3 = cache.PreviouseCache3;
                    newCache.Ids3 = cache.Ids3;
                    foreach (var id in newCache.Ids3)
                    {
                        if (!cache.IdToHash.TryGetValue(id, out var oldHash))
                            throw this.Context.Exception("Should Not Happen");
                        newCache.IdToHash.Add(id, oldHash);
                    }
                }


                if (result4.HasChanges)
                {
                    var performed = await result4.Perform;
                    newCache.Ids4 = new string[performed.Count];
                    newCache.PreviouseCache4 = result4.Cache;

                    for (int i = 0; i < performed.Count; i++)
                    {
                        var child = performed[i];

                        if (child.HasChanges)
                        {
                            var childPerformed = await child.Perform;

                            
                            if (cache == null || !cache.IdToHash.TryGetValue(childPerformed.Id, out string? oldHash))
                                oldHash = null;
                            var childHashChanges = oldHash != childPerformed.Hash;

                            list.Add(StageResult.Create(childPerformed, childHashChanges, childPerformed.Id, childPerformed.Hash));
                            newCache.IdToHash.Add(child.Id, childPerformed.Id);

                        }
                        else
                        {

                            var childTask = LazyTask.Create(async () =>
                            {
                                var childPerform = await child.Perform;
                                return childPerform;
                            });
                            if (cache is null || !cache.IdToHash.TryGetValue(child.Id, out var oldHash))
                                throw this.Context.Exception("Should Not Happen");
                            list.Add(StageResult.Create(childTask, false, child.Id, oldHash));
                            newCache.IdToHash.Add(child.Id, oldHash);

                        }
                        newCache.Ids4[i] = child.Id;
                    }

                }
                else
                {
                    if (cache is null)
                        throw this.Context.Exception("Should Not Happen");
                    for (int i = 0; i < cache.Ids4.Length; i++)
                    {
                        var currentIndex = i;
                        var childTask = LazyTask.Create(async () =>
                        {
                            var performed = await result4.Perform;
                            var chiledIndex = performed[currentIndex];
                            var childPerform = await chiledIndex.Perform;
                            // We are in the no changes part. So ther must be no changes.
                            System.Diagnostics.Debug.Assert(!chiledIndex.HasChanges);

                            return childPerform;
                        });
                        if (cache is null || !cache.IdToHash.TryGetValue(cache.Ids4[currentIndex], out var oldHash))
                            throw this.Context.Exception("Should Not Happen");

                        list.Add(StageResult.Create(childTask, false, cache.Ids4[currentIndex], oldHash));
                    }
                    newCache.PreviouseCache4 = cache.PreviouseCache4;
                    newCache.Ids4 = cache.Ids4;
                    foreach (var id in newCache.Ids4)
                    {
                        if (!cache.IdToHash.TryGetValue(id, out var oldHash))
                            throw this.Context.Exception("Should Not Happen");
                        newCache.IdToHash.Add(id, oldHash);
                    }
                }


                return (result:list.ToImmutable(),cache: newCache);
            });

            var hasChanges = false
                ||result1.HasChanges
                ||result2.HasChanges
                ||result3.HasChanges
                ||result4.HasChanges
                ;
            var ids = ImmutableList<string>.Empty.ToBuilder();

            if (hasChanges || cache is null)
            {
                var performed = await task;
                hasChanges = performed.result.Any(x => x.HasChanges);
                if (!hasChanges && cache != null)
                    hasChanges = !performed.cache.Ids1.SequenceEqual(cache.Ids1);
                if (!hasChanges && cache != null)
                    hasChanges = !performed.cache.Ids2.SequenceEqual(cache.Ids2);
                if (!hasChanges && cache != null)
                    hasChanges = !performed.cache.Ids3.SequenceEqual(cache.Ids3);
                if (!hasChanges && cache != null)
                    hasChanges = !performed.cache.Ids4.SequenceEqual(cache.Ids4);
                ids.AddRange(performed.cache.Ids1);
                ids.AddRange(performed.cache.Ids2);
                ids.AddRange(performed.cache.Ids3);
                ids.AddRange(performed.cache.Ids4);
                return StageResultList.Create(performed.result, hasChanges, ids.ToImmutable(), performed.cache);
            }
            else
            {
                ids.AddRange(cache.Ids1);
                ids.AddRange(cache.Ids2);
                ids.AddRange(cache.Ids3);
                ids.AddRange(cache.Ids4);
                var actualTask = LazyTask.Create(async ()=>{
                    var temp = await task;
                return temp.result;
            });
            return StageResultList.Create(actualTask, hasChanges, ids.ToImmutable(), cache);
            }
        }
    }
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning disable CA1819 // Properties should not return arrays
#pragma warning disable CA2227 // Collection properties should be read only

    public class ConcatStageManyCache<TCache1, TCache2, TCache3, TCache4>
    {
        public string[] Ids1 { get; set; }
        public TCache1 PreviouseCache1 { get; set; }

        public string[] Ids2 { get; set; }
        public TCache2 PreviouseCache2 { get; set; }

        public string[] Ids3 { get; set; }
        public TCache3 PreviouseCache3 { get; set; }

        public string[] Ids4 { get; set; }
        public TCache4 PreviouseCache4 { get; set; }

        public Dictionary<string, string> IdToHash { get; set; }

        public ConcatStageManyCache()
        {
            this.IdToHash = new Dictionary<string, string>();
        }
    }
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning restore CA1819 // Properties should not return arrays


}

namespace Stasistium
{


    public static partial class StageExtensions
    {
        public static ConcatStage<T, TCache1> Concat<T, TCache1>(this StageBase<T, TCache1> input1, string? name = null)
            where TCache1 : class
        {
            if(input1 is null)
                 throw new ArgumentNullException(nameof(input1));
            return new ConcatStage<T, TCache1>(input1.DoIt, input1.Context, name);
        }
        public static ConcatStage<T, TCache1, TCache2> Concat<T, TCache1, TCache2>(this StageBase<T, TCache1> input1, StageBase<T, TCache2> input2, string? name = null)
            where TCache1 : class
            where TCache2 : class
        {
            if(input1 is null)
                 throw new ArgumentNullException(nameof(input1));
            if(input2 is null)
                 throw new ArgumentNullException(nameof(input2));
            return new ConcatStage<T, TCache1, TCache2>(input1.DoIt, input2.DoIt, input1.Context, name);
        }
        public static ConcatStage<T, TCache1, TCache2, TCache3> Concat<T, TCache1, TCache2, TCache3>(this StageBase<T, TCache1> input1, StageBase<T, TCache2> input2, StageBase<T, TCache3> input3, string? name = null)
            where TCache1 : class
            where TCache2 : class
            where TCache3 : class
        {
            if(input1 is null)
                 throw new ArgumentNullException(nameof(input1));
            if(input2 is null)
                 throw new ArgumentNullException(nameof(input2));
            if(input3 is null)
                 throw new ArgumentNullException(nameof(input3));
            return new ConcatStage<T, TCache1, TCache2, TCache3>(input1.DoIt, input2.DoIt, input3.DoIt, input1.Context, name);
        }
        public static ConcatStage<T, TCache1, TCache2, TCache3, TCache4> Concat<T, TCache1, TCache2, TCache3, TCache4>(this StageBase<T, TCache1> input1, StageBase<T, TCache2> input2, StageBase<T, TCache3> input3, StageBase<T, TCache4> input4, string? name = null)
            where TCache1 : class
            where TCache2 : class
            where TCache3 : class
            where TCache4 : class
        {
            if(input1 is null)
                 throw new ArgumentNullException(nameof(input1));
            if(input2 is null)
                 throw new ArgumentNullException(nameof(input2));
            if(input3 is null)
                 throw new ArgumentNullException(nameof(input3));
            if(input4 is null)
                 throw new ArgumentNullException(nameof(input4));
            return new ConcatStage<T, TCache1, TCache2, TCache3, TCache4>(input1.DoIt, input2.DoIt, input3.DoIt, input4.DoIt, input1.Context, name);
        }

        public static ConcatManyStage<T, TItemCache1, TCache1> Concat<T, TItemCache1, TCache1>(this MultiStageBase<T, TItemCache1, TCache1> input1, string? name = null)
            where TItemCache1 : class
        where TCache1 : class
        {
            if(input1 is null)
                 throw new ArgumentNullException(nameof(input1));
            return new ConcatManyStage<T, TItemCache1, TCache1>(input1.DoIt, input1.Context, name);
        }
        public static ConcatManyStage<T, TItemCache1, TCache1, TItemCache2, TCache2> Concat<T, TItemCache1, TCache1, TItemCache2, TCache2>(this MultiStageBase<T, TItemCache1, TCache1> input1, MultiStageBase<T, TItemCache2, TCache2> input2, string? name = null)
            where TItemCache1 : class
        where TCache1 : class
            where TItemCache2 : class
        where TCache2 : class
        {
            if(input1 is null)
                 throw new ArgumentNullException(nameof(input1));
            if(input2 is null)
                 throw new ArgumentNullException(nameof(input2));
            return new ConcatManyStage<T, TItemCache1, TCache1, TItemCache2, TCache2>(input1.DoIt, input2.DoIt, input1.Context, name);
        }
        public static ConcatManyStage<T, TItemCache1, TCache1, TItemCache2, TCache2, TItemCache3, TCache3> Concat<T, TItemCache1, TCache1, TItemCache2, TCache2, TItemCache3, TCache3>(this MultiStageBase<T, TItemCache1, TCache1> input1, MultiStageBase<T, TItemCache2, TCache2> input2, MultiStageBase<T, TItemCache3, TCache3> input3, string? name = null)
            where TItemCache1 : class
        where TCache1 : class
            where TItemCache2 : class
        where TCache2 : class
            where TItemCache3 : class
        where TCache3 : class
        {
            if(input1 is null)
                 throw new ArgumentNullException(nameof(input1));
            if(input2 is null)
                 throw new ArgumentNullException(nameof(input2));
            if(input3 is null)
                 throw new ArgumentNullException(nameof(input3));
            return new ConcatManyStage<T, TItemCache1, TCache1, TItemCache2, TCache2, TItemCache3, TCache3>(input1.DoIt, input2.DoIt, input3.DoIt, input1.Context, name);
        }
        public static ConcatManyStage<T, TItemCache1, TCache1, TItemCache2, TCache2, TItemCache3, TCache3, TItemCache4, TCache4> Concat<T, TItemCache1, TCache1, TItemCache2, TCache2, TItemCache3, TCache3, TItemCache4, TCache4>(this MultiStageBase<T, TItemCache1, TCache1> input1, MultiStageBase<T, TItemCache2, TCache2> input2, MultiStageBase<T, TItemCache3, TCache3> input3, MultiStageBase<T, TItemCache4, TCache4> input4, string? name = null)
            where TItemCache1 : class
        where TCache1 : class
            where TItemCache2 : class
        where TCache2 : class
            where TItemCache3 : class
        where TCache3 : class
            where TItemCache4 : class
        where TCache4 : class
        {
            if(input1 is null)
                 throw new ArgumentNullException(nameof(input1));
            if(input2 is null)
                 throw new ArgumentNullException(nameof(input2));
            if(input3 is null)
                 throw new ArgumentNullException(nameof(input3));
            if(input4 is null)
                 throw new ArgumentNullException(nameof(input4));
            return new ConcatManyStage<T, TItemCache1, TCache1, TItemCache2, TCache2, TItemCache3, TCache3, TItemCache4, TCache4>(input1.DoIt, input2.DoIt, input3.DoIt, input4.DoIt, input1.Context, name);
        }
    }
}
