using LibGit2Sharp;
using System;
using System.IO;

namespace Stasistium.Documents
{
    internal class GitFileDocument : DocumentBase, IDocument<Stream>
    {
        private readonly Func<Stream> createStreamCallback;

        private GitFileDocument(string id, string contentHash, Func<Stream> createStreamCallback, MetadataContainer metadata, IGeneratorContext context) : base(id, metadata, contentHash, context)
        {
            this.createStreamCallback = createStreamCallback;
        }

        internal GitFileDocument(string path, Blob blob, IGeneratorContext context, MetadataContainer? metadata) : base(path, metadata, blob.Sha, context)
        {
            this.createStreamCallback = () => blob.GetContentStream();
        }

        public Stream Value => this.CreateReadStream();

        public Stream CreateReadStream() => this.createStreamCallback();

        public IDocument<TNew> With<TNew>(TNew newItem, string newHash) => new Document<TNew>(newItem, newHash, this.Id, this.Metadata, this.Context);
        public IDocument<TNew> With<TNew>(Func<TNew> newItem, string newHash) => new DocumentLazy<TNew>(newItem, newHash, this.Id, this.Metadata, this.Context);
        public IDocument<Stream> With(MetadataContainer metadata) => new GitFileDocument(this.Id, this.ContentHash, this.createStreamCallback, metadata, this.Context);

        IDocument IDocument.With(MetadataContainer metadata) => this.With(metadata);

        public IDocument<Stream> WithId(string id) => new GitFileDocument(id, this.ContentHash, this.createStreamCallback, this.Metadata, this.Context);
        IDocument IDocument.WithId(string id) => this.WithId(id);


    }
}
