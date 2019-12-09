using LibGit2Sharp;
using StaticSite.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace StaticSite.Modules
{
    public class GitRefToFiles<TPreviousCache> : ModuleBase<ImmutableList<IDocument>, string>
    {
        private readonly ModulePerformHandler<GitRef, TPreviousCache> input;


        public GitRefToFiles(ModulePerformHandler<GitRef, TPreviousCache> input, GeneratorContext context) : base(context)
        {
            this.input = input ?? throw new ArgumentNullException(nameof(input));
        }



        protected override async Task<ModuleResult<ImmutableList<IDocument>, string>> Do([AllowNull]BaseCache<string> cache, OptionToken options)
        {
            if (cache != null && cache.PreviousCache.Length != 1)
                throw new ArgumentException($"This cache should have exactly one predecessor but had {cache.PreviousCache}");
            var inputResult = await this.input(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
            {
                var previousPerform = await inputResult.Perform;
                var source = previousPerform.result;

                var queue = new Queue<Tree>();
                queue.Enqueue(source.Tip.Tree);

                var blobs = ImmutableList<IDocument>.Empty.ToBuilder();

                while (queue.TryDequeue(out var tree))
                {
                    foreach (var entry in tree)
                    {
                        switch (entry.Target)
                        {
                            case Blob blob:
                                var hash = HexHelper.FromHexString(blob.Sha).AsMemory();
                                var document = new FileDocument(entry.Path, hash, () => blob.GetContentStream());
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

                return (result: blobs.ToImmutable(), cache: new BaseCache<string>(source.Tip.Sha, new BaseCache[] { previousPerform.cache }.AsMemory()));
            });


            bool hasChanges = inputResult.HasChanges;

            if (hasChanges)
            {
                // if we have changes we'll check if there are acall changes.
                // since the task is cached in LazyTask, we will NOT perform the work twice.
                var result = await task;
                hasChanges = cache?.Item != result.cache.Item;

            }

            return ModuleResult.Create(task, hasChanges);


        }
    }

}
