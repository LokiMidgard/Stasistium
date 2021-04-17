using LibGit2Sharp;
using Stasistium.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class GitRefToFilesStage : StageBase<GitRefStage, Stream>
    {
        private readonly bool addGitMetadata;

        [StageName("Files")]
        public GitRefToFilesStage(bool addGitMetadata, IGeneratorContext context, string? name) : base(context, name)
        {
            this.addGitMetadata = addGitMetadata;
        }

        protected override async Task<ImmutableList<IDocument<Stream>>> Work(ImmutableList<IDocument<GitRefStage>> sources, OptionToken options)
        {
            if (sources is null)
                throw new ArgumentNullException(nameof(sources));

            var blobs = ImmutableList<IDocument<Stream>>.Empty.ToBuilder();
            foreach (var source in sources)
            {
                var queue = new Queue<Tree>();
                queue.Enqueue(source.Value.Tip.Tree);

                while (queue.TryDequeue(out var tree))
                {
                    var all = await Task.WhenAll(tree.Select(async entry =>
                    {
                        switch (entry.Target)
                        {
                            case Blob blob:
                                var document = new GitFileDocument(entry.Path, blob, this.Context, null).With(source.Metadata);
                                if (this.addGitMetadata)
                                {
                                    var commits = await Task.Run(() => source.Value.GetCommits(entry.Path).Select(x => new Commit(x))).ConfigureAwait(false);
                                    document = document.With(document.Metadata.Add(new GitMetadata(commits.ToImmutableList())));
                                }
                                return document as object;

                            case Tree subTree:
                                return subTree as object;

                            case GitLink link:
                                throw new NotSupportedException("Git link is not supported at the momtent");

                            default:
                                throw new NotSupportedException($"The type {entry.Target?.GetType().FullName ?? "<NULL>"} is not supported as target");
                        }
                    })).ConfigureAwait(false);

                    foreach (var subTree in all.OfType<Tree>())
                        queue.Enqueue(subTree);

                    blobs.AddRange(all.OfType<IDocument<Stream>>());
                }

            }
            return blobs.ToImmutable();
        }
    }

    public class GitMetadata
    {
        private GitMetadata()
        {

        }
        public GitMetadata(ImmutableList<Commit> commits)
        {
            this.FileCommits = commits;
        }

        public ImmutableList<Commit> FileCommits { get; private set; }
    }



    public class Commit
    {
        private Commit() { }
        public Commit(string sha, string message, Signature author, Signature committer)
        {
            this.Sha = sha ?? throw new ArgumentNullException(nameof(sha));
            this.Message = message ?? throw new ArgumentNullException(nameof(message));
            this.Author = author ?? throw new ArgumentNullException(nameof(author));
            this.Committer = committer ?? throw new ArgumentNullException(nameof(committer));
        }

        internal Commit(LibGit2Sharp.Commit x)
        {
            this.Sha = x.Sha;
            this.Message = x.Message;
            this.Author = new Signature(x.Author);
            this.Committer = new Signature(x.Committer);
        }

        public string Sha { get; }
        public string Message { get; }
        public Signature Author { get; }
        public Signature Committer { get; }
    }
    public class Signature
    {
        public Signature(string name, string email, DateTimeOffset date)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Email = email ?? throw new ArgumentNullException(nameof(email));
            this.Date = date;
        }

        internal Signature(LibGit2Sharp.Signature signature)
        {
            this.Name = signature.Name;
            this.Email = signature.Email;
            this.Date = signature.When;
        }

        public string Name { get; }
        public string Email { get; }
        public DateTimeOffset Date { get; }
    }
}
