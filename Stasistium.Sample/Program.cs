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
            using var context = new GeneratorContext();

            var configFile = context.StageFromResult("config.json", x => x)
                .File()
                .Json()
                .For<Config>();

            var contentRepo = configFile.Transform(x => x.With(x.Value.ContentRepo, x.Value.ContentRepo))
                .GitModul();

            var schemaRepo = configFile.Transform(x => x.With(x.Value.SchemaRepo, x.Value.SchemaRepo).With(x.Metadata.Add(new HostMetadata() { Host = x.Value.Host })))
                .GitModul();

            var layoutProvider = configFile.Transform(x => x.With(x.Value.Layouts, x.Value.Layouts)).FileSystem().FileProvider("Layout");

            var generatorOptions = new GenerationOptions()
            {
                CompressCache = false,
                Refresh = false
            };
            var s = System.Diagnostics.Stopwatch.StartNew();
            var files = contentRepo
                .SelectMany(input =>
                    input
                    .Transform(x => x.With(x.Metadata.Add(new GitMetadata() { Name = x.Value.FrindlyName, Type = x.Value.Type })))
                    .GitRefToFiles()
                    .Sidecar()
                        .For<BookMetadata>(".metadata")
                    .Where(x => System.IO.Path.GetExtension(x.Id) == ".md")
                    .Select(x => x.Markdown().MarkdownToHtml().TextToStream())
                    .Transform(x => x.WithId(Path.Combine(Enum.GetName(typeof(GitRefType), x.Metadata.GetValue<GitMetadata>()!.Type)!, x.Metadata.GetValue<GitMetadata>()!.Name, x.Id)))
                );
            //.Where(x => x.Id == "origin/master")
            //.SingleEntry()

            var razorProvider = files
                .FileProvider("Content")
                .Concat(layoutProvider)
                .RazorProvider("Content", "Layout/ViewStart.cshtml");

            var rendered = files.Select(x => x.Razor(razorProvider).TextToStream());
            var hostReplacementRegex = new System.Text.RegularExpressions.Regex(@"(?<host>http://nota\.org)/schema/", System.Text.RegularExpressions.RegexOptions.Compiled);

            var schemaFiles = schemaRepo

                .SelectMany(input =>
                 input
                 .Transform(x => x.With(x.Metadata.Add(new GitMetadata() { Name = x.Value.FrindlyName, Type = x.Value.Type })))
                 .GitRefToFiles()
                 .Where(x => System.IO.Path.GetExtension(x.Id) != ".md")
                 .Select(x =>
                    x.ToText()
                    .Transform(y =>
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
                    .TextToStream()
                 )
                 .Transform(x =>
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

                        );

            var g = rendered
                .Transform(x => x.WithId(Path.ChangeExtension(x.Id, ".html")))
                .Concat(schemaFiles)
                .Persist(new DirectoryInfo("out"), generatorOptions)
                ;

            await g.UpdateFiles().ConfigureAwait(false);

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
