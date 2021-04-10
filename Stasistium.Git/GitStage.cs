using LibGit2Sharp;
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

    public class GitRepo
    {
        public string? Url { get; set; }
        public string? PrimaryBranchName { get; set; }
    }

    public class GitCloneStage : StageBase<GitRepo, GitRefStage>
    {
        private readonly Dictionary<GitRepo, (DirectoryInfo workingDirectory, Repository repository)> repoLookup = new Dictionary<GitRepo, (DirectoryInfo workingDirectory, Repository repository)>();

        public GitCloneStage(IGeneratorContext context, string? name) : base(context, name)
        {
        }

        protected override async Task<ImmutableList<IDocument<GitRefStage>>> Work(ImmutableList<IDocument<GitRepo>> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            var builder = ImmutableList.CreateBuilder<IDocument<GitRefStage>>();

            foreach (var document in input)
            {
                // The git library is nor thread save, so we should not paralize this!
                await this.Work(document, options, builder).ConfigureAwait(false);
            }
            return builder.ToImmutable();
        }
        private async Task Work(IDocument<GitRepo> input, OptionToken options, ImmutableList<IDocument<GitRefStage>>.Builder builder)
        {
            Repository repo;
            DirectoryInfo workingDir;
            if (input.Value.Url is null)
                throw new System.ArgumentNullException("Url is null");
            if (this.repoLookup.TryGetValue(input.Value, out var oldData))
            {
                (workingDir, repo) = oldData;
                if (options.RefreshRemoteSources)
                {
                    // The git library is nor thread save, so we should not paralize this!
                    foreach (var remote in repo.Network.Remotes)
                        await Task.Run(() => Commands.Fetch(repo, remote.Name, Array.Empty<string>(), new FetchOptions() { }, null)).ConfigureAwait(false);
                }
            }
            else
            {
                workingDir = this.Context.TempDir();
                repo = await Task.Run(() =>
                {
                    var repo = new Repository(Repository.Clone(input.Value.Url, workingDir.FullName, new CloneOptions() { IsBare = true }));
                    return repo;
                }).ConfigureAwait(false);
                this.Context.DisposeOnDispose(repo);
            }

            // for branches we ignore the local ones. we just cloned the repo and the local one is the same as the remote.
            builder.AddRange(
             repo.Tags.Select(x => new GitRefStage(x, repo)).Concat(repo.Branches.Where(x => x.IsRemote).Select(x => new GitRefStage(x, repo)))
                .Select(x => this.Context.CreateDocument(x, x.Hash, x.FrindlyName).With(input.Metadata)).OrderBy(x => x.Id));
        }

    }


    public class GitCloneStringStage : StageBase<string, GitRefStage>
    {
        private readonly Dictionary<string, (DirectoryInfo workingDirectory, Repository repository)> repoLookup = new Dictionary<string, (DirectoryInfo workingDirectory, Repository repository)>();
        [StageName("GitClone")]
        public GitCloneStringStage(IGeneratorContext context, string? name) : base(context, name)
        {
        }

        protected override async Task<ImmutableList<IDocument<GitRefStage>>> Work(ImmutableList<IDocument<string>> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            var builder = ImmutableList.CreateBuilder<IDocument<GitRefStage>>();

            foreach (var document in input)
            {
                // The git library is nor thread save, so we should not paralize this!
                await this.Work(document, options, builder).ConfigureAwait(false);
            }
            return builder.ToImmutable();
        }
        private async Task Work(IDocument<string> input, OptionToken options, ImmutableList<IDocument<GitRefStage>>.Builder builder)
        {
            Repository repo;
            DirectoryInfo workingDir;

            if (this.repoLookup.TryGetValue(input.Value, out var oldData))
            {
                (workingDir, repo) = oldData;
                if (options.RefreshRemoteSources)
                {
                    // The git library is nor thread save, so we should not paralize this!
                    foreach (var remote in repo.Network.Remotes)
                        await Task.Run(() => Commands.Fetch(repo, remote.Name, Array.Empty<string>(), new FetchOptions() { }, null)).ConfigureAwait(false);
                }
            }
            else
            {
                workingDir = this.Context.TempDir();
                repo = await Task.Run(() => new Repository(Repository.Clone(input.Value, workingDir.FullName, new CloneOptions() { IsBare = true }))).ConfigureAwait(false);
                this.Context.DisposeOnDispose(repo);
            }

            // for branches we ignore the local ones. we just cloned the repo and the local one is the same as the remote.
            builder.AddRange(
             repo.Tags.Select(x => new GitRefStage(x, repo)).Concat(repo.Branches.Where(x => x.IsRemote).Select(x => new GitRefStage(x, repo)))
                .Select(x => this.Context.CreateDocument(x, x.Hash, x.FrindlyName).With(input.Metadata)).OrderBy(x => x.Id));
        }

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
