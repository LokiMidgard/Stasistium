using LibGit2Sharp;
using System;

namespace StaticSite.Documents
{
    public class GitRef
    {
        public GitRefType Type { get; }
        public string FrindlyName { get; }
        internal Commit Tip { get; }

        public string Hash { get; }

        internal ICommitLog Commits
        {
            get { return this.Repository.Commits.QueryBy(new CommitFilter { IncludeReachableFrom = Tip }); }
        }

        internal Repository Repository { get; }

        internal GitRef(Branch branch, Repository repository)
        {
            this.Type = GitRefType.Branch;
            this.Tip = branch.Tip;
            this.FrindlyName = branch.FriendlyName;
            this.Repository = repository;
            this.Hash = this.Tip.Sha;
        }

        internal GitRef(Tag tag, Repository repository)
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
