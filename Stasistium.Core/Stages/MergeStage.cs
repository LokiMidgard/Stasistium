﻿using Stasistium.Core;
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

        private readonly StagePerformHandler<TIn1, TInputItemCache, TInputCache1> input1;
        private readonly StagePerformHandler<TIn2, TInputCache2> input2;

        private readonly Func<IDocument<TIn1>, IDocument<TIn2>, IDocument<TOut>> mergeFunction;


        public MergeStage(StagePerformHandler<TIn1, TInputItemCache, TInputCache1> input1, StagePerformHandler<TIn2, TInputCache2> input2, Func<IDocument<TIn1>, IDocument<TIn2>, IDocument<TOut>> mergeFunction, GeneratorContext context) : base(context)
        {
            this.input1 = input1 ?? throw new ArgumentNullException(nameof(input1));
            this.input2 = input2 ?? throw new ArgumentNullException(nameof(input2));
            this.mergeFunction = mergeFunction ?? throw new ArgumentNullException(nameof(mergeFunction));
        }

        protected override async Task<StageResultList<TOut, string, MergeCache<TInputCache1, TInputCache2>>> DoInternal([AllowNull] MergeCache<TInputCache1, TInputCache2>? cache, OptionToken options)
        {
            var inputReadyToPerform = await this.input1(cache?.Cache1, options).ConfigureAwait(false);
            var inputSingle = await this.input2(cache?.Cache2, options).ConfigureAwait(false);

            var task = LazyTask.Create(async () =>
            {
                var (inputList, inputListCache) = await inputReadyToPerform.Perform;

                var results = await Task.WhenAll(inputList.Select(async currentItem =>
                {
                    var currentTask = LazyTask.Create(async () =>
                    {
                        var (currentItemPerformed, currentCache) = await currentItem.Perform;
                        var (currentSinglePerformed, _) = await inputSingle.Perform;
                        var result = this.mergeFunction(currentItemPerformed, currentSinglePerformed);

                        return (result: result, hash: result.Hash);
                    });
                    bool currentItemHashChanges;

                    if (cache == null || cache.InputIdToOutputId.TryGetValue(currentItem.Id, out string? currentId))
                        currentId = null;
                    string itemHash;
                    if (currentItem.HasChanges || inputSingle.HasChanges || currentId is null || cache is null)
                    {
                        var (performing, newItemCache) = await currentTask;
                        currentId = performing.Id;

                        if (cache == null || cache.OutputIdToHash.TryGetValue(currentId, out string? oldHash))
                            oldHash = null;
                        currentItemHashChanges = oldHash != newItemCache;
                        itemHash = newItemCache;
                    }
                    else
                    {
                        currentItemHashChanges = false;
                        itemHash = cache.OutputIdToHash[currentId];
                    }

                    return (result: StageResult.Create(currentTask, currentItemHashChanges, currentId), inputId: currentItem.Id, hash: itemHash);

                })).ConfigureAwait(false);

                TInputCache2 singleCache;
                if (inputSingle.HasChanges || cache is null)
                {
                    var (_, newSingleCache) = await inputSingle.Perform;
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
            }
            else
            {
                documentIds = cache.DocumentIds.ToImmutableList();
            }

            return StageResultList.Create(task, hasChanges, documentIds);
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
        public static MergeStage<TOut, TIn1, TInputItemCache, TInputCache1, TIn2, TInputCache2> Merge<TOut, TIn1, TInputItemCache, TInputCache1, TIn2, TInputCache2>(this MultiStageBase<TIn1, TInputItemCache, TInputCache1> input, StageBase<TIn2, TInputCache2> combine, Func<IDocument<TIn1>, IDocument<TIn2>, IDocument<TOut>> mergeFunction)
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
            return new MergeStage<TOut, TIn1, TInputItemCache, TInputCache1, TIn2, TInputCache2>(input.DoIt, combine.DoIt, mergeFunction, combine.Context);
        }
    }
}