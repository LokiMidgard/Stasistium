
using Stasistium.Documents;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Stasistium.Stages;
using System;

using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Stasistium.Stages
{
    public class ConcatStage<T> : StageBase<T, T, T>
    {
        public ConcatStage(IGeneratorContext context, string? name) : base(context, name)
        {
        }

        protected override Task<ImmutableList<IDocument<T>>> Work(ImmutableList<IDocument<T>> input1, ImmutableList<IDocument<T>> input2, OptionToken options)
        {
            if (input1 is null)
                throw new ArgumentNullException(nameof(input1));
            if (input2 is null)
                throw new ArgumentNullException(nameof(input2));
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            return Task.FromResult(input1.AddRange(input2));
        }

    }

}