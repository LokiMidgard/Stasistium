﻿using StaticSite.Documents;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace StaticSite.Stages
{
    public class StaticStage<TResult> : StageBase<TResult, string>
    {
        private readonly string id = Guid.NewGuid().ToString();
        private readonly Func<TResult, string> hashFunction;
        public TResult Value { get; set; }

        public StaticStage(TResult result, Func<TResult, string> hashFunction, GeneratorContext context) : base(context)
        {
            this.Value = result;
            this.hashFunction = hashFunction ?? throw new ArgumentNullException(nameof(hashFunction));
        }

        protected override Task<StageResult<TResult, string>> DoInternal([AllowNull] BaseCache<string>? cache, OptionToken options)
        {
            var contentHash = this.hashFunction(this.Value);
            return Task.FromResult(StageResult.Create(
                perform: LazyTask.Create(() => (this.Context.Create(this.Value, contentHash, this.id), BaseCache.Create(contentHash, ReadOnlyMemory<BaseCache>.Empty))),
                hasChanges: cache == null || !Equals(cache.Item, contentHash),
                documentId: this.id));
        }
    }


}