
using Stasistium.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public abstract class StaticStage : StageBase
    {
        internal StaticStage(IGeneratorContext context, string? name) : base(context, name)
        {
        }

        public abstract Task Invoke(OptionToken options);
    }

    public class StaticStage<TResult> : StaticStage,  IStageBaseOutput<TResult>
    {
        private readonly IDocument<TResult> document;

        public event StagePerform<TResult>? PostStages;

        public StaticStage(string id, TResult value, Func<TResult, string> hashFunction, IGeneratorContext context, string? name = null) : base(context, name)
        {
            if (hashFunction is null)
                throw new ArgumentNullException(nameof(hashFunction));
            var contentHash = hashFunction(value);
            this.document = this.Context.CreateDocument(value, contentHash, id);
        }

        public override Task Invoke(OptionToken options)
        {
            return Task
            .WhenAll(this.PostStages?.GetInvocationList()
                .Cast<StagePerform<TResult>>()
                .Select(s => s(ImmutableList.Create(this.document), options)) ?? Array.Empty<Task>()
            );
        }
    }
}
