using Stasistium.Documents;
using Stasistium.Stages;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class GitCommitsStage : StageBase<GitRefStage, GitReposetoryMetadata>
    {
        public GitCommitsStage(IGeneratorContext context, string? name) : base(context, name)
        {
             
        }

        protected override async Task<ImmutableList<IDocument<GitReposetoryMetadata>>> Work(ImmutableList<IDocument<GitRefStage>> input, OptionToken options)
        {
            if (input is null)
                throw new System.ArgumentNullException(nameof(input));
            
            var documents = await Task.WhenAll(input.Select(async refDocument =>
                       {
                           var x = refDocument.Value;
                           var gitReposetoryMetadata = await Task.Run(() => new GitReposetoryMetadata(x.GetCommits().Select(y => new Commit(y)).ToImmutableList())).ConfigureAwait(false);
                           return refDocument.With(gitReposetoryMetadata, gitReposetoryMetadata.Commits.First().Sha);
                       })).ConfigureAwait(false);

            return documents.ToImmutableList();
        }

    }
}
