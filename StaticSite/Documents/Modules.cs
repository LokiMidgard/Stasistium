using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace StaticSite.Documents
{
    public delegate Task<ModuleResult<TResult, TCache>> ModulePerformHandler<TResult, TCache>([AllowNull] BaseCache cache, OptionToken options);

    public static class Modules
    {
        public static GitModule<T> GitModul<T>(this ModuleBase<string, T> input)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new GitModule<T>(input.DoIt, input.Context);
        }

        public static GitRefToFiles<T> GitRefToFiles<T>(this ModuleBase<GitRef, T> input)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new GitRefToFiles<T>(input.DoIt, input.Context);
        }

        public static WhereModule<TCheck, TPreviousCache> Where<TCheck, TPreviousCache>(this ModuleBase<ImmutableList<TCheck>, TPreviousCache> input, Func<TCheck, Task<bool>> predicate, Func<TCheck, string> hashFunction)
            where TPreviousCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            if (hashFunction is null)
                throw new ArgumentNullException(nameof(hashFunction));
            return new WhereModule<TCheck, TPreviousCache>(input.DoIt, predicate, hashFunction, input.Context);
        }
        public static SingleModule<TCheck, TPreviousCache> Single<TCheck, TPreviousCache>(this ModuleBase<ImmutableList<TCheck>, TPreviousCache> input, Func<TCheck, Task<string>> hashFunction)
            where TPreviousCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (hashFunction is null)
                throw new ArgumentNullException(nameof(hashFunction));
            return new SingleModule<TCheck, TPreviousCache>(input.DoIt, hashFunction, input.Context);
        }
        public static SelectModule<TIn, TOut, TPreviousCache> Select<TIn, TOut, TPreviousCache>(this ModuleBase<ImmutableList<TIn>, TPreviousCache> input, Func<TIn, Task<TOut>> predicate, Func<TOut, Task<ReadOnlyMemory<byte>>> hashFunction)
            where TPreviousCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            if (hashFunction is null)
                throw new ArgumentNullException(nameof(hashFunction));
            return new SelectModule<TIn, TOut, TPreviousCache>(input.DoIt, predicate, hashFunction, input.Context);
        }

        public static ModuleBase<TResult, TResult> FromResult<TResult>(TResult result, GeneratorContext context)
            where TResult : class
            => new StaticModule<TResult>(result, context);

        private class StaticModule<T> : ModuleBase<T, T>
            where T : class
        {
            private readonly T result;

            public StaticModule(T result, GeneratorContext context) : base(context)
            {
                this.result = result;

            }

            protected override Task<ModuleResult<T, T>> Do([AllowNull] BaseCache<T> cache, OptionToken options)
            {
                return Task.FromResult(ModuleResult.Create(LazyTask.Create(() => (this.result, new BaseCache<T>(this.result, ReadOnlyMemory<BaseCache>.Empty))), !Equals(cache, this.result)));

            }
        }

    }

    public class WhereModule<TCheck, TPreviousCache> : ModuleBase<ImmutableList<TCheck>, string[]>
    {
        public WhereModule(ModulePerformHandler<ImmutableList<TCheck>, TPreviousCache> input, Func<TCheck, Task<bool>> predicate, Func<TCheck, string> hashFunction, GeneratorContext context) : base(context)
        {
            this.input = input ?? throw new ArgumentNullException(nameof(input));
            this.predicate = predicate;
            this.hashFunction = hashFunction;

        }

        private readonly ModulePerformHandler<ImmutableList<TCheck>, TPreviousCache> input;
        private readonly Func<TCheck, Task<bool>> predicate;
        private readonly Func<TCheck, string> hashFunction;


        protected override async Task<ModuleResult<ImmutableList<TCheck>, string[]>> Do([AllowNull] BaseCache<string[]> cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 1)
                throw new ArgumentException($"This cache should have exactly one predecessor but had {cache.PreviousCache}");
            var inputResult = await this.input(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {
                var previousPerform = await inputResult.Perform;
                var source = previousPerform.result;

                var filted = (await Task.WhenAll(source.Select(async x => (value: x, pass: await this.predicate(x).ConfigureAwait(false)))).ConfigureAwait(false)).Where(x => x.pass).Select(x => x.value).ToImmutableList();

                var hashArray = filted.Select(this.hashFunction).ToArray();

                return (result: filted, cache: BaseCache.Create(hashArray, previousPerform.cache));
            });


            bool hasChanges = inputResult.HasChanges;

            if (hasChanges || options.Refresh)
            {
                // if we have changes we'll check if there are acall changes.
                // since the task is cached in LazyTask, we will NOT perform the work twice.
                var result = await task;
                if (cache is null)
                    hasChanges = true;
                else
                    hasChanges = !HahsEqual(cache.Item, result.cache.Item);

            }

            return ModuleResult.Create(task, hasChanges);
        }


        private static bool HahsEqual(string[] x, string[] y)
        {
            if (x.Length != y.Length)
                return false;

            for (int i = 0; i < x.Length; i++)
            {
                var x2 = x[i];
                var y2 = y[i];

                if (!x2.Equals(y2))
                    return false;
            }
            return true;
        }
    }

    public class SelectModule<TIn, TOut, TPreviousCache> : ModuleBase<ImmutableList<TOut>, ReadOnlyMemory<byte>[]>
    {
        public SelectModule(ModulePerformHandler<ImmutableList<TIn>, TPreviousCache> input, Func<TIn, Task<TOut>> selector, Func<TOut, Task<ReadOnlyMemory<byte>>> hashFunction, GeneratorContext context) : base(context)
        {
            this.input = input ?? throw new ArgumentNullException(nameof(input));
            this.predicate = selector;
            this.hashFunction = hashFunction;

        }

        private readonly ModulePerformHandler<ImmutableList<TIn>, TPreviousCache> input;
        private readonly Func<TIn, Task<TOut>> predicate;
        private readonly Func<TOut, Task<ReadOnlyMemory<byte>>> hashFunction;


        protected override async Task<ModuleResult<ImmutableList<TOut>, ReadOnlyMemory<byte>[]>> Do([AllowNull] BaseCache<ReadOnlyMemory<byte>[]> cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 1)
                throw new ArgumentException($"This cache should have exactly one predecessor but had {cache.PreviousCache}");
            var inputResult = await this.input(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {
                var previousPerform = await inputResult.Perform;
                var source = previousPerform.result;

                var filted = (await Task.WhenAll(source.Select(this.predicate)).ConfigureAwait(false)).ToImmutableList();

                var hashArray = await Task.WhenAll(filted.Select(this.hashFunction)).ConfigureAwait(false);

                return (result: filted, cache: BaseCache.Create(hashArray, previousPerform.cache));
            });


            bool hasChanges = inputResult.HasChanges;

            if (hasChanges || options.Refresh)
            {
                // if we have changes we'll check if there are acall changes.
                // since the task is cached in LazyTask, we will NOT perform the work twice.
                var result = await task;
                if (cache is null)
                    hasChanges = true;
                else
                    hasChanges = !HahsEqual(cache.Item, result.cache.Item);

            }

            return ModuleResult.Create(task, hasChanges);
        }


        private static bool HahsEqual(ReadOnlyMemory<byte>[] x, ReadOnlyMemory<byte>[] y)
        {
            if (x.Length != y.Length)
                return false;
            var y1 = y;
            var x1 = x;

            for (int i = 0; i < x.Length; i++)
            {
                var x2 = x1[i].Span;
                var y2 = y1[i].Span;

                if (x2.Length != y2.Length)
                    return false;

                for (int j = 0; j < x2.Length; j++)
                {
                    if (x2[j] != y2[j])
                        return false;
                }
            }

            return true;
        }


    }

    public class SingleModule<TIn, TPreviousCache> : ModuleBase<TIn, string>
    {
        public SingleModule(ModulePerformHandler<ImmutableList<TIn>, TPreviousCache> input, Func<TIn, Task<string>> hashFunction, GeneratorContext context) : base(context)
        {
            this.input = input ?? throw new ArgumentNullException(nameof(input));
            this.hashFunction = hashFunction;

        }

        private readonly ModulePerformHandler<ImmutableList<TIn>, TPreviousCache> input;
        private readonly Func<TIn, Task<string>> hashFunction;


        protected override async Task<ModuleResult<TIn, string>> Do([AllowNull] BaseCache<string> cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 1)
                throw new ArgumentException($"This cache should have exactly one predecessor but had {cache.PreviousCache}");
            var inputResult = await this.input(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {
                var previousPerform = await inputResult.Perform;
                var source = previousPerform.result;

                var filted = source.Single();
                var hashArray = await this.hashFunction(filted).ConfigureAwait(false);

                return (result: filted, cache: BaseCache.Create(hashArray, previousPerform.cache));
            });


            bool hasChanges = inputResult.HasChanges;

            if (hasChanges || options.Refresh)
            {
                // if we have changes we'll check if there are acall changes.
                // since the task is cached in LazyTask, we will NOT perform the work twice.
                var result = await task;
                if (cache is null)
                    hasChanges = true;
                else
                    hasChanges = !Equals(cache.Item, result.cache.Item);

            }

            return ModuleResult.Create(task, hasChanges);
        }
    }


}
