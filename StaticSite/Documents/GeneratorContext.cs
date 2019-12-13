using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace StaticSite.Documents
{
    public class GeneratorContext : IDisposable
    {
        private const string TempFolder = "Temp";
        private readonly HashAlgorithm algorithm = SHA256.Create();

        public string GetHashForString(string toHash)
        {
            var bytes = this.algorithm.ComputeHash(Encoding.UTF8.GetBytes(toHash));

            var sb = new StringBuilder(bytes.Length << 1);
            foreach (byte b in bytes)
                sb.Append(b.ToString("X2", System.Globalization.CultureInfo.InvariantCulture));

            return sb.ToString();
        }

        public IDocument<T> Create<T>(T value, string contentHash, string id, MetadataContainer? metadata = null)
        {
            return new Document<T>(value, contentHash, id, metadata, this);
        }


        public System.IO.DirectoryInfo TempDir()
        {
            return new DirectoryInfo(Path.Combine(TempFolder, Guid.NewGuid().ToString()));
        }
        public System.IO.DirectoryInfo ChachDir()
        {
            throw new NotImplementedException();
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
