using StaticSite.Documents;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace StaticSite.Modules
{

    public abstract class ModuleBase<TResult, TCache>
        where TCache : class
    {

        private (Guid lastId, Task<ModuleResult<TResult, TCache>> result) lastRun;

        protected ModuleBase(GeneratorContext context)
        {
            this.Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        protected abstract Task<ModuleResult<TResult, TCache>> DoInternal([AllowNull]BaseCache<TCache>? cache, OptionToken options);

        public Task<ModuleResult<TResult, TCache>> DoIt([AllowNull]BaseCache? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            var cast = cache as BaseCache<TCache>;
            if (cache != null && cast is null)
                throw new ArgumentException($"Cache must be of type {nameof(BaseCache)}<{typeof(TCache).FullName}> but was {cache.GetType().FullName}", nameof(cache));

            var lastRun = this.lastRun;
            if (lastRun.lastId == options.GenerationId)
                return lastRun.result;

            var result = this.DoInternal(cast, options);
            this.lastRun = (options.GenerationId, result);
            return result;
        }

        public GeneratorContext Context { get; }
    }

    public abstract class MultiModuleBase<TResult, TCacheResult, TCache>
     where TCache : class
    {

        private (Guid lastId, Task<ModuleResultList<TResult, TCacheResult, TCache>> result) lastRun;

        protected MultiModuleBase(GeneratorContext context)
        {
            this.Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        protected abstract Task<ModuleResultList<TResult, TCacheResult, TCache>> DoInternal([AllowNull]BaseCache<TCache>? cache, OptionToken options);

        public Task<ModuleResultList<TResult, TCacheResult, TCache>> DoIt([AllowNull]BaseCache cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            var cast = cache as BaseCache<TCache>;
            if (cache != null && cast is null)
                throw new ArgumentException($"Cache must be of type {nameof(BaseCache)}<{typeof(TCache).FullName}> but was {cache.GetType().FullName}", nameof(cache));

            var lastRun = this.lastRun;
            if (lastRun.lastId == options.GenerationId)
                return lastRun.result;

            var result = this.DoInternal(cast, options);
            this.lastRun = (options.GenerationId, result);
            return result;
        }

        public GeneratorContext Context { get; }
    }


    public class GenerationOptions
    {
        public bool Refresh { set; get; } = true;

        public bool CompressCache { get; set; } = true;
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