using Stasistium.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class FileSystemStage<T> : GeneratedHelper.Multiple.Simple.OutputMultiSimpleInputSingle1List0StageBase<string, T, Stream>
        where T : class
    {
        public FileSystemStage(StagePerformHandler<string, T> inputSingle0, GeneratorContext context) : base(inputSingle0, context)
        {
        }

        protected override Task<ImmutableList<IDocument<Stream>>> Work(IDocument<string> input, OptionToken options)
        {
            if (input is null)
                throw new System.ArgumentNullException(nameof(input));
            var path = input.Value;
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


            return Task.FromResult(list.Select(x => new FileDocument(x, root, null, this.Context) as IDocument<Stream>).ToImmutableList());
        }
        private class FileDocument : DocumentBase, IDocument<Stream>
        {
            public FileDocument(FileInfo fileInfo, DirectoryInfo root, MetadataContainer? metadata, GeneratorContext context) : base(Path.GetRelativePath(root.FullName, fileInfo.FullName), metadata, GetHash(fileInfo, context), context)
            {
                this.FileInfo = fileInfo;
                this.Root = root;
            }

            private static string GetHash(FileInfo fileInfo, GeneratorContext context)
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

}