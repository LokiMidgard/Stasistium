using StaticSite.Documents;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace StaticSite.Stages
{
    public class SelectStage<TIn, TInItemCache, TInCache, TOut> : OutputMultiInputSingle0List1StageBase<TIn, TInItemCache, TInCache, TOut, string, ImmutableList<string>>
    {

        private readonly Func<IDocument<TIn>, Task<IDocument<TOut>>> predicate;

        public SelectStage(StagePerformHandler<TIn, TInItemCache, TInCache> inputList0, Func<IDocument<TIn>, Task<IDocument<TOut>>> selector, GeneratorContext context, bool updateOnRefresh = false) : base(inputList0, context, updateOnRefresh)
        {
            this.predicate = selector;
        }


        protected override async Task<(ImmutableList<StageResult<TOut, string>> result, BaseCache<ImmutableList<string>> cache)> Work(StageResultList<TIn, TInItemCache, TInCache> inputList0, [AllowNull] ImmutableList<string> cache, [AllowNull] ImmutableDictionary<string, BaseCache<string>> childCaches, OptionToken options)
        {
            if (inputList0 is null)
                throw new ArgumentNullException(nameof(inputList0));
            var input = await inputList0.Perform;

            var list = await Task.WhenAll(input.result.Select(async item =>
            {
                if (item.HasChanges)
                {
                    var newSource = await item.Perform;
                    var transformed = await this.predicate(newSource.result).ConfigureAwait(false);

                    var hasChanges = true;
                    if (childCaches != null && childCaches.TryGetValue(transformed.Id, out var oldHash))
                        hasChanges = oldHash.Item != transformed.Hash;
                    return StageResult.Create(transformed, BaseCache.Create(transformed.Hash, newSource.cache), hasChanges, transformed.Id);
                }
                else
                {
                    return StageResult.Create(LazyTask.Create(async () =>
                    {

                        var newSource = await item.Perform;
                        var transformed = await this.predicate(newSource.result).ConfigureAwait(false);

                        return (transformed, BaseCache.Create(transformed.Hash, newSource.cache));
                    }), false, item.Id);
                }
            })).ConfigureAwait(false);
            return (list.ToImmutableList(), BaseCache.Create(list.Select(x => x.Id).ToImmutableList(), input.cache));
        }

    }


}
