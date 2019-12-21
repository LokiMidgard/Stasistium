using Stasistium.Core;
using Stasistium.Documents;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class ToListStage<T, TCache> : MultiStageBase<T, TCache, (TCache subCache, string id)[]>
        where TCache : class
    {
        private readonly StagePerformHandler<T, TCache>[] source;


        public ToListStage(GeneratorContext context, params StagePerformHandler<T, TCache>[] source) : base(context)
        {
            this.source = source;
        }

        protected override async Task<StageResultList<T, TCache, (TCache subCache, string id)[]>> DoInternal([AllowNull] (TCache subCache, string id)[]? cache, OptionToken options)
        {

            var result = await Task.WhenAll(this.source.Zip(cache ?? Enumerable.Repeat<(TCache subCache, string id)>(default, this.source.Length), async (stage, cache) => (result: await stage(cache.subCache, options).ConfigureAwait(false), cache: cache))).ConfigureAwait(false);

            var task = LazyTask.Create(async () =>
            {

                var list = await Task.WhenAll(result.Select(async item =>
                {
                    var r = item.result;
                    var oldCache = item.cache;
                    if (r.HasChanges)
                    {
                        var (entryResult, entryCache) = await r.Perform;
                        return (result: r, cache: entryCache, id: r.Id);
                    }
                    else
                    {
                        return (result: r, cache: oldCache.subCache, id: r.Id);
                    }
                })).ConfigureAwait(false);


                return (list.Select(x => x.result).ToImmutableList(), list.Select(x => (x.cache, x.id)).ToArray());

            });

            bool hasChanges = result.Any(x => x.result.HasChanges);
            var ids = cache?.Select(x => x.id);
            if (hasChanges)
            {
                var (newResult, newCache) = await task;
                var newIds = newCache.Select(x => x.id);

                hasChanges = newResult.Any(x => x.HasChanges) || ids is null || !newIds.SequenceEqual(ids);
                ids = newIds;
            }

            return StageResultList.Create(task, hasChanges, ids.ToImmutableList());

        }
    }
#pragma warning restore CA1819 // Properties should not return arrays
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.




}
