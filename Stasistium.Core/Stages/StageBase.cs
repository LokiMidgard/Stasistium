using Stasistium.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class StageBase : IStageBase
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

            while (type.IsNested && type.DeclaringType is not null)
            {
                type = type.DeclaringType;
                if (type.IsGenericType)
                    type = type.GetGenericTypeDefinition();

                baseName = $"{type.Name}.{baseName}";
            }

            return $"{baseName}-{Guid.NewGuid()}";
        }

        public virtual Task Reset() => Task.CompletedTask;
    }

    public delegate Task StagePerform<T>(ImmutableList<IDocument<T>> cache, OptionToken options);


    public abstract class StageBaseSimple<TIn, TResult> : StageBase<TIn, TResult>, IStageBaseInput<TIn>, IStageBaseOutput<TResult>
    {
        protected StageBaseSimple(IGeneratorContext context, string? name) : base(context, name)
        {
        }

        protected abstract Task<IDocument<TResult>> Work(IDocument<TIn> input, OptionToken options);
        protected override sealed async Task<ImmutableList<IDocument<TResult>>> Work(ImmutableList<IDocument<TIn>> input, OptionToken options)
        {
            var result = await Task.WhenAll(input.Select(x => this.Work(x, options))).ConfigureAwait(false);
            return result.ToImmutableList();
        }
    }
    public abstract class StageBase<TIn, TResult> : StageBase, IStageBaseInput<TIn>, IStageBaseOutput<TResult>
    {
        public event StagePerform<TResult>? PostStages;

        protected StageBase(IGeneratorContext context, string? name) : base(context, name)
        {
        }


        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(ImmutableList<IDocument<TIn>> input, OptionToken options);

        async Task IStageBaseInput<TIn>.DoIt(ImmutableList<IDocument<TIn>> input, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            using var indent = this.Context.Logger.Indent();
            this.Context.Logger.Info($"BEGIN");
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();

            var result = await this.Work(input, options).ConfigureAwait(false);
            stopWatch.Stop();
            this.Context.Logger.Info($"END Took {stopWatch.Elapsed}");

            await Task
               .WhenAll(this.PostStages?.GetInvocationList()
                    .Cast<StagePerform<TResult>>()
                    .Select(s => s(result, options)) ?? Array.Empty<Task>()
               ).ConfigureAwait(false);
        }
    }



    public abstract class StageBaseSink<TIn> : StageBase, IStageBaseInput<TIn>
    {

        protected StageBaseSink(IGeneratorContext context, string? name) : base(context, name)
        {
        }


        protected abstract Task Work(ImmutableList<IDocument<TIn>> input, OptionToken options);

        async Task IStageBaseInput<TIn>.DoIt(ImmutableList<IDocument<TIn>> input, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            using var indent = this.Context.Logger.Indent();
            this.Context.Logger.Info($"BEGIN");
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();

            await this.Work(input, options).ConfigureAwait(false);
            stopWatch.Stop();
            this.Context.Logger.Info($"END Took {stopWatch.Elapsed}");
        }
    }

    public abstract class StageBase< TIn1, TIn2, TResult> : StageBase, IStageBaseInput<TIn1, TIn2>, IStageBaseOutput<TResult>
    {
        public event StagePerform<TResult>? PostStages;

        private TaskCompletionSource<(ImmutableList<IDocument<TIn2>>, OptionToken)> seccondArgument = new TaskCompletionSource<(ImmutableList<IDocument<TIn2>>, OptionToken)>();

        protected StageBase(IGeneratorContext context, string? name) : base(context, name)
        {
        }


        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(ImmutableList<IDocument<TIn1>> input1, ImmutableList<IDocument<TIn2>> input2, OptionToken options);

        async Task IStageBaseInput<TIn1, TIn2>.DoIt1(ImmutableList<IDocument<TIn1>> in1, OptionToken options)
        {
            var (in2, otherToken) = await this.seccondArgument.Task.ConfigureAwait(false);

            if (otherToken != options)
                throw new ArgumentException("OptionToken does not match.");

            var result = await this.Work(in1, in2, options).ConfigureAwait(false);

            await Task
               .WhenAll(this.PostStages?.GetInvocationList()
                    .Cast<StagePerform<TResult>>()
                    .Select(s => s(result, options)) ?? Array.Empty<Task>()
               ).ConfigureAwait(false);

        }

        Task IStageBaseInput<TIn1, TIn2>.DoIt2(ImmutableList<IDocument<TIn2>> in2, OptionToken options)
        {
            this.seccondArgument.SetResult((in2, options));
            return Task.CompletedTask;
        }

        public override Task Reset()
        {
            this.seccondArgument = new TaskCompletionSource<(ImmutableList<IDocument<TIn2>>, OptionToken)>();
            return base.Reset();
        }
    }


    public class OptionToken : IEquatable<OptionToken>
    {
        public bool RefreshRemoteSources { get; }
        public Guid GenerationId { get; }


        internal OptionToken(bool refresh)
        {
            this.GenerationId = Guid.NewGuid();
            this.RefreshRemoteSources = refresh;
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