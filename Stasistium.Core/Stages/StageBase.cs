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
            Name = name ?? GenerateName();
            Context = context?.ForName(Name) ?? throw new ArgumentNullException(nameof(context));
        }

        public IGeneratorContext Context { get; }
        public string Name { get; }
        private string GenerateName()
        {
            Type? type = GetType();
            string baseName;

            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }

            baseName = type.Name;

            while (type.IsNested && type.DeclaringType is not null)
            {
                type = type.DeclaringType;
                if (type.IsGenericType)
                {
                    type = type.GetGenericTypeDefinition();
                }

                baseName = $"{type.Name}.{baseName}";
            }

            return $"{baseName}-{Guid.NewGuid()}";
        }


        public override string ToString()
        {
            return Name;
        }
    }

    public delegate Task StagePerform<T>(ImmutableList<IDocument<T>> input, OptionToken options);


    public abstract class StageBaseSimple<TIn, TResult> : StageBase<TIn, TResult>, IStageBaseInput<TIn>, IStageBaseOutput<TResult>
    {
        protected StageBaseSimple(IGeneratorContext context, string? name) : base(context, name)
        {
        }

        protected abstract Task<IDocument<TResult>> Work(IDocument<TIn> input, OptionToken options);
        protected sealed override async Task<ImmutableList<IDocument<TResult>>> Work(ImmutableList<IDocument<TIn>> input, OptionToken options)
        {
            IDocument<TResult>[]? result = await Task.WhenAll(input.Select(x => Work(x, options))).ConfigureAwait(false);
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
            {
                throw new ArgumentNullException(nameof(options));
            }

            Context.Logger.Info($"BEGIN");
            System.Diagnostics.Stopwatch? stopWatch = System.Diagnostics.Stopwatch.StartNew();
            ImmutableList<IDocument<TResult>>? result;

            using (IDisposable? indent = Context.Logger.Indent())
            {
                try
                {
                    result = await Work(input, options).ConfigureAwait(false);
                    if (options.CheckUniqueID)
                    {
                        IEnumerable<string>? doubles = result.GroupBy(x => x.Id)
                            .Select(x => (id: x.Key, count: x.Count()))
                            .Where(x => x.count > 1)
                            .Select(x => $"{x.id}: {x.count}");
                        if (doubles.Any())
                        {
                            Context.Logger.Error($"Found multiple IDs:\n{string.Join("\n", doubles)}");
                            options.TryBreak(Context);
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
                            {
                                System.Diagnostics.Debugger.Break();
                            }
                            else
                            {
                                System.Diagnostics.Debugger.Launch();
                            }
                        }
                        catch (Exception ex)
                        {
                            Context.Logger.Error($"Faild to lunch debugger {ex}");
                        }
                    }
                    Context.Logger.Error(e.ToString());
                    result = ImmutableList<IDocument<TResult>>.Empty;
                }
                stopWatch.Stop();
            }
            Context.Logger.Info($"END Took\t{stopWatch.Elapsed}");

            await Task
               .WhenAll(PostStages?.GetInvocationList()
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
            {
                throw new ArgumentNullException(nameof(options));
            }

            Context.Logger.Info($"BEGIN");
            System.Diagnostics.Stopwatch? stopWatch = System.Diagnostics.Stopwatch.StartNew();
            using (IDisposable? indent = Context.Logger.Indent())
            {


                try
                {
                    await Work(input, options).ConfigureAwait(false);

                }
                catch (Exception e)
                {
                    Context.Logger.Error($"Error {e}");
                }
                stopWatch.Stop();
            }
            Context.Logger.Info($"END Took {stopWatch.Elapsed}");
        }
    }

    public abstract class StageBase<TIn1, TIn2, TResult> : StageBase, IStageBaseInput<TIn1, TIn2>, IStageBaseOutput<TResult>
    {
        public event StagePerform<TResult>? PostStages;

        private System.Collections.Concurrent.ConcurrentDictionary<OptionToken, TaskCompletionSource<(ImmutableList<IDocument<TIn2>> input, OptionToken otherOption, TaskCompletionSource<object?> completed)>> argumentCompletion = new();

        protected StageBase(IGeneratorContext context, string? name) : base(context, name)
        {
        }


        protected abstract Task<ImmutableList<IDocument<TResult>>> Work(ImmutableList<IDocument<TIn1>> input1, ImmutableList<IDocument<TIn2>> input2, OptionToken options);

        async Task IStageBaseInput<TIn1, TIn2>.DoIt1(ImmutableList<IDocument<TIn1>> in1, OptionToken options)
        {

            TaskCompletionSource<(ImmutableList<IDocument<TIn2>> input, OptionToken otherOption, TaskCompletionSource<object?> completed)>? seccondArgument = argumentCompletion.GetOrAdd(options, (opt) => new TaskCompletionSource<(ImmutableList<IDocument<TIn2>> input, OptionToken otherOption, TaskCompletionSource<object?> completed)>());

            (ImmutableList<IDocument<TIn2>> in2, OptionToken otherToken, TaskCompletionSource<object?> finishSource) = await seccondArgument.Task.ConfigureAwait(false);

            if (!otherToken.HaveSameRoot(options))
            {
                throw new ArgumentException("OptionToken does not match.");
            }

            ImmutableList<IDocument<TResult>>? result;
            try
            {
                result = await Work(in1, in2, options).ConfigureAwait(false);
                if (options.CheckUniqueID)
                {
                    IEnumerable<string>? doubles = result.GroupBy(x => x.Id)
                        .Select(x => (id: x.Key, count: x.Count()))
                        .Where(x => x.count > 1)
                        .Select(x => $"{x.id}: {x.count}");
                    if (doubles.Any())
                    {
                        Context.Logger.Error($"Found multiple IDs:\n{string.Join("\n", doubles)}");
                        options.TryBreak(Context);
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
                        {
                            System.Diagnostics.Debugger.Break();
                        }
                        else
                        {
                            System.Diagnostics.Debugger.Launch();
                        }
                    }
                    catch (Exception ex)
                    {
                        Context.Logger.Error($"Faild to lunch debugger {ex}");
                    }
                }
                Context.Logger.Error(e.ToString());
                result = ImmutableList<IDocument<TResult>>.Empty;
            }
            await Task
               .WhenAll(PostStages?.GetInvocationList()
                    .Cast<StagePerform<TResult>>()
                    .Select(s => s(result, options)) ?? Array.Empty<Task>()
               ).ConfigureAwait(false);
            finishSource.SetResult(null);
        }

        async Task IStageBaseInput<TIn1, TIn2>.DoIt2(ImmutableList<IDocument<TIn2>> in2, OptionToken options)
        {
            TaskCompletionSource<(ImmutableList<IDocument<TIn2>> input, OptionToken otherOption, TaskCompletionSource<object?> completed)>? seccondArgument = argumentCompletion.GetOrAdd(options, (opt) => new TaskCompletionSource<(ImmutableList<IDocument<TIn2>> input, OptionToken otherOption, TaskCompletionSource<object?> completed)>());
            TaskCompletionSource<object?>? finish = new();
            seccondArgument.SetResult((in2, options, finish));
            await finish.Task.ConfigureAwait(false);
            argumentCompletion.TryRemove(options, out _);
        }

    }


    public class OptionToken : IEquatable<OptionToken>
    {
        private readonly GenerationOptions root;

        public bool RefreshRemoteSources => root.Refresh;
        public bool BreakOnError => root.BreakOnError;

        public bool CheckUniqueID => root.CheckUniqueID;

        public ImmutableArray<Guid> GenerationId { get; }

        public void TryBreak(IGeneratorContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (BreakOnError)
            {
                try
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                    else
                    {
                        System.Diagnostics.Debugger.Launch();
                    }
                }
                catch (Exception ex)
                {
                    context.Logger.Error($"Faild to lunch debugger {ex}");
                }
            }
        }

        internal OptionToken(GenerationOptions root)
        {
            GenerationId = ImmutableArray.Create(Guid.NewGuid());
            this.root = root;
        }
        private OptionToken(OptionToken parent)
        {
            GenerationId = parent.GenerationId.Add(Guid.NewGuid());
            root = parent.root;
        }
        public OptionToken CreateSubToken()
        {
            return new OptionToken(this);
        }
        public override bool Equals(object? obj)
        {
            return obj is OptionToken token && Equals(token);
        }

        public bool Equals([AllowNull] OptionToken? other)
        {
            if (other is null)
            {
                return false;
            }

            return GenerationId.SequenceEqual(other.GenerationId);
        }
        public bool IsSuperTokenOf(OptionToken other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return other.IsSubTokenOf(this);
        }
        public bool IsSubTokenOf(OptionToken other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (GenerationId.Length > other.GenerationId.Length)
            {
                for (int i = 0; i < other.GenerationId.Length; i++)
                {
                    if (other.GenerationId[i] != GenerationId[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            HashCode hash = new();
            for (int i = 0; i < GenerationId.Length; i++)
            {
                hash.Add(GenerationId[i]);
            }

            return hash.ToHashCode();
        }

        public bool HaveSameRoot(OptionToken options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return options.GenerationId[0] == GenerationId[0];
        }

        public static bool operator ==(OptionToken? left, OptionToken? right)
        {
            if (left is null && right is null)
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(OptionToken? left, OptionToken? right)
        {
            return !(left == right);
        }
    }

}