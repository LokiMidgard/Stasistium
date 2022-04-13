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
            {
                BaseContext = wrapper.BaseContext;
            }
            else if (baseContext is GeneratorContext context)
            {
                BaseContext = context;
            }
            else if (baseContext is null)
            {
                throw new ArgumentNullException(nameof(baseContext));
            }
            else
            {
                throw new NotSupportedException($"Implementation {baseContext.GetType()} is not supported");
            }

            Name = name;
        }

        public DirectoryInfo CacheFolder => BaseContext.CacheFolder;

        public MetadataContainer EmptyMetadata => BaseContext.EmptyMetadata;

        public ILogger Logger => BaseContext.Logger.WithName(Name);

        public DirectoryInfo TempFolder => BaseContext.TempFolder;

        public DirectoryInfo ChachDir()
        {
            return BaseContext.ChachDir();
        }

        public IDocument<T> CreateDocument<T>(T value, string contentHash, string id, MetadataContainer? metadata = null)
        {
            return BaseContext.CreateDocument(value, contentHash, id, metadata);
        }

        public Exception Exception(string message)
        {
            return BaseContext.Exception(message);
        }

        public string GetHashForStream(Stream toHash)
        {
            return BaseContext.GetHashForStream(toHash);
        }

        public string GetHashForString(string toHash)
        {
            return BaseContext.GetHashForString(toHash);
        }

        public IStageBaseOutput<TResult> StageFromResult<TResult>(string id, TResult result, Func<TResult, string> hashFunction)
        {
            return BaseContext.StageFromResult(id, result, hashFunction);
        }

        public DirectoryInfo TempDir()
        {
            return BaseContext.TempDir();
        }

        public void Warning(string message, Exception? e = null)
        {
            BaseContext.Warning(message, e);
        }

        public string GetHashForObject(object? value)
        {
            return BaseContext.GetHashForObject(value);
        }

        public void DisposeOnDispose(IDisposable disposable)
        {
            BaseContext.DisposeOnDispose(disposable);
        }

        public void DisposeOnDispose(IAsyncDisposable disposable)
        {
            BaseContext.DisposeOnDispose(disposable);
        }

        public ValueTask DisposeAsync()
        {
            return BaseContext.DisposeAsync();
        }

        public bool Equals(IGeneratorContext? other)
        {
            return BaseContext.Equals(other);
        }
    }
    public sealed class GeneratorContext : IGeneratorContext
    {
        private readonly Func<object, string?>? objectToStingRepresentation;

        private readonly System.Collections.Concurrent.ConcurrentBag<IDisposable> disposables = new();
        private readonly System.Collections.Concurrent.ConcurrentBag<IAsyncDisposable> asyncDisposables = new();

        public ILogger Logger => logger;
        private readonly Logger logger;
        public DirectoryInfo CacheFolder { get; }
        public DirectoryInfo TempFolder { get; }

        public MetadataContainer EmptyMetadata { get; }

        public GeneratorContext(DirectoryInfo? cacheFolder = null, DirectoryInfo? tempFolder = null, Func<object, string?>? objectToStingRepresentation = null, TextWriter? logger = null)
        {
            CacheFolder = cacheFolder ?? new DirectoryInfo("Cache");
            TempFolder = tempFolder ?? new DirectoryInfo("Temp");
            EmptyMetadata = MetadataContainer.EmptyFromContext(this);
            this.objectToStingRepresentation = objectToStingRepresentation;
            this.logger = new Logger(logger ?? Console.Out);
        }

        public void DisposeOnDispose(IDisposable disposable)
        {
            if (disposable is null)
            {
                throw new ArgumentNullException(nameof(disposable));
            }

            disposables.Add(disposable);
        }
        public void DisposeOnDispose(IAsyncDisposable disposable)
        {
            if (disposable is null)
            {
                throw new ArgumentNullException(nameof(disposable));
            }

            asyncDisposables.Add(disposable);
        }

        public string GetHashForString(string toHash)
        {
            using SHA256? algorithm = SHA256.Create();
            byte[]? bytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(toHash ?? string.Empty));

            StringBuilder? sb = new(bytes.Length << 1);
            foreach (byte b in bytes)
            {
                _ = sb.Append(b.ToString("X2", System.Globalization.CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }

        public string GetHashForStream(Stream toHash)
        {
            using SHA256? algorithm = SHA256.Create();
            byte[]? bytes = algorithm.ComputeHash(toHash);

            StringBuilder? sb = new(bytes.Length << 1);
            foreach (byte b in bytes)
            {
                _ = sb.Append(b.ToString("X2", System.Globalization.CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }

        private readonly List<Stages.StaticStage> staticStages = new();

        public IStageBaseOutput<TResult> StageFromResult<TResult>(string id, TResult result, Func<TResult, string> hashFunction)
        {
            StaticStage<TResult>? stage = new(id, result, hashFunction, this);
            staticStages.Add(stage);
            return stage;
        }

        public Task Run(GenerationOptions option) => Task.WhenAll(staticStages.Select(stage => stage.Invoke(option.Token)));



        public string GetHashForObject(object? value)
        {

            System.Globalization.CultureInfo? c = System.Globalization.CultureInfo.InvariantCulture;
            return GetHashForString(value switch
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
                _ => GetStringForObject(value)
            });

            string TupleToString(System.Runtime.CompilerServices.ITuple tuple)
            {
                StringBuilder? str = new();
                for (int i = 0; i < tuple.Length; i++)
                {
                    _ = str.Append('<')
                    .Append(System.Net.WebUtility.HtmlEncode(GetHashForObject(tuple[i])))
                    .Append('>');
                }

                return str.ToString();
            }
            string EnumberableToString(System.Collections.IEnumerable enumerable)
            {
                StringBuilder? str = new();
                foreach (object? item in enumerable)
                {
                    _ = str.Append('<')
                    .Append(System.Net.WebUtility.HtmlEncode(GetHashForObject(item)))
                    .Append('>');
                }

                return str.ToString();
            }
        }



        private string GetStringForObject(object obj)
        {
            string? result = objectToStingRepresentation?.Invoke(obj);
            if (result is null)
            {
                Type? type = obj.GetType();
                if (type.IsEnum)
                {
                    return obj.ToString() ?? string.Empty;
                }

                StringBuilder? str = new();
                foreach (System.Reflection.PropertyInfo? property in type.GetProperties(System.Reflection.BindingFlags.Instance).OrderBy(x => x.Name))
                {
                    _ = str.Append('<')
                    .Append(property.Name)
                    .Append("><")
                    .Append(GetHashForObject(property.GetValue(obj)))
                    .Append('>');
                }
                return str.ToString();
            }
            return result;
        }

        public void Warning(string message, Exception? e = null)
        {
            Console.WriteLine(message);
            if (e != null)
            {
                Console.WriteLine(e.ToString());
            }
        }


        public IDocument<T> CreateDocument<T>(T value, string contentHash, string id, MetadataContainer? metadata = null)
        {
            return new Document<T>(value, contentHash, id, metadata, this);
        }


        public System.IO.DirectoryInfo TempDir()
        {
            DirectoryInfo? directoryInfo = new(Path.Combine(TempFolder.FullName, Guid.NewGuid().ToString()));
            directoryInfo.Create();
            return directoryInfo;
        }
        public System.IO.DirectoryInfo ChachDir()
        {
            CacheFolder.Create();
            return CacheFolder;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        public async ValueTask DisposeAsync()
        {
            if (!disposedValue)
            {
                while (disposables.TryTake(out IDisposable? disposable))
                {
                    disposable.Dispose();
                }

                while (asyncDisposables.TryTake(out IAsyncDisposable? disposable))
                {
                    await disposable.DisposeAsync();
                }


                Helper.Delete.Readonly(TempFolder.FullName);
                await logger.DisposeAsync();
                disposedValue = true;
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

        public override bool Equals(object? obj)
        {
            if (obj is IGeneratorContext other)
            {
                return Equals(other);
            }

            return false;
        }
        public bool Equals(IGeneratorContext? other)
        {
            if (other is GeneratorContextWrapper wrapper)
            {
                return Equals(wrapper.BaseContext);
            }
            else if (other is GeneratorContext context)
            {
                return ReferenceEquals(this, context);
            }

            return false;
        }
    }

    internal class LoggerWrapper : ILogger
    {
        public LoggerWrapper(Logger baseLogger, string name)
        {
            BaseLogger = baseLogger ?? throw new ArgumentNullException(nameof(baseLogger));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public Logger BaseLogger { get; }
        public string Name { get; }

        public IDisposable Indent()
        {
            return ((ILogger)BaseLogger).Indent();
        }

        public void Info(string text)
        {
            ((ILogger)BaseLogger).Info($"{{{Name}}}\t{text}");
        }
        public void Error(string text)
        {
            ((ILogger)BaseLogger).Error($"{{{Name}}}\t{text}");
        }
        public void Verbose(string text)
        {
            ((ILogger)BaseLogger).Verbose($"{{{Name}}}\t{text}");
        }
    }
    // the logger is not owned, it can e.g. be the Console...
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
    internal class Logger : ILogger, IAsyncDisposable
    {
        private readonly System.CodeDom.Compiler.IndentedTextWriter logger;
#pragma warning restore CA1001 // Types that own disposable fields should be disposable

        internal Logger(TextWriter writer)
        {
            logger = new System.CodeDom.Compiler.IndentedTextWriter(writer);
        }

        public void Info(string text)
        {
            logger.WriteLine("[INFO]\t" + text);
        }
        public void Verbose(string text)
        {
            logger.WriteLine("[VERBOSE]\t" + text);
        }
        public void Error(string text)
        {
            logger.WriteLine("[ERROR]\t" + text);
        }

        public IDisposable Indent()
        {
            logger.Indent++;
            return new IndentWrapper(this);
        }

        public ValueTask DisposeAsync()
        {
            return logger.DisposeAsync();
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
                if (!disposedValue)
                {
                    logger.logger.Indent--;
                    disposedValue = true;
                }
            }
            #endregion

        }
    }
}

