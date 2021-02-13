using Stasistium.Documents;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;

using Stasistium.Stages;

namespace Stasistium.Stages
{
    public class GroupByStage<TInput, TResult, TKey> : StageBase<TInput, TResult>
    {
        private readonly Func<IStageBaseOutput<TInput>, TKey, IStageBaseOutput<TResult>> createPipline;
        private readonly Func<IDocument<TInput>, TKey> keySelector;


        public GroupByStage(Func<IStageBaseOutput<TInput>, TKey, IStageBaseOutput<TResult>> createPipline, Func<IDocument<TInput>, TKey> keySelector, IGeneratorContext context, string? name) : base(context, name)
        {
            this.createPipline = createPipline;
            this.keySelector = keySelector; 
        }

        protected async override Task<ImmutableList<IDocument<TResult>>> Work(ImmutableList<IDocument<TInput>> input, OptionToken options)
        {
            var groups = input.GroupBy(this.keySelector);

            var results = await Task.WhenAll(groups.Select(group =>
            {
                var currentStartStage = SubPipeline.Create((IStageBaseOutput<TInput> start) => this.createPipline(start, group.Key), this.Context);
                return currentStartStage.Invoke(group.ToImmutableList(), options);
            })).ConfigureAwait(false);

            return results.SelectMany(x => x).ToImmutableList();
        }

    }
    public class SubPiplineHelper<TInput, TResult> : StageBase, IStageBaseOutput<TInput>
    {
        public event StagePerform<TInput>? PostStages;

        private TaskCompletionSource<ImmutableList<IDocument<TResult>>> completionSource = new TaskCompletionSource<ImmutableList<IDocument<TResult>>>();

        public Task<ImmutableList<IDocument<TResult>>> Result => this.completionSource.Task;



        private SubPiplineHelper(IGeneratorContext context) : base(context, null)
        {
        }

        // It also exists in the non generic variant which delegates to this. We need access to private members...
#pragma warning disable CA1000 // Do not declare static members on generic types
        public static SubPiplineHelper<TInput, TResult> Create(Func<IStageBaseOutput<TInput>, IStageBaseOutput<TResult>> createPipeline, IGeneratorContext context)
        {
            if (createPipeline is null)
                throw new ArgumentNullException(nameof(createPipeline));
            var start = new SubPiplineHelper<TInput, TResult>(context);
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
