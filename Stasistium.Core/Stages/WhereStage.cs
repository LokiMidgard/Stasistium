using Stasistium.Documents;
using Stasistium.Stages;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Stasistium.Stages
{

    public class WhereStage<T> : StageBase<T, T>
    {
        private readonly Func<IDocument<T>, bool> predicate;

        public WhereStage(Expression<Func<IDocument<T>, bool>> predicate, IGeneratorContext context, string? name = null) : base(context, name ?? predicate?.ToString())
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            this.predicate = predicate.Compile();
        }


        protected override Task<ImmutableList<IDocument<T>>> Work(ImmutableList<IDocument<T>> input, OptionToken options)
        {
            return Task.FromResult(input.Where(this.predicate).ToImmutableList());
        }


    }
    public class WhereAsyncStage<T> : StageBase<T, T>
    {
        private readonly Func<IDocument<T>, Task<bool>> predicate;

        public WhereAsyncStage(Expression<Func<IDocument<T>, Task<bool>>> predicate, IGeneratorContext context, string? name = null) : base(context, name ?? predicate?.ToString())
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            this.predicate = predicate.Compile();
        }


        protected override async Task<ImmutableList<IDocument<T>>> Work(ImmutableList<IDocument<T>> input, OptionToken options)
        {
            var evaluatedFilter = await Task.WhenAll(input.Select(async x => (value: x, filter: await this.predicate(x).ConfigureAwait(false)))).ConfigureAwait(false);
            return evaluatedFilter.Where(x => x.filter).Select(x => x.value).ToImmutableList();
        }


    }
}