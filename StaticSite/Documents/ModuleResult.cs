using System;

namespace StaticSite.Documents
{
    public static class ModuleResult
    {
        public static ModuleResult<TResult, TCache> Create<TResult, TCache>(LazyTask<(TResult result, BaseCache<TCache> cache)> perform, bool hasChanges) => new ModuleResult<TResult, TCache>(perform, hasChanges);
    }

    public class ModuleResult<TResult, TCache>
    {
        public ModuleResult(LazyTask<(TResult result, BaseCache<TCache> cache)> perform, bool hasChanges)
        {
            this.Perform = perform ?? throw new ArgumentNullException(nameof(perform));
            this.HasChanges = hasChanges;
        }

        public LazyTask<(TResult result, BaseCache<TCache> cache)> Perform { get; }
        public bool HasChanges { get; }


    }

}
