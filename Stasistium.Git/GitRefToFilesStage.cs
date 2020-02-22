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
    public class GitRefToFilesStage<TPreviousCache> : GeneratedHelper.Multiple.Simple.OutputMultiSimpleInputSingle1List0StageBase<GitRefStage, TPreviousCache, Stream>
        where TPreviousCache : class
    {


        public GitRefToFilesStage(StageBase<GitRefStage, TPreviousCache> input, IGeneratorContext context, string? name) : base(input, context, name)
        {
        }

        protected override async Task<ImmutableList<IDocument<Stream>>> Work(IDocument<GitRefStage> source, OptionToken options)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            var queue = new Queue<Tree>();
            queue.Enqueue(source.Value.Tip.Tree);

            var blobs = ImmutableList<IDocument<Stream>>.Empty.ToBuilder();

            while (queue.TryDequeue(out var tree))
            {
                var all = await Task.WhenAll(tree.Select(async entry =>
                {
                    switch (entry.Target)
                    {
                        case Blob blob:
                            var document = new GitFileDocument(entry.Path, blob, this.Context, null).With(source.Metadata);
                            var commits = await Task.Run(() => source.Value.GetCommits(entry.Path).Select(x => new Commit(x))).ConfigureAwait(false);
                            document = document.With(document.Metadata.Add(new GitRefToFilesStage<TPreviousCache>.Metadata(commits.ToImmutableList())));
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

            return blobs.ToImmutable();
        }

        public class Metadata
        {
            public Metadata(ImmutableList<Commit> commits)
            {
                this.FileCommits = commits;
            }

            public ImmutableList<Commit> FileCommits { get; }
        }


    }
    public class Commit
    {
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
