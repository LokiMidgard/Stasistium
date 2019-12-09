using LibGit2Sharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StaticSite.Documents;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace StaticSite.Modules
{
    public class GitModule<TPreviousCache> : SingleInputModuleBase<ImmutableList<GitRef>, ImmutableDictionary<string, (GitRefType type, string hash)>, string, TPreviousCache>
    {
        private Repository? repo;
        private System.IO.DirectoryInfo? workingDir;


        public GitModule(ModulePerformHandler<string, TPreviousCache> input, GeneratorContext context) : base(input, context, true)
        {
        }

        protected override async Task<(ImmutableList<GitRef> result, BaseCache<ImmutableDictionary<string, (GitRefType type, string hash)>> cache)> Work((string result, BaseCache<TPreviousCache> cache) input, bool previousHadChanges, OptionToken options)
        {
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
                this.repo = await Task.Run(() => new Repository(Repository.Clone(source, this.workingDir.FullName, new CloneOptions() { IsBare = true }))).ConfigureAwait(false);
            }
            else if (options.Refresh)
            {
                // The git library is nor thread save, so we should not paralize this!
                foreach (var remote in this.repo.Network.Remotes)
                    await Task.Run(() => Commands.Fetch(this.repo, remote.Name, Array.Empty<string>(), new FetchOptions() { }, null)).ConfigureAwait(false);
            }
            // for branches we ignore the local ones. we just cloned the repo and the local one is the same as the remote.
            var refs = this.repo.Tags.Select(x => new GitRef(x, this.repo)).Concat(this.repo.Branches.Where(x => x.IsRemote).Select(x => new GitRef(x, this.repo))).ToImmutableList();
            return (list: refs, cache: BaseCache.Create(refs.ToImmutableDictionary(x => x.FrindlyName, x => (x.Type, x.Tip.Sha)), (new BaseCache[] { input.cache }).AsMemory()));
        }

       

        protected override Task<bool> Changed([System.Diagnostics.CodeAnalysis.AllowNull] ImmutableDictionary<string, (GitRefType type, string hash)> item1, ImmutableDictionary<string, (GitRefType type, string hash)> item2)
        {
            if (item2 is null)
                throw new ArgumentNullException(nameof(item2));


            if (item2 is null)
                return Task.FromResult(true);

            if (item2.Count != item2.Count)
                return Task.FromResult(false);

            foreach (var pair in item2)
            {
                if (!item2.TryGetValue(pair.Key, out var sha))
                    return Task.FromResult(false);
                if (sha.hash != pair.Value.hash || sha.type != pair.Value.type)
                    return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }


    }

}
