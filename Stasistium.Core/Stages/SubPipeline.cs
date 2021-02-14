using Stasistium.Documents;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public static class SubPipeline
    {
        public static SubPipline<TInput, TResult> Create<TInput, TResult>(Func<IStageBaseOutput<TInput>, IStageBaseOutput<TResult>> createPipeline, IGeneratorContext context)
        {
            return SubPipline<TInput, TResult>.Create(createPipeline, context);
        }
    }

    public class SubPipline<TInput, TResult> : StageBase, IStageBaseOutput<TInput>
    {
        public event StagePerform<TInput>? PostStages;

        private TaskCompletionSource<ImmutableList<IDocument<TResult>>> completionSource = new TaskCompletionSource<ImmutableList<IDocument<TResult>>>();

        public Task<ImmutableList<IDocument<TResult>>> Result => this.completionSource.Task;



        private SubPipline(IGeneratorContext context) : base(context, null)
        {
        }

        // It also exists in the non generic variant which delegates to this. We need access to private members...
#pragma warning disable CA1000 // Do not declare static members on generic types
        public static SubPipline<TInput, TResult> Create(Func<IStageBaseOutput<TInput>, IStageBaseOutput<TResult>> createPipeline, IGeneratorContext context)
        {
            if (createPipeline is null)
                throw new ArgumentNullException(nameof(createPipeline));
            var start = new SubPipline<TInput, TResult>(context);
            var pipline = createPipeline(start);
            pipline.PostStages += (result, options) =>
            {
                start.completionSource.SetResult(result);
                return Task.CompletedTask;
            };
            return start;
        }
#pragma warning restore CA1000 // Do not declare static members on generic types

        public async Task<ImmutableList<IDocument<TResult>>> Invoke(ImmutableList<IDocument<TInput>> input, OptionToken options)
        {
            await Task
            .WhenAll(this.PostStages?.GetInvocationList()
                .Cast<StagePerform<TInput>>()
                .Select(s => s(input, options)) ?? Array.Empty<Task>()
            ).ConfigureAwait(false);
            return await this.Result.ConfigureAwait(false);
        }
    }

}
