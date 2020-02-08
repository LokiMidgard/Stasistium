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
    public class SassStage<TSingleCache, TListCache, TListItemCache> : Stages.GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple1List1StageBase<string, TSingleCache, string, TListItemCache, TListCache, string>
        where TSingleCache : class
        where TListItemCache : class
        where TListCache : class
    {
        public SassStage(StagePerformHandler<string, TSingleCache> inputSingle0, StagePerformHandler<string, TListItemCache, TListCache> inputList0, IGeneratorContext context, string? name) : base(inputSingle0, inputList0, context, name)
        {
        }

        protected override Task<IDocument<string>> Work(IDocument<string> input, ImmutableList<IDocument<string>> all, OptionToken options)
        {
            if (input is null)
                throw new System.ArgumentNullException(nameof(input));
            var resolver = new RelativePathResolver(input.Id, all.Select(x => x.Id));
            var lookup = all.ToDictionary(x => x.Id, x => x);
            var result = Scss.ConvertToCss(input.Value, new ScssOptions()
            {
                InputFile = input.Id,
                TryImport = (string file, string path, out string? scss, out string? map) =>
                {
                    var combind = Path.Combine(path, file).Replace('\\', '/');

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

            return Task.FromResult(input
                .WithId(newId)
                .With(result.Css, this.Context.GetHashForString(result.Css)));
        }

    }
}
namespace Stasistium
{
    public static class SassExtension
    {
        public static Sass.SassStage<TSingleCache, TListCache, TListItemCache> Sass<TSingleCache, TListCache, TListItemCache>(this StageBase<string, TSingleCache> inputSingle0, MultiStageBase<string, TListItemCache, TListCache> inputList0, string? name = null)
                    where TSingleCache : class
        where TListItemCache : class
        where TListCache : class
        {
            if (inputSingle0 is null)
                throw new System.ArgumentNullException(nameof(inputSingle0));
            if (inputList0 is null)
                throw new System.ArgumentNullException(nameof(inputList0));
            return new Sass.SassStage<TSingleCache, TListCache, TListItemCache>(inputSingle0.DoIt, inputList0.DoIt, inputSingle0.Context, name);
        }
    }
}