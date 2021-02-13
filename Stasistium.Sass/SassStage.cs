using SharpScss;
using Stasistium.Documents;
using Stasistium.Stages;
using System.Linq;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Stasistium;

namespace Stasistium.Sass
{
    public class SassStage : StageBase<string, string>
    {
        public SassStage(IGeneratorContext context, string? name) : base(context, name)
        {
        }

        protected override Task<ImmutableList<IDocument<string>>> Work(ImmutableList<IDocument<string>> all, OptionToken options)
        {
            return Task.FromResult(all.Select(input =>
            {
                var resolver = new RelativePathResolver(input.Id, all.Select(x => x.Id));
                var lookup = all.ToDictionary(x => x.Id, x => x);
                var result = Scss.ConvertToCss(input.Value, new ScssOptions()
                {
                    InputFile = input.Id,
                    TryImport = (string file, string path, out string? scss, out string? map) =>
                    {
                        // don't know where Scss gets the full path when we give only text and relative path.
                        var combind = file.Replace('\\', '/');

                        var IdToSearch = resolver[combind];
                        if (IdToSearch is null)
                        {
                            scss = null;
                            map = null;
                            return false;
                        }

                        var otherDocument = lookup[IdToSearch];

                        scss = otherDocument.Value; // TODO: handle the loading of scss for the specified file
                        map = null;
                        return true;
                    }
                });

                var newId = input.Id;
                if (Path.GetExtension(newId) == ".scss")
                    newId = Path.ChangeExtension(newId, ".css");

                return input
                    .WithId(newId)
                    .With(result.Css, this.Context.GetHashForString(result.Css));

            }).ToImmutableList());


        }
      

    }
}