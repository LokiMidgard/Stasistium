using Stasistium.Core;
using Stasistium.Documents;
using System;
using System.Collections.Immutable;

namespace Stasistium.Stages
{
    public static class StageResult
    {
        public static StageResult<TResult, TCache> Create<TResult, TCache>(LazyTask<IDocument<TResult>> perform, bool hasChanges, string documentId, TCache cache)
        where TCache : class
            => new StageResult<TResult, TCache>(perform, hasChanges, documentId, cache);
        public static StageResult<TResult, TCache> Create<TResult, TCache>(IDocument<TResult> result, bool hasChanges, string documentId, TCache cache)
        where TCache : class
            => new StageResult<TResult, TCache>(LazyTask.Create(() => result), hasChanges, documentId, cache);
    }
    public static class StageResultList
    {
        public static StageResultList<TResult, TResultCache, TCache> Create<TResult, TResultCache, TCache>(LazyTask<ImmutableList<StageResult<TResult, TResultCache>>> perform, bool hasChanges, ImmutableList<string> documentId, TCache cache)
            where TResultCache : class
            where TCache : class
        => new StageResultList<TResult, TResultCache, TCache>(perform, hasChanges, documentId, cache);
        public static StageResultList<TResult, TResultCache, TCache> Create<TResult, TResultCache, TCache>(ImmutableList<StageResult<TResult, TResultCache>> result, bool hasChanges, ImmutableList<string> documentId, TCache cache)
            where TCache : class
            where TResultCache : class
            => new StageResultList<TResult, TResultCache, TCache>(LazyTask.Create(() => result), hasChanges, documentId, cache);
    }

    public class StageResult<TResult, TCache>
        where TCache : class
    {
        public StageResult(LazyTask<IDocument<TResult>> perform, bool hasChanges, string documentId, TCache cache)
        {
            this.Perform = perform ?? throw new ArgumentNullException(nameof(perform));
            this.HasChanges = hasChanges;
            this.Id = documentId;
            this.Cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public LazyTask<IDocument<TResult>> Perform { get; }
        public bool HasChanges { get; }

        public TCache Cache { get; }

        /// <summary>
        /// The Id of the Document, if knwon
        /// </summary>
        public string Id { get; }
    }
    public class StageResultList<TResult, TCacheResult, TCache>
        where TCache : class
        where TCacheResult : class
    {
        public StageResultList(LazyTask<ImmutableList<StageResult<TResult, TCacheResult>>> perform, bool hasChanges, ImmutableList<string> ids, TCache cache)
        {
            this.Perform = perform ?? throw new ArgumentNullException(nameof(perform));
            this.HasChanges = hasChanges;
            this.Ids = ids;
            this.Cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public LazyTask<ImmutableList<StageResult<TResult, TCacheResult>>> Perform { get; }
        public bool HasChanges { get; }
        public TCache Cache { get; }
        /// <summary>
        /// The Id's of the entrys, if known.
        /// </summary>
        public ImmutableList<string> Ids { get; }

    }

}
