using Stasistium.Documents;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.IO;
using Stasistium.Stages;
using System.Linq;

namespace Stasistium
{
    public delegate Task<StageResult<TResult, TCache>> StagePerformHandler<TResult, TCache>([AllowNull] TCache cache, OptionToken options);
    public delegate Task<StageResultList<TResult, TResultCache, TCache>> StagePerformHandler<TResult, TResultCache, TCache>([AllowNull] TCache cache, OptionToken options);


    public static partial class StageExtensions
    {


        public static FileStage<T> File<T>(this StageBase<string, T> input)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new FileStage<T>(input.DoIt, input.Context);
        }

        public class JsonHelper<TInCache>
            where TInCache : class
        {
            private readonly StageBase<Stream, TInCache> input;

            internal JsonHelper(StageBase<Stream, TInCache> input)
            {
                this.input = input;
            }

            public JsonStage<TInCache, TOut> For<TOut>()
            {
                return new JsonStage<TInCache, TOut>(this.input.DoIt, this.input.Context);
            }
        }
        public static JsonHelper<TInCache> Json<TInCache>(this StageBase<Stream, TInCache> input)
            where TInCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new JsonHelper<TInCache>(input);
        }


        public static FileSystemStage<T> FileSystem<T>(this StageBase<string, T> input)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new FileSystemStage<T>(input.DoIt, input.Context);
        }

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

        public static TransformStage<TIn, TInCache, TOut> Transform<TIn, TInCache, TOut>(this StageBase<TIn, TInCache> input, Func<IDocument<TIn>, Task<IDocument<TOut>>> predicate)
    where TInCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            return new TransformStage<TIn, TInCache, TOut>(input.DoIt, predicate, input.Context);
        }

        public static TransformStage<TIn, TInCache, TOut> Transform<TIn, TInCache, TOut>(this StageBase<TIn, TInCache> input, Func<IDocument<TIn>, IDocument<TOut>> predicate)
            where TInCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            return new TransformStage<TIn, TInCache, TOut>(input.DoIt, x => Task.FromResult(predicate(x)), input.Context);
        }

        public static SelectStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache> Select<TInput, TInputItemCache, TInputCache, TResult, TItemCache>(this MultiStageBase<TInput, TInputItemCache, TInputCache> input, Func<StageBase<TInput, Stages.GeneratedHelper.CacheId<string>>, StageBase<TResult, TItemCache>> createPipline)
            where TInputCache : class
            where TInputItemCache : class
            where TItemCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new SelectStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache>(input.DoIt, createPipline, input.Context);
        }

        public static SelectManyStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache, TCache> SelectMany<TInput, TInputItemCache, TInputCache, TResult, TItemCache, TCache>(this MultiStageBase<TInput, TInputItemCache, TInputCache> input, Func<StageBase<TInput, Stages.GeneratedHelper.CacheId<string>>, MultiStageBase<TResult, TItemCache, TCache>> createPipline)
            where TCache : class
            where TInputCache : class
            where TInputItemCache : class
            where TItemCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new SelectManyStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache, TCache>(input.DoIt, createPipline, input.Context);
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
