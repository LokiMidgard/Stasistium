using LibGit2Sharp;
using Stasistium.Documents;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class GitStage<TPreviousCache> : GeneratedHelper.Multiple.Simple.OutputMultiSimpleInputSingle1List0StageBase<string, TPreviousCache, GitRef>
        where TPreviousCache : class
    {
        private Repository? repo;
        private string? previousSource;
        private System.IO.DirectoryInfo? workingDir;


        public GitStage(StagePerformHandler<string, TPreviousCache> input, GeneratorContext context) : base(input, context, true)
        {
        }

        protected override async Task<ImmutableList<IDocument<GitRef>>> Work(IDocument<string> source, OptionToken options)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            if (this.repo is null || source.Value != this.previousSource)
            {
                if (this.repo != null)
                {
                    if (this.workingDir is null)
                        throw new InvalidOperationException("the working dir should exist if repo does.");
                    // TODO: Should we realy dispose this already?
                    // I think we need to track who else has a reference to an object cretaed by this repo :/
                    this.repo.Dispose();
                    // this.workingDir.Delete(true);
                }

                this.workingDir = this.Context.TempDir();
                this.previousSource = source.Value;
                this.repo = await Task.Run(() => new Repository(Repository.Clone(source.Value, this.workingDir.FullName, new CloneOptions() { IsBare = true }))).ConfigureAwait(false);
            }
            else if (options.Refresh)
            {
                // The git library is nor thread save, so we should not paralize this!
                foreach (var remote in this.repo.Network.Remotes)
                    await Task.Run(() => Commands.Fetch(this.repo, remote.Name, Array.Empty<string>(), new FetchOptions() { }, null)).ConfigureAwait(false);
            }


            // for branches we ignore the local ones. we just cloned the repo and the local one is the same as the remote.
            var refs = this.repo.Tags.Select(x => new GitRef(x, this.repo)).Concat(this.repo.Branches.Where(x => x.IsRemote).Select(x => new GitRef(x, this.repo)))
                .Select(x => this.Context.Create(x, x.Hash, x.FrindlyName)).OrderBy(x => x.Id).ToArray();
            return refs.ToImmutableList();


        }

        protected override Task<bool?> ForceUpdate((string id, string hash)[]? ids, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            return Task.FromResult<bool?>(options.Refresh);
        }
    }

}
