using SharpScss;

using Stasistium;
using Stasistium.Documents;
using Stasistium.Stages;

using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
                RelativePathResolver? resolver = new RelativePathResolver(input.Id, all.Select(x => x.Id));
                System.Collections.Generic.Dictionary<string, IDocument<string>>? lookup = all.ToDictionary(x => x.Id, x => x);
                ScssResult result = Scss.ConvertToCss(input.Value, new ScssOptions()
                {
                    InputFile = input.Id,
                    TryImport = (ref string file, string path, out string? scss, out string? map) =>
                    {
                        // don't know where Scss gets the full path when we give only text and relative path.
                        string combind = file.Replace('\\', '/');

                        string? IdToSearch = resolver[combind];
                        if (IdToSearch is null)
                        {
                            scss = null;
                            map = null;
                            return false;
                        }

                        IDocument<string> otherDocument = lookup[IdToSearch];

                        scss = otherDocument.Value; // TODO: handle the loading of scss for the specified file
                        map = null;
                        return true;
                    }
                });

                string? newId = input.Id;
                if (Path.GetExtension(newId) == ".scss")
                {
                    newId = Path.ChangeExtension(newId, ".css");
                }

                return input
                    .WithId(newId)
                    .With(result.Css, Context.GetHashForString(result.Css));

            }).ToImmutableList());


        }


    }
}