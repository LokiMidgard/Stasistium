using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Stasistium.Documents;
using Stasistium.Stages;
using Stasistium;

namespace Stasistium.Razor
{

    public class FileProviderStage<TInputItemCache, TInputCache> : Stages.GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple0List1StageBase<Stream, TInputItemCache, TInputCache, IFileProvider>
        where TInputCache : class
        where TInputItemCache : class
    {
        public FileProviderStage(string providerId, MultiStageBase<Stream, TInputItemCache, TInputCache> inputList0, IGeneratorContext context, string? name) : base(inputList0, context,name)
        {
            this.ProviderId = providerId;
        }

        public string ProviderId { get; }

        protected override Task<IDocument<IFileProvider>> Work(ImmutableList<IDocument<Stream>> inputList0, OptionToken options)
        {
            var provider = new FileProvider(inputList0, this.ProviderId);

            var hash = this.Context.GetHashForString(string.Join(",", inputList0.Select(x => x.Hash)));
            IDocument<IFileProvider> document = this.Context.CreateDocument(provider, hash, this.ProviderId);
            return Task.FromResult(document);

        }

        private class FileProvider : IFileProvider
        {
            private Dictionary<string, StageProviderDirectory> directoryLookup = new Dictionary<string, StageProviderDirectory>();
            private Dictionary<string, IFileInfo> fileLookup = new Dictionary<string, IFileInfo>();


            public FileProvider(IEnumerable<IDocument<Stream>> files, string id)
            {
                if (files is null)
                    throw new ArgumentNullException(nameof(files));
                foreach (var document in files)
                {
                    var dir = Path.Combine(id, Path.GetDirectoryName(document.Id) ?? string.Empty);
                    dir = dir.Replace('\\', '/');
                    if (!this.directoryLookup.ContainsKey(dir))
                    {
                        var value = new StageProviderDirectory(dir);
                        this.directoryLookup.Add(dir, value);
                        this.fileLookup.Add(dir, value);
                    }

                    var directory = this.directoryLookup[dir];

                    var file = new StageProviderFile(document, id);
                    this.fileLookup.Add(Path.Combine(id, document.Id).Replace('\\', '/'), file);
                    directory.Add(file);
                }
            }

            public IDirectoryContents GetDirectoryContents(string subpath)
            {
                subpath = subpath.Replace('\\', '/').TrimStart('/');
                if (this.directoryLookup.TryGetValue(subpath, out var dir))
                    return dir;
                return new NonExistingDir(subpath);
            }

            public IFileInfo GetFileInfo(string subpath)
            {
                subpath = subpath.Replace('\\', '/').TrimStart('/');
                if (this.fileLookup.TryGetValue(subpath, out var dir))
                    return dir;
                return new NonExistingFile(subpath);
            }

            private class ChangeToken : IChangeToken
            {
                public static readonly ChangeToken Instance = new ChangeToken();
                private ChangeToken()
                {

                }
                public bool HasChanged => false;

                public bool ActiveChangeCallbacks => false;

                public IDisposable RegisterChangeCallback(Action<object> callback, object state) => Disposable.Instance;
                private sealed class Disposable : IDisposable
                {
                    private Disposable()
                    {

                    }
                    public static readonly Disposable Instance = new Disposable();
                    public void Dispose()
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            public IChangeToken Watch(string filter) => ChangeToken.Instance;

            private class NonExistingFile : IFileInfo
            {
                public NonExistingFile(string physicalPath)
                {
                    this.PhysicalPath = physicalPath;
                    this.Name = Path.GetFileName(physicalPath);
                }
                public bool Exists => false;

                public long Length => -1;

                public string PhysicalPath { get; }

                public string Name { get; }

                public DateTimeOffset LastModified => default;

                public virtual bool IsDirectory => false;

                public Stream CreateReadStream()
                {
                    throw new NotImplementedException();
                }
            }

            private class NonExistingDir : NonExistingFile, IDirectoryContents
            {
                public NonExistingDir(string physicalPath) : base(physicalPath)
                {
                }

                public override bool IsDirectory => true;

                public IEnumerator<IFileInfo> GetEnumerator()
                {
                    yield break;
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    yield break;
                }
            }

            private class StageProviderDirectory : IDirectoryContents, IFileInfo
            {
                private readonly List<StageProviderFile> fileList = new List<StageProviderFile>();
                public StageProviderDirectory(string physicalPath)
                {
                    this.PhysicalPath = physicalPath;
                    this.Name = Path.GetFileName(physicalPath);
                }
                public bool Exists => true;

                public long Length => -1;

                public string PhysicalPath { get; }

                public string Name { get; }

                public DateTimeOffset LastModified => default;

                public bool IsDirectory => true;

                public Stream CreateReadStream()
                {
                    throw new NotImplementedException();
                }

                public IEnumerator<IFileInfo> GetEnumerator()
                {
                    return this.fileList.GetEnumerator();
                }

                internal void Add(StageProviderFile file)
                {
                    this.fileList.Add(file);
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.fileList.GetEnumerator();
                }
            }
            private class StageProviderFile : IFileInfo
            {
                private readonly IDocument<Stream> document;
                private readonly string id;

                public StageProviderFile(IDocument<Stream> document, string id)
                {
                    this.document = document;
                    this.id = id;
                }

                public bool Exists => true;

                public long Length
                {
                    get
                    {
                        using var stream = this.document.Value;
                        return stream.Length;
                    }
                }

                public string PhysicalPath => Path.Combine(this.id, this.document.Id);

                public string Name => Path.Combine(this.id, Path.GetFileName(this.document.Id));

                public DateTimeOffset LastModified => default;

                public bool IsDirectory => false;

                public Stream CreateReadStream()
                {
                    return this.document.Value;
                }
            }
        }
    }
}
