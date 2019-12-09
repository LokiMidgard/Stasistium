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

        private readonly Repository repository;

        internal ICommitLog Commits
        {
            get { return this.repository.Commits.QueryBy(new CommitFilter { IncludeReachableFrom = Tip }); }
        }



        internal GitRef(Branch branch, Repository repository)
        {
            this.Type = GitRefType.Branch;
            this.Tip = branch.Tip;
            this.FrindlyName = branch.FriendlyName;
            this.repository = repository;
            this.Hash = this.Tip.Sha;
        }

        internal GitRef(Tag tag, Repository repository)
        {
            this.Type = GitRefType.Tag;
            this.Tip = (Commit)tag.Target;
            this.FrindlyName = tag.FriendlyName;
            this.repository = repository;
            this.Hash = this.Tip.Sha;
        }
    }
    public enum GitRefType
    {
        Branch,
        Tag
    }

}
