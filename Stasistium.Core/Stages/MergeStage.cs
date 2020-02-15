using Stasistium.Core;
using Stasistium.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Stasistium.Stages;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class MergeStage<TOut, TIn1, TInputItemCache, TInputCache1, TIn2, TInputCache2> : MultiStageBase<TOut, string, MergeCache<TInputCache1, TInputCache2>>
        where TInputCache1 : class
        where TInputCache2 : class
        where TInputItemCache : class
    {

        private readonly MultiStageBase<TIn1, TInputItemCache, TInputCache1> input1;
        private readonly StageBase<TIn2, TInputCache2> input2;

        private readonly Func<IDocument<TIn1>, IDocument<TIn2>, IDocument<TOut>> mergeFunction;


        public MergeStage(MultiStageBase<TIn1, TInputItemCache, TInputCache1> input1, StageBase<TIn2, TInputCache2> input2, Func<IDocument<TIn1>, IDocument<TIn2>, IDocument<TOut>> mergeFunction, IGeneratorContext context, string? name = null) : base(context, name)
        {
            this.input1 = input1 ?? throw new ArgumentNullException(nameof(input1));
            this.input2 = input2 ?? throw new ArgumentNullException(nameof(input2));
            this.mergeFunction = mergeFunction ?? throw new ArgumentNullException(nameof(mergeFunction));
        }

        protected override async Task<StageResultList<TOut, string, MergeCache<TInputCache1, TInputCache2>>> DoInternal([AllowNull] MergeCache<TInputCache1, TInputCache2>? cache, OptionToken options)
        {
            var inputReadyToPerform = await this.input1.DoIt(cache?.Cache1, options).ConfigureAwait(false);
            var inputSingle = await this.input2.DoIt(cache?.Cache2, options).ConfigureAwait(false);

            var task = LazyTask.Create(async () =>
            {
                var inputList = await inputReadyToPerform.Perform;
                var inputListCache = inputReadyToPerform.Cache;
                var results = await Task.WhenAll(inputList.Select(async currentItem =>
                {
                    var currentTask = LazyTask.Create(async () =>
                    {
                        var currentItemPerformed = await currentItem.Perform;
                        var currentCache = currentItem.Cache;
                        var currentSinglePerformed = await inputSingle.Perform;
                        var result = this.mergeFunction(currentItemPerformed, currentSinglePerformed);

                        return (result: result, hash: result.Hash);
                    });
                    bool currentItemHashChanges;

                    if (cache == null || cache.InputIdToOutputId.TryGetValue(currentItem.Id, out string? currentId))
                        currentId = null;
                    if (currentItem.HasChanges || inputSingle.HasChanges || currentId is null || cache is null)
                    {
                        var (performing, newItemCache) = await currentTask;
                        currentId = performing.Id;

                        if (cache == null || cache.OutputIdToHash.TryGetValue(currentId, out string? oldHash))
                            oldHash = null;
                        currentItemHashChanges = oldHash != newItemCache;
                        return (result: StageResult.Create(performing, currentItemHashChanges, currentId, newItemCache), inputId: currentItem.Id, hash: newItemCache);
                    }
                    else
                    {
                        currentItemHashChanges = false;
                        var itemHash = cache.OutputIdToHash[currentId];
                        var actualCurrentTask = LazyTask.Create(async () =>
                        {
                            var temp = await currentTask;
                            return temp.result;
                        });
                        return (result: StageResult.Create(actualCurrentTask, currentItemHashChanges, currentId, itemHash), inputId: currentItem.Id, hash: itemHash);
                    }


                })).ConfigureAwait(false);

                TInputCache2 singleCache;
                if (inputSingle.HasChanges || cache is null)
                {
                    var newSingleCache = inputSingle.Cache;
                    singleCache = newSingleCache;
                }
                else
                {
                    singleCache = cache.Cache2;
                }

                var newCache = new MergeCache<TInputCache1, TInputCache2>()
                {
                    Cache1 = inputListCache,
                    Cache2 = singleCache,
                    InputIdToOutputId = results.ToDictionary(x => x.inputId, x => x.result.Id),
                    OutputIdToHash = results.ToDictionary(x => x.result.Id, x => x.hash),
                    DocumentIds = results.Select(x => x.result.Id).ToArray()
                };

                return (results.Select(x => x.result).ToImmutableList(), newCache);
            });

            bool hasChanges = inputReadyToPerform.HasChanges || inputSingle.HasChanges || cache is null;
            System.Collections.Immutable.ImmutableList<string> documentIds;

            if (hasChanges || cache is null)
            {
                var perform = await task;
                documentIds = perform.newCache.DocumentIds.ToImmutableList();
                if (!inputSingle.HasChanges && cache != null)
                {
                    hasChanges = perform.Item1.Any(x => x.HasChanges)
                        || cache.DocumentIds.SequenceEqual(documentIds);
                }
                return StageResultList.Create(perform.Item1, hasChanges, documentIds, perform.newCache);
            }
            else
            {
                documentIds = cache.DocumentIds.ToImmutableList();
                var actualTask = LazyTask.Create(async () =>
                {
                    var temp = await task;
                    return temp.Item1;
                });

                return StageResultList.Create(actualTask, hasChanges, documentIds, cache);
            }

        }
    }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning disable CA1819 // Properties should not return arrays
#pragma warning disable CA2227 // Collection properties should be read only
    public class MergeCache<TInputCache1, TInputCache2>
    {
        public TInputCache1 Cache1 { get; set; }
        public TInputCache2 Cache2 { get; set; }

        public Dictionary<string, string> InputIdToOutputId { get; set; }
        public Dictionary<string, string> OutputIdToHash { get; set; }

        // we need the order so we cant use the dictionarys above
        public string[] DocumentIds { get; set; }
    }
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CA1819 // Properties should not return arrays
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
}

namespace Stasistium
{
    public static partial class StageExtensions
    {
        public static MergeStage<TOut, TIn1, TInputItemCache, TInputCache1, TIn2, TInputCache2> Merge<TOut, TIn1, TInputItemCache, TInputCache1, TIn2, TInputCache2>(this MultiStageBase<TIn1, TInputItemCache, TInputCache1> input, StageBase<TIn2, TInputCache2> combine, Func<IDocument<TIn1>, IDocument<TIn2>, IDocument<TOut>> mergeFunction, string? name = null)
         where TInputCache1 : class
        where TInputCache2 : class
        where TInputItemCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (combine is null)
                throw new ArgumentNullException(nameof(combine));
            if (mergeFunction is null)
                throw new ArgumentNullException(nameof(mergeFunction));
            return new MergeStage<TOut, TIn1, TInputItemCache, TInputCache1, TIn2, TInputCache2>(input, combine, mergeFunction, combine.Context, name);
        }
    }
}