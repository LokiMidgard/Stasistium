﻿using Stasistium.Core;
using Stasistium.Documents;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class FileStage<TInCache> : StageBase<Stream, FileStageCache<TInCache>>
        where TInCache : class
    {
        private readonly StagePerformHandler<string, TInCache> input;

        public FileStage(StagePerformHandler<string, TInCache> input, IGeneratorContext context, string? name = null) : base(context, name)
        {
            this.input = input;
        }


        protected override async Task<StageResult<Stream, FileStageCache<TInCache>>> DoInternal([AllowNull] FileStageCache<TInCache>? cache, OptionToken options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            var input = await this.input(cache?.PreviousCache, options).ConfigureAwait(false);

            var task = LazyTask.Create(async () =>
            {
                var result = await input.Perform;
                var file = new FileInfo(result.Value);
                if (!file.Exists)
                    throw this.Context.Exception($"File \"{file.FullName}\" does not exists");

                var document = new FileDocument(file, file.Directory, null, this.Context) as IDocument<Stream>;
                document = document.With(result.Metadata);
                var fileStageCache = new FileStageCache<TInCache>()
                {
                    Path = file.FullName,
                    PreviousCache = input.Cache,
                    LastWriteTimeUtc = file.LastWriteTimeUtc,
                    LastHash = document.Hash
                };
                return (result: document, cache: fileStageCache);
            });

            var hasChanges = input.HasChanges;

            if (!hasChanges && cache != null)
            {
                var lastWriteTime = File.GetLastWriteTimeUtc(cache.Path);
                hasChanges = lastWriteTime != cache.LastWriteTimeUtc;
            }

            string id;
            if (hasChanges || cache is null)
            {
                var result = await task;
                id = result.result.Id;
                hasChanges = result.result.Hash != cache?.LastHash;
                return StageResult.Create(result.result, hasChanges, id, result.cache);
            }
            else
            {
                var actualTask = LazyTask.Create(async () =>
                {
                    var temp = await task;
                    return temp.result;
                });
                id = Path.GetFileName(cache.Path);
                return StageResult.Create(actualTask, hasChanges, id, cache);
            }

        }

    }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public class FileStageCache<T>
    {
        public T PreviousCache { get; set; }

        public DateTime LastWriteTimeUtc { get; set; }
        public string Path { get; set; }
        public string LastHash { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
}
