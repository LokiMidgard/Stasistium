using StaticSite.Documents;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace StaticSite.Stages
{
    public delegate Task<StageResult<TResult, TCache>> StagePerformHandler<TResult, TCache>([AllowNull] BaseCache cache, OptionToken options);
    public delegate Task<StageResultList<TResult, TResultCache, TCache>> StagePerformHandler<TResult, TResultCache, TCache>([AllowNull] BaseCache cache, OptionToken options);


    public static class Stage
    {

        public static PersistStage<TItemCache, TCache> Persist<TItemCache, TCache>(this MultiStageBase<System.IO.Stream, TItemCache, TCache> stage, System.IO.DirectoryInfo output, GenerationOptions generatorOptions)
            where TCache : class
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

        public static MarkdownStreamStage<T> Markdown<T>(this StageBase<System.IO.Stream, T> input)
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
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            return new WhereStage<TCheck, TPreviousItemCache, TPreviousCache>(input.DoIt, predicate, input.Context);
        }
        public static WhereStage<TCheck, TPreviousItemCache, TPreviousCache> Where<TCheck, TPreviousItemCache, TPreviousCache>(this MultiStageBase<TCheck, TPreviousItemCache, TPreviousCache> input, Func<IDocument<TCheck>, bool> predicate)
            where TPreviousCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            return new WhereStage<TCheck, TPreviousItemCache, TPreviousCache>(input.DoIt, x => Task.FromResult(predicate(x)), input.Context);
        }
        public static SingleStage<TCheck, TPreviousItemCache, TPreviousCache> Single<TCheck, TPreviousItemCache, TPreviousCache>(this MultiStageBase<TCheck, TPreviousItemCache, TPreviousCache> input)
            where TPreviousCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new SingleStage<TCheck, TPreviousItemCache, TPreviousCache>(input.DoIt, input.Context);
        }
        public static SelectStage<TIn, TInITemCache, TInCache, TOut> Select<TIn, TInITemCache, TInCache, TOut>(this MultiStageBase<TIn, TInITemCache, TInCache> input, Func<IDocument<TIn>, Task<IDocument<TOut>>> predicate)
            where TInCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            return new SelectStage<TIn, TInITemCache, TInCache, TOut>(input.DoIt, predicate, input.Context);
        }

        public static StaticStage<TResult> FromResult<TResult>(TResult result, Func<TResult, string> hashFunction, GeneratorContext context)
            => new StaticStage<TResult>(result, hashFunction, context);
    }


}
