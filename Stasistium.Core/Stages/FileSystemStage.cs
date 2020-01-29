using Stasistium.Core;
using Stasistium.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Stasistium.Stages
{

    public class FileSystemStage<T> : MultiStageBase<Stream, string, FileSystemCache<T>>
        where T : class
    {
        private readonly StagePerformHandler<string, T> input;

        public FileSystemStage(StagePerformHandler<string, T> input, IGeneratorContext context, string? name) : base(context, name)
        {
            this.input = input;
        }

        protected override async Task<StageResultList<Stream, string, FileSystemCache<T>>> DoInternal([AllowNull] FileSystemCache<T>? cache, OptionToken options)
        {
            var result = await this.input(cache?.PreviousCache, options).ConfigureAwait(false);

            var task = LazyTask.Create(async () =>
            {

                var performed = await result.Perform;

                var newPreviousCache = result.Cache;
                var path = performed.Value;
                var root = new DirectoryInfo(path);

                if (!root.Exists)
                    throw this.Context.Exception("Folder does not exists.");

                var queue = new Queue<DirectoryInfo>();
                queue.Enqueue(root);

                var list = new List<FileInfo>();

                while (queue.TryDequeue(out var directory))
                {
                    list.AddRange(directory.GetFiles());

                    foreach (var subDirectory in directory.GetDirectories())
                    {
                        queue.Enqueue(subDirectory);
                    }
                }


                var callculated = list.Select(file =>
                {
                    var id = Path.GetRelativePath(root.FullName, file.FullName);
                    var writeTime = file.LastWriteTimeUtc;

                    if (cache is null || !cache.PathToWriteTime.TryGetValue(id, out var lastWriteTime))
                        lastWriteTime = default;
                    var hasChanges = lastWriteTime != writeTime;

                    if (hasChanges || cache is null)
                    {
                        // check if we actually have changes.
                        var document = new FileDocument(file, root, performed.Metadata, this.Context);
                        if (cache is null || !cache.PathToHash.TryGetValue(id, out string? lastHash))
                            lastHash = null;

                        hasChanges = document.Hash != lastHash;

                        return (result: StageResult.Create(document as IDocument<Stream>, hasChanges, document.Id, document.Hash), writeTime, hash: document.Hash, id);
                    }
                    else
                    {
                        if (cache is null || !cache.PathToHash.TryGetValue(id, out string? lastHash))
                            throw this.Context.Exception("Should not happpen");

                        var subTask = LazyTask.Create(() =>
                        {
                            var document = new FileDocument(file, root, performed.Metadata, this.Context);
                            return document as IDocument<Stream>;
                        });

                        return (result: StageResult.Create(subTask, hasChanges, id, lastHash), writeTime, hash: lastHash, id);
                    }
                }).ToArray();


                var newCache = new FileSystemCache<T>()
                {
                    PreviousCache = newPreviousCache,
                    PathToHash = callculated.ToDictionary(x => x.id, x => x.hash),
                    PathToWriteTime = callculated.ToDictionary(x => x.id, x => x.writeTime),
                    IdOrder = callculated.Select(x => x.id).ToArray()

                };
                var resultList = callculated.Select(X => X.result).ToImmutableList();
                return (result: resultList, newCache);
            });

            var r = await task;
            var hasChanges = r.result.Any(x => x.HasChanges)
                                || cache is null
                                || !cache.IdOrder.SequenceEqual(r.newCache.IdOrder);
            var ids = r.newCache.IdOrder.ToImmutableList();

            return StageResultList.Create(r.result, hasChanges, ids, r.newCache);
        }
    }

    public class FileSystemCache<T>
    {
        public T PreviousCache { get; set; }
        public Dictionary<string, DateTime> PathToWriteTime { get; set; }
        public Dictionary<string, string> PathToHash { get; set; }
        public string[] IdOrder { get; set; }
    }


    internal class FileDocument : DocumentBase, IDocument<Stream>
    {
        public FileDocument(FileInfo fileInfo, DirectoryInfo root, MetadataContainer? metadata, IGeneratorContext context) : base(Path.GetRelativePath(root.FullName, fileInfo.FullName), metadata, GetHash(fileInfo, context), context)
        {
            this.FileInfo = fileInfo;
            this.Root = root;
        }

        private static string GetHash(FileInfo fileInfo, IGeneratorContext context)
        {
            using var stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            return context.GetHashForStream(stream);
        }

        public Stream Value => this.FileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read);

        public FileInfo FileInfo { get; }
        public DirectoryInfo Root { get; }

        public IDocument<TNew> With<TNew>(TNew newItem, string newHash)
        {
            return new Document<TNew>(newItem, newHash, this.Id, this.Metadata, this.Context);
        }

        public IDocument<TNew> With<TNew>(Func<TNew> newItem, string newHash)
        {
            return new DocumentLazy<TNew>(newItem, newHash, this.Id, this.Metadata, this.Context);
        }

        public IDocument<Stream> With(MetadataContainer metadata)
        {
            return new DocumentLazy<Stream>(() => this.Value, this.ContentHash, this.Id, metadata, this.Context);
        }

        public IDocument<Stream> WithId(string id)
        {
            return new DocumentLazy<Stream>(() => this.Value, this.ContentHash, id, this.Metadata, this.Context);
        }

        IDocument IDocument.With(MetadataContainer metadata)
        {
            return this.With(metadata);
        }

        IDocument IDocument.WithId(string id)
        {
            return this.WithId(id);
        }
    }


}