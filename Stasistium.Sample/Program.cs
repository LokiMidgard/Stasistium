using Stasistium.Documents;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Stasistium.Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await using var context = new GeneratorContext();

            var configFile = context.StageFromResult("config ", "config.json", x => x)
                .File()
                .Json<Config>();

            var contentRepo = configFile.Select(x => x.With(x.Value.ContentRepo, x.Value.ContentRepo))
                .GitClone();

            var schemaRepo = configFile.Select(x => x.With(x.Value.SchemaRepo, x.Value.SchemaRepo).With(x.Metadata.Add(new HostMetadata() { Host = x.Value.Host })))
                .GitClone();

            var layoutProvider = configFile
                .Select(x => x.With(x.Value.Layouts, x.Value.Layouts))
                .FileSystem()
                .FileProvider("Layout");

            var generatorOptions = new GenerationOptions()
            {
                CompressCache = false,
                Refresh = false
            };
            var s = System.Diagnostics.Stopwatch.StartNew();
            var files = contentRepo
                .Where(x => true)
                .Select(x => x.With(x.Metadata.Add(new GitMetadata() { Name = x.Value.FrindlyName, Type = x.Value.Type })))
                .Files(true)
                .Sidecar<BookMetadata>(".metadata")
                   .Where(x => System.IO.Path.GetExtension(x.Id) == ".md")
                   .Markdown()
                   .ToHtml()
                   .ToStream()
                    .Select(x => x.WithId(Path.Combine(Enum.GetName(typeof(GitRefType), x.Metadata.GetValue<GitMetadata>()!.Type)!, x.Metadata.GetValue<GitMetadata>()!.Name, x.Id)));
            //.Where(x => x.Id == "origin/master")
            //.SingleEntry()

            var razorProvider = files
                .FileProvider("Content")
                .Concat(layoutProvider)
                .RazorProvider("Content", viewStartId: "Layout/ViewStart.cshtml");

            var rendered = files.Razor(razorProvider).ToStream();
            var hostReplacementRegex = new System.Text.RegularExpressions.Regex(@"(?<host>http://nota\.org)/schema/", System.Text.RegularExpressions.RegexOptions.Compiled);

            var schemaFiles = schemaRepo
                .Select(x => x.With(x.Metadata.Add(new GitMetadata() { Name = x.Value.FrindlyName, Type = x.Value.Type })))
                 .Files(true)
                 .Where(x => System.IO.Path.GetExtension(x.Id) != ".md")

                    .ToText()
                    .Select(y =>
                    {
                        var gitData = y.Metadata.GetValue<GitMetadata>()!;
                        string version;

                        if (gitData.Type == GitRefType.Branch && gitData.Name == "master")
                            version = "vNext";
                        else if (gitData.Type == GitRefType.Branch)
                            version = "draft/" + gitData.Name;
                        else
                            version = gitData.Name;


                        var host = y.Metadata.GetValue<HostMetadata>()!.Host;

                        var newText = hostReplacementRegex.Replace(y.Value, @$"{host}/schema/{version}/");

                        return y.With(newText, y.Context.GetHashForString(newText));
                    })
                    .ToStream()

                 .Select(x =>
                 {
                     var gitData = x.Metadata.GetValue<GitMetadata>()!;
                     string version;

                     if (gitData.Type == GitRefType.Branch && gitData.Name == "master")
                         version = "vNext";
                     else if (gitData.Type == GitRefType.Branch)
                         version = "draft/" + gitData.Name;
                     else
                         version = gitData.Name;


                     return x.WithId($"schema/{version}/{x.Id.TrimStart('/')}");
                 })

                ;


            rendered
                .Select(x => x.WithId(Path.ChangeExtension(x.Id, ".html")))
                .Concat(schemaFiles)
                .Persist(new DirectoryInfo("out"))

                ;

            await context.Run(generatorOptions);
            //await g.UpdateFiles().ConfigureAwait(false);

        }


#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public class GitMetadata
        {
            public string Name { get; internal set; }
            public GitRefType Type { get; internal set; }
        }

        public class BookMetadata
        {
            public string Title { get; set; }
            public int Chapter { get; set; }
        }
    }

    public class Config
    {
        public string ContentRepo { get; set; }
        public string SchemaRepo { get; set; }
        public string Layouts { get; set; }

        public string Host { get; set; }
    }

    public class HostMetadata
    {
        public string Host { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

    public class PageLayoutMetadata
    {
        public string? Layout { get; set; }
    }



}
