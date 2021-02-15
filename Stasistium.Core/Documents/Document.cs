using System;
using System.Collections.Generic;
using System.Text;

namespace Stasistium.Documents
{



    public abstract class DocumentBase
    {
        public string Id { get; }

        public MetadataContainer Metadata { get; }
        public IGeneratorContext Context { get; }

        public string Hash { get; }

        public string ContentHash { get; }

        protected DocumentBase(string id, MetadataContainer? metadata, string contetnHash, IGeneratorContext context)
        {
            this.Id = id ?? throw new ArgumentNullException(nameof(id));
            this.ContentHash = contetnHash ?? throw new ArgumentNullException(nameof(contetnHash));
            this.Context = context ?? throw new ArgumentNullException(nameof(context));
            this.Metadata = metadata ?? this.Context.EmptyMetadata;

            var toHash = $"<{System.Net.WebUtility.HtmlEncode(this.Id)}><{System.Net.WebUtility.HtmlEncode(this.Metadata.Hash)}><{System.Net.WebUtility.HtmlEncode(this.ContentHash)}>";
            this.Hash = this.Context.GetHashForString(toHash);

        }
    }
    public class DocumentLazy<T> : DocumentBase, IDocument<T>
    {
        private readonly Func<T> valueCallback;
        public DocumentLazy(Func<T> valueCallback, string contentHash, string id, MetadataContainer? metadata, IGeneratorContext context) : base(id, metadata, contentHash, context)
        {
            if (string.IsNullOrEmpty(contentHash))
                throw new ArgumentException($"'{nameof(contentHash)}' cannot be null or empty", nameof(contentHash));
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException($"'{nameof(id)}' cannot be null or empty", nameof(id));
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            this.valueCallback = valueCallback ?? throw new ArgumentNullException(nameof(valueCallback));

            if (System.Diagnostics.Debugger.IsAttached && typeof(T).IsInstanceOfType(typeof(System.IO.Stream)))
            {
                var data = (object?)valueCallback();
                if (data is not null)
                {
                    using var stream = (System.IO.Stream)data;
                    if (contentHash != context.GetHashForStream(stream) && System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                }
                else 
                {
                    System.Diagnostics.Debugger.Break();
                }
            }

        }

        public T Value => this.valueCallback();

        public IDocument<TNew> With<TNew>(TNew newItem, string newHash) => new Document<TNew>(newItem, newHash, this.Id, this.Metadata, this.Context);
        public IDocument<TNew> With<TNew>(Func<TNew> newItem, string newHash) => new DocumentLazy<TNew>(newItem, newHash, this.Id, this.Metadata, this.Context);
        public IDocument<T> With(MetadataContainer metadata) => new DocumentLazy<T>(this.valueCallback, this.ContentHash, this.Id, metadata, this.Context);
        IDocument IDocument.With(MetadataContainer metadata) => this.With(metadata);
        public IDocument<T> WithId(string id) => new DocumentLazy<T>(this.valueCallback, this.ContentHash, id, this.Metadata, this.Context);
        IDocument IDocument.WithId(string id) => this.WithId(id);
    }

    public class Document<T> : DocumentBase, IDocument<T>
    {
        public Document(T value, string contentHash, string id, MetadataContainer? metadata, IGeneratorContext context) : base(id, metadata, contentHash, context)
        {
            this.Value = value;
        }

        public T Value { get; }


        public IDocument<TNew> With<TNew>(TNew newItem, string newHash) => new Document<TNew>(newItem, newHash, this.Id, this.Metadata, this.Context);
        public IDocument<TNew> With<TNew>(Func<TNew> newItem, string newHash) => new DocumentLazy<TNew>(newItem, newHash, this.Id, this.Metadata, this.Context);
        public IDocument<T> With(MetadataContainer metadata) => new Document<T>(this.Value, this.ContentHash, this.Id, metadata, this.Context);
        IDocument IDocument.With(MetadataContainer metadata) => this.With(metadata);
        public IDocument<T> WithId(string id) => new Document<T>(this.Value, this.ContentHash, id, this.Metadata, this.Context);
        IDocument IDocument.WithId(string id) => this.WithId(id);

    }
}
