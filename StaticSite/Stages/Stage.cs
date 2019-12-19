using StaticSite.Documents;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Toolkit.Parsers.Markdown;

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

        public static StaticStage<TResult> FromResult<TResult>(TResult result, Func<TResult, string> hashFunction, GeneratorContext context)
            => new StaticStage<TResult>(result, hashFunction, context);

        public static SidecarHelper<TPreviousItemCache, TPreviousListCache> Sidecar<TPreviousItemCache, TPreviousListCache>(this MultiStageBase<Stream, TPreviousItemCache, TPreviousListCache> stage)
            where TPreviousListCache : class
            where TPreviousItemCache : class
        {
            return new SidecarHelper<TPreviousItemCache, TPreviousListCache>(stage);
        }

        public static SelectStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache> Select<TInput, TInputItemCache, TInputCache, TResult, TItemCache>(this MultiStageBase<TInput, TInputItemCache, TInputCache> input, Func<StageBase<TInput, GeneratedHelper.CacheId<string>>, StageBase<TResult, TItemCache>> createPipline)
            where TInputCache : class
            where TInputItemCache : class
            where TItemCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new SelectStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache>(input.DoIt, createPipline, input.Context);
        }

        public static MarkdownToHtmlStage<T> MarkdownToHtml<T>(this StageBase<MarkdownDocument, T> input)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new MarkdownToHtmlStage<T>(input.DoIt, input.Context);
        }
        public static TextToStreamStage<T> TextToStream<T>(this StageBase<string, T> input)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new TextToStreamStage<T>(input.DoIt, input.Context);
        }

    }

    public class MarkdownToHtmlStage<TInputCache> : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple1List0StageBase<Microsoft.Toolkit.Parsers.Markdown.MarkdownDocument, TInputCache, string>
        where TInputCache : class
    {
        public MarkdownToHtmlStage(StagePerformHandler<MarkdownDocument, TInputCache> inputSingle0, GeneratorContext context) : base(inputSingle0, context)
        {
        }

        protected override Task<IDocument<string>> Work(IDocument<MarkdownDocument> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            var text = input.Value.ToString();
            return Task.FromResult(input.With(text, this.Context.GetHashForString(text)));
        }
    }
    public class TextToStreamStage<TInputCache> : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple1List0StageBase<string, TInputCache, Stream>
        where TInputCache : class
    {
        public TextToStreamStage(StagePerformHandler<string, TInputCache> inputSingle0, GeneratorContext context) : base(inputSingle0, context)
        {
        }

        protected override Task<IDocument<Stream>> Work(IDocument<string> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            var output = input.With(() => new MemoryStream(System.Text.Encoding.UTF8.GetBytes(input.Value)), input.Hash);
            return Task.FromResult<IDocument<Stream>>(output);
        }
    }

}
