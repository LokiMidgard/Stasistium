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


        public GroupByStage(Func<IDocument<TInput>, TKey> keySelector, Func<IStageBaseOutput<TInput>, TKey, IStageBaseOutput<TResult>> createPipline, IGeneratorContext context, string? name) : base(context, name)
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

}
