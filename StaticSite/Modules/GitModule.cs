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
    public class GitModule<TPreviousCache> : ModuleBase<ImmutableList<GitRef>, ImmutableDictionary<string, (GitRefType type, string hash)>>
    {
        private readonly ModulePerformHandler<string, TPreviousCache> input;
        private Repository? repo;
        private System.IO.DirectoryInfo? workingDir;


        public GitModule(ModulePerformHandler<string, TPreviousCache> input, GeneratorContext context) : base(context)
        {
            this.input = input;

        }
        protected override async Task<ModuleResult<ImmutableList<GitRef>, ImmutableDictionary<string, (GitRefType type, string hash)>>> Do(BaseCache<ImmutableDictionary<string, (GitRefType type, string hash)>>? cache, OptionToken options)
        {

            if (cache != null && cache.PreviousCache.Length != 1)
                throw new ArgumentException($"This cache should have exactly one predecessor but had {cache.PreviousCache}");
            var inputResult = await this.input(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
             {

                 var previousPerform = await inputResult.Perform;
                 var source = previousPerform.result;

                 if (this.repo is null || inputResult.HasChanges)
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
                 return (list: refs, cache: BaseCache.Create(refs.ToImmutableDictionary(x => x.FrindlyName, x => (x.Type, x.Tip.Sha)), new BaseCache[] { previousPerform.cache }.AsMemory()));
             });


            bool hasChanges = inputResult.HasChanges;

            if (options.Refresh || hasChanges)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                hasChanges = hasChanges = Changed(cache?.Item, result.cache.Item);

            }

            return ModuleResult.Create(task, hasChanges);
        }

        private static bool Changed(ImmutableDictionary<string, (GitRefType type, string hash)>? cache, ImmutableDictionary<string, (GitRefType type, string hash)> cache2)
        {
            if (cache2 is null)
                throw new ArgumentNullException(nameof(cache2));


            if (cache is null)
                return true;

            if (cache.Count != cache2.Count)
                return false;

            foreach (var pair in cache2)
            {
                if (!cache.TryGetValue(pair.Key, out var sha))
                    return false;
                if (sha.hash != pair.Value.hash || sha.type != pair.Value.type)
                    return false;
            }

            return true;
        }


    }

}
