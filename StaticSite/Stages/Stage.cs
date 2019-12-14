using StaticSite.Documents;
using System;
using System.Linq;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.IO;

namespace StaticSite.Stages
{
    public delegate Task<StageResult<TResult, TCache>> StagePerformHandler<TResult, TCache>([AllowNull] BaseCache cache, OptionToken options);
    public delegate Task<StageResultList<TResult, TResultCache, TCache>> StagePerformHandler<TResult, TResultCache, TCache>([AllowNull] BaseCache cache, OptionToken options);


    public static class Stage
    {

        public static PersistStage<TItemCache, TCache> Persist<TItemCache, TCache>(this MultiStageBase<System.IO.Stream, TItemCache, TCache> stage, System.IO.DirectoryInfo output, GenerationOptions generatorOptions)
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

        public static GitStage<T> GitModul<T>(this StageBase<string, T> input)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new GitStage<T>(input.DoIt, input.Context);
        }

        public static GitRefToFilesStage<T> GitRefToFiles<T>(this StageBase<GitRef, T> input)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new GitRefToFilesStage<T>(input.DoIt, input.Context);
        }

        public static MarkdownStreamStage<T> Markdown<T>(this StageBase<System.IO.Stream, T> input)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new MarkdownStreamStage<T>(input.DoIt, input.Context);
        }
        public static MarkdownStringStage<T> Markdown<T>(this StageBase<string, T> input)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new MarkdownStringStage<T>(input.DoIt, input.Context);
        }

        public static WhereStage<TCheck, TPreviousItemCache, TPreviousCache> Where<TCheck, TPreviousItemCache, TPreviousCache>(this MultiStageBase<TCheck, TPreviousItemCache, TPreviousCache> input, Func<IDocument<TCheck>, Task<bool>> predicate)
            where TPreviousCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            return new WhereStage<TCheck, TPreviousItemCache, TPreviousCache>(input.DoIt, predicate, input.Context);
        }
        public static WhereStage<TCheck, TPreviousItemCache, TPreviousCache> Where<TCheck, TPreviousItemCache, TPreviousCache>(this MultiStageBase<TCheck, TPreviousItemCache, TPreviousCache> input, Func<IDocument<TCheck>, bool> predicate)
            where TPreviousCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            return new WhereStage<TCheck, TPreviousItemCache, TPreviousCache>(input.DoIt, x => Task.FromResult(predicate(x)), input.Context);
        }
        public static SingleStage<TCheck, TPreviousItemCache, TPreviousCache> SingleEntry<TCheck, TPreviousItemCache, TPreviousCache>(this MultiStageBase<TCheck, TPreviousItemCache, TPreviousCache> input)
            where TPreviousCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new SingleStage<TCheck, TPreviousItemCache, TPreviousCache>(input.DoIt, input.Context);
        }
        public static SelectStage<TIn, TInITemCache, TInCache, TOut> Select<TIn, TInITemCache, TInCache, TOut>(this MultiStageBase<TIn, TInITemCache, TInCache> input, Func<IDocument<TIn>, Task<IDocument<TOut>>> predicate)
            where TInCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            return new SelectStage<TIn, TInITemCache, TInCache, TOut>(input.DoIt, predicate, input.Context);
        }

        public static StaticStage<TResult> FromResult<TResult>(TResult result, Func<TResult, string> hashFunction, GeneratorContext context)
            => new StaticStage<TResult>(result, hashFunction, context);

        public static SidecarHelper<TPreviousItemCache, TPreviousListCache> Sidecar<TPreviousItemCache, TPreviousListCache>(this MultiStageBase<System.IO.Stream, TPreviousItemCache, TPreviousListCache> stage)
            where TPreviousListCache : class
        {
            return new SidecarHelper<TPreviousItemCache, TPreviousListCache>(stage);
        }
    }

    public class SidecarHelper<TPreviousItemCache, TPreviousListCache>
            where TPreviousListCache : class
    {
        private readonly MultiStageBase<Stream, TPreviousItemCache, TPreviousListCache> stage;

        public SidecarHelper(MultiStageBase<Stream, TPreviousItemCache, TPreviousListCache> stage)
        {
            this.stage = stage;
        }

        public SidecarMetadata<TMetadata, TPreviousItemCache, TPreviousListCache> For<TMetadata>(string extension, MetadataUpdate<TMetadata>? updateCallback = null)
        {
            return new SidecarMetadata<TMetadata, TPreviousItemCache, TPreviousListCache>(this.stage.DoIt, extension, updateCallback, this.stage.Context);
        }
    }

    public class SidecarMetadata<TMetadata, TPreviousItemCache, TPreviousListCache> : OutputMultiInputSingle0List1StageBase<System.IO.Stream, TPreviousItemCache, TPreviousListCache, System.IO.Stream, string, ImmutableList<string>>
    {
        private readonly MetadataUpdate<TMetadata>? update;

        public SidecarMetadata(StagePerformHandler<System.IO.Stream, TPreviousItemCache, TPreviousListCache> inputList0, string sidecarExtension, MetadataUpdate<TMetadata>? update, GeneratorContext context) : base(inputList0, context)
        {
            if (sidecarExtension is null)
                throw new ArgumentNullException(nameof(sidecarExtension));
            if (!sidecarExtension.StartsWith(".", StringComparison.InvariantCultureIgnoreCase))
                sidecarExtension = "." + sidecarExtension;
            this.SidecarExtension = sidecarExtension;
            this.update = update;
        }

        public string SidecarExtension { get; }

        protected override async Task<(ImmutableList<StageResult<System.IO.Stream, string>> result, BaseCache<ImmutableList<string>> cache)> Work(StageResultList<System.IO.Stream, TPreviousItemCache, TPreviousListCache> inputList0, [AllowNull] ImmutableList<string> cache, [AllowNull] ImmutableDictionary<string, BaseCache<string>>? childCaches, OptionToken options)
        {
            if (inputList0 is null)
                throw new ArgumentNullException(nameof(inputList0));
            var (result, inputCache) = await inputList0.Perform;

            var sidecarLookup = result.Where(x => System.IO.Path.GetExtension(x.Id) == this.SidecarExtension)
                .ToDictionary(x => System.IO.Path.Combine(System.IO.Path.GetDirectoryName(x.Id) ?? string.Empty, System.IO.Path.GetFileNameWithoutExtension(x.Id)));

            var files = result.Where(x => System.IO.Path.GetExtension(x.Id) != this.SidecarExtension);



            var list = await Task.WhenAll(files.Select(async file =>
           {
               if (sidecarLookup.TryGetValue(file.Id, out var sidecar) && (file.HasChanges || sidecar.HasChanges))
               {
                   var (fileResult, fileCache) = await file.Perform;
                   var (sidecarResult, sidecarCache) = await sidecar.Perform;



                   var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                       .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
                       .Build();

                   var oldMetadata = fileResult.Metadata;
                   MetadataContainer? newMetadata;
                   try
                   {
                       using var stream = sidecarResult.Value;
                       using var reader = new System.IO.StreamReader(stream);
                       var metadata = deserializer.Deserialize<TMetadata>(reader);

                       if (metadata != null)
                           if (this.update != null)
                               newMetadata = oldMetadata.AddOrUpdate(metadata.GetType(), metadata, (oldValue, newValue) => this.update((TMetadata)oldValue! /*AllowNull is set, so why the warnign?*/, (TMetadata)newValue));
                           else
                               newMetadata = oldMetadata.Add(metadata.GetType(), metadata);
                       else
                           newMetadata = null;
                   }
                   catch (YamlDotNet.Core.YamlException e) when (e.InnerException is null) // Hope that only happens when it does not match.
                   {
                       newMetadata = null;
                   }

                   if (newMetadata != null)
                       fileResult = fileResult.With(newMetadata);

                   var hasChanges = true;
                   if (childCaches != null && childCaches.TryGetValue(fileResult.Id, out var oldChildCache))
                       hasChanges = oldChildCache.Item != fileResult.Hash;

                   var childCache = BaseCache.Create(fileResult.Hash, new ReadOnlyMemory<BaseCache>(new BaseCache[] { fileCache, sidecarCache }));
                   return (result: StageResult.Create<System.IO.Stream, string>(fileResult, childCache, hasChanges, fileResult.Id), childCache);
               }
               else if (file.HasChanges)
               {
                   var (fileResult, fileCache) = await file.Perform;
                   var hasChanges = true;
                   if (childCaches != null && childCaches.TryGetValue(fileResult.Id, out var oldChildCache))
                       hasChanges = oldChildCache.Item != fileResult.Hash;
                   System.Diagnostics.Debug.Assert(hasChanges); // if the original file had changes so must this have.
                   var childCache = BaseCache.Create(fileResult.Hash, new ReadOnlyMemory<BaseCache>(new BaseCache[] { fileCache }));
                   return (result: StageResult.Create<System.IO.Stream, string>(fileResult, childCache, hasChanges, fileResult.Id), childCache);
               }
               else
               {
                   var task = LazyTask.Create(async () =>
                   {
                       var (fileResult, fileCache) = await file.Perform;
                       var hasChanges = true;
                       if (childCaches != null && childCaches.TryGetValue(fileResult.Id, out var oldChildCache))
                           hasChanges = oldChildCache.Item != fileResult.Hash;

                       return (fileResult, BaseCache.Create(fileResult.Hash, fileCache));
                   });
                   if (childCaches is null || !childCaches.TryGetValue(file.Id, out var childCache))
                       throw this.Context.Exception("The previous cache should exist if we had no changes.");

                   return (result: StageResult.Create(task, false, file.Id), childCache);
               }
           })).ConfigureAwait(false);

            return (list.Select(x => x.result).ToImmutableList(), BaseCache.Create(list.Select(x => x.result.Id).ToImmutableList(), inputCache, list.ToImmutableDictionary(x => x.result.Id, x => x.childCache as BaseCache)));
        }
    }


}
