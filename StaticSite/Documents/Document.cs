using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StaticSite.Documents
{
    class FileDocument : IDocument
    {
        private readonly Func<Stream> createStreamCallback;

        public FileDocument(string id, ReadOnlyMemory<byte> hash, Func<Stream> createStreamCallback, MetadataContainer? metadata = null)
        {
            this.Id = id ?? throw new ArgumentNullException(nameof(id));
            this.Hash = hash;
            this.createStreamCallback = createStreamCallback;
            this.Metadata = metadata ?? MetadataContainer.Empty;
        }

        public string Id { get; }

        public MetadataContainer Metadata { get; }

        public ReadOnlyMemory<byte> Hash { get; }

        public Stream CreateReadStream() => createStreamCallback();
    }
}
