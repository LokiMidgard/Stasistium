using Stasistium.Documents;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.IO;
using Stasistium.Stages;

namespace Stasistium
{
    public delegate Task<StageResult<TResult, TCache>> StagePerformHandler<TResult, TCache>([AllowNull] TCache cache, OptionToken options);
    public delegate Task<StageResultList<TResult, TResultCache, TCache>> StagePerformHandler<TResult, TResultCache, TCache>([AllowNull] TCache cache, OptionToken options);


    public static class StageExtensions
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
        public static TransformStage<TIn, TInITemCache, TInCache, TOut> Transform<TIn, TInITemCache, TInCache, TOut>(this MultiStageBase<TIn, TInITemCache, TInCache> input, Func<IDocument<TIn>, Task<IDocument<TOut>>> predicate)
            where TInCache : class
            where TInITemCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            return new TransformStage<TIn, TInITemCache, TInCache, TOut>(input.DoIt, predicate, input.Context);
        }
        public static TransformStage<TIn, TInITemCache, TInCache, TOut> Transform<TIn, TInITemCache, TInCache, TOut>(this MultiStageBase<TIn, TInITemCache, TInCache> input, Func<IDocument<TIn>, IDocument<TOut>> predicate)
            where TInCache : class
            where TInITemCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            return new TransformStage<TIn, TInITemCache, TInCache, TOut>(input.DoIt, x => Task.FromResult(predicate(x)), input.Context);
        }

        public static StaticStage<TResult> FromResult<TResult>(TResult result, Func<TResult, string> hashFunction, GeneratorContext context)
            => new StaticStage<TResult>(result, hashFunction, context);

        public static SelectStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache> Select<TInput, TInputItemCache, TInputCache, TResult, TItemCache>(this MultiStageBase<TInput, TInputItemCache, TInputCache> input, Func<StageBase<TInput, Stages.GeneratedHelper.CacheId<string>>, StageBase<TResult, TItemCache>> createPipline)
            where TInputCache : class
            where TInputItemCache : class
            where TItemCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new SelectStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache>(input.DoIt, createPipline, input.Context);
        }

        public static TextToStreamStage<T> TextToStream<T>(this StageBase<string, T> input)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new TextToStreamStage<T>(input.DoIt, input.Context);
        }

    }

}
