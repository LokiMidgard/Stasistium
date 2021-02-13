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
    public class SelectManyStage<TInput, TResult> : StageBase<TInput, TResult>
    {

        protected override Task<ImmutableList<IDocument<TResult>>> Work(ImmutableList<IDocument<TInput>> input, OptionToken options)
        {
            return Task.FromResult(input.SelectMany(this.transform).ToImmutableList());
        }


        private readonly Func<IDocument<TInput>, ImmutableList<IDocument<TResult>>> transform;


        public SelectManyStage(Func<IDocument<TInput>, ImmutableList<IDocument<TResult>>> transform, IGeneratorContext context, string? name) : base(context, name)
        {
            this.transform = transform;
        }
    }
}
