using Stasistium.Core;
using Stasistium.Documents;
using System;
using System.Collections.Immutable;

namespace Stasistium.Stages
{
    public static class StageResult
    {
        public static StageResult<TResult, TCache> CreateStageResult<TResult, TCache>(IGeneratorContext context, LazyTask<IDocument<TResult>> perform, bool hasChanges, string documentId, TCache cache, string hash)
        where TCache : class
            => new StageResult<TResult, TCache>(perform, hasChanges, documentId, cache, hash, context);
        public static StageResult<TResult, TCache> CreateStageResult<TResult, TCache>(IGeneratorContext context, IDocument<TResult> result, bool hasChanges, string documentId, TCache cache, string hash)
        where TCache : class
            => new StageResult<TResult, TCache>(LazyTask.Create(() => result), hasChanges, documentId, cache, hash, context);
    }
    public static class StageResultList
    {
        public static StageResultList<TResult, TResultCache, TCache> CreateStageResultList<TResult, TResultCache, TCache>(IGeneratorContext context, LazyTask<ImmutableList<StageResult<TResult, TResultCache>>> perform, bool hasChanges, ImmutableList<string> documentId, TCache cache, string hash)
            where TResultCache : class
            where TCache : class
        => new StageResultList<TResult, TResultCache, TCache>(perform, hasChanges, documentId, cache, hash, context);
        public static StageResultList<TResult, TResultCache, TCache> CreateStageResultList<TResult, TResultCache, TCache>(IGeneratorContext context, ImmutableList<StageResult<TResult, TResultCache>> result, bool hasChanges, ImmutableList<string> documentId, TCache cache, string hash)
            where TCache : class
            where TResultCache : class
            => new StageResultList<TResult, TResultCache, TCache>(LazyTask.Create(() => result), hasChanges, documentId, cache, hash, context);
    }

    [System.Diagnostics.DebuggerDisplay("StageResult Id: {Id} has changes: {HasChanges}")]
    public class StageResult<TResult, TCache>
        where TCache : class
    {
        public StageResult(LazyTask<IDocument<TResult>> perform, bool hasChanges, string documentId, TCache cache, string hash, IGeneratorContext context)
        {
            this.Perform = perform ?? throw new ArgumentNullException(nameof(perform));
            this.HasChanges = hasChanges;
            this.Id = documentId;
            this.Cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.Hash = hash;
            this.Context = context ?? throw new ArgumentNullException(nameof(context));
            this.Context.Logger.Info($"{documentId} {(hasChanges ? "Changed" : "remained unchanged")}");
        }

        public LazyTask<IDocument<TResult>> Perform { get; }
        public bool HasChanges { get; }

        public TCache Cache { get; }
        public string Hash { get; }
        public IGeneratorContext Context { get; }

        /// <summary>
        /// The Id of the Document, if knwon
        /// </summary>
        public string Id { get; }
    }
    [System.Diagnostics.DebuggerDisplay("StageResultList Ids: {IdList} has changes: {HasChanges}")]
    public class StageResultList<TResult, TCacheResult, TCache>
        where TCache : class
        where TCacheResult : class
    {
        public StageResultList(LazyTask<ImmutableList<StageResult<TResult, TCacheResult>>> perform, bool hasChanges, ImmutableList<string> ids, TCache cache, string hash, IGeneratorContext context)
        {
            this.Perform = perform ?? throw new ArgumentNullException(nameof(perform));
            this.HasChanges = hasChanges;
            this.Ids = ids;
            this.Cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.Hash = hash;
            this.Context = context ?? throw new ArgumentNullException(nameof(context));
            this.Context.Logger.Info($"{string.Join(", ", ids)} {(hasChanges ? "Changed" : "remained unchanged")}");
        }

        public LazyTask<ImmutableList<StageResult<TResult, TCacheResult>>> Perform { get; }
        public bool HasChanges { get; }
        public TCache Cache { get; }
        public string Hash { get; }
        public IGeneratorContext Context { get; }

        /// <summary>
        /// The Id's of the entrys, if known.
        /// </summary>
        public ImmutableList<string> Ids { get; }

        private string IdList => string.Join(", ", this.Ids);

    }

}
