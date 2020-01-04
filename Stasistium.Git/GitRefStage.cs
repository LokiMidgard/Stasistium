using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stasistium.Documents
{
    public class GitRefStage
    {
        public GitRefType Type { get; }
        public string FrindlyName { get; }
        internal Commit Tip { get; }

        public string Hash { get; }

        internal IEnumerable<Commit> GetCommits(string? path = null)
        {
            if (path is null)
                return this.Repository.Commits.QueryBy(new CommitFilter { IncludeReachableFrom = Tip, SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Time });
            return this.Repository.Commits.QueryBy(path, new CommitFilter { IncludeReachableFrom = Tip, SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Time }).Select(x => x.Commit);
        }

        internal Repository Repository { get; }

        internal GitRefStage(Branch branch, Repository repository)
        {
            this.Type = GitRefType.Branch;
            this.Tip = branch.Tip;
            if (branch.FriendlyName.StartsWith("origin/", StringComparison.InvariantCultureIgnoreCase))
                this.FrindlyName = branch.FriendlyName.Substring("origin/".Length);
            else
                this.FrindlyName = branch.FriendlyName;
            this.Repository = repository;
            this.Hash = this.Tip.Sha;
        }

        internal GitRefStage(Tag tag, Repository repository)
        {
            this.Type = GitRefType.Tag;
            this.Tip = (Commit)tag.Target;
            this.FrindlyName = tag.FriendlyName;
            this.Repository = repository;
            this.Hash = this.Tip.Sha;
        }
    }
    public enum GitRefType
    {
        Branch,
        Tag
    }

}
