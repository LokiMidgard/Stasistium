using Stasistium;
using Stasistium.Documents;
using Stasistium.Stages;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Stasistium.Test
{
    public class MultiAssertStage<T> : MultiStageBase<string, string, MultiAssertStage<T>.Cache>
        where T : class
    {
        private readonly MultiStageBase<string, string, T> input;

        private readonly System.Collections.Concurrent.ConcurrentBag<Run> runs = new System.Collections.Concurrent.ConcurrentBag<Run>();


        public IAssert Assert
        {
            get
            {
                var assertHelper = new AssertHelper(this.runs.ToImmutableList());
                this.runs.Clear();
                return assertHelper;
            }
        }

        public MultiAssertStage(MultiStageBase<string, string, T> input, IGeneratorContext context, string? name = null) : base(context, name)
        {
            this.input = input;
        }

        protected override async Task<StageResultList<string, string, MultiAssertStage<T>.Cache>> DoInternal([AllowNull] MultiAssertStage<T>.Cache? cache, OptionToken options)
        {
            var r = await this.input.DoIt(cache?.PreviousCache, options);

            var inputHadChanges = r.HasChanges;
            var inputIds = r.Ids;
            var result = await r.Perform;
            var newInputCache = r.Cache;
            var subs = await Task.WhenAll(result.Select(async x =>
            {
                var subHasChanges = x.HasChanges;
                var subId = x.Id;
                var subResult = await x.Perform;
                var subCache = x.Cache;
                return (subHasChanges, subId, subResult, subResult.Value, subResult.Id, subResult.Hash, subCache);
            }));

            this.runs.Add(new Run()
            {
                HadChanges = inputHadChanges,
                Ids = inputIds,
                Entrys = subs
            });

            var list = subs.Select(x => this.Context.CreateStageResult(x.subResult, x.subHasChanges, x.Id, x.subCache, x.Hash)).ToImmutableList();

            return this.Context.CreateStageResultList(list, inputHadChanges || list.Any(x => x.HasChanges), list.Select(x => x.Id).ToImmutableList(), new Cache() { PreviousCache = newInputCache }, this.Context.GetHashForObject(list.Select(x => x.Hash)));
        }


        public interface IAssert
        {

        }
        private class AssertHelper : IAssert
        {
            private ImmutableList<MultiAssertStage<T>.Run> immutableList;

            public AssertHelper(ImmutableList<MultiAssertStage<T>.Run> immutableList)
            {
                this.immutableList = immutableList;
            }



        }

        private class Run
        {
            public bool HadChanges { get; internal set; }
            public ImmutableList<string> Ids { get; internal set; }
            public (bool subHasChanges, string subId, IDocument<string> subResult, string Value, string Id, string Hash, string subCache)[] Entrys { get; internal set; }
        }

        public class Cache
        {
            public T PreviousCache { get; set; }
        }
    }


}
