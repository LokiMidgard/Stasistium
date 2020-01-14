using Stasistium.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Stasistium.Stages;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class ListToSingleStage<TIn, TInItemCache, TInCache, TOut> : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple0List1StageBase<TIn, TInItemCache, TInCache, TOut>
        where TInItemCache : class
        where TInCache : class
    {
        private readonly Func<ImmutableList<IDocument<TIn>>, IDocument<TOut>> transform;

        public ListToSingleStage(StagePerformHandler<TIn, TInItemCache, TInCache> inputList0, Func<ImmutableList<IDocument<TIn>>, IDocument<TOut>> transform, IGeneratorContext context, string? name) : base(inputList0, context, name)
        {
            this.transform = transform ?? throw new ArgumentNullException(nameof(transform));
        }

        protected override Task<IDocument<TOut>> Work(ImmutableList<IDocument<TIn>> inputList0, OptionToken options)
        {
            return Task.FromResult(this.transform(inputList0));
        }
    }
}

namespace Stasistium
{
    public static partial class StageExtensions
    {
        public static ListToSingleStage<TIn, TInItemCache, TInCache, TOut> ListToSingle<TIn, TInItemCache, TInCache, TOut>(this MultiStageBase<TIn, TInItemCache, TInCache> input, Func<ImmutableList<IDocument<TIn>>, IDocument<TOut>> transform, string? name = null)
            where TInItemCache : class
            where TInCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (transform is null)
                throw new ArgumentNullException(nameof(transform));
            return new ListToSingleStage<TIn, TInItemCache, TInCache, TOut>(input.DoIt, transform, input.Context, name);
        }
    }
}