using LibGit2Sharp;
using Stasistium.Core;
using Stasistium.Documents;
using Stasistium.Helper;
using Stasistium.Stages;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Stasistium.Stages
{

    public class GitCloneStage<T> : MultiStageBase<GitRefStage, string, GitCache<T>>
        where T : class
    {
        private readonly StageBase<string, T> input;

        public GitCloneStage(StageBase<string, T> input, IGeneratorContext context, string? name) : base(context, name)
        {
            this.input = input;
        }

        protected override async Task<StageResultList<GitRefStage, string, GitCache<T>>> DoInternal([AllowNull] GitCache<T>? cache, OptionToken options)
        {
            var result = await this.input.DoIt(cache?.PreviousCache, options).ConfigureAwait(false);

            var task = LazyTask.Create(async () =>
            {
                var source = await result.Perform;

                Repository repo;
                DirectoryInfo workingDir;
                if (cache != null && source.Value == cache.PreviousSource && cache.Repo != null)
                {

                    repo = cache.Repo;
                    workingDir = new DirectoryInfo(cache.WorkingDir ?? throw new InvalidOperationException("the working dir should exist if repo does."));
                    if (options.RefreshRemoteSources)
                    {
                        // The git library is nor thread save, so we should not paralize this!
                        foreach (var remote in repo.Network.Remotes)
                            await Task.Run(() => Commands.Fetch(repo, remote.Name, Array.Empty<string>(), new FetchOptions() { }, null)).ConfigureAwait(false);
                    }
                }
                else
                {

                    if (cache?.Repo != null) // remote url changed
                    {
                        if (cache.WorkingDir is null)
                            throw new InvalidOperationException("the working dir should exist if repo does.");
                        cache.Repo.Dispose();
                        Delete.Readonly(cache.WorkingDir);
                    }

                    workingDir = this.Context.TempDir();
                    repo = await Task.Run(() => new Repository(Repository.Clone(source.Value, workingDir.FullName, new CloneOptions() { IsBare = true }))).ConfigureAwait(false);
                    this.Context.DisposeOnDispose(repo);
                }


                // for branches we ignore the local ones. we just cloned the repo and the local one is the same as the remote.
                var refs = repo.Tags.Select(x => new GitRefStage(x, repo)).Concat(repo.Branches.Where(x => x.IsRemote).Select(x => new GitRefStage(x, repo)))
                    .Select(x => this.Context.CreateDocument(x, x.Hash, x.FrindlyName).With(source.Metadata)).OrderBy(x => x.Id)
                    .Select(x =>
                    {
                        if (cache is null || !cache.IdToHash.TryGetValue(x.Id, out string? oldHash))
                            oldHash = null;
                        return this.Context.CreateStageResult(x, x.Hash != oldHash, x.Id, x.Hash, x.Hash);
                    })
                    .ToArray();

                var hash = this.Context.GetHashForObject(refs.Select(x => x.Hash));

                var newCache = new GitCache<T>(repo, result.Cache, source.Value, workingDir.FullName, hash, refs.Select(x => x.Id).ToArray(), refs.ToDictionary(x => x.Id, x => x.Hash), this.Context);

                return (result: refs.ToImmutableList(), newCache);
            });

            bool hasChanges;
            GitCache<T> newCache;

            // we need to update the cache and can't use the old one since wie store the repo in it that will not serelized
            //if (result.HasChanges || cache is null)
            {
                var temp = await task;
                newCache = temp.newCache;
                hasChanges = cache is null
                    || cache.Hash != newCache.Hash
                    || temp.result.Any(x => x.HasChanges);
            }
            //else
            //{
            //    hasChanges = false;
            //    newCache = cache;
            //}


            var actualTask = LazyTask.Create(async () => { return (await task).result; });
            return this.Context.CreateStageResultList(actualTask, hasChanges, newCache.Ids.ToImmutableList(), newCache, newCache.Hash);
        }
    }

    public class GitCache<T>
        where T : class
    {
        private readonly string createdBy;
        internal Repository? Repo { get; private set; }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        [Obsolete("Only for Deserialisation", true)]
        public GitCache()
        {
            this.createdBy = "Serelisation";
        }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        public GitCache(Repository repo, T previousCache, string previousSource, string workingDir, string hash, string[] ids, Dictionary<string, string> idToHash, IGeneratorContext context)
        {
            this.createdBy = "Normal";

            if (string.IsNullOrEmpty(hash))
                throw new ArgumentException("message", nameof(hash));
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            this.Repo = repo ?? throw new ArgumentNullException(nameof(repo));
            this.PreviousCache = previousCache ?? throw new ArgumentNullException(nameof(previousCache));
            this.PreviousSource = previousSource ?? throw new ArgumentNullException(nameof(previousSource));
            this.WorkingDir = workingDir ?? throw new ArgumentNullException(nameof(workingDir));
            this.Hash = hash;
            this.Ids = ids ?? throw new ArgumentNullException(nameof(ids));
            this.IdToHash = idToHash ?? throw new ArgumentNullException(nameof(idToHash));
#pragma warning disable CA2000 // Dispose objects before losing scope
            context.DisposeOnDispose(new Disposer(this));
#pragma warning restore CA2000 // Dispose objects before losing scope
        }

        private class Disposer : IDisposable
        {
            private GitCache<T> parent;

            public Disposer(GitCache<T> parent)
            {
                this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
            }

            public void Dispose()
            {
                if (this.parent.Repo is null)
                    return;
                this.parent.Repo.Dispose();
                this.parent.Repo = null;
            }
        }

        public T PreviousCache { get; set; }
        public string PreviousSource { get; set; }
        public string WorkingDir { get; set; }
        public string Hash { get; set; }
        public string[] Ids { get; set; }
        public Dictionary<string, string> IdToHash { get; set; }
    }

    public class GitReposetoryMetadata
    {
        public GitReposetoryMetadata(ImmutableList<Commit> commits)
        {
            this.Commits = commits ?? throw new ArgumentNullException(nameof(commits));
        }

        public ImmutableList<Commit> Commits { get; }
    }

}
