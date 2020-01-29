using Stasistium.Documents;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using Stasistium.Core;
using Stasistium.Stages;

namespace Stasistium.Stages
{
    public class SelectStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache> : MultiStageBase<TResult, TItemCache, SelectCache<TInputCache, TItemCache>>
    where TItemCache : class
    where TInputItemCache : class
    where TInputCache : class
    {

        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, (Start @in, StageBase<TResult, TItemCache> @out)> startLookup = new System.Collections.Concurrent.ConcurrentDictionary<string, (Start @in, StageBase<TResult, TItemCache> @out)>();

        private readonly Func<StageBase<TInput, StartCache<TInputCache>>, StageBase<TResult, TItemCache>> createPipline;

        private readonly StagePerformHandler<TInput, TInputItemCache, TInputCache> input;

        public SelectStage(StagePerformHandler<TInput, TInputItemCache, TInputCache> input, Func<StageBase<TInput, StartCache<TInputCache>>, StageBase<TResult, TItemCache>> createPipline, IGeneratorContext context, string? name = null) : base(context, name)
        {
            this.input = input ?? throw new ArgumentNullException(nameof(input));
            this.createPipline = createPipline ?? throw new ArgumentNullException(nameof(createPipline));
        }

        protected override async Task<StageResultList<TResult, TItemCache, SelectCache<TInputCache, TItemCache>>> DoInternal([AllowNull] SelectCache<TInputCache, TItemCache>? cache, OptionToken options)
        {
            var input = await this.input(cache?.PreviousCache, options).ConfigureAwait(false);

            var task = LazyTask.Create(async () =>
            {
                var inputResult = await input.Perform;
                var inputCache = input.Cache;
                var resultList = await Task.WhenAll(inputResult.Select(async item =>
                {

                    var pipe = this.startLookup.GetOrAdd(item.Id, id =>
                    {
                        var start = new Start(this, id, this.Context);
                        var end = this.createPipline(start);
                        return (start, end);
                    });

                    if (cache == null || !cache.InputItemCacheLookup.TryGetValue(item.Id, out TItemCache? lastCache) || !cache.InputItemHashLookup.TryGetValue(item.Id, out string? lastHash) || !cache.InputItemOutputIdLookup.TryGetValue(item.Id, out string? lastOutputId))
                    {
                        lastCache = null;
                        lastHash = null;
                        lastOutputId = null;
                    }
                    var pipeDone = await pipe.@out.DoIt(lastCache, options).ConfigureAwait(false);

                    if (pipeDone.HasChanges)
                    {
                        var itemResult = await pipeDone.Perform;
                        var itemCache = pipeDone.Cache;
                        return (result: StageResult.Create(itemResult, itemResult.Hash != lastHash, itemResult.Id, itemCache), lastCache: itemCache, lastHash: itemResult.Hash, inputId: item.Id);
                    }
                    else
                    {
                        if (lastOutputId is null)
                            throw new InvalidOperationException("This should not happen.");

                        return (result: StageResult.Create(pipeDone.Perform, false, lastOutputId, pipeDone.Cache), lastCache: lastCache, lastHash: lastHash, inputId: item.Id);

                    }
                })).ConfigureAwait(false);



                var newCache = new SelectCache<TInputCache, TItemCache>()
                {
                    InputItemCacheLookup = resultList.ToDictionary(x => x.inputId, x => x.lastCache),
                    InputItemHashLookup = resultList.ToDictionary(x => x.inputId, x => x.lastHash),
                    InputItemOutputIdLookup = resultList.ToDictionary(x => x.inputId, x => x.result.Id),
                    OutputIdOrder = resultList.Select(x => x.result.Id).ToArray(),
                    PreviousCache = inputCache
                };


                return (resultList.Select(x => x.result).ToImmutableList(), newCache);
            });

            var hasChanges = input.HasChanges;
            var ids = cache?.OutputIdOrder;
            if (hasChanges || ids is null)
            {
                var (work, newCache) = await task;

                ids = newCache.OutputIdOrder;

                if (cache != null)
                {
                    hasChanges = !newCache.OutputIdOrder.SequenceEqual(cache.OutputIdOrder) || work.Any(x => x.HasChanges);
                }

                return StageResultList.Create(work, hasChanges, ids.ToImmutableList(), newCache);
            }
            else
            {
                var actualTask = LazyTask.Create(async () =>
                {
                    var temp = await task;
                    return temp.Item1;
                });

                return StageResultList.Create(actualTask, hasChanges, ids.ToImmutableList(), cache);
            }
        }

        private class Start : StageBase<TInput, StartCache<TInputCache>>
        {
            private readonly SelectStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache> parent;

            private readonly string key;

            public Start(SelectStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache> parent, string key, IGeneratorContext context, string? name = null) : base(context, name)
            {
                this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
                this.key = key;
            }


            protected override async Task<StageResult<TInput, StartCache<TInputCache>>> DoInternal([AllowNull] StartCache<TInputCache>? cache, OptionToken options)
            {
                var input = await this.parent.input(cache?.PreviousCache, options).ConfigureAwait(false);

                var task = LazyTask.Create(async () =>
                {
                    var inputResult = await input.Perform;
                    var inputCache = input.Cache;
                    var current = inputResult.Single(x => x.Id == this.key);
                    var subResult = await current.Perform;

                    var newCache = new StartCache<TInputCache>()
                    {
                        PreviousCache = inputCache,
                        Id = subResult.Id,
                        Hash = subResult.Hash
                    };
                    return (result: subResult, newCache);
                });

                string id;

                bool hasChanges = input.HasChanges;
                if (hasChanges || cache is null)
                {
                    var list = await input.Perform;
                    var current = list.Single(x => x.Id == this.key);
                    if (current.HasChanges || cache is null)
                    {
                        var result = await task;
                        id = result.result.Id;
                        hasChanges = cache is null || result.result.Hash != cache.Hash;
                        return StageResult.Create(result.result, hasChanges, id, result.newCache);
                    }
                    else
                    {
                        id = cache.Id;
                    }




                }
                else
                {
                    id = cache.Id;
                }

                var actualTask = LazyTask.Create(async () =>
                {
                    var tmep = await task;
                    return tmep.result;
                });

                return StageResult.Create(actualTask, hasChanges, id, cache);
            }


        }

    }

    public class SelectStage<TInput1, TInputItemCache1, TInputCache1, TInput2, TInputCache2, TResult, TItemCache> : MultiStageBase<TResult, TItemCache, SelectCache<TInputCache1, TInputCache2, TItemCache>>
where TItemCache : class
where TInputItemCache1 : class
where TInputCache1 : class
where TInputCache2 : class
    {

        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, (Start @in, StageBase<TResult, TItemCache> @out)> startLookup = new System.Collections.Concurrent.ConcurrentDictionary<string, (Start @in, StageBase<TResult, TItemCache> @out)>();

        private readonly Func<StageBase<TInput1, StartCache<TInputCache1>>, StageBase<TInput2, TInputCache2>, StageBase<TResult, TItemCache>> createPipline;

        private readonly StagePerformHandler<TInput1, TInputItemCache1, TInputCache1> input;
        private readonly StageBase<TInput2, TInputCache2> input2;

        public SelectStage(StagePerformHandler<TInput1, TInputItemCache1, TInputCache1> input, StageBase<TInput2, TInputCache2> input2, Func<StageBase<TInput1, StartCache<TInputCache1>>, StageBase<TInput2, TInputCache2>, StageBase<TResult, TItemCache>> createPipline, IGeneratorContext context, string? name = null) : base(context, name)
        {
            this.input = input ?? throw new ArgumentNullException(nameof(input));
            this.input2 = input2 ?? throw new ArgumentNullException(nameof(input2));
            this.createPipline = createPipline ?? throw new ArgumentNullException(nameof(createPipline));
        }

        protected override async Task<StageResultList<TResult, TItemCache, SelectCache<TInputCache1, TInputCache2, TItemCache>>> DoInternal([AllowNull] SelectCache<TInputCache1, TInputCache2, TItemCache>? cache, OptionToken options)
        {
            var input = await this.input(cache?.PreviousCache, options).ConfigureAwait(false);
            var input2 = await this.input2.DoIt(cache?.PreviousCache2, options).ConfigureAwait(false);

            var task = LazyTask.Create(async () =>
            {
                var inputResult = await input.Perform;
                var inputResult2 = await input2.Perform;
                var inputCache = input.Cache;
                var inputCache2 = input2.Cache;

                var resultList = await Task.WhenAll(inputResult.Select(async item =>
                {

                    var pipe = this.startLookup.GetOrAdd(item.Id, id =>
                    {
                        var start = new Start(this, id, this.Context);
                        var end = this.createPipline(start, this.input2);
                        return (start, end);
                    });

                    if (cache == null || !cache.InputItemCacheLookup.TryGetValue(item.Id, out TItemCache? lastCache) || !cache.InputItemHashLookup.TryGetValue(item.Id, out string? lastHash) || !cache.InputItemOutputIdLookup.TryGetValue(item.Id, out string? lastOutputId))
                    {
                        lastCache = null;
                        lastHash = null;
                        lastOutputId = null;
                    }
                    var pipeDone = await pipe.@out.DoIt(lastCache, options).ConfigureAwait(false);

                    if (pipeDone.HasChanges)
                    {
                        var itemResult = await pipeDone.Perform;
                        var itemCache = pipeDone.Cache;
                        return (result: StageResult.Create(itemResult, itemResult.Hash != lastHash, itemResult.Id, itemCache), lastCache: itemCache, lastHash: itemResult.Hash, inputId: item.Id);
                    }
                    else
                    {
                        if (lastOutputId is null)
                            throw new InvalidOperationException("This should not happen.");

                        return (result: StageResult.Create(pipeDone.Perform, false, lastOutputId, pipeDone.Cache), lastCache: lastCache, lastHash: lastHash, inputId: item.Id);

                    }
                })).ConfigureAwait(false);



                var newCache = new SelectCache<TInputCache1, TInputCache2, TItemCache>()
                {
                    InputItemCacheLookup = resultList.ToDictionary(x => x.inputId, x => x.lastCache),
                    InputItemHashLookup = resultList.ToDictionary(x => x.inputId, x => x.lastHash),
                    InputItemOutputIdLookup = resultList.ToDictionary(x => x.inputId, x => x.result.Id),
                    OutputIdOrder = resultList.Select(x => x.result.Id).ToArray(),
                    PreviousCache = inputCache,
                    PreviousCache2 = inputCache2
                };


                return (resultList.Select(x => x.result).ToImmutableList(), newCache);
            });

            var hasChanges = input.HasChanges || input2.HasChanges;
            var ids = cache?.OutputIdOrder;
            if (hasChanges || ids is null || cache is null)
            {
                var (work, newCache) = await task;

                ids = newCache.OutputIdOrder;

                if (cache != null)
                {
                    hasChanges = !newCache.OutputIdOrder.SequenceEqual(cache.OutputIdOrder) || work.Any(x => x.HasChanges);
                }

                return StageResultList.Create(work, hasChanges, ids.ToImmutableList(), newCache);
            }
            var actualTask = LazyTask.Create(async () =>
            {
                var temp = await task;
                return temp.Item1;
            });
            return StageResultList.Create(actualTask, hasChanges, ids.ToImmutableList(), cache);
        }





        private class Start : StageBase<TInput1, StartCache<TInputCache1>>
        {
            private readonly SelectStage<TInput1, TInputItemCache1, TInputCache1, TInput2, TInputCache2, TResult, TItemCache> parent;

            private readonly string key;

            public Start(SelectStage<TInput1, TInputItemCache1, TInputCache1, TInput2, TInputCache2, TResult, TItemCache> parent, string key, IGeneratorContext context, string? name = null) : base(context, name)
            {
                this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
                this.key = key;
            }


            protected override async Task<StageResult<TInput1, StartCache<TInputCache1>>> DoInternal([AllowNull] StartCache<TInputCache1>? cache, OptionToken options)
            {
                var input = await this.parent.input(cache?.PreviousCache, options).ConfigureAwait(false);

                var task = LazyTask.Create(async () =>
                {
                    var inputResult = await input.Perform;
                    var inputCache = input.Cache;
                    var current = inputResult.Single(x => x.Id == this.key);
                    var subResult = await current.Perform;

                    var newCache = new StartCache<TInputCache1>()
                    {
                        PreviousCache = inputCache,
                        Id = subResult.Id,
                        Hash = subResult.Hash
                    };
                    return (result: subResult, newCache);
                });

                string id;

                bool hasChanges = input.HasChanges;
                if (hasChanges || cache is null)
                {
                    var list = await input.Perform;
                    var current = list.Single(x => x.Id == this.key);
                    if (current.HasChanges || cache is null)
                    {
                        var result = await task;
                        var newCache = current.Cache;
                        id = result.result.Id;
                        hasChanges = cache is null || result.result.Hash != cache.Hash;
                        return StageResult.Create(result.result, hasChanges, id, result.newCache);

                    }
                    else
                    {
                        id = cache.Id;
                    }




                }
                else
                {
                    id = cache.Id;
                }
                var actualTask = LazyTask.Create(async () =>
                {
                    var temp = await task;
                    return temp.result;
                });
                return StageResult.Create(actualTask, hasChanges, id, cache);
            }


        }
    }


    public class StartCache<TInputCache>
    {
        public TInputCache PreviousCache { get; set; }
        public string Id { get; set; }
        public string Hash { get; set; }
    }
}

namespace Stasistium
{
    public partial class StageExtensions
    {
        public static SelectStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache> Select<TInput, TInputItemCache, TInputCache, TResult, TItemCache>(this MultiStageBase<TInput, TInputItemCache, TInputCache> input, Func<StageBase<TInput, StartCache<TInputCache>>, StageBase<TResult, TItemCache>> createPipline, string? name = null)
    where TInputCache : class
    where TInputItemCache : class
    where TItemCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new SelectStage<TInput, TInputItemCache, TInputCache, TResult, TItemCache>(input.DoIt, createPipline, input.Context, name);
        }
        public static SelectStage<TInput, TInputItemCache, TInputCache, TInput2, TInputCache2, TResult, TItemCache> Select<TInput, TInputItemCache, TInputCache, TInput2, TInputCache2, TResult, TItemCache>(this MultiStageBase<TInput, TInputItemCache, TInputCache> input, StageBase<TInput2, TInputCache2> input2, Func<StageBase<TInput, StartCache<TInputCache>>, StageBase<TInput2, TInputCache2>, StageBase<TResult, TItemCache>> createPipline, string? name = null)
    where TInputCache : class
    where TInputCache2 : class
    where TInputItemCache : class
    where TItemCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new SelectStage<TInput, TInputItemCache, TInputCache, TInput2, TInputCache2, TResult, TItemCache>(input.DoIt, input2, createPipline, input.Context, name);
        }

    }
}