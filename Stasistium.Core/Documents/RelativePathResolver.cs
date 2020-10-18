using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;
using Stasistium;

namespace Stasistium.Documents
{
    public class RelativePathResolver
    {
        private readonly string relativeTo;
        public readonly Dictionary<string, string> lookup;

        public string? this[string index]
        {
            get
            {
                if (this.lookup.TryGetValue(index, out var result))
                    return result;
                return null;
            }
        }

        public RelativePathResolver(string relativeTo, IEnumerable<string> documents)
        {
            this.relativeTo = relativeTo;
            this.lookup = documents.SelectMany(this.GetPathes).ToDictionary(x => x.relativeOrFullPath, x => x.fullPath);
        }

        private IEnumerable<(string relativeOrFullPath, string fullPath)> GetPathes(string fullpath)
        {
            yield return ("/" + fullpath, fullpath);

            var currentFolder = System.IO.Path.GetDirectoryName(this.relativeTo)?.Replace('\\', '/');
            int depth = 0;
            while (true)
            {
                if (currentFolder is null)
                    yield break;
                if (!currentFolder.EndsWith('/') && currentFolder.Length > 0)
                    currentFolder += '/';
                if (fullpath.StartsWith(currentFolder, StringComparison.InvariantCulture))
                {
                    var prefix = string.Join("", Enumerable.Repeat("../", depth));
                    yield return (prefix + fullpath.Substring(currentFolder.Length), fullpath);
                    yield break;
                }
                currentFolder = currentFolder.TrimEnd('/');
                depth++;
                currentFolder = System.IO.Path.GetDirectoryName(currentFolder)?.Replace('\\', '/');

            }
        }

        public RelativePathResolver ForPath(string newPath)
        {
            var absolutePath = this[newPath];
            if (absolutePath is null)
                throw new ArgumentOutOfRangeException(nameof(newPath), $"Path {newPath} was not found.");
            return new RelativePathResolver(absolutePath, this.lookup.Values.Distinct());
        }
    }


}
