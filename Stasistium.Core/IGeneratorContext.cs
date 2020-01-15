using Stasistium.Stages;
using System;
using System.IO;

namespace Stasistium.Documents
{
    public interface IGeneratorContext : IAsyncDisposable
    {
        DirectoryInfo CacheFolder { get; }
        MetadataContainer EmptyMetadata { get; }
        ILogger Logger { get; }
        DirectoryInfo TempFolder { get; }

        DirectoryInfo ChachDir();
        IDocument<T> Create<T>(T value, string contentHash, string id, MetadataContainer? metadata = null);
        void DisposeOnDispose(IDisposable disposable);
        void DisposeOnDispose(IAsyncDisposable disposable);
        Exception Exception(string message);
        string GetHashForObject(object? value);
        string GetHashForStream(Stream toHash);
        string GetHashForString(string toHash);
        StaticStage<TResult> StageFromResult<TResult>(TResult result, Func<TResult, string> hashFunction);
        DirectoryInfo TempDir();
        void Warning(string message, Exception? e = null);

        internal IGeneratorContext ForName(string name)
        {
            if (this is IGeneratorContext context)
                return new GeneratorContextWrapper(context, name);
            if (this is GeneratorContextWrapper wrapper)
                return new GeneratorContextWrapper(wrapper.BaseContext, name);
            throw new NotSupportedException("This Context is not supported.");
        }
    }
}