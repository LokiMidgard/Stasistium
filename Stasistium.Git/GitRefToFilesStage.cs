using LibGit2Sharp;
using Stasistium.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class GitRefToFilesStage<TPreviousCache> : GeneratedHelper.Multiple.Simple.OutputMultiSimpleInputSingle1List0StageBase<GitRef, TPreviousCache, Stream>
        where TPreviousCache : class
    {


        public GitRefToFilesStage(StagePerformHandler<GitRef, TPreviousCache> input, GeneratorContext context) : base(input, context)
        {
        }

        protected override Task<ImmutableList<IDocument<Stream>>> Work(IDocument<GitRef> source, OptionToken options)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            var queue = new Queue<Tree>();
            queue.Enqueue(source.Value.Tip.Tree);

            var blobs = ImmutableList<IDocument<Stream>>.Empty.ToBuilder();

            while (queue.TryDequeue(out var tree))
            {
                foreach (var entry in tree)
                {
                    switch (entry.Target)
                    {
                        case Blob blob:
                            var document = new GitFileDocument(entry.Path, blob, this.Context, null);
                            blobs.Add(document);
                            break;

                        case Tree subTree:
                            queue.Enqueue(subTree);
                            break;

                        case GitLink link:
                            throw new NotSupportedException("Git link is not supported at the momtent");

                        default:
                            throw new NotSupportedException($"The type {entry.Target?.GetType().FullName ?? "<NULL>"} is not supported as target");
                    }
                }
            }

            return Task.FromResult(blobs.ToImmutable());
        }

    }

}
