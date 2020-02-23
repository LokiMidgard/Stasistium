using Stasistium.Stages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Stasistium.Documents
{



    internal sealed class GeneratorContextWrapper : IGeneratorContext
    {
        public GeneratorContext BaseContext { get; }
        public string Name { get; }

        public GeneratorContextWrapper(IGeneratorContext baseContext, string name)
        {
            if (baseContext is GeneratorContextWrapper wrapper)
                this.BaseContext = wrapper.BaseContext;
            else if (baseContext is GeneratorContext context)
                this.BaseContext = context;
            else if (baseContext is null)
                throw new ArgumentNullException(nameof(baseContext));
            else
                throw new NotSupportedException($"Implementation {baseContext.GetType()} is not supported");

            this.Name = name;
        }

        public DirectoryInfo CacheFolder => this.BaseContext.CacheFolder;

        public MetadataContainer EmptyMetadata => this.BaseContext.EmptyMetadata;

        public ILogger Logger => this.BaseContext.Logger.WithName(this.Name);

        public DirectoryInfo TempFolder => this.BaseContext.TempFolder;

        public DirectoryInfo ChachDir()
        {
            return this.BaseContext.ChachDir();
        }

        public IDocument<T> CreateDocument<T>(T value, string contentHash, string id, MetadataContainer? metadata = null)
        {
            return this.BaseContext.CreateDocument(value, contentHash, id, metadata);
        }

        public Exception Exception(string message)
        {
            return this.BaseContext.Exception(message);
        }

        public string GetHashForStream(Stream toHash)
        {
            return this.BaseContext.GetHashForStream(toHash);
        }

        public string GetHashForString(string toHash)
        {
            return this.BaseContext.GetHashForString(toHash);
        }

        public StaticStage<TResult> StageFromResult<TResult>(string id, TResult result, Func<TResult, string> hashFunction)
        {
            return this.BaseContext.StageFromResult(id, result, hashFunction);
        }

        public DirectoryInfo TempDir()
        {
            return this.BaseContext.TempDir();
        }

        public void Warning(string message, Exception? e = null)
        {
            this.BaseContext.Warning(message, e);
        }

        public string GetHashForObject(object? value)
        {
            return this.BaseContext.GetHashForObject(value);
        }

        public void DisposeOnDispose(IDisposable disposable)
        {
            this.BaseContext.DisposeOnDispose(disposable);
        }

        public void DisposeOnDispose(IAsyncDisposable disposable)
        {
            this.BaseContext.DisposeOnDispose(disposable);
        }

        public ValueTask DisposeAsync()
        {
            return this.BaseContext.DisposeAsync();
        }

        public bool Equals(IGeneratorContext other)
        {
            return this.BaseContext.Equals(other);
        }
    }
    public sealed class GeneratorContext : IGeneratorContext
    {
        private readonly HashAlgorithm algorithm = SHA256.Create();
        private readonly Func<object, string?>? objectToStingRepresentation;

        private readonly System.Collections.Concurrent.ConcurrentBag<IDisposable> disposables = new System.Collections.Concurrent.ConcurrentBag<IDisposable>();
        private readonly System.Collections.Concurrent.ConcurrentBag<IAsyncDisposable> asyncDisposables = new System.Collections.Concurrent.ConcurrentBag<IAsyncDisposable>();

        public ILogger Logger => this.logger;
        private readonly Logger logger;
        public DirectoryInfo CacheFolder { get; }
        public DirectoryInfo TempFolder { get; }

        public MetadataContainer EmptyMetadata { get; }

        public GeneratorContext(DirectoryInfo? cacheFolder = null, DirectoryInfo? tempFolder = null, Func<object, string?>? objectToStingRepresentation = null, TextWriter? logger = null)
        {
            this.CacheFolder = cacheFolder ?? new DirectoryInfo("Cache");
            this.TempFolder = tempFolder ?? new DirectoryInfo("Temp");
            this.EmptyMetadata = MetadataContainer.EmptyFromContext(this);
            this.objectToStingRepresentation = objectToStingRepresentation;
            this.logger = new Logger(logger ?? Console.Out);
        }

        public void DisposeOnDispose(IDisposable disposable)
        {
            if (disposable is null)
                throw new ArgumentNullException(nameof(disposable));
            this.disposables.Add(disposable);
        }
        public void DisposeOnDispose(IAsyncDisposable disposable)
        {
            if (disposable is null)
                throw new ArgumentNullException(nameof(disposable));
            this.asyncDisposables.Add(disposable);
        }

        public string GetHashForString(string toHash)
        {
            var bytes = this.algorithm.ComputeHash(Encoding.UTF8.GetBytes(toHash));

            var sb = new StringBuilder(bytes.Length << 1);
            foreach (byte b in bytes)
                sb.Append(b.ToString("X2", System.Globalization.CultureInfo.InvariantCulture));

            return sb.ToString();
        }

        public string GetHashForStream(Stream toHash)
        {
            var bytes = this.algorithm.ComputeHash(toHash);

            var sb = new StringBuilder(bytes.Length << 1);
            foreach (byte b in bytes)
                sb.Append(b.ToString("X2", System.Globalization.CultureInfo.InvariantCulture));

            return sb.ToString();
        }

        public Stages.StaticStage<TResult> StageFromResult<TResult>(string id, TResult result, Func<TResult, string> hashFunction)
    => new Stages.StaticStage<TResult>(id, result, hashFunction, this);



        public string GetHashForObject(object? value)
        {

            var c = System.Globalization.CultureInfo.InvariantCulture;
            return this.GetHashForString(value switch
            {
                string s => s,
                int i => i.ToString(c),
                long l => l.ToString(c),
                uint i => i.ToString(c),
                ulong l => l.ToString(c),
                byte b => b.ToString(c),
                bool b => b.ToString(c),
                IDocument d => d.Hash,
                System.IO.FileInfo fileInfo => fileInfo.FullName,
                System.IO.DirectoryInfo direcoryInfo => direcoryInfo.FullName,
                DateTime date => date.Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture),
                DateTimeOffset date => date.Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture),
                null => "",
                System.Runtime.CompilerServices.ITuple tuple => TupleToString(tuple),
                System.Collections.IEnumerable enumerable => EnumberableToString(enumerable),
                _ => this.GetStringForObject(value)
            }); ; ;

            string TupleToString(System.Runtime.CompilerServices.ITuple tuple)
            {
                var str = new StringBuilder();
                for (int i = 0; i < tuple.Length; i++)
                {
                    str.Append("<");
                    str.Append(System.Net.WebUtility.HtmlEncode(this.GetHashForObject(tuple[i])));
                    str.Append(">");
                }

                return str.ToString();
            }
            string EnumberableToString(System.Collections.IEnumerable enumerable)
            {
                var str = new StringBuilder();
                foreach (var item in enumerable)
                {
                    str.Append("<");
                    str.Append(System.Net.WebUtility.HtmlEncode(this.GetHashForObject(item)));
                    str.Append(">");
                }

                return str.ToString();
            }
        }



        private string GetStringForObject(object obj)
        {
            var result = this.objectToStingRepresentation?.Invoke(obj);
            if (result is null)
            {
                var type = obj.GetType();
                if (type.IsEnum)
                    return obj.ToString();

                var str = new StringBuilder();
                foreach (var property in type.GetProperties().OrderBy(x => x.Name))
                {
                    str.Append("<");
                    str.Append(property.Name);
                    str.Append("><");
                    str.Append(this.GetHashForObject(property.GetValue(obj)));
                    str.Append(">");
                }
                return str.ToString();
            }
            return result;
        }

        public void Warning(string message, Exception? e = null)
        {
            Console.WriteLine(message);
            if (e != null)
                Console.WriteLine(e.ToString());
        }


        public IDocument<T> CreateDocument<T>(T value, string contentHash, string id, MetadataContainer? metadata = null)
        {
            return new Document<T>(value, contentHash, id, metadata, this);
        }


        public System.IO.DirectoryInfo TempDir()
        {
            var directoryInfo = new DirectoryInfo(Path.Combine(this.TempFolder.FullName, Guid.NewGuid().ToString()));
            directoryInfo.Create();
            return directoryInfo;
        }
        public System.IO.DirectoryInfo ChachDir()
        {
            this.CacheFolder.Create();
            return this.CacheFolder;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        public async ValueTask DisposeAsync()
        {
            if (!this.disposedValue)
            {
                while (this.disposables.TryTake(out var disposable))
                    disposable.Dispose();

                while (this.asyncDisposables.TryTake(out var disposable))
                    await disposable.DisposeAsync();

                this.algorithm.Dispose();

                Helper.Delete.Readonly(this.TempFolder.FullName);
                await this.logger.DisposeAsync();
                this.disposedValue = true;
            }

        }
        #endregion


        public Exception Exception(string message)
        {

            return new NotImplementedException(message);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is IGeneratorContext other)
                return this.Equals(other);
            return false;
        }
        public bool Equals(IGeneratorContext other)
        {
            if (other is GeneratorContextWrapper wrapper)
                return this.Equals(wrapper.BaseContext);
            else if (other is GeneratorContext context)
                return ReferenceEquals(this, context);
            return false;
        }
    }

    internal class LoggerWrapper : ILogger
    {
        public LoggerWrapper(Logger baseLogger, string name)
        {
            this.BaseLogger = baseLogger ?? throw new ArgumentNullException(nameof(baseLogger));
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public Logger BaseLogger { get; }
        public string Name { get; }

        public IDisposable Indent()
        {
            return ((ILogger)this.BaseLogger).Indent();
        }

        public void Info(string text)
        {
            ((ILogger)this.BaseLogger).Info($"{this.Name}: {text}");
        }
    }
    internal class Logger : ILogger, IAsyncDisposable
    {
        private readonly System.CodeDom.Compiler.IndentedTextWriter logger;

        internal Logger(TextWriter writer)
        {
            this.logger = new System.CodeDom.Compiler.IndentedTextWriter(writer);
        }

        public void Info(string text)
        {
            this.logger.WriteLine(text);
        }

        public IDisposable Indent()
        {
            this.logger.Indent++;
            return new IndentWrapper(this);
        }

        public ValueTask DisposeAsync()
        {
            return this.logger.DisposeAsync();
        }

        private sealed class IndentWrapper : IDisposable
        {
            private readonly Logger logger;

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            public IndentWrapper(Logger logger)
            {
                this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public void Dispose()
            {
                if (!this.disposedValue)
                {
                    this.logger.logger.Indent--;
                    this.disposedValue = true;
                }
            }
            #endregion

        }
    }
}

