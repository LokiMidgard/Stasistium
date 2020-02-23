using Stasistium.Documents;
using System;
using System.Linq;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.IO;
using Stasistium.Core;

namespace Stasistium.Stages
{
    public class SidecarMetadata<TMetadata, TInItemCache, TInCache> : MultiStageBase<Stream, string, TransformStageCache<TInCache>>//: OutputMultiInputSingle0List1StageBase<Stream, TPreviousItemCache, TPreviousListCache, Stream, string, ImmutableList<string>>
        where TInCache : class
        where TInItemCache : class
    {
        private readonly MultiStageBase<Stream, TInItemCache, TInCache> input;
        private readonly MetadataUpdate<TMetadata>? update;

        public SidecarMetadata(MultiStageBase<Stream, TInItemCache, TInCache> input, string sidecarExtension, MetadataUpdate<TMetadata>? update, IGeneratorContext context, string? name = null) : base(context, name)
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

        protected override async Task<StageResultList<Stream, string, TransformStageCache<TInCache>>> DoInternal([AllowNull] TransformStageCache<TInCache>? cache, OptionToken options)
        {
            var input = await this.input.DoIt(cache?.PreviousCache, options).ConfigureAwait(false);

            var task = LazyTask.Create(async () =>
            {
                var inputList = await input.Perform;

                var sidecarLookup = inputList.Where(x => Path.GetExtension(x.Id) == this.SidecarExtension)
                    .ToDictionary(x => Path.Combine(Path.GetDirectoryName(x.Id) ?? string.Empty, Path.GetFileNameWithoutExtension(x.Id)));

                var files = inputList.Where(x => Path.GetExtension(x.Id) != this.SidecarExtension);


                var list = await Task.WhenAll(files.Select(async file =>
                {
                    if (sidecarLookup.TryGetValue(file.Id, out var sidecar) && (file.HasChanges || sidecar.HasChanges))
                    {
                        var fileResult = await file.Perform;
                        var fileCache = file.Cache;
                        var sidecarResult = await sidecar.Perform;
                        var sidecarCache = sidecar.Cache;


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

                        return (result: StageResult.CreateStageResult(this.Context, fileResult, hasChanges, fileResult.Id, fileResult.Hash, fileResult.Hash), inputId: file.Id);
                    }
                    else if (file.HasChanges)
                    {
                        var fileResult = await file.Perform;
                        var fileCache = file.Cache;
                        var hasChanges = true;
                        if (cache != null && cache.Transformed.TryGetValue(fileResult.Id, out var oldHash))
                            hasChanges = oldHash != fileResult.Hash;
                        System.Diagnostics.Debug.Assert(hasChanges); // if the original file had changes so must this have.

                        return (result: StageResult.CreateStageResult(this.Context, fileResult, hasChanges, fileResult.Id, fileResult.Hash, fileResult.Hash), inputId: file.Id);
                    }
                    else
                    {

                        if (cache == null || !cache.InputToOutputId.TryGetValue(file.Id, out var oldOutputId) || !cache.Transformed.TryGetValue(file.Id, out var oldOutputHash))
                            throw this.Context.Exception("No changes, so old value should be there.");


                        var task = LazyTask.Create(async () =>
                        {
                            var fileResult = await file.Perform;
                            var fileCache = file.Cache;
                            return fileResult;
                        });

                        return (result: StageResult.CreateStageResult(this.Context, task, false, oldOutputId, oldOutputHash, oldOutputHash), inputId: file.Id);
                    }
                })).ConfigureAwait(false);



                var newCache = new TransformStageCache<TInCache>()
                {
                    InputToOutputId = list.ToDictionary(x => x.inputId, x => x.result.Id),
                    OutputIdOrder = list.Select(x => x.result.Id).ToArray(),
                    PreviousCache = input.Cache,
                    Transformed = list.ToDictionary(x => x.result.Id, x => x.result.Hash),
                    Hash = this.Context.GetHashForObject(list.Select(x => x.result.Hash))
                };
                return (result: list.Select(x => x.result).ToImmutableList(), cache: newCache);
            });

            bool hasChanges = input.HasChanges;
            if (input.HasChanges || cache == null)
            {

                var (list, c) = await task;

                hasChanges = false;
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
                return this.Context.CreateStageResultList(list, hasChanges, c.OutputIdOrder.ToImmutableList(), c, c.Hash, input.Cache);
            }

            var actiualTask = LazyTask.Create(async () =>
            {
                var temp = await task;
                return temp.result;
            });

            return this.Context.CreateStageResultList(actiualTask, hasChanges, cache.OutputIdOrder.ToImmutableList(), cache, cache.Hash, input.Cache);
        }

    }


}
