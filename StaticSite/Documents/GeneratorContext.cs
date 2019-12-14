using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace StaticSite.Documents
{
    public class GeneratorContext : IDisposable
    {
        private readonly HashAlgorithm algorithm = SHA256.Create();
        private readonly Func<object, string?>? objectToStingRepresentation;

        public DirectoryInfo CacheFolder { get; }
        public DirectoryInfo TempFolder { get; }

        public MetadataContainer EmptyMetadata { get; } 

        public GeneratorContext(DirectoryInfo? cacheFolder = null, DirectoryInfo? tempFolder = null, Func<object, string?>? objectToStingRepresentation = null)
        {
            this.CacheFolder = cacheFolder ?? new DirectoryInfo("Cache");
            this.TempFolder = tempFolder ?? new DirectoryInfo("Temp");
            this.EmptyMetadata = MetadataContainer.EmptyFromContext(this);
            this.objectToStingRepresentation = objectToStingRepresentation;
        }

        public string GetHashForString(string toHash)
        {
            var bytes = this.algorithm.ComputeHash(Encoding.UTF8.GetBytes(toHash));

            var sb = new StringBuilder(bytes.Length << 1);
            foreach (byte b in bytes)
                sb.Append(b.ToString("X2", System.Globalization.CultureInfo.InvariantCulture));

            return sb.ToString();
        }



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
                throw new InvalidCastException($"For type {obj.GetType().FullName} exists no convertion to string. Use the {nameof(this.objectToStingRepresentation)} paramter of {nameof(GeneratorContext)}.");
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


        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.algorithm.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                this.disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~GeneratorContext()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        internal Exception Exception(string message)
        {
            throw new NotImplementedException(message);
        }
        #endregion
    }

}
