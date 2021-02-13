using Stasistium.Documents;
using Stasistium.Stages;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class OrderByStage<T> : StageBase<T, T>
    {
        private readonly Comparison<IDocument<T>> comparision;

        public OrderByStage(Comparison<IDocument<T>> comparision, IGeneratorContext context, string? name) : base(context, name)
        {
            this.comparision = comparision;
        }

        protected override Task<ImmutableList<IDocument<T>>> Work(ImmutableList<IDocument<T>> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            return Task.FromResult(input.Sort(comparision));
        }
    }
}
