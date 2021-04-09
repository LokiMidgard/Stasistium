using Stasistium.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Stasistium.Serelizer;
using Stasistium.Stages;

namespace Stasistium.Stages
{
    public class PersistStage : StageBaseSink<Stream>
    {

        protected async override Task Work(ImmutableList<IDocument<Stream>> files, OptionToken options)
        {
            // find all files that no longer exist and delete those
            var allFiles = new HashSet<string>(files.Select(x => Path.Combine(this.output.FullName, x.Id).Replace('\\', '/')));
            var directoryQueue = new Queue<DirectoryInfo>();
            var directoryStack = new Stack<DirectoryInfo>();
            directoryQueue.Enqueue(this.output);

            while (directoryQueue.TryDequeue(out var current))
            {
                if (!current.Exists)
                    continue;

                this.Context.Logger.Info($"push {current} to directory stack");
                directoryStack.Push(current);

                var subDirectorys = current.GetDirectories();
                foreach (var subDirectory in subDirectorys)
                {
                    directoryQueue.Enqueue(subDirectory);
                }
            }

            while (directoryStack.TryPop(out var currentDirectory))
            {
                if (!currentDirectory.Exists)
                    continue;

                foreach (var subFile in currentDirectory.GetFiles())
                    if (!allFiles.Contains(subFile.FullName.Replace('\\', '/')))
                    {
                        this.Context.Logger.Info($"Deleting {subFile}");
                        subFile.Delete();
                    }

                if (currentDirectory.GetFiles().Length == 0 && currentDirectory.GetDirectories().Length == 0)
                {
                    this.Context.Logger.Info($"Deleting {currentDirectory}");

                    currentDirectory.Delete(false);
                }
            }

            // Get all changed files and persit those
            var tasks = files.Select(x => x).Select((Func<IDocument<Stream>, Task>)(async file =>
            {
                var fileInfo = new FileInfo(Path.Combine(this.output.FullName, file.Id));
                fileInfo.Directory.Create();
                using var outStream = fileInfo.Open(FileMode.Create, FileAccess.Write, FileShare.None);
                using var inStream = file.Value;
                this.Context.Logger.Info($"Writing {file.Id}");

                await inStream.CopyToAsync(outStream).ConfigureAwait(false);
            }));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private readonly DirectoryInfo output;

        public PersistStage(DirectoryInfo output, IGeneratorContext context, string? name) : base(context, name)
        {
            this.output = output ?? throw new ArgumentNullException(nameof(output));
        }
    }
}