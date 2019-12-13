using LibGit2Sharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StaticSite.Documents;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace StaticSite.Stages
{
    public class GitStage<TPreviousCache> : SingleInputMultipleStageBase<GitRef, (GitRefType type, string hash), ImmutableList<string>, string, TPreviousCache>
    {
        private Repository? repo;
        private System.IO.DirectoryInfo? workingDir;


        public GitStage(StagePerformHandler<string, TPreviousCache> input, GeneratorContext context) : base(input, context, true)
        {
        }


        protected override async Task<(ImmutableList<StageResult<GitRef, (GitRefType type, string hash)>> result, BaseCache<ImmutableList<string>> cache)> Work((IDocument<string> result, BaseCache<TPreviousCache> cache) input, bool previousHadChanges, [AllowNull] ImmutableList<string> cache, [AllowNull] ImmutableDictionary<string, BaseCache<(GitRefType type, string hash)>> childCaches, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            var source = input.result;

            if (this.repo is null || previousHadChanges)
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
                .Select(x =>
                {
                    var hasChanges = true;
                    if (childCaches != null && childCaches.TryGetValue(x.FrindlyName, out var o))
                        hasChanges = o.Item.hash != x.Hash || o.Item.type != x.Type;
                    var itemCache = BaseCache.Create((x.Type, x.Hash));
                    return StageResult.Create(this.Context.Create(x, x.Hash, x.FrindlyName), itemCache, hasChanges, x.FrindlyName);
                }).OrderBy(x => x.Id).ToArray();
            return (refs.ToImmutableList(), BaseCache.Create(refs.Select(x => x.Id).ToImmutableList(), input.cache));

        }

    }

}
