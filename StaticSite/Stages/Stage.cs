using StaticSite.Documents;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace StaticSite.Stages
{
    public delegate Task<StageResult<TResult, TCache>> StagePerformHandler<TResult, TCache>([AllowNull] TCache cache, OptionToken options);
    public delegate Task<StageResultList<TResult, TResultCache, TCache>> StagePerformHandler<TResult, TResultCache, TCache>([AllowNull] TCache cache, OptionToken options);


    public static class Stage
    {

        public static PersistStage<TItemCache, TCache> Persist<TItemCache, TCache>(this MultiStageBase<Stream, TItemCache, TCache> stage, DirectoryInfo output, GenerationOptions generatorOptions)
            where TCache : class
            where TItemCache : class
        {
            if (stage is null)
                throw new ArgumentNullException(nameof(stage));
            if (output is null)
                throw new ArgumentNullException(nameof(output));
            if (generatorOptions is null)
                throw new ArgumentNullException(nameof(generatorOptions));
            return new PersistStage<TItemCache, TCache>(stage.DoIt, output, generatorOptions, stage.Context);
        }

        public static GitStage<T> GitModul<T>(this StageBase<string, T> input)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new GitStage<T>(input.DoIt, input.Context);
        }

        public static GitRefToFilesStage<T> GitRefToFiles<T>(this StageBase<GitRef, T> input)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new GitRefToFilesStage<T>(input.DoIt, input.Context);
        }

        public static MarkdownStreamStage<T> Markdown<T>(this StageBase<Stream, T> input)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new MarkdownStreamStage<T>(input.DoIt, input.Context);
        }
        public static MarkdownStringStage<T> Markdown<T>(this StageBase<string, T> input)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new MarkdownStringStage<T>(input.DoIt, input.Context);
        }

        public static WhereStage<TCheck, TPreviousItemCache, TPreviousCache> Where<TCheck, TPreviousItemCache, TPreviousCache>(this MultiStageBase<TCheck, TPreviousItemCache, TPreviousCache> input, Func<IDocument<TCheck>, Task<bool>> predicate)
            where TPreviousCache : class
            where TPreviousItemCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            return new WhereStage<TCheck, TPreviousItemCache, TPreviousCache>(input.DoIt, predicate, input.Context);
        }
        public static WhereStage<TCheck, TPreviousItemCache, TPreviousCache> Where<TCheck, TPreviousItemCache, TPreviousCache>(this MultiStageBase<TCheck, TPreviousItemCache, TPreviousCache> input, Func<IDocument<TCheck>, bool> predicate)
            where TPreviousCache : class
            where TPreviousItemCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            return new WhereStage<TCheck, TPreviousItemCache, TPreviousCache>(input.DoIt, x => Task.FromResult(predicate(x)), input.Context);
        }
        public static SingleStage<TCheck, TPreviousItemCache, TPreviousCache> SingleEntry<TCheck, TPreviousItemCache, TPreviousCache>(this MultiStageBase<TCheck, TPreviousItemCache, TPreviousCache> input)
            where TPreviousCache : class
            where TPreviousItemCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new SingleStage<TCheck, TPreviousItemCache, TPreviousCache>(input.DoIt, input.Context);
        }
        public static SelectStage<TIn, TInITemCache, TInCache, TOut> Select<TIn, TInITemCache, TInCache, TOut>(this MultiStageBase<TIn, TInITemCache, TInCache> input, Func<IDocument<TIn>, Task<IDocument<TOut>>> predicate)
            where TInCache : class
            where TInITemCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            return new SelectStage<TIn, TInITemCache, TInCache, TOut>(input.DoIt, predicate, input.Context);
        }

        public static StaticStage<TResult> FromResult<TResult>(TResult result, Func<TResult, string> hashFunction, GeneratorContext context)
            => new StaticStage<TResult>(result, hashFunction, context);

        public static SidecarHelper<TPreviousItemCache, TPreviousListCache> Sidecar<TPreviousItemCache, TPreviousListCache>(this MultiStageBase<Stream, TPreviousItemCache, TPreviousListCache> stage)
            where TPreviousListCache : class
            where TPreviousItemCache : class
        {
            return new SidecarHelper<TPreviousItemCache, TPreviousListCache>(stage);
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public class SidecarHelper<TPreviousItemCache, TPreviousListCache>
                where TPreviousListCache : class
            where TPreviousItemCache : class
        {
            private readonly MultiStageBase<Stream, TPreviousItemCache, TPreviousListCache> stage;

            public SidecarHelper(MultiStageBase<Stream, TPreviousItemCache, TPreviousListCache> stage)
            {
                this.stage = stage;
            }

            public SidecarMetadata<TMetadata, TPreviousItemCache, TPreviousListCache> For<TMetadata>(string extension, MetadataUpdate<TMetadata>? updateCallback = null)
            {
                return new SidecarMetadata<TMetadata, TPreviousItemCache, TPreviousListCache>(this.stage.DoIt, extension, updateCallback, this.stage.Context);
            }
        }
    }

    //public class Split<TInputList0, TPreviousItemCache0, TPreviousListCache0, TResult, TResultCache, TCache> : OutputMultiInputSingle0List1StageBase<TInputList0, TPreviousItemCache0, TPreviousListCache0, TResult, string, ImmutableList<string>>
    //where TResultCache : class
    //where TCache : class
    //where TPreviousItemCache0 : class
    //where TPreviousListCache0 : class
    //{

    //    private readonly Dictionary<string, (Start @in, StageBase<TResult, TResultCache> @out)> startLookup = new Dictionary<string, (Start @in, StageBase<TResult, TResultCache> @out)>();

    //    private readonly Func<StageBase<TInputList0, CacheId<string>>, StageBase<TResult, TResultCache>> createPipline;

    //    protected override async Task<(ImmutableList<StageResult<TResult, string>> result, BaseCache<ImmutableList<string>> cache)> Work(StageResultList<TInputList0, TPreviousItemCache0, TPreviousListCache0> inputList0, [AllowNull] ImmutableList<string> cache, [AllowNull] ImmutableDictionary<string, BaseCache<string>>? childCaches, OptionToken options)
    //    {

    //        var (input, _) = await inputList0.Perform;

    //        foreach (var i in input)
    //        {
    //            if (this.startLookup.TryGetValue(i.Id, out var pipe))
    //            {
    //                pipe.@in.In = i;
    //            }
    //            else
    //            {
    //                var start = new Start(i, this.Context);
    //                var end = this.createPipline(start);
    //                pipe = (start, end);
    //                this.startLookup.Add(i.Id, pipe);
    //            }

    //            pipe.@out.DoIt()

    //            StageResult.Create()

    //        }

    //        throw new NotImplementedException();
    //    }


    //    private class Start : OutputSingleInputSingle0List0StageBase<TInputList0, string>
    //    {
    //        private bool hasChanges;
    //        private StageResult<TInputList0, TPreviousItemCache0> @in;

    //        public Start(StageResult<TInputList0, TPreviousItemCache0> initial, GeneratorContext context) : base(context)
    //        {
    //            this.In = initial;
    //        }

    //        public StageResult<TInputList0, TPreviousItemCache0> In
    //        {
    //            get => this.@in; set
    //            {
    //                this.@in = value;
    //                this.hasChanges = value.HasChanges;
    //            }
    //        }

    //        protected override async Task<(IDocument<TInputList0> result, BaseCache<string> cache)> Work(OptionToken options)
    //        {
    //            // reset changes when calculated;
    //            this.hasChanges = false;

    //            var result = await this.In.Perform;

    //            return (result.result, BaseCache.Create(result.result.Id, result.cache));
    //        }

    //        protected override bool ForceUpdate(string? cache, OptionToken options) => this.hasChanges;
    //    }
    //}

}
