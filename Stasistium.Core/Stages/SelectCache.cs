using System.Collections.Generic;

namespace Stasistium.Stages
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning disable CA2227 // Collection properties should be read only
#pragma warning disable CA1819 // Properties should not return arrays
    public class SelectCache<TInputCache, TItemCache>
    {
        public TInputCache PreviousCache { get; set; }
        /// <summary>
        /// Output Ids ORderd
        /// </summary>
        public string[] OutputIdOrder { get; set; }
        /// <summary>
        /// InputId to cache
        /// </summary>
        public Dictionary<string, TItemCache> InputItemCacheLookup { get; set; }
        /// <summary>
        /// InputId to OutputHash
        /// </summary>
        public Dictionary<string, string> InputItemHashLookup { get; set; }
        /// <summary>
        /// InputId to OutputId
        /// </summary>
        public Dictionary<string, string> InputItemOutputIdLookup { get; set; }
        public string Hash { get;  set; }
    }
    public class SelectCache<TInputCache1, TInputCache2, TItemCache>
    {
        public TInputCache1 PreviousCache { get; set; }
        public TInputCache2 PreviousCache2 { get; set; }
        /// <summary>
        /// Output Ids ORderd
        /// </summary>
        public string[] OutputIdOrder { get; set; }
        /// <summary>
        /// InputId to cache
        /// </summary>
        public Dictionary<string, TItemCache> InputItemCacheLookup { get; set; }
        /// <summary>
        /// InputId to OutputHash
        /// </summary>
        public Dictionary<string, string> InputItemHashLookup { get; set; }
        /// <summary>
        /// InputId to OutputId
        /// </summary>
        public Dictionary<string, string> InputItemOutputIdLookup { get; set; }
    }
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning restore CA1819 // Properties should not return arrays


}
