using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StaticSite.Documents
{
    class GitFileDocument : IDocument<Stream>
    {
        private readonly Func<Stream> createStreamCallback;

        internal GitFileDocument(string path, Blob blob, MetadataContainer? metadata = null)
        {
            this.Id = path ?? throw new ArgumentNullException(nameof(path));
            this.Hash = blob.Sha;
            this.createStreamCallback = () => blob.GetContentStream();
            this.Metadata = metadata ?? MetadataContainer.Empty;
        }

        public string Id { get; }

        public MetadataContainer Metadata { get; }

        public string Hash { get; }

        public Stream Value => this.CreateReadStream();

        public Stream CreateReadStream() => this.createStreamCallback();
    }

    public class DocumentLazy<T> : IDocument<T>
    {
        private readonly Func<T> valueCallback;
        public DocumentLazy(Func<T> valueCallback, string hash, string id, MetadataContainer metadata)
        {
            this.valueCallback = valueCallback;
            this.Hash = hash ?? throw new ArgumentNullException(nameof(hash));
            this.Id = id ?? throw new ArgumentNullException(nameof(id));
            this.Metadata = metadata ?? MetadataContainer.Empty;
        }

        public T Value => this.valueCallback();

        public string Hash { get; }

        public string Id { get; }

        public MetadataContainer Metadata { get; }

        public IDocument<TNew> With<TNew>(TNew newItem, string newHash) => new Document<TNew>(newItem, newHash, this.Id, this.Metadata);
        public IDocument<TNew> With<TNew>(Func<TNew> newItem, string newHash) => new DocumentLazy<TNew>(newItem, newHash, this.Id, this.Metadata);
        public IDocument<T> With(MetadataContainer metadata) => new Document<T>(this.Value, this.Hash, this.Id, metadata);
    }

    public class Document<T> : IDocument<T>
    {
        public Document(T value, string hash, string id, MetadataContainer metadata)
        {
            this.Value = value;
            this.Hash = hash ?? throw new ArgumentNullException(nameof(hash));
            this.Id = id ?? throw new ArgumentNullException(nameof(id));
            this.Metadata = metadata ?? MetadataContainer.Empty;
        }

        public T Value { get; }

        public string Hash { get; }

        public string Id { get; }

        public MetadataContainer Metadata { get; }

        public IDocument<TNew> With<TNew>(TNew newItem, string newHash) => new Document<TNew>(newItem, newHash, this.Id, this.Metadata);
        public IDocument<TNew> With<TNew>(Func<TNew> newItem, string newHash) => new DocumentLazy<TNew>(newItem, newHash, this.Id, this.Metadata);
        public IDocument<T> With(MetadataContainer metadata) => new Document<T>(this.Value, this.Hash, this.Id, metadata);
    }
}
