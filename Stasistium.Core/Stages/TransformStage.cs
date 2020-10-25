using Stasistium.Core;
using Stasistium.Documents;
using Stasistium.Stages;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class TransformStage<TIn, TInItemCache, TInCache, TOut> : MultiStageBase<TOut, string, TransformStageCache<TInCache>>
        where TInCache : class
        where TInItemCache : class
    {
        private readonly MultiStageBase<TIn, TInItemCache, TInCache> input;
        private readonly Func<IDocument<TIn>, Task<IDocument<TOut>>> transform;

        public TransformStage(MultiStageBase<TIn, TInItemCache, TInCache> input, Func<IDocument<TIn>, Task<IDocument<TOut>>> selector, IGeneratorContext context, string? name = null) : base(context, name)
        {
            this.input = input;
            this.transform = selector;
        }

        protected override async Task<StageResultList<TOut, string, TransformStageCache<TInCache>>> DoInternal([AllowNull] TransformStageCache<TInCache>? cache, OptionToken options)
        {

            var input = await this.input.DoIt(cache?.PreviousCache , options).ConfigureAwait(false);

            var task = LazyTask.Create(async () =>
            {

                var inputList = await input.Perform;


                var list = await Task.WhenAll(inputList.Select(async subInput =>
                {

                    if (subInput.HasChanges)
                    {
                        var subResult = await subInput.Perform;
                        var transformed = await this.transform(subResult).ConfigureAwait(false);
                        bool hasChanges = true;
                        if (cache != null && cache.Transformed.TryGetValue(transformed.Id, out var oldHash))
                            hasChanges = oldHash != transformed.Hash;

                        return (result: StageResult.CreateStageResult(this.Context, transformed, hasChanges, transformed.Id, transformed.Hash, transformed.Hash), inputId: subInput.Id);
                    }
                    else
                    {
                        if (cache == null || !cache.InputToOutputId.TryGetValue(subInput.Id, out var oldOutputId) || !cache.Transformed.TryGetValue(oldOutputId, out var oldOutputHash))
                            throw this.Context.Exception("No changes, so old value should be there.");

                        return (result: StageResult.CreateStageResult(this.Context, LazyTask.Create(async () =>
                         {

                             var newSource = await subInput.Perform;
                             var transformed = await this.transform(newSource).ConfigureAwait(false);

                             return transformed;
                         }), false, oldOutputId, oldOutputHash, oldOutputHash),
                        inputId: subInput.Id);

                    }
                })).ConfigureAwait(false);

                var newCache = new TransformStageCache<TInCache>()
                {
                    InputToOutputId = list.ToDictionary(x => x.inputId, x => x.result.Id),
                    OutputIdOrder = list.Select(x => x.result.Id).ToArray(),
                    PreviousCache  = input.Cache,
                    Transformed = list.ToDictionary(x => x.result.Id, x => x.result.Hash),
                    Hash = this.Context.GetHashForObject(list.Select(x => x.result.Hash))
                };
                return (result: list.Select(x => x.result).ToImmutableList(), cache: newCache);
            });

            bool hasChanges = input.HasChanges;
            if (input.HasChanges || cache == null)
            {

                var (list, c) = await task;


                if (!hasChanges && list.Count != cache?.OutputIdOrder.Length)
                    hasChanges = true;

                if (!hasChanges && cache != null)
                {
                    for (int i = 0; i < cache.OutputIdOrder.Length && !hasChanges; i++)
                    {
                        if (list[i].Id != cache.OutputIdOrder[i])
                            hasChanges = true;
                        if (list[i].HasChanges)
                            hasChanges = true;
                    }
                }
                return this.Context.CreateStageResultList(list, hasChanges, c.OutputIdOrder.ToImmutableList(), c, c.Hash, input.Cache);

            }

            var actualTask = LazyTask.Create(async () =>
            {
                var temp = await task;
                return temp.result;
            });

            return this.Context.CreateStageResultList(actualTask, hasChanges, cache.OutputIdOrder.ToImmutableList(), cache, cache.Hash, input.Cache);
        }



    }
    public class TransformStage<TIn, TInCache, TOut> : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple1List0StageBase<TIn, TInCache, TOut>
        where TInCache : class
    {
        private readonly Func<IDocument<TIn>, Task<IDocument<TOut>>> transform;

        public TransformStage(StageBase<TIn, TInCache> inputSingle0, Func<IDocument<TIn>, Task<IDocument<TOut>>> selector, IGeneratorContext context, string? name = null) : base(inputSingle0, context, name)
        {
            this.transform = selector;
        }

        protected override Task<IDocument<TOut>> Work(IDocument<TIn> inputSingle0, OptionToken options)
        {
            return this.transform(inputSingle0);
        }

    }



}

namespace Stasistium
{


    public static partial class StageExtensions
    {
        public static TransformStage<TIn, TInITemCache, TInCache, TOut> Transform<TIn, TInITemCache, TInCache, TOut>(this MultiStageBase<TIn, TInITemCache, TInCache> input, Func<IDocument<TIn>, Task<IDocument<TOut>>> predicate, string? name = null)
            where TInCache : class
            where TInITemCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            return new TransformStage<TIn, TInITemCache, TInCache, TOut>(input, predicate, input.Context, name);
        }
        public static TransformStage<TIn, TInITemCache, TInCache, TOut> Transform<TIn, TInITemCache, TInCache, TOut>(this MultiStageBase<TIn, TInITemCache, TInCache> input, Func<IDocument<TIn>, IDocument<TOut>> predicate, string? name = null)
            where TInCache : class
            where TInITemCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            return new TransformStage<TIn, TInITemCache, TInCache, TOut>(input, x => Task.FromResult(predicate(x)), input.Context, name);
        }

        public static TransformStage<TIn, TInCache, TOut> Transform<TIn, TInCache, TOut>(this StageBase<TIn, TInCache> input, Func<IDocument<TIn>, Task<IDocument<TOut>>> predicate, string? name = null)
    where TInCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            return new TransformStage<TIn, TInCache, TOut>(input, predicate, input.Context, name);
        }

        public static TransformStage<TIn, TInCache, TOut> Transform<TIn, TInCache, TOut>(this StageBase<TIn, TInCache> input, Func<IDocument<TIn>, IDocument<TOut>> predicate, string? name = null)
            where TInCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            return new TransformStage<TIn, TInCache, TOut>(input, x => Task.FromResult(predicate(x)), input.Context, name);
        }
    }
}