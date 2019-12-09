using LibGit2Sharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace StaticSite.Documents
{
    public class GitModule<TPreviousCache> : ModuleBase<ImmutableList<GitRef>, ImmutableDictionary<string, (GitRefType type, string hash)>>
    {
        private readonly ModulePerformHandler<string, TPreviousCache> input;
        private Repository? repo;
        private System.IO.DirectoryInfo? workingDir;


        public GitModule(ModulePerformHandler<string, TPreviousCache> input, GeneratorContext context) : base(context)
        {
            this.input = input;

        }
        protected override async Task<ModuleResult<ImmutableList<GitRef>, ImmutableDictionary<string, (GitRefType type, string hash)>>> Do(BaseCache<ImmutableDictionary<string, (GitRefType type, string hash)>>? cache, OptionToken options)
        {

            if (cache != null && cache.PreviousCache.Length != 1)
                throw new ArgumentException($"This cache should have exactly one predecessor but had {cache.PreviousCache}");
            var inputResult = await this.input(cache?.PreviousCache.Span[0], options).ConfigureAwait(false);


            var task = LazyTask.Create(async () =>
             {

                 var previousPerform = await inputResult.Perform;
                 var source = previousPerform.result;

                 if (this.repo is null || inputResult.HasChanges)
                 {
                     if (this.repo != null)
                     {
                         if (this.workingDir is null)
                             throw new InvalidOperationException("the working dir should exist if repo does.");
                         // TODO: Should we realy dispose this already?
                         // I think we need to track who else has a reference to an object cretaed by this repo :/
                         this.repo.Dispose();
                         // this.workingDir.Delete(true);
                     }

                     this.workingDir = this.Context.TempDir();
                     this.repo = await Task.Run(() => new Repository(Repository.Clone(source, this.workingDir.FullName, new CloneOptions() { IsBare = true }))).ConfigureAwait(false);
                 }
                 else if (options.Refresh)
                 {
                     // The git library is nor thread save, so we should not paralize this!
                     foreach (var remote in this.repo.Network.Remotes)
                         await Task.Run(() => Commands.Fetch(this.repo, remote.Name, Array.Empty<string>(), new FetchOptions() { }, null)).ConfigureAwait(false);
                 }
                 // for branches we ignore the local ones. we just cloned the repo and the local one is the same as the remote.
                 var refs = this.repo.Tags.Select(x => new GitRef(x, this.repo)).Concat(this.repo.Branches.Where(x => x.IsRemote).Select(x => new GitRef(x, this.repo))).ToImmutableList();
                 return (list: refs, cache: BaseCache.Create(refs.ToImmutableDictionary(x => x.FrindlyName, x => (x.Type, x.Tip.Sha)), new BaseCache[] { previousPerform.cache }.AsMemory()));
             });


            bool hasChanges = inputResult.HasChanges;

            if (options.Refresh || hasChanges)
            {
                // if we should refresh we need to update the repo or if the previous input was different
                // we need to perform the network operation to ensure we have no changes

                var result = await task;
                hasChanges = hasChanges = Changed(cache?.Item, result.cache.Item);

            }

            return ModuleResult.Create(task, hasChanges);
        }

        private static bool Changed(ImmutableDictionary<string, (GitRefType type, string hash)>? cache, ImmutableDictionary<string, (GitRefType type, string hash)> cache2)
        {
            if (cache2 is null)
                throw new ArgumentNullException(nameof(cache2));


            if (cache is null)
                return true;

            if (cache.Count != cache2.Count)
                return false;

            foreach (var pair in cache2)
            {
                if (!cache.TryGetValue(pair.Key, out var sha))
                    return false;
                if (sha.hash != pair.Value.hash || sha.type != pair.Value.type)
                    return false;
            }

            return true;
        }


    }

    public abstract class BaseCache
    {
        private protected BaseCache(ReadOnlyMemory<BaseCache> previousCache)
        {
            this.PreviousCache = previousCache;

        }
        public ReadOnlyMemory<BaseCache> PreviousCache { get; }

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
                    curentIndex = idLookup[current];
                }
                else
                {
                    curentIndex = index++;
                    idLookup.Add(current, curentIndex);
                }

                var previous = new JArray();
                var previousCaches = current.PreviousCache.Span;

                for (int i = 0; i < previousCaches.Length; i++)
                    previous.Add(new JValue(idLookup[previousCaches[i]]));

                var entry = new JObject
                {
                    { "type", type },
                    { "assembly", assembly },
                    { "cache", jObject },
                    { "id", curentIndex },
                    { "previous",  previous }
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
                var assemblyName = entry["assembly"].ToObject<string>();
                var typeName = entry["type"].ToObject<string>();


                var previous = new BaseCache[previousArray.Count];
                for (int j = 0; j < previousArray.Count; j++)
                {
                    var jValue = (JValue)previousArray[j];
                    var value = (int)(long)jValue.Value;
                    previous[j] = idLookup[value];
                }


                var assembly = System.Reflection.Assembly.Load(new System.Reflection.AssemblyName(assemblyName));
                //var type = assembly.GetType(typeName);

                var constructorArguments = new object[] { jObject, new ReadOnlyMemory<BaseCache>(previous) };

                previousCache = (BaseCache)assembly.CreateInstance(typeName, false, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, constructorArguments, null, null)!;

                if (previousCache == null)
                    throw new InvalidOperationException();

                idLookup.Add(id, previousCache);

            }

            return previousCache;
        }

        public static BaseCache<T> Create<T>(T item, ReadOnlyMemory<BaseCache> previous)
            => new BaseCache<T>(item, previous);
        public static BaseCache<T> Create<T>(T item, BaseCache previous)
            => new BaseCache<T>(item, new BaseCache[] { previous }.AsMemory());
        public static BaseCache<T> Create<T>(T item)
            => new BaseCache<T>(item, ReadOnlyMemory<BaseCache>.Empty);
    }


    public sealed class BaseCache<TCacheItem> : BaseCache
    {
        public BaseCache(TCacheItem item, ReadOnlyMemory<BaseCache> previousCache) : base(previousCache)
        {
            this.Item = item;
        }

        internal BaseCache(JToken json, ReadOnlyMemory<BaseCache> previousCache) : base(previousCache)
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
