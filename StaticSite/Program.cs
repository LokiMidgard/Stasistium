using StaticSite.Documents;
using StaticSite.Stages;
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace StaticSite
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var context = new GeneratorContext();
            var startModule = Stage.FromResult("https://github.com/nota-game/nota.git", x => x, context);
            var generatorOptions = new GenerationOptions()
            {
                CompressCache = true,
                Refresh = false
            };
            var s = System.Diagnostics.Stopwatch.StartNew();
            var g = startModule
                .GitModul()
                .Where(x => x.Id == "origin/master")
                .Single()
                .GitRefToFiles()
                .Where(x => System.IO.Path.GetExtension(x.Id) == ".md")
                .Persist(new DirectoryInfo("out"), generatorOptions)
                ;

            await g.UpdateFiles().ConfigureAwait(false);

        }
    }
}
