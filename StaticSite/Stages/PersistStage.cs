using StaticSite.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StaticSite.Stages
{
    public class PersistStage<TPreviousItemCache, TPreviousCache>
        where TPreviousCache : class
        where TPreviousItemCache : class
    {
        private readonly GenerationOptions generatorOptions;
        private readonly StagePerformHandler<Stream, TPreviousItemCache, TPreviousCache> inputList;
        private readonly DirectoryInfo output;
        private readonly GeneratorContext context;

        public PersistStage(StagePerformHandler<System.IO.Stream, TPreviousItemCache, TPreviousCache> inputList, DirectoryInfo output, GenerationOptions generatorOptions, GeneratorContext context)
        {
            this.generatorOptions = generatorOptions;
            this.inputList = inputList;
            this.output = output;
            this.context = context;
        }

        public async Task UpdateFiles()
        {
            var cacheDir = this.context.ChachDir();
            var cacehFile = new FileInfo(Path.Combine(cacheDir.FullName, "cache"));

            // Read the old Cache
            TPreviousCache? cache;
            if (cacehFile.Exists)
            {

                try
                {
                    using var stream = cacehFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                    using var compressed = this.generatorOptions.CompressCache ? new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress) as Stream : stream;
                    cache = await Serelizer.Load<TPreviousCache>(compressed).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    this.context.Warning("Problem Reading Cache. Will recreate from scratch.", e);
                    cache = null;
                }
            }
            else
                cache = null;

            var result = await this.inputList(cache, this.generatorOptions.Token).ConfigureAwait(false);

            if (result.HasChanges)
            {
                var (files, newCache) = await result.Perform;

                // find all files that no longer exist and delete those
                var allFiles = new HashSet<string>(result.Ids.Select(x => Path.Combine(this.output.FullName, x)));
                var directoryQueue = new Stack<DirectoryInfo>();
                directoryQueue.Push(this.output);
                while (directoryQueue.TryPop(out var currentDirectory))
                {
                    if (!currentDirectory.Exists)
                        continue;

                    var subDirectorys = currentDirectory.GetDirectories();
                    if (subDirectorys.Length > 0)
                        directoryQueue.Push(currentDirectory); // we wan't to delete the current Directory when empty. For that we need to visit it again after we visited all subdirectorys
                    foreach (var subDirectory in subDirectorys)
                        directoryQueue.Push(subDirectory);

                    foreach (var subFile in currentDirectory.GetFiles())
                        if (!allFiles.Contains(subFile.FullName))
                            subFile.Delete();

                    if (currentDirectory.GetFiles().Length == 0 && currentDirectory.GetDirectories().Length == 0)
                        currentDirectory.Delete(false);
                }

                // Get all changed files and persit those
                var fileResults = await Task.WhenAll(files.Where(x => x.HasChanges).Select(async x => await x.Perform)).ConfigureAwait(false);
                var tasks = fileResults.Select(x => x.result).Select(async file =>
                {
                    var fileInfo = new FileInfo(Path.Combine(this.output.FullName, file.Id));
                    fileInfo.Directory.Create();
                    using var outStream = fileInfo.Open(FileMode.Create, FileAccess.Write, FileShare.None);
                    using var inStream = file.Value;

                    await inStream.CopyToAsync(outStream).ConfigureAwait(false);
                });
                await Task.WhenAll(tasks).ConfigureAwait(false);

                // Write new cache
                using (var stream = cacehFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
                using (var compressed = this.generatorOptions.CompressCache ? new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionLevel.Fastest) as Stream : stream)
                    await Serelizer.Write(newCache, compressed, !this.generatorOptions.CompressCache).ConfigureAwait(false);
            }

        }

    }


}
