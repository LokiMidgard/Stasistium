using Stasistium.Core;
using Stasistium.Documents;
using System;
using System.Collections.Immutable;

namespace Stasistium.Stages
{
    public static class StageResult
    {
        public static StageResult<TResult, TCache> Create<TResult, TCache>(LazyTask<(IDocument<TResult> result, TCache cache)> perform, bool hasChanges, string documentId)
            => new StageResult<TResult, TCache>(perform, hasChanges, documentId);
        public static StageResult<TResult, TCache> Create<TResult, TCache>(IDocument<TResult> result, TCache cache, bool hasChanges, string documentId)
            => new StageResult<TResult, TCache>(LazyTask.Create(() => (result, cache)), hasChanges, documentId);
    }
    public static class StageResultList
    {
        public static StageResultList<TResult, TResultCache, TCache> Create<TResult, TResultCache, TCache>(LazyTask<(ImmutableList<StageResult<TResult, TResultCache>> result, TCache cache)> perform, bool hasChanges, ImmutableList<string> documentId)
        => new StageResultList<TResult, TResultCache, TCache>(perform, hasChanges, documentId);
        public static StageResultList<TResult, TResultCache, TCache> Create<TResult, TResultCache, TCache>(ImmutableList<StageResult<TResult, TResultCache>> result, TCache cache, bool hasChanges, ImmutableList<string> documentId)
            => new StageResultList<TResult, TResultCache, TCache>(LazyTask.Create(() => (result, cache)), hasChanges, documentId);
    }

    public class StageResult<TResult, TCache>
    {
        public StageResult(LazyTask<(IDocument<TResult> result, TCache cache)> perform, bool hasChanges, string documentId)
        {
            this.Perform = perform ?? throw new ArgumentNullException(nameof(perform));
            this.HasChanges = hasChanges;
            this.Id = documentId;
        }

        public LazyTask<(IDocument<TResult> result, TCache cache)> Perform { get; }
        public bool HasChanges { get; }

        /// <summary>
        /// The Id of the Document, if knwon
        /// </summary>
        public string Id { get; }
    }
    public class StageResultList<TResult, TCacheResult, TCache>
    {
        public StageResultList(LazyTask<(ImmutableList<StageResult<TResult, TCacheResult>> result, TCache cache)> perform, bool hasChanges, ImmutableList<string> ids)
        {
            this.Perform = perform ?? throw new ArgumentNullException(nameof(perform));
            this.HasChanges = hasChanges;
            this.Ids = ids;
        }

        public LazyTask<(ImmutableList<StageResult<TResult, TCacheResult>> result, TCache cache)> Perform { get; }
        public bool HasChanges { get; }

        /// <summary>
        /// The Id's of the entrys, if known.
        /// </summary>
        public ImmutableList<string> Ids { get; }

    }

}
