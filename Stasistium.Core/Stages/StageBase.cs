using Stasistium.Documents;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class StageBase
    {
        internal StageBase(IGeneratorContext context, string? name)
        {
            this.Name = name ?? this.GenerateName();
            this.Context = context?.ForName(this.Name) ?? throw new ArgumentNullException(nameof(context));
        }

        public IGeneratorContext Context { get; }
        public string Name { get; }
        private string GenerateName()
        {
            var type = this.GetType();
            string baseName;

            if (type.IsGenericType)
                type = type.GetGenericTypeDefinition();

            baseName = type.Name;

            while (type.IsNested)
            {
                type = type.DeclaringType;
                if (type.IsGenericType)
                    type = type.GetGenericTypeDefinition();

                baseName = $"{type.Name}.{baseName}";
            }

            return baseName + Guid.NewGuid().ToString();
        }
    }
    public abstract class StageBase<TResult, TCache> : StageBase
        where TCache : class
    {

        private (Guid lastId, Task<StageResult<TResult, TCache>> result) lastRun;

        protected StageBase(IGeneratorContext context, string? name) : base(context, name)
        {
        }

        protected abstract Task<StageResult<TResult, TCache>> DoInternal([AllowNull]TCache? cache, OptionToken options);

        public Task<StageResult<TResult, TCache>> DoIt([AllowNull]TCache? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            using var indent = this.Context.Logger.Indent();
            this.Context.Logger.Info($"BEGIN");
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {

                var lastRun = this.lastRun;
                if (lastRun.lastId == options.GenerationId)
                    return lastRun.result;

                var result = this.DoInternal(cache, options);
                this.lastRun = (options.GenerationId, result);
                return result;

            }
            finally
            {
                stopWatch.Stop();
                this.Context.Logger.Info($"END Took {stopWatch.Elapsed}");
            }
        }


    }

    public abstract class MultiStageBase<TResult, TCacheResult, TCache> : StageBase
     where TCache : class
     where TCacheResult : class
    {

        private (Guid lastId, Task<StageResultList<TResult, TCacheResult, TCache>> result) lastRun;

        protected MultiStageBase(IGeneratorContext context, string? name = null) : base(context, name)
        {
        }



        protected abstract Task<StageResultList<TResult, TCacheResult, TCache>> DoInternal([AllowNull]TCache? cache, OptionToken options);

        public Task<StageResultList<TResult, TCacheResult, TCache>> DoIt([AllowNull]TCache? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            using var indent = this.Context.Logger.Indent();
            this.Context.Logger.Info($"BEGIN");
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var lastRun = this.lastRun;
                if (lastRun.lastId == options.GenerationId)
                    return lastRun.result;

                var result = this.DoInternal(cache, options);
                this.lastRun = (options.GenerationId, result);
                return result;
            }
            finally
            {
                stopWatch.Stop();
                this.Context.Logger.Info($"END Took {stopWatch.Elapsed}");
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

        public bool Equals([AllowNull] OptionToken? other)
        {
            if (other is null)
                return false;
            return this.GenerationId.Equals(other.GenerationId);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.GenerationId);
        }

        public static bool operator ==(OptionToken? left, OptionToken? right)
        {
            if (left is null && right is null)
                return true;
            if (left is null || right is null)
                return false;
            return left.Equals(right);
        }

        public static bool operator !=(OptionToken? left, OptionToken? right)
        {
            return !(left == right);
        }
    }

}