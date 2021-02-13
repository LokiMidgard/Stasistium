using Stasistium.Documents;
using Stasistium.Stages;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class SingleStage<T> : StageBase<T, T>
    {
        public SingleStage(IGeneratorContext context, string? name) : base(context, name)
        {
        }

        protected override Task<ImmutableList<IDocument<T>>> Work(ImmutableList<IDocument<T>> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            if (input.Count != 1)
                throw this.Context.Exception($"Input should only have one element, but had {input.Count}.");

            return Task.FromResult(input);
        }
    }

}