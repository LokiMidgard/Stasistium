using LibGit2Sharp;
using StaticSite.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StaticSite.Stages
{
    public class GitRefToFilesStage<TPreviousCache> : SingleInputMultipleStageBase<Stream, string, string, GitRef, TPreviousCache>
    {


        public GitRefToFilesStage(StagePerformHandler<GitRef, TPreviousCache> input, GeneratorContext context) : base(input, context)
        {
        }

        protected override Task<(ImmutableList<StageResult<Stream, string>> result, BaseCache<string> cache)> Work((IDocument<GitRef> result, BaseCache<TPreviousCache> cache) input, bool previousHadChanges, [AllowNull] string cache, [AllowNull] ImmutableDictionary<string, BaseCache<string>> childCaches, OptionToken options)
        {
            var source = input.result;

            var queue = new Queue<Tree>();
            queue.Enqueue(source.Value.Tip.Tree);

            var blobs = ImmutableList<StageResult<Stream, string>>.Empty.ToBuilder();

            while (queue.TryDequeue(out var tree))
            {
                foreach (var entry in tree)
                {
                    switch (entry.Target)
                    {
                        case Blob blob:
                            var document = new GitFileDocument(entry.Path, blob, this.Context, MetadataContainer.Empty);
                            var hasChanges = true;
                            if (childCaches != null && childCaches.TryGetKey(document.Id, out var oldFileHash))
                                hasChanges = oldFileHash != document.Hash;

                            blobs.Add(StageResult.Create(document, BaseCache.Create(document.Hash), hasChanges, document.Id));
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

            return Task.FromResult((result: blobs.ToImmutable(), cache: BaseCache.Create(source.Hash, input.cache)));
        }

    }

}
