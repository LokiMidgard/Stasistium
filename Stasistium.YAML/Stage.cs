using Stasistium.Documents;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.IO;
using Stasistium.Stages;

namespace Stasistium
{

    public static class YamlStageExtensions
    {
        public static SidecarHelper<TPreviousItemCache, TPreviousListCache> Sidecar<TPreviousItemCache, TPreviousListCache>(this MultiStageBase<Stream, TPreviousItemCache, TPreviousListCache> stage, string? name = null)
            where TPreviousListCache : class
            where TPreviousItemCache : class
        {
            return new SidecarHelper<TPreviousItemCache, TPreviousListCache>(stage, name);
        }
    }

}
