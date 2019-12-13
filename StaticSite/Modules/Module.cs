using StaticSite.Documents;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace StaticSite.Modules
{
    public delegate Task<ModuleResult<TResult, TCache>> ModulePerformHandler<TResult, TCache>([AllowNull] BaseCache cache, OptionToken options);
    public delegate Task<ModuleResultList<TResult, TResultCache, TCache>> ModulePerformHandler<TResult, TResultCache, TCache>([AllowNull] BaseCache cache, OptionToken options);


    public static class Module
    {

        public static PersistStage<TItemCache, TCache> Persist<TItemCache, TCache>(this MultiModuleBase<System.IO.Stream, TItemCache, TCache> stage, System.IO.DirectoryInfo output, GenerationOptions generatorOptions)
            where TCache : class
        {
            if (stage is null)
                throw new ArgumentNullException(nameof(stage));
            if (output is null)
                throw new ArgumentNullException(nameof(output));
            if (generatorOptions is null)
                throw new ArgumentNullException(nameof(generatorOptions));
            return new PersistStage<TItemCache, TCache>(stage.DoIt, output, generatorOptions, stage.Context);
        }

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

        public static MarkdownStreamModule<T> Markdown<T>(this ModuleBase<System.IO.Stream, T> input)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new MarkdownStreamModule<T>(input.DoIt, input.Context);
        }
        public static MarkdownStringModule<T> Markdown<T>(this ModuleBase<string, T> input)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new MarkdownStringModule<T>(input.DoIt, input.Context);
        }

        public static WhereModule<TCheck, TPreviousItemCache, TPreviousCache> Where<TCheck, TPreviousItemCache, TPreviousCache>(this MultiModuleBase<TCheck, TPreviousItemCache, TPreviousCache> input, Func<IDocument<TCheck>, Task<bool>> predicate)
            where TPreviousCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            return new WhereModule<TCheck, TPreviousItemCache, TPreviousCache>(input.DoIt, predicate, input.Context);
        }
        public static WhereModule<TCheck, TPreviousItemCache, TPreviousCache> Where<TCheck, TPreviousItemCache, TPreviousCache>(this MultiModuleBase<TCheck, TPreviousItemCache, TPreviousCache> input, Func<IDocument<TCheck>, bool> predicate)
            where TPreviousCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            return new WhereModule<TCheck, TPreviousItemCache, TPreviousCache>(input.DoIt, x => Task.FromResult(predicate(x)), input.Context);
        }
        public static SingleModule<TCheck, TPreviousItemCache, TPreviousCache> Single<TCheck, TPreviousItemCache, TPreviousCache>(this MultiModuleBase<TCheck, TPreviousItemCache, TPreviousCache> input)
            where TPreviousCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new SingleModule<TCheck, TPreviousItemCache, TPreviousCache>(input.DoIt, input.Context);
        }
        public static SelectModule<TIn, TInITemCache, TInCache, TOut> Select<TIn, TInITemCache, TInCache, TOut>(this MultiModuleBase<TIn, TInITemCache, TInCache> input, Func<IDocument<TIn>, Task<IDocument<TOut>>> predicate)
            where TInCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            return new SelectModule<TIn, TInITemCache, TInCache, TOut>(input.DoIt, predicate, input.Context);
        }

        public static StaticModule<TResult> FromResult<TResult>(TResult result, Func<TResult, string> hashFunction, GeneratorContext context)
            => new StaticModule<TResult>(result, hashFunction, context);
    }

    public class StaticModule<TResult> : ModuleBase<TResult, string>
    {
        private readonly string id = Guid.NewGuid().ToString();
        private readonly Func<TResult, string> hashFunction;
        public TResult Value { get; set; }

        public StaticModule(TResult result, Func<TResult, string> hashFunction, GeneratorContext context) : base(context)
        {
            this.Value = result;
            this.hashFunction = hashFunction ?? throw new ArgumentNullException(nameof(hashFunction));
        }

        protected override Task<ModuleResult<TResult, string>> DoInternal([AllowNull] BaseCache<string>? cache, OptionToken options)
        {
            var contentHash = this.hashFunction(this.Value);
            return Task.FromResult(ModuleResult.Create(
                perform: LazyTask.Create(() => (this.Context.Create(this.Value, contentHash, this.id), BaseCache.Create(contentHash, ReadOnlyMemory<BaseCache>.Empty))),
                hasChanges: cache == null || !Equals(cache.Item, contentHash),
                documentId: this.id));
        }
    }


    public class WhereModule<TCheck, TPreviousItemCache, TPreviousCache> : OutputMultiInputSingle0List1ModuleBase<TCheck, TPreviousItemCache, TPreviousCache, TCheck, TPreviousItemCache, ImmutableList<string>>
    {
        private readonly Func<IDocument<TCheck>, Task<bool>> predicate;

        public WhereModule(ModulePerformHandler<TCheck, TPreviousItemCache, TPreviousCache> inputList0, Func<IDocument<TCheck>, Task<bool>> predicate, GeneratorContext context, bool updateOnRefresh = false) : base(inputList0, context, updateOnRefresh)
        {
            this.predicate = predicate;
        }

        protected override async Task<(ImmutableList<ModuleResult<TCheck, TPreviousItemCache>> result, BaseCache<ImmutableList<string>> cache)> Work(ModuleResultList<TCheck, TPreviousItemCache, TPreviousCache> inputList0, [AllowNull] ImmutableList<string> cache, [AllowNull] ImmutableDictionary<string, BaseCache<TPreviousItemCache>> childCaches, OptionToken options)
        {
            if (inputList0 is null)
                throw new ArgumentNullException(nameof(inputList0));
            var input = await inputList0.Perform;

            await Task.WhenAll(input.result.Where(x => x.HasChanges).Select(async x => await x.Perform)).ConfigureAwait(false);


            var list = await Task.WhenAll(input.result.Select(async item =>
            {
                bool pass;

                if (item.HasChanges)
                {
                    var (result, itemCache) = await item.Perform;
                    pass = await this.predicate(result).ConfigureAwait(false);
                }
                else
                {
                    // since HasChanges if false, it was present in the last invocation
                    // if it is passed this it was added to thec cache otherwise not.
                    pass = childCaches.ContainsKey(item.Id);
                }
                // where seems to be a little special. HasChanges will not be set if the result of the predicate changed
                // otherwise if a document has changes but gets not filtered and was not filtered last time, it would not have
                // changes set.
                // If filtering chages this should be part of the Cache not ChildCache
                if (pass)
                    return item;
                else
                    return null;
            })).ConfigureAwait(false);



            return (list.Where(x => x != null).ToImmutableList(), BaseCache.Create(list.Where(x => x != null).Select(x => x.Id).ToImmutableList(), input.cache));
        }

    }

    public class SelectModule<TIn, TInItemCache, TInCache, TOut> : OutputMultiInputSingle0List1ModuleBase<TIn, TInItemCache, TInCache, TOut, string, ImmutableList<string>>
    {

        private readonly Func<IDocument<TIn>, Task<IDocument<TOut>>> predicate;

        public SelectModule(ModulePerformHandler<TIn, TInItemCache, TInCache> inputList0, Func<IDocument<TIn>, Task<IDocument<TOut>>> selector, GeneratorContext context, bool updateOnRefresh = false) : base(inputList0, context, updateOnRefresh)
        {
            this.predicate = selector;
        }


        protected override async Task<(ImmutableList<ModuleResult<TOut, string>> result, BaseCache<ImmutableList<string>> cache)> Work(ModuleResultList<TIn, TInItemCache, TInCache> inputList0, [AllowNull] ImmutableList<string> cache, [AllowNull] ImmutableDictionary<string, BaseCache<string>> childCaches, OptionToken options)
        {
            if (inputList0 is null)
                throw new ArgumentNullException(nameof(inputList0));
            var input = await inputList0.Perform;

            var list = await Task.WhenAll(input.result.Select(async item =>
            {
                if (item.HasChanges)
                {
                    var newSource = await item.Perform;
                    var transformed = await this.predicate(newSource.result).ConfigureAwait(false);

                    var hasChanges = true;
                    if (childCaches != null && childCaches.TryGetValue(transformed.Id, out var oldHash))
                        hasChanges = oldHash.Item != transformed.Hash;
                    return ModuleResult.Create(transformed, BaseCache.Create(transformed.Hash, newSource.cache), hasChanges, transformed.Id);
                }
                else
                {
                    return ModuleResult.Create(LazyTask.Create(async () =>
                    {

                        var newSource = await item.Perform;
                        var transformed = await this.predicate(newSource.result).ConfigureAwait(false);

                        return (transformed, BaseCache.Create(transformed.Hash, newSource.cache));
                    }), false, item.Id);
                }
            })).ConfigureAwait(false);
            return (list.ToImmutableList(), BaseCache.Create(list.Select(x => x.Id).ToImmutableList(), input.cache));
        }

    }

    public class SingleModule<TIn, TPreviousItemCache, TPreviousCache> : OutputSingleInputSingle0List1ModuleBase<TIn, TPreviousItemCache, TPreviousCache, TIn, string>
    {
        public SingleModule(ModulePerformHandler<TIn, TPreviousItemCache, TPreviousCache> input, GeneratorContext context) : base(input, context)
        {
        }


        protected override async Task<(IDocument<TIn> result, BaseCache<string> cache)> Work(ModuleResultList<TIn, TPreviousItemCache, TPreviousCache> inputList0, OptionToken options)
        {
            if (inputList0 is null)
                throw new ArgumentNullException(nameof(inputList0));
            var (result, cache) = await inputList0.Perform;

            if (result.Count != 1)
                throw this.Context.Exception($"There should only be one Document but where {result.Count}");
            var element = await result[0].Perform;
            return (element.result, BaseCache.Create(element.result.Hash, cache));
        }
    }


}
