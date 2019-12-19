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
                CompressCache = false,
                Refresh = false
            };
            var s = System.Diagnostics.Stopwatch.StartNew();
            var g = startModule
                .GitModul()
                .Where(x => x.Id == "origin/master")
                .SingleEntry()
                .GitRefToFiles()
                .Sidecar()
                    .For<BookMetadata>(".metadata")
                .Where(x => System.IO.Path.GetExtension(x.Id) == ".md")
                .Select(x=>x.Markdown().MarkdownToHtml().TextToStream())
                //.Select(x=> { 
                
                //})
                .Persist(new DirectoryInfo("out"), generatorOptions)
                ;

            await g.UpdateFiles().ConfigureAwait(false);

        }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public class BookMetadata
        {
            public string Title { get; set; }
            public int Chapter { get; set; }
        }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}
