﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace StaticSite.Documents
{
    public abstract class BaseCache
    {
        private protected BaseCache(ReadOnlyMemory<BaseCache> previousCache, ImmutableDictionary<string, BaseCache> childCache)
        {
            this.PreviousCache = previousCache;
            this.ChildCache = childCache;
        }
        public ReadOnlyMemory<BaseCache> PreviousCache { get; }
        public ImmutableDictionary<string, BaseCache> ChildCache { get; }

        public abstract JToken Serelize();


        public static JArray Write(BaseCache baseCache)
        {
            var result = new JArray();


            var stack = new Stack<BaseCache>();

            var queu = new Queue<BaseCache>();
            queu.Enqueue(baseCache);

            while (queu.TryDequeue(out var queuEntry))
            {
                stack.Push(queuEntry);
                foreach (var item in queuEntry.PreviousCache.Span)
                    queu.Enqueue(item);
                foreach (var item in queuEntry.ChildCache.Values)
                    queu.Enqueue(item);
            }

            int index = 0;
            var idLookup = new Dictionary<BaseCache, int>();

            while (stack.TryPop(out var current))
            {
                var jObject = current.Serelize();
                var type = current.GetType().FullName!; // Null is only returned if type is a generic type paramter. We have an instanciated object, so no it is not null.
                var assembly = current.GetType().Assembly.FullName!;

                int curentIndex;
                if (idLookup.ContainsKey(current))
                {
                    continue;
                    //curentIndex = idLookup[current];
                }
                else
                {
                    curentIndex = index++;
                    idLookup.Add(current, curentIndex);
                }

                var previous = new JArray();
                var child = new JArray();
                var previousCaches = current.PreviousCache.Span;
                var childCaches = current.ChildCache;

                for (int i = 0; i < previousCaches.Length; i++)
                    previous.Add(new JValue(idLookup[previousCaches[i]]));
                foreach (var item in childCaches)
                    child.Add(new JObject { { "key", item.Key }, { "id", idLookup[item.Value] } });

                var entry = new JObject
                {
                    { "type", type },
                    { "assembly", assembly },
                    { "cache", jObject },
                    { "id", curentIndex },
                    { "previous",  previous },
                    { "child",  child }
                };

                result.Add(entry);
            }

            return result;
        }

        internal static BaseCache Load(JArray json)
        {
            if (json.Count == 0)
                throw new ArgumentException("There must be at least on value", nameof(json));
            BaseCache previousCache = null!;
            var idLookup = new Dictionary<int, BaseCache>();

            for (int i = 0; i < json.Count; i++)
            {
                var entry = json[i];
                var jObject = entry["cache"];
                var id = entry["id"].ToObject<int>();
                var previousArray = (JArray)entry["previous"];
                var childArray = (JArray)entry["child"];
                var assemblyName = entry["assembly"].ToObject<string>();
                var typeName = entry["type"].ToObject<string>();


                var previous = new BaseCache[previousArray.Count];
                for (int j = 0; j < previous.Length; j++)
                {
                    var jValue = (JValue)previousArray[j];
                    var value = (int)(long)jValue.Value;
                    previous[j] = idLookup[value];
                }

                var child = ImmutableDictionary<string, BaseCache>.Empty.ToBuilder();
                for (int j = 0; j < childArray.Count; j++)
                {
                    var jValue = (JObject)childArray[j];
                    var key = jValue.Value<string>("key");
                    var value = (int)jValue.Value<long>("id");
                    child[key] = idLookup[value];
                }


                var assembly = System.Reflection.Assembly.Load(new System.Reflection.AssemblyName(assemblyName));
                //var type = assembly.GetType(typeName);

                var constructorArguments = new object[] { jObject, new ReadOnlyMemory<BaseCache>(previous), child.ToImmutable() };

                previousCache = (BaseCache)assembly.CreateInstance(typeName, false, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, constructorArguments, null, null)!;

                if (previousCache == null)
                    throw new InvalidOperationException();

                idLookup.Add(id, previousCache);

            }

            return previousCache;
        }

        public static BaseCache<T> Create<T>(T item, ReadOnlyMemory<BaseCache> previous, ImmutableDictionary<string, BaseCache>? child = null)
            => new BaseCache<T>(item, previous, child ?? ImmutableDictionary<string, BaseCache>.Empty);
        public static BaseCache<T> Create<T>(T item, BaseCache previous, ImmutableDictionary<string, BaseCache>? child = null)
            => new BaseCache<T>(item, new BaseCache[] { previous }.AsMemory(), child ?? ImmutableDictionary<string, BaseCache>.Empty);
        public static BaseCache<T> Create<T>(T item)
            => new BaseCache<T>(item, ReadOnlyMemory<BaseCache>.Empty, ImmutableDictionary<string, BaseCache>.Empty);
    }
    public sealed class BaseCache<TCacheItem> : BaseCache
    {
        public BaseCache(TCacheItem item, ReadOnlyMemory<BaseCache> previousCache, ImmutableDictionary<string, BaseCache> childCache) : base(previousCache, childCache)
        {
            this.Item = item;
        }

        internal BaseCache(JToken json, ReadOnlyMemory<BaseCache> previousCache, ImmutableDictionary<string, BaseCache> childCache) : base(previousCache, childCache)
        {
            this.Item = this.Deserelize(json);
        }

        public TCacheItem Item { get; }

        public override JToken Serelize()
        {

            return JToken.FromObject(Item);
        }

        private TCacheItem Deserelize(JToken json)
        {
            return json.ToObject<TCacheItem>();
        }

    }
}
