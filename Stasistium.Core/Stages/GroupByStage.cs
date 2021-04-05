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



    public delegate IStageBaseOutput<TResult> GroupPipeline<TResult, TKey, TInput>(IStageBaseOutput<TKey> keyStage, IStageBaseOutput<TInput> inputStage);
    public class GroupByStage<TInput, TResult, TKey> : StageBase<TInput, TResult>
    {
        private readonly SubPipline<TInput, TInput> inputStage;
        private readonly SubPipline<TKey, TKey> keyStage;
        private readonly IStageBaseOutput<TResult> createPipline;
        private readonly Func<IDocument<TInput>, TKey> keySelector;


        public GroupByStage(Func<IDocument<TInput>, TKey> keySelector, GroupPipeline<TResult, TKey, TInput> createPipline, IGeneratorContext context, string? name) : base(context, name)
        {
            if (createPipline is null)
                throw new ArgumentNullException(nameof(createPipline));
            (this.inputStage, _) = SubPipeline.Create((IStageBaseOutput<TInput> start) => start, context);
            (this.keyStage, _) = SubPipeline.Create((IStageBaseOutput<TKey> start) => start, context);

            this.createPipline = createPipline(this.keyStage, this.inputStage);
            this.keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        }

        protected async override Task<ImmutableList<IDocument<TResult>>> Work(ImmutableList<IDocument<TInput>> input, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            var groups = input.GroupBy(this.keySelector);
            // create a token just for this invocation
            options = options.CreateSubToken();

            var builder = ImmutableList.CreateBuilder<IDocument<TResult>>();
            this.createPipline.PostStages += CreatePipline_PostStages;
            Task CreatePipline_PostStages(ImmutableList<IDocument<TResult>> input, OptionToken pipelineOptions)
            {
                if (!pipelineOptions.IsSubTokenOf(options))
                    return Task.CompletedTask;

                builder.AddRange(input);

                return Task.CompletedTask;
            }

            await Task.WhenAll(groups.Select(async group =>
            {
                var subOptions = options.CreateSubToken();
                await Task.WhenAll(this.inputStage.Invoke(group.ToImmutableList(), subOptions),
                                this.keyStage.Invoke(ImmutableList.Create(this.Context.CreateDocument(group.Key, this.Context.GetHashForObject(group.Key), this.Context.GetHashForObject(group.Key))), subOptions)).ConfigureAwait(false);
                //var currentStartStage = SubPipeline.Create((IStageBaseOutput<TInput> start) => this.createPipline(start, group.Key), this.Context);
                //return currentStartStage.Invoke(group.ToImmutableList(), options);
            })).ConfigureAwait(false);

            this.createPipline.PostStages -= CreatePipline_PostStages;

            return builder.ToImmutable();
        }

    }


    public delegate IStageBaseOutput<TResult> GroupPipeline<TResult, TKey, TInput, TAdditionalInput>(IStageBaseOutput<TKey> keyStage, IStageBaseOutput<TInput> inputStage, IStageBaseOutput<TAdditionalInput> additionalInput)
;
    public class GroupByStage<TInput, TResult, TKey, TAdditionalInput> : StageBase<TInput, TAdditionalInput, TResult>

    {
        private readonly SubPipline<TInput, TInput> inputStage;
        private readonly SubPipline<TAdditionalInput, TAdditionalInput> additional;
        private readonly SubPipline<TKey, TKey> keyStage;
        private readonly IStageBaseOutput<TResult> createPipline;
        private readonly Func<IDocument<TInput>, TKey> keySelector;


        public GroupByStage(Func<IDocument<TInput>, TKey> keySelector, GroupPipeline<TResult, TKey, TInput, TAdditionalInput> createPipline, IGeneratorContext context, string? name) : base(context, name)
        {
            if (createPipline is null)
                throw new ArgumentNullException(nameof(createPipline));
            this.inputStage = SubPipeline.Create<TInput>(context);
            this.additional = SubPipeline.Create<TAdditionalInput>(context);
            this.keyStage = SubPipeline.Create<TKey>(context);

            this.createPipline = createPipline(this.keyStage, this.inputStage, this.additional);
            this.keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        }

        protected override async Task<ImmutableList<IDocument<TResult>>> Work(ImmutableList<IDocument<TInput>> input, ImmutableList<IDocument<TAdditionalInput>> aditional, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            var groups = input.GroupBy(this.keySelector);
            // create a token just for this invocation
            options = options.CreateSubToken();

            var builder = ImmutableList.CreateBuilder<IDocument<TResult>>();
            this.createPipline.PostStages += CreatePipline_PostStages;
            Task CreatePipline_PostStages(ImmutableList<IDocument<TResult>> input, OptionToken pipelineOptions)
            {
                if (!pipelineOptions.IsSubTokenOf(options))
                    return Task.CompletedTask;

                builder.AddRange(input);

                return Task.CompletedTask;
            }

            await Task.WhenAll(groups.Select(async group =>
            {
                var subOptions = options.CreateSubToken();
                await Task.WhenAll(this.inputStage.Invoke(group.ToImmutableList(), subOptions),
                    this.additional.Invoke(aditional, subOptions),
                    this.keyStage.Invoke(ImmutableList.Create(this.Context.CreateDocument(group.Key, this.Context.GetHashForObject(group.Key), this.Context.GetHashForObject(group.Key))), subOptions)).ConfigureAwait(false);
                //var currentStartStage = SubPipeline.Create((IStageBaseOutput<TInput> start) => this.createPipline(start, group.Key), this.Context);
                //return currentStartStage.Invoke(group.ToImmutableList(), options);
            })).ConfigureAwait(false);

            this.createPipline.PostStages -= CreatePipline_PostStages;

            return builder.ToImmutable();
        }

    }


}
