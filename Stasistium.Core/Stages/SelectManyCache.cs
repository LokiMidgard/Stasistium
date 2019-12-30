using System.Collections.Generic;

namespace Stasistium.Stages
{
    public class SelectManyCache<TInputCache, TItemCache, TCache>
        where TInputCache : class
        where TItemCache : class
        where TCache : class
    {
        public TInputCache PreviousCache { get; set; }

        ///// <summary>
        ///// InputId to cache
        ///// </summary>
        //public Dictionary<string, TItemCache> InputItemCacheLookup { get; set; }

            public string[] OutputIdOrder { get; set; }

        /// <summary>
        /// InputId to cache
        /// </summary>
        public Dictionary<string, TCache> InputCacheLookup { get; set; }

        /// <summary>
        /// InputId to OutputHash
        /// </summary>
        public Dictionary<string, string> OutputItemIdToHash { get; set; }

        /// <summary>
        /// InputId to OutputIds
        /// </summary>
        public Dictionary<string, string[]> InputItemToResultItemIdLookup { get; set; }

    }
}