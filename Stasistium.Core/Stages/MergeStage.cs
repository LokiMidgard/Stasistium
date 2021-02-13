
using Stasistium.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Stasistium.Stages;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class MergeStage< TIn1, TIn2, TOut> : StageBase<TIn1, TIn2, TOut>
    {

        protected override Task<ImmutableList<IDocument<TOut>>> Work(ImmutableList<IDocument<TIn1>> input1, ImmutableList<IDocument<TIn2>> input2, OptionToken options)
        {
            var joind = from i1 in input1
                        from i2 in input2
                        select this.mergeFunction(i1, i2);
            return Task.FromResult(joind.ToImmutableList());
        }

        private readonly Func<IDocument<TIn1>, IDocument<TIn2>, IDocument<TOut>> mergeFunction;

        public MergeStage(Func<IDocument<TIn1>, IDocument<TIn2>, IDocument<TOut>> mergeFunction, IGeneratorContext context, string? name) : base(context, name)
        {
            this.mergeFunction = mergeFunction;
        }
    }
}