﻿using System.Collections.Generic;

namespace Stasistium.Stages
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning disable CA1819 // Properties should not return arrays
#pragma warning disable CA2227 // Collection properties should be read only
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public class TransformStageCache<TInCache>
        where TInCache : class
    {
        public TInCache ParentCache { get; set; }

        public string[] OutputIdOrder { get; set; }
        public Dictionary<string, string> Transformed { get; set; }
        public Dictionary<string, string> InputToOutputId { get; set; }

    }
#pragma warning restore CA1819 // Properties should not return arrays
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.



}