using Stasistium.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Stasistium.Serelizer;

namespace Stasistium.Stages
{
    public class PersistStage<TPreviousItemCache, TPreviousCache>
        where TPreviousCache : class
        where TPreviousItemCache : class
    {
        private readonly GenerationOptions generatorOptions;
        private readonly MultiStageBase<Stream, TPreviousItemCache, TPreviousCache> inputList;
        private readonly DirectoryInfo output;

        public string Name { get; }

        private readonly IGeneratorContext context;
        private readonly FileInfo cacehFile;
        private TPreviousCache? cache;

        public PersistStage(MultiStageBase<System.IO.Stream, TPreviousItemCache, TPreviousCache> inputList, DirectoryInfo output, GenerationOptions generatorOptions, IGeneratorContext context, string? name = null)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            this.generatorOptions = generatorOptions;
            this.inputList = inputList;
            this.output = output;
            this.Name = name ?? this.GetType().GetGenericTypeDefinition().Name + Guid.NewGuid().ToString();
            this.context = context.ForName(this.Name);


            var cacheDir = this.context.ChachDir();
            this.cacehFile = new FileInfo(Path.Combine(cacheDir.FullName, "cache"));
            this.context.DisposeOnDispose(new Disposer(this));
        }

        public async Task UpdateFiles()
        {


            // Read the old Cache

            if (this.cacehFile.Exists && this.cache is null)
            {

                try
                {
                    using var stream = this.cacehFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                    using var compressed = this.generatorOptions.CompressCache ? new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress) as Stream : stream;
                    this.cache = await JsonSerelizer.Load<TPreviousCache>(compressed).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    this.context.Warning("Problem Reading Cache. Will recreate from scratch.", e);
                    this.cache = null;
                }
            }


            var result = await this.inputList.DoIt(this.cache, this.generatorOptions.Token).ConfigureAwait(false);
            this.context.Logger.Info($"Cache is {(result.HasChanges ? "INVALID" : "valid")}");
            if (result.HasChanges)
            {
                var files = await result.Perform;
                // find all files that no longer exist and delete those
                var allFiles = new HashSet<string>(result.Ids.Select(x => Path.Combine(this.output.FullName, x).Replace('\\', '/')));
                var directoryQueue = new Queue<DirectoryInfo>();
                var directoryStack = new Stack<DirectoryInfo>();
                directoryQueue.Enqueue(this.output);

                while (directoryQueue.TryDequeue(out var current))
                {
                    if (!current.Exists)
                        continue;

                    directoryStack.Push(current);

                    var subDirectorys = current.GetDirectories();
                    foreach (var subDirectory in subDirectorys)
                        directoryQueue.Enqueue(subDirectory);
                }

                while (directoryStack.TryPop(out var currentDirectory))
                {
                    if (!currentDirectory.Exists)
                        continue;

                    foreach (var subFile in currentDirectory.GetFiles())
                        if (!allFiles.Contains(subFile.FullName.Replace('\\', '/')))
                        {
                            this.context.Logger.Info($"Deleting {subFile}");
                            subFile.Delete();
                        }

                    if (currentDirectory.GetFiles().Length == 0 && currentDirectory.GetDirectories().Length == 0)
                        currentDirectory.Delete(false);
                }

                // Get all changed files and persit those
                var fileResults = await Task.WhenAll(files.Where(x => x.HasChanges).Select(async x => await x.Perform)).ConfigureAwait(false);
                var tasks = fileResults.Select(x => x).Select(async file =>
                {
                    var fileInfo = new FileInfo(Path.Combine(this.output.FullName, file.Id));
                    fileInfo.Directory.Create();
                    using var outStream = fileInfo.Open(FileMode.Create, FileAccess.Write, FileShare.None);
                    using var inStream = file.Value;
                    this.context.Logger.Info($"Writing {file.Id}");

                    await inStream.CopyToAsync(outStream).ConfigureAwait(false);
                });
                await Task.WhenAll(tasks).ConfigureAwait(false);

            }

        }

        private class Disposer : IAsyncDisposable
        {

            private readonly PersistStage<TPreviousItemCache, TPreviousCache> parent;

            public Disposer(PersistStage<TPreviousItemCache, TPreviousCache> parent)
            {
                this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
            }

            public async ValueTask DisposeAsync()
            {
                await this.parent.PersistCache().ConfigureAwait(false);
            }
        }

        private async Task PersistCache()
        {
            if (this.cache is null)
                return;
            // Write new cache
            using var stream = this.cacehFile.Open(FileMode.Create, FileAccess.Write, FileShare.None);
            using var compressed = this.generatorOptions.CompressCache ? new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionLevel.Fastest) as Stream : stream;
            await JsonSerelizer.Write(this.cache, compressed, !this.generatorOptions.CompressCache).ConfigureAwait(false);
        }
    }


}
