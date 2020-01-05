using Stasistium.Core;
using Stasistium.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class StaticStage<TResult> : StageBase<TResult, string>
    {
        private readonly string id = Guid.NewGuid().ToString();
        private readonly Func<TResult, string> hashFunction;
        public TResult Value { get; set; }

        public StaticStage(TResult result, Func<TResult, string> hashFunction, GeneratorContext context) : base(context)
        {
            this.Value = result;
            this.hashFunction = hashFunction ?? throw new ArgumentNullException(nameof(hashFunction));
        }

        protected override Task<StageResult<TResult, string>> DoInternal([AllowNull] string? cache, OptionToken options)
        {
            var contentHash = this.hashFunction(this.Value);
            return Task.FromResult(StageResult.Create(
                result: this.Context.Create(this.Value, contentHash, this.id),
                cache: contentHash,
                hasChanges: cache != contentHash,
                documentId: this.id));
        }
    }


    public class ConcatStageMany2<T, TItemCache1, TCache1> : MultiStageBase<T, string, ConcatStageManyCache<TCache1>>
        where TItemCache1 : class
        where TCache1 : class
    {
        StagePerformHandler<T, TCache1, TCache1> input;

        public ConcatStageMany2(GeneratorContext context) : base(context)
        {
        }

        protected override async Task<StageResultList<T, string, ConcatStageManyCache<TCache1>>> DoInternal([AllowNull] ConcatStageManyCache<TCache1>? cache, OptionToken options)
        {
            var result = await this.input(cache?.PreviouseCache1, options).ConfigureAwait(false);

            var task = LazyTask.Create(async () =>
            {
                var list = ImmutableList<StageResult<T, string>>.Empty.ToBuilder();
                var newCache = new ConcatStageManyCache<TCache1>();


                if (result.HasChanges)
                {
                    var performed = await result.Perform;
                    newCache.Ids1 = new string[performed.result.Count];
                    newCache.PreviouseCache1 = performed.cache;

                    for (int i = 0; i < performed.result.Count; i++)
                    {
                        var child = performed.result[i];

                        if (child.HasChanges)
                        {
                            var childPerformed = await child.Perform;

                            string? oldHash = null;
                            if (cache != null && !cache.IdToHash.TryGetValue(childPerformed.result.Id, out oldHash))
                                throw this.Context.Exception("Should Not Happen");
                            var childHashChanges = oldHash != childPerformed.result.Hash;

                            list.Add(StageResult.Create(childPerformed.result, childPerformed.result.Hash, childHashChanges, childPerformed.result.Id));
                            newCache.IdToHash.Add(child.Id, childPerformed.result.Id);

                        }
                        else
                        {

                            var childTask = LazyTask.Create(async () =>
                            {
                                var childPerform = await child.Perform;
                                return (childPerform.result, childPerform.result.Hash);
                            });
                            list.Add(StageResult.Create(childTask, false, child.Id));
                            if (cache is null || !cache.IdToHash.TryGetValue(child.Id, out var oldHash))
                                throw this.Context.Exception("Should Not Happen");
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

                        var childTask = LazyTask.Create(async () =>
                        {
                            var performed = await result.Perform;
                            var chiledIndex = performed.result[i];
                            var childPerform = await chiledIndex.Perform;
                            // We are in the no changes part. So ther must be no changes.
                            System.Diagnostics.Debug.Assert(!chiledIndex.HasChanges);

                            return (childPerform.result, childPerform.result.Hash);
                        });
                        list.Add(StageResult.Create(childTask, false, cache.Ids1[i]));
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

            var hasChanges = result.HasChanges;
            var ids = ImmutableList<string>.Empty.ToBuilder();

            if (hasChanges || cache is null)
            {
                var performed = await task;
                hasChanges = performed.result.Any(x => x.HasChanges);
                if (!hasChanges && cache != null)
                {
                    hasChanges = !performed.cache.Ids1.SequenceEqual(cache.Ids1);
                }
                ids.AddRange(performed.cache.Ids1);

            }
            else
            {
                ids.AddRange(cache.Ids1);
            }

            return StageResultList.Create(task, hasChanges, ids.ToImmutable());
        }
    }

    //public class ConcatStageManyCache<TPrevious>
    //{
    //    public TPrevious PreviouseCache1 { get; set; }
    //    public string[] Ids1 { get; set; }
    //    public Dictionary<string, string> IdToHash { get; set; }
    //}


}
