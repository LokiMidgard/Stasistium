
using Stasistium.Documents;
using Stasistium.Stages;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Stasistium.Stages
{

    public class FileSystemStage<T> : StageBase<string, Stream>
    {
        public FileSystemStage(IGeneratorContext context, string? name) : base(context, name)
        {
        }

        protected override Task<ImmutableList<IDocument<Stream>>> Work(ImmutableList<IDocument<string>> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            var builder = ImmutableList.CreateBuilder<IDocument<Stream>>();


            foreach (var pathDocument in input)
            {
                var path = pathDocument.Value;
                var root = new DirectoryInfo(path);

                if (!root.Exists)
                    throw this.Context.Exception($"Folder {path} does not exists.");

                var queue = new Queue<DirectoryInfo>();
                queue.Enqueue(root);

                var list = new List<FileInfo>();

                while (queue.TryDequeue(out var directory))
                {
                    builder.AddRange(directory.GetFiles().Select(ToDocuments));

                    foreach (var subDirectory in directory.GetDirectories())
                    {
                        queue.Enqueue(subDirectory);
                    }
                }

                IDocument<Stream> ToDocuments(FileInfo file)
                {
                    var document = new FileDocument(file, root, pathDocument.Metadata, this.Context);
                    return document;
                }
            }

            return Task.FromResult(builder.ToImmutable());
        }
    }




    internal class FileDocument : DocumentBase, IDocument<Stream>
    {
        public FileDocument(FileInfo fileInfo, DirectoryInfo root, MetadataContainer? metadata, IGeneratorContext context) : base(Path.GetRelativePath(root.FullName, fileInfo.FullName).Replace('\\', '/'), metadata, GetHash(fileInfo, context), context)
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

