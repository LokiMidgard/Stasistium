using Stasistium.Documents;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public static class SubPipeline
    {
        public static (SubPipline<TInput, TResult> input, IStageBaseOutput<TResult> output) Create<TInput, TResult>(Func<IStageBaseOutput<TInput>, IStageBaseOutput<TResult>> createPipeline, IGeneratorContext context)
        {
            return SubPipline<TInput, TResult>.Create(createPipeline, context);
        }
    }

    public class SubPipline<TInput, TResult> : StageBase, IStageBaseOutput<TInput>
    {
        public event StagePerform<TInput>? PostStages;

        private SubPipline(IGeneratorContext context) : base(context, null)
        {
        }

        // It also exists in the non generic variant which delegates to this. We need access to private members...
#pragma warning disable CA1000 // Do not declare static members on generic types
        public static (SubPipline<TInput, TResult> input, IStageBaseOutput<TResult> output) Create(Func<IStageBaseOutput<TInput>, IStageBaseOutput<TResult>> createPipeline, IGeneratorContext context)
        {
            if (createPipeline is null)
                throw new ArgumentNullException(nameof(createPipeline));
            var start = new SubPipline<TInput, TResult>(context);
            var pipline = createPipeline(start);
            return (start, pipline);
        }
#pragma warning restore CA1000 // Do not declare static members on generic types

        public async Task Invoke(ImmutableList<IDocument<TInput>> input, OptionToken options)
        {
            await Task
            .WhenAll(this.PostStages?.GetInvocationList()
                .Cast<StagePerform<TInput>>()
                .Select(s => s(input, options)) ?? Array.Empty<Task>()
            ).ConfigureAwait(false);
        }
    }

}
