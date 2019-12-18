using StaticSite.Documents;
using System;
using System.Linq;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.IO;

namespace StaticSite.Stages
{
    public class SidecarMetadata<TMetadata, TInItemCache, TInCache> : MultiStageBase<Stream, string, SelectStageCache<TInCache>>//: OutputMultiInputSingle0List1StageBase<Stream, TPreviousItemCache, TPreviousListCache, Stream, string, ImmutableList<string>>
        where TInCache : class
        where TInItemCache : class
    {
        private readonly StagePerformHandler<Stream, TInItemCache, TInCache> input;
        private readonly MetadataUpdate<TMetadata>? update;

        public SidecarMetadata(StagePerformHandler<Stream, TInItemCache, TInCache> input, string sidecarExtension, MetadataUpdate<TMetadata>? update, GeneratorContext context) : base(context)
        {
            if (sidecarExtension is null)
                throw new ArgumentNullException(nameof(sidecarExtension));
            if (!sidecarExtension.StartsWith(".", StringComparison.InvariantCultureIgnoreCase))
                sidecarExtension = "." + sidecarExtension;
            this.input = input ?? throw new ArgumentNullException(nameof(input));
            this.SidecarExtension = sidecarExtension;
            this.update = update;
        }

        public string SidecarExtension { get; }

        protected override async Task<StageResultList<Stream, string, SelectStageCache<TInCache>>> DoInternal([AllowNull] SelectStageCache<TInCache>? cache, OptionToken options)
        {
            var input = await this.input(cache?.ParentCache, options).ConfigureAwait(false);

            var task = LazyTask.Create(async () =>
            {
                var inputList = await input.Perform;

                var sidecarLookup = inputList.result.Where(x => Path.GetExtension(x.Id) == this.SidecarExtension)
                    .ToDictionary(x => Path.Combine(Path.GetDirectoryName(x.Id) ?? string.Empty, Path.GetFileNameWithoutExtension(x.Id)));

                var files = inputList.result.Where(x => Path.GetExtension(x.Id) != this.SidecarExtension);


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
                            using var reader = new StreamReader(stream);
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
                        if (cache != null && cache.Transformed.TryGetValue(fileResult.Id, out var oldHash))
                            hasChanges = oldHash != fileResult.Hash;

                        return (result: StageResult.Create<Stream, string>(fileResult, fileResult.Hash, hasChanges, fileResult.Id), inputId: file.Id, outputHash: fileResult.Hash);
                    }
                    else if (file.HasChanges)
                    {
                        var (fileResult, fileCache) = await file.Perform;
                        var hasChanges = true;
                        if (cache != null && cache.Transformed.TryGetValue(fileResult.Id, out var oldHash))
                            hasChanges = oldHash != fileResult.Hash;
                        System.Diagnostics.Debug.Assert(hasChanges); // if the original file had changes so must this have.

                        return (result: StageResult.Create<Stream, string>(fileResult, fileResult.Hash, hasChanges, fileResult.Id), inputId: file.Id, outputHash: fileResult.Hash);
                    }
                    else
                    {

                        if (cache == null || !cache.InputToOutputId.TryGetValue(file.Id, out var oldOutputId) || !cache.Transformed.TryGetValue(file.Id, out var oldOutputHash))
                            throw this.Context.Exception("No changes, so old value should be there.");


                        var task = LazyTask.Create(async () =>
                        {
                            var (fileResult, fileCache) = await file.Perform;

                            return (fileResult, fileResult.Hash);
                        });

                        return (result: StageResult.Create(task, false, oldOutputId), inputId: file.Id, outputHash: oldOutputHash);
                    }
                })).ConfigureAwait(false);



                var newCache = new SelectStageCache<TInCache>()
                {
                    InputToOutputId = list.ToDictionary(x => x.inputId, x => x.result.Id),
                    OutputIdOrder = list.Select(x => x.result.Id).ToArray(),
                    ParentCache = inputList.cache,
                    Transformed = list.ToDictionary(x => x.result.Id, x => x.outputHash)
                };
                return (result: list.Select(x => x.result).ToImmutableList(), cache: newCache);
            });

            bool hasChanges = input.HasChanges;
            var newCache = cache;
            if (input.HasChanges || newCache == null)
            {

                var (list, c) = await task;
                newCache = c;


                if (!hasChanges && list.Count != cache?.OutputIdOrder.Length)
                    hasChanges = true;

                if (!hasChanges && cache != null)
                {
                    for (int i = 0; i < cache.OutputIdOrder.Length && !hasChanges; i++)
                    {
                        if (list[i].Id != cache.OutputIdOrder[i])
                            hasChanges = true;
                        if (list[i].HasChanges)
                            hasChanges = true;
                    }
                }
            }

            return StageResultList.Create(task, hasChanges, newCache.OutputIdOrder.ToImmutableList());
        }

    }


}
