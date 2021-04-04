using Stasistium.Documents;
using System;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Linq;

namespace Stasistium.Stages
{
    public class CrossJoinStage<TInput, TAditional, TResult> : StageBase<TInput, TAditional, TResult>
    {
        protected override Task<ImmutableList<IDocument<TResult>>> Work(ImmutableList<IDocument<TInput>> input, ImmutableList<IDocument<TAditional>> additinoal, OptionToken options)
        {
            return Task.FromResult(input.SelectMany(x => additinoal.Select(y => this.transform(x, y))).ToImmutableList());
        }


        private readonly Func<IDocument<TInput>, IDocument<TAditional>, IDocument<TResult>> transform;


        public CrossJoinStage(Func<IDocument<TInput>, IDocument<TAditional>, IDocument<TResult>> transform, IGeneratorContext context, string? name) : base(context, name)
        {
            this.transform = transform;
        }
    }
}
