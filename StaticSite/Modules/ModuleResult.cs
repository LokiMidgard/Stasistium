using StaticSite.Documents;
using System;
using System.Collections.Immutable;

namespace StaticSite.Modules
{
    public static class ModuleResult
    {
        public static ModuleResult<TResult, TCache> Create<TResult, TCache>(LazyTask<(IDocument<TResult> result, BaseCache<TCache> cache)> perform, bool hasChanges, string documentId)
            => new ModuleResult<TResult, TCache>(perform, hasChanges, documentId);
        public static ModuleResult<TResult, TCache> Create<TResult, TCache>(IDocument<TResult> result, BaseCache<TCache> cache, bool hasChanges, string documentId)
            => new ModuleResult<TResult, TCache>(LazyTask.Create(() => (result, cache)), hasChanges, documentId);
        public static ModuleResultList<TResult, TResultCache, TCache> Create<TResult, TResultCache, TCache>(LazyTask<(ImmutableList<ModuleResult<TResult, TResultCache>> result, BaseCache<TCache> cache)> perform, bool hasChanges, ImmutableList<string> documentId)
            => new ModuleResultList<TResult, TResultCache, TCache>(perform, hasChanges, documentId);
        public static ModuleResultList<TResult, TResultCache, TCache> Create<TResult, TResultCache, TCache>(ImmutableList<ModuleResult<TResult, TResultCache>> result, BaseCache<TCache> cache, bool hasChanges, ImmutableList<string> documentId)
            => new ModuleResultList<TResult, TResultCache, TCache>(LazyTask.Create(() => (result, cache)), hasChanges, documentId);
    }

    public class ModuleResult<TResult, TCache>
    {
        public ModuleResult(LazyTask<(IDocument<TResult> result, BaseCache<TCache> cache)> perform, bool hasChanges, string documentId)
        {
            this.Perform = perform ?? throw new ArgumentNullException(nameof(perform));
            this.HasChanges = hasChanges;
            this.Id = documentId;
        }

        public LazyTask<(IDocument<TResult> result, BaseCache<TCache> cache)> Perform { get; }
        public bool HasChanges { get; }

        /// <summary>
        /// The Id of the Document, if knwon
        /// </summary>
        public string Id { get; }
    }
    public class ModuleResultList<TResult, TCacheResult, TCache>
    {
        public ModuleResultList(LazyTask<(ImmutableList<ModuleResult<TResult, TCacheResult>> result, BaseCache<TCache> cache)> perform, bool hasChanges, ImmutableList<string> ids)
        {
            this.Perform = perform ?? throw new ArgumentNullException(nameof(perform));
            this.HasChanges = hasChanges;
            this.Ids = ids;
        }

        public LazyTask<(ImmutableList<ModuleResult<TResult, TCacheResult>> result, BaseCache<TCache> cache)> Perform { get; }
        public bool HasChanges { get; }

        /// <summary>
        /// The Id's of the entrys, if known.
        /// </summary>
        public ImmutableList<string> Ids { get; }

    }

}
