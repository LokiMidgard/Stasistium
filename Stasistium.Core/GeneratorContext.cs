using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Stasistium.Documents
{
    public sealed class GeneratorContext : IDisposable
    {
        private readonly HashAlgorithm algorithm = SHA256.Create();
        private readonly Func<object, string?>? objectToStingRepresentation;

        public Logger Logger { get; }

        public DirectoryInfo CacheFolder { get; }
        public DirectoryInfo TempFolder { get; }

        public MetadataContainer EmptyMetadata { get; }

        public GeneratorContext(DirectoryInfo? cacheFolder = null, DirectoryInfo? tempFolder = null, Func<object, string?>? objectToStingRepresentation = null, TextWriter? logger = null)
        {
            this.CacheFolder = cacheFolder ?? new DirectoryInfo("Cache");
            this.TempFolder = tempFolder ?? new DirectoryInfo("Temp");
            this.EmptyMetadata = MetadataContainer.EmptyFromContext(this);
            this.objectToStingRepresentation = objectToStingRepresentation;
            this.Logger = new Logger(logger ?? Console.Out);
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

        public Stages.StaticStage<TResult> StageFromResult<TResult>(TResult result, Func<TResult, string> hashFunction)
    => new Stages.StaticStage<TResult>(result, hashFunction, this);



        internal string GetHashForObject(object? value)
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
                DateTime date => date.ToString(System.Globalization.CultureInfo.InvariantCulture),
                DateTimeOffset date => date.ToString(System.Globalization.CultureInfo.InvariantCulture),
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


        public IDocument<T> Create<T>(T value, string contentHash, string id, MetadataContainer? metadata = null)
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

        public void Dispose()
        {
            if (!this.disposedValue)
            {
                this.algorithm.Dispose();

                this.disposedValue = true;
            }
        }
        #endregion

        public Exception Exception(string message)
        {

            throw new NotImplementedException(message);
        }
    }

    public class Logger
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
