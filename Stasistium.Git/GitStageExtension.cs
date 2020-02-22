﻿using Stasistium.Documents;
using System;
using Stasistium.Stages;

namespace Stasistium
{
    public static class GitStageExtension
    {
        public static GitStage<T> GitModul<T>(this StageBase<string, T> input, string? name = null)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new GitStage<T>(input, input.Context, name);
        }

        public static GitRefToFilesStage<T> GitRefToFiles<T>(this StageBase<GitRefStage, T> input, bool addGitMetadata = false, string? name = null)
            where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new GitRefToFilesStage<T>(input, addGitMetadata, input.Context, name);
        }
    }

}
