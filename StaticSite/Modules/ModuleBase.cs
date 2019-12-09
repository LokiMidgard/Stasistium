using StaticSite.Documents;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace StaticSite.Modules
{
    public abstract class SingleInputModuleBase<TResult, TCache, TInput, TPreviousCache> : ModuleBase<TResult, TCache>
        where TCache : class
    {
        private readonly ModulePerformHandler<TInput, TPreviousCache> input;
        private readonly bool updateOnRefresh;

        public SingleInputModuleBase(ModulePerformHandler<TInput, TPreviousCache> input, GeneratorContext context, bool updateOnRefresh = false) : base(context)
        {
            this.input = input;
            this.updateOnRefresh = updateOnRefresh;
        }


        protected abstract Task<(TResult result, BaseCache<TCache> cache)> Work((TInput result, BaseCache<TPreviousCache> cache) input, bool previousHadChanges, OptionToken options);


        protected sealed override async Task<ModuleResult<TResult, TCache>> Do([AllowNull] BaseCache<TCache> cache, OptionToken options)
        {

            if (cache != null && cache.PreviousCache.Length != 1)
                throw new ArgumentException($"This cache should have exactly one predecessor but had {cache.PreviousCache}");
            var inputResult = await this.input(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);



            var task = LazyTask.Create(async () =>
            {

                var previousPerform = await inputResult.Perform;
                var source = previousPerform.result;

                return await this.Work(previousPerform, inputResult.HasChanges, options).ConfigureAwait(false);
            });


            bool hasChanges = inputResult.HasChanges;

            if (hasChanges || (this.updateOnRefresh && options.Refresh))
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                hasChanges = await this.Changed(cache?.Item, result.cache.Item).ConfigureAwait(false);
            }

            return ModuleResult.Create(task, hasChanges);
        }

        protected virtual Task<bool> Changed([AllowNull]TCache item1, TCache item2)
        {
            return Task.FromResult(Equals(item1, item2));
        }
    }

    public abstract class ModuleBase<TResult, TCache>
        where TCache : class
    {

        private (Guid lastId, System.Threading.Tasks.Task<ModuleResult<TResult, TCache>> result) lastRun;

        protected ModuleBase(GeneratorContext context)
        {
            this.Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        protected abstract System.Threading.Tasks.Task<ModuleResult<TResult, TCache>> Do([AllowNull]BaseCache<TCache> cache, OptionToken options);

        public Task<ModuleResult<TResult, TCache>> DoIt([AllowNull]BaseCache cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            var cast = cache as BaseCache<TCache>;
            if (cache != null && cast is null)
                throw new ArgumentException($"Cache must be of type {nameof(BaseCache)}<{typeof(TCache).FullName}> but was {cache.GetType().FullName}", nameof(cache));

            var lastRun = this.lastRun;
            if (lastRun.lastId == options.GenerationId)
                return lastRun.result;

            var result = this.Do(cast, options);
            this.lastRun = (options.GenerationId, result);
            return result;
        }

        public GeneratorContext Context { get; }
    }

    public class GenerationOptions
    {
        public bool Refresh { set; get; } = true;

        public OptionToken Token
        {
            get
            {
                var token = new OptionToken(this.Refresh);
                return token;

            }
        }
    }

    public class OptionToken : IEquatable<OptionToken>
    {
        public bool Refresh { get; }
        public Guid GenerationId { get; }


        internal OptionToken(bool refresh)
        {
            this.GenerationId = Guid.NewGuid();
            this.Refresh = refresh;
        }

        public override bool Equals(object? obj)
        {
            return obj is OptionToken token && this.Equals(token);
        }

        public bool Equals([AllowNull] OptionToken other)
        {
            return this.GenerationId.Equals(other.GenerationId);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.GenerationId);
        }

        public static bool operator ==(OptionToken left, OptionToken right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OptionToken left, OptionToken right)
        {
            return !(left == right);
        }
    }

}