using System;
using System.IO;

namespace StaticSite.Documents
{
    public class GeneratorContext
    {
        private const string TempFolder = "Temp";
        public System.IO.DirectoryInfo TempDir()
        {
            return new DirectoryInfo(Path.Combine(TempFolder, Guid.NewGuid().ToString()));
        }
        public System.IO.DirectoryInfo ChachDir()
        {
            throw new NotImplementedException();
        }
    }

}
