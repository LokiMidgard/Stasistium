﻿using Stasistium.Documents;
using System;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace Stasistium.Stages
{
    public class ListTransform<TInput, TResult> : StageBase<TInput, TResult>
    {
        private readonly Func<ImmutableList<IDocument<TInput>>, IEnumerable<IDocument<TResult>>> transform;

        public ListTransform(Func<ImmutableList<IDocument<TInput>>, ImmutableList<IDocument<TResult>>> transform, IGeneratorContext context, string? name) : base(context, name)
        {
            this.transform = transform;
        }

        public ListTransform(Func<ImmutableList<IDocument<TInput>>, IDocument<TResult>> transform, IGeneratorContext context, string? name) : base(context, name)
        {
            this.transform = input => ImmutableList.Create(transform(input));
        }

        protected override Task<ImmutableList<IDocument<TResult>>> Work(ImmutableList<IDocument<TInput>> input, OptionToken options)
        {
            var result = this.transform(input);

            if (result is ImmutableList<IDocument<TResult>> immutable)
                return Task.FromResult(immutable);
            return Task.FromResult(result.ToImmutableList());
        }
    }
}
