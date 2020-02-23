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

    public class WhereStage<TOut, TInItemCache, TInCache> : MultiStageBase<TOut, TInItemCache, WhereStageCache<TInCache>>// : OutputMultiInputSingle0List1StageBase<TCheck, TPreviousItemCache, TPreviousCache, TCheck, TPreviousItemCache, ImmutableList<string>>
        where TInCache : class
        where TInItemCache : class
    {
        private readonly MultiStageBase<TOut, TInItemCache, TInCache> input;
        private readonly Func<IDocument<TOut>, Task<bool>> predicate;

        public WhereStage(MultiStageBase<TOut, TInItemCache, TInCache> input, Func<IDocument<TOut>, Task<bool>> predicate, IGeneratorContext context, string? name = null) : base(context, name)
        {
            this.input = input;
            this.predicate = predicate;
        }

        protected override async Task<StageResultList<TOut, TInItemCache, WhereStageCache<TInCache>>> DoInternal([AllowNull] WhereStageCache<TInCache>? cache, OptionToken options)
        {


            var input = await this.input.DoIt(cache?.PreviousCache , options).ConfigureAwait(false);

            var task = LazyTask.Create(async () =>
            {

                var inputList = await input.Perform;


                var list = (await Task.WhenAll(inputList.Select(async subInput =>
                {
                    bool pass;

                    if (subInput.HasChanges)
                    {
                        var result = await subInput.Perform;
                        var itemCache = subInput.Cache;
                        pass = await this.predicate(result).ConfigureAwait(false);
                    }
                    else
                    {
                        if (cache is null)
                            throw new InvalidOperationException("This shoudl not happen. if item has no changes, ther must be a child cache.");
                        // since HasChanges if false, it was present in the last invocation
                        // if it is passed this it was added to thec cache otherwise not.
                        pass = cache.OutputIdOrder.Contains(subInput.Id);
                    }

                    if (pass)
                        return subInput;
                    else
                        return null;


                })).ConfigureAwait(false)).Where(x => x != null);

                var newCache = new WhereStageCache<TInCache>()
                {
                    OutputIdOrder = list.Select(x => x.Id).ToArray(),
                    PreviousCache  = input.Cache,
                    Hash = this.Context.GetHashForObject(list.Select(x => x.Hash))
                };
                return (result: list.ToImmutableList(), cache: newCache);
            });

            bool hasChanges = input.HasChanges;
            if (input.HasChanges || cache == null)
            {

                var (list, c) = await task;

                hasChanges = false;
                if (!hasChanges && list.Count != cache?.OutputIdOrder.Length)
                    hasChanges = true;

                if (!hasChanges && cache != null)
                {
                    for (int i = 0; i < cache.OutputIdOrder.Length && !hasChanges; i++)
                    {
                        if (list[i].Id != cache.OutputIdOrder[i])
                            hasChanges = true;
                        if (list[i].HasChanges)
                            hasChanges = true;
                    }
                }
                return this.Context.CreateStageResultList(list, hasChanges, c.OutputIdOrder.ToImmutableList(), c, this.Context.GetHashForObject(list.Select(x => x.Hash)), input.Cache);

            }
            var actualTask = LazyTask.Create(async () =>
            {
                var temp = await task;
                return temp.result;
            });
            return this.Context.CreateStageResultList(actualTask, hasChanges, cache.OutputIdOrder.ToImmutableList(), cache, cache.Hash, input.Cache);
        }


    }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning disable CA1819 // Properties should not return arrays
#pragma warning disable CA2227 // Collection properties should be read only
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public class WhereStageCache<TInCache> : IHavePreviousCache<TInCache>
        where TInCache : class
    {
        public TInCache PreviousCache  { get; set; }

        public string[] OutputIdOrder { get; set; }
        public string Hash { get; set; }
    }
#pragma warning restore CA1819 // Properties should not return arrays
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.




}
