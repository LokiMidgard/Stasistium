using Stasistium.Documents;
using Stasistium.Stages;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class GitCommitsStage<T> : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple1List0StageBase<GitRefStage, T, GitReposetoryMetadata>
        where T : class
    {
        public GitCommitsStage(StageBase<GitRefStage, T> inputSingle0, IGeneratorContext context, string? name) : base(inputSingle0, context, name)
        {
        }

        protected override async Task<IDocument<GitReposetoryMetadata>> Work(IDocument<GitRefStage> input, OptionToken options)
        {
            if (input is null)
                throw new System.ArgumentNullException(nameof(input));
            var x = input.Value;
            var gitReposetoryMetadata = await Task.Run(() => new GitReposetoryMetadata(x.GetCommits().Select(y => new Commit(y)).ToImmutableList())).ConfigureAwait(false);
            return input.With(gitReposetoryMetadata, gitReposetoryMetadata.Commits.First().Sha);
        }
    }
}

namespace Stasistium
{
    public static partial class GitStageExtension
    {
        public static GitCommitsStage<T> GitCommits<T>(this StageBase<GitRefStage, T> input, string? name = null)
            where T : class
        {
            if (input is null)
                throw new System.ArgumentNullException(nameof(input));
            return new GitCommitsStage<T>(input, input.Context, name);
        }
    }
}


