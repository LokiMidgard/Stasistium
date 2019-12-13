using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StaticSite.Documents
{



    public abstract class DocumentBase
    {
        public string Id { get; }

        public MetadataContainer Metadata { get; }
        public GeneratorContext Context { get; }

        public string Hash { get; }

        public string ContentHash { get; }

        protected DocumentBase(string id, MetadataContainer? metadata, string contetnHash, GeneratorContext context)
        {
            this.Id = id ?? throw new ArgumentNullException(nameof(id));
            this.Metadata = metadata ?? MetadataContainer.Empty;
            this.ContentHash = contetnHash ?? throw new ArgumentNullException(nameof(contetnHash));
            this.Context = context ?? throw new ArgumentNullException(nameof(context));

            var toHash = $"<{System.Net.WebUtility.HtmlEncode(this.Id)}><{System.Net.WebUtility.HtmlEncode(this.Metadata.Hash)}><{System.Net.WebUtility.HtmlEncode(this.ContentHash)}>";
            this.Hash = this.Context.GetHashForString(toHash);

        }
    }
    class GitFileDocument : DocumentBase, IDocument<Stream>
    {
        private readonly Func<Stream> createStreamCallback;

        private GitFileDocument(string id, string contentHash, Func<Stream> createStreamCallback, MetadataContainer metadata, GeneratorContext context) : base(id, metadata, contentHash, context)
        {
            this.createStreamCallback = createStreamCallback;
        }

        internal GitFileDocument(string path, Blob blob, GeneratorContext context, MetadataContainer? metadata) : base(path, metadata, blob.Sha, context)
        {
            this.createStreamCallback = () => blob.GetContentStream();
        }

        public Stream Value => this.CreateReadStream();

        public Stream CreateReadStream() => this.createStreamCallback();

        public IDocument<TNew> With<TNew>(TNew newItem, string newHash) => new Document<TNew>(newItem, newHash, this.Id, this.Metadata, this.Context);
        public IDocument<TNew> With<TNew>(Func<TNew> newItem, string newHash) => new DocumentLazy<TNew>(newItem, newHash, this.Id, this.Metadata, this.Context);
        public IDocument<Stream> With(MetadataContainer metadata) => new GitFileDocument(this.Id, this.Hash, this.createStreamCallback, metadata, this.Context);

        IDocument IDocument.With(MetadataContainer metadata) => this.With(metadata);

    }

    public class DocumentLazy<T> : DocumentBase, IDocument<T>
    {
        private readonly Func<T> valueCallback;
        public DocumentLazy(Func<T> valueCallback, string contentHash, string id, MetadataContainer? metadata, GeneratorContext context) : base(id, metadata ?? MetadataContainer.Empty, contentHash, context)
        {
            this.valueCallback = valueCallback;
        }

        public T Value => this.valueCallback();

        public IDocument<TNew> With<TNew>(TNew newItem, string newHash) => new Document<TNew>(newItem, newHash, this.Id, this.Metadata, this.Context);
        public IDocument<TNew> With<TNew>(Func<TNew> newItem, string newHash) => new DocumentLazy<TNew>(newItem, newHash, this.Id, this.Metadata, this.Context);
        public IDocument<T> With(MetadataContainer metadata) => new Document<T>(this.Value, this.Hash, this.Id, metadata, this.Context);
        IDocument IDocument.With(MetadataContainer metadata) => this.With(metadata);
    }

    public class Document<T> : DocumentBase, IDocument<T>
    {
        public Document(T value, string contentHash, string id, MetadataContainer? metadata, GeneratorContext context) : base(id, metadata, contentHash, context)
        {
            this.Value = value;
        }

        public T Value { get; }


        public IDocument<TNew> With<TNew>(TNew newItem, string newHash) => new Document<TNew>(newItem, newHash, this.Id, this.Metadata, this.Context);
        public IDocument<TNew> With<TNew>(Func<TNew> newItem, string newHash) => new DocumentLazy<TNew>(newItem, newHash, this.Id, this.Metadata, this.Context);
        public IDocument<T> With(MetadataContainer metadata) => new Document<T>(this.Value, this.Hash, this.Id, metadata, this.Context);
        IDocument IDocument.With(MetadataContainer metadata) => this.With(metadata);

    }
}
