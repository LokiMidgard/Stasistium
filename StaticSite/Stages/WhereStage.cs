using StaticSite.Documents;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace StaticSite.Stages
{
    public class WhereStage<TCheck, TPreviousItemCache, TPreviousCache> : OutputMultiInputSingle0List1StageBase<TCheck, TPreviousItemCache, TPreviousCache, TCheck, TPreviousItemCache, ImmutableList<string>>
    {
        private readonly Func<IDocument<TCheck>, Task<bool>> predicate;

        public WhereStage(StagePerformHandler<TCheck, TPreviousItemCache, TPreviousCache> inputList0, Func<IDocument<TCheck>, Task<bool>> predicate, GeneratorContext context, bool updateOnRefresh = false) : base(inputList0, context, updateOnRefresh)
        {
            this.predicate = predicate;
        }

        protected override async Task<(ImmutableList<StageResult<TCheck, TPreviousItemCache>> result, BaseCache<ImmutableList<string>> cache)> Work(StageResultList<TCheck, TPreviousItemCache, TPreviousCache> inputList0, [AllowNull] ImmutableList<string> cache, [AllowNull] ImmutableDictionary<string, BaseCache<TPreviousItemCache>>? childCaches, OptionToken options)
        {
            if (inputList0 is null)
                throw new ArgumentNullException(nameof(inputList0));
            var input = await inputList0.Perform;

            await Task.WhenAll(input.result.Where(x => x.HasChanges).Select(async x => await x.Perform)).ConfigureAwait(false);


            var list = await Task.WhenAll(input.result.Select(async item =>
            {
                bool pass;

                if (item.HasChanges)
                {
                    var (result, itemCache) = await item.Perform;
                    pass = await this.predicate(result).ConfigureAwait(false);
                }
                else
                {
                    if (childCaches is null)
                        throw new InvalidOperationException("This shoudl not happen. if item has no changes, ther must be a child cache.");
                    // since HasChanges if false, it was present in the last invocation
                    // if it is passed this it was added to thec cache otherwise not.
                    pass = childCaches.ContainsKey(item.Id);
                }
                // where seems to be a little special. HasChanges will not be set if the result of the predicate changed
                // otherwise if a document has changes but gets not filtered and was not filtered last time, it would not have
                // changes set.
                // If filtering chages this should be part of the Cache not ChildCache
                if (pass)
                    return item;
                else
                    return null;
            })).ConfigureAwait(false);



            return (list.Where(x => x != null).ToImmutableList(), BaseCache.Create(list.Where(x => x != null).Select(x => x.Id).ToImmutableList(), input.cache));
        }

    }


}
