﻿using Stasistium.Documents;
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


        public override string ToString()
        {
            return this.Name;
        }
    }

    public delegate Task StagePerform<T>(ImmutableList<IDocument<T>> input, OptionToken options);


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
            this.Context.Logger.Info($"BEGIN");
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            ImmutableList<IDocument<TResult>>? result;

            using (var indent = this.Context.Logger.Indent())
            {
                try
                {
                    result = await this.Work(input, options).ConfigureAwait(false);
                    if (options.CheckUniqueID)
                    {
                        var doubles = result.GroupBy(x => x.Id)
                            .Select(x => (id: x.Key, count: x.Count()))
                            .Where(x => x.count > 1)
                            .Select(x => $"{x.id}: {x.count}");
                        if (doubles.Any())
                        {
                            this.Context.Logger.Error($"Found multiple IDs:\n{string.Join("\n", doubles)}");
                            options.TryBreak(this.Context);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (options.BreakOnError)
                    {
                        try
                        {
                            if (System.Diagnostics.Debugger.IsAttached)
                                System.Diagnostics.Debugger.Break();
                            else
                                System.Diagnostics.Debugger.Launch();

                        }
                        catch (Exception ex)
                        {
                            this.Context.Logger.Error($"Faild to lunch debugger {ex}");
                        }
                    }
                    this.Context.Logger.Error(e.ToString());
                    result = ImmutableList<IDocument<TResult>>.Empty;
                }
                stopWatch.Stop();
            }
            this.Context.Logger.Info($"END Took\t{stopWatch.Elapsed}");

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
            this.Context.Logger.Info($"BEGIN");
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            using (var indent = this.Context.Logger.Indent())
            {


                try
                {
                    await this.Work(input, options).ConfigureAwait(false);

                }
                catch (Exception e)
                {
                    this.Context.Logger.Error($"Error {e}");
                }
                stopWatch.Stop();
            }
            this.Context.Logger.Info($"END Took {stopWatch.Elapsed}");
        }
    }

    public abstract class StageBase<TIn1, TIn2, TResult> : StageBase, IStageBaseInput<TIn1, TIn2>, IStageBaseOutput<TResult>
    {
        public event StagePerform<TResult>? PostStages;

        private System.Collections.Concurrent.ConcurrentDictionary<OptionToken, TaskCompletionSource<(ImmutableList<IDocument<TIn2>> input, OptionToken otherOption, TaskCompletionSource<object?> completed)>> argumentCompletion = new System.Collections.Concurrent.ConcurrentDictionary<OptionToken, TaskCompletionSource<(ImmutableList<IDocument<TIn2>> input, OptionToken otherOption, TaskCompletionSource<object?> completed)>>();

        protected StageBase(IGeneratorContext context, string? name) : base(context, name)
        {
        }


        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(ImmutableList<IDocument<TIn1>> input1, ImmutableList<IDocument<TIn2>> input2, OptionToken options);

        async Task IStageBaseInput<TIn1, TIn2>.DoIt1(ImmutableList<IDocument<TIn1>> in1, OptionToken options)
        {

            var seccondArgument = this.argumentCompletion.GetOrAdd(options, (opt) => new TaskCompletionSource<(ImmutableList<IDocument<TIn2>> input, OptionToken otherOption, TaskCompletionSource<object?> completed)>());

            var (in2, otherToken, finishSource) = await seccondArgument.Task.ConfigureAwait(false);

            if (!otherToken.HaveSameRoot(options))
                throw new ArgumentException("OptionToken does not match.");
            ImmutableList<IDocument<TResult>>? result;
            try
            {
                result = await this.Work(in1, in2, options).ConfigureAwait(false);
                if (options.CheckUniqueID)
                {
                    var doubles = result.GroupBy(x => x.Id)
                        .Select(x => (id: x.Key, count: x.Count()))
                        .Where(x => x.count > 1)
                        .Select(x => $"{x.id}: {x.count}");
                    if (doubles.Any())
                    {
                        this.Context.Logger.Error($"Found multiple IDs:\n{string.Join("\n", doubles)}");
                        options.TryBreak(this.Context);
                    }
                }
            }
            catch (Exception e)
            {
                if (options.BreakOnError)
                {
                    try
                    {
                        if (System.Diagnostics.Debugger.IsAttached)
                            System.Diagnostics.Debugger.Break();
                        else
                            System.Diagnostics.Debugger.Launch();

                    }
                    catch (Exception ex)
                    {
                        this.Context.Logger.Error($"Faild to lunch debugger {ex}");
                    }
                }
                this.Context.Logger.Error(e.ToString());
                result = ImmutableList<IDocument<TResult>>.Empty;
            }
            await Task
               .WhenAll(this.PostStages?.GetInvocationList()
                    .Cast<StagePerform<TResult>>()
                    .Select(s => s(result, options)) ?? Array.Empty<Task>()
               ).ConfigureAwait(false);
            finishSource.SetResult(null);
        }

        async Task IStageBaseInput<TIn1, TIn2>.DoIt2(ImmutableList<IDocument<TIn2>> in2, OptionToken options)
        {
            var seccondArgument = this.argumentCompletion.GetOrAdd(options, (opt) => new TaskCompletionSource<(ImmutableList<IDocument<TIn2>> input, OptionToken otherOption, TaskCompletionSource<object?> completed)>());
            var finish = new TaskCompletionSource<object?>();
            seccondArgument.SetResult((in2, options, finish));
            await finish.Task.ConfigureAwait(false);
            this.argumentCompletion.TryRemove(options, out _);
        }

    }


    public class OptionToken : IEquatable<OptionToken>
    {
        private readonly GenerationOptions root;

        public bool RefreshRemoteSources => this.root.Refresh;
        public bool BreakOnError => this.root.BreakOnError;

        public bool CheckUniqueID => this.root.CheckUniqueID;

        public ImmutableArray<Guid> GenerationId { get; }

        public void TryBreak(IGeneratorContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            if (this.BreakOnError)
            {
                try
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                    else
                        System.Diagnostics.Debugger.Launch();

                }
                catch (Exception ex)
                {
                    context.Logger.Error($"Faild to lunch debugger {ex}");
                }
            }
        }

        internal OptionToken(GenerationOptions root)
        {
            this.GenerationId = ImmutableArray.Create(Guid.NewGuid());
            this.root = root;
        }
        private OptionToken(OptionToken parent)
        {
            this.GenerationId = parent.GenerationId.Add(Guid.NewGuid());
            this.root = parent.root;
        }
        public OptionToken CreateSubToken()
        {
            return new OptionToken(this);
        }
        public override bool Equals(object? obj)
        {
            return obj is OptionToken token && this.Equals(token);
        }

        public bool Equals([AllowNull] OptionToken? other)
        {
            if (other is null)
                return false;

            return this.GenerationId.SequenceEqual(other.GenerationId);
        }
        public bool IsSuperTokenOf(OptionToken other)
        {
            if (other is null)
                throw new ArgumentNullException(nameof(other));
            return other.IsSubTokenOf(this);
        }
        public bool IsSubTokenOf(OptionToken other)
        {
            if (other is null)
                throw new ArgumentNullException(nameof(other));
            if (this.GenerationId.Length > other.GenerationId.Length)
            {
                for (int i = 0; i < other.GenerationId.Length; i++)
                {
                    if (other.GenerationId[i] != this.GenerationId[i])
                        return false;
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            for (var i = 0; i < this.GenerationId.Length; i++)
                hash.Add(this.GenerationId[i]);
            return hash.ToHashCode();
        }

        public bool HaveSameRoot(OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            return options.GenerationId[0] == this.GenerationId[0];
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