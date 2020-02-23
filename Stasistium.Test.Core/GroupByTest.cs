//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Stasistium.Documents;
//using Stasistium.Stages;
//using System.Collections.Immutable;
//using System.Threading.Tasks;

//namespace Stasistium.Test.Core
//{
//    [TestClass]
//    public class GroupByTest
//    {
//        [TestMethod]
//        public async Task NoChanges()
//        {
//            await using var context = new GeneratorContext();
//            var mok = new MultiStageMok(context)
//            {
//                Current = ImmutableList<(string id, string content)>.Empty
//                .Add(("a1", "test1"))
//                .Add(("a2", "test2"))
//                .Add(("b1", "test3"))
//                .Add(("b2", "test4"))
//            };

//            var lookup = new System.Collections.Concurrent.ConcurrentDictionary<char, MultiAssertStage<StartCache<ImmutableList<(string id, string content)>, char>>>();

//            var group = mok.GroupBy(x => x.Id[0],
//                (input, key) =>
//                {
//                    var stage = new MultiAssertStage<StartCache<ImmutableList<(string id, string content)>, char>>(input, input.Context);
//                    var success = lookup.TryAdd(key, stage);
//                    Assert.IsTrue(success, "Create pipline should only be called once for each key");
//                    return stage;
//                });
//            var options = new GenerationOptions()
//            {

//            };
//            var stageResultList = await group.DoIt(null, options.Token);
//            var result = await stageResultList.Perform;
//            var cache = stageResultList.Cache;

//            var stageResultList2 = await group.DoIt(cache, options.Token);
//            var result2 = await stageResultList.Perform;
//            var cache2 = stageResultList.Cache;
//            Assert.IsTrue(stageResultList.HasChanges, "First Run");
//            Assert.IsFalse(stageResultList2.HasChanges, "No Changed Data");
//        }
//        [TestMethod]
//        public async Task DocumentChanged()
//        {
//            await using var context = new GeneratorContext();
//            var mok = new MultiStageMok(context)
//            {
//                Current = ImmutableList<(string id, string content)>.Empty
//                .Add(("a1", "test1"))
//                .Add(("a2", "test2"))
//                .Add(("b1", "test3"))
//                .Add(("b2", "test4"))
//            };

//            var lookup = new System.Collections.Concurrent.ConcurrentDictionary<char, MultiAssertStage<StartCache<ImmutableList<(string id, string content)>, char>>>();

//            var group = mok.GroupBy(x => x.Id[0],
//                (input, key) =>
//                {
//                    var stage = new MultiAssertStage<StartCache<ImmutableList<(string id, string content)>, char>>(input, input.Context);
//                    var success = lookup.TryAdd(key, stage);
//                    Assert.IsTrue(success, "Create pipline should only be called once for each key");
//                    return stage;
//                });
//            var options = new GenerationOptions()
//            {

//            };
//            var stageResultList = await group.DoIt(null, options.Token);
//            var result = await stageResultList.Perform;
//            var cache = stageResultList.Cache;

//            mok.Current = ImmutableList<(string id, string content)>.Empty
//                .Add(("a1", "test1"))
//                .Add(("a2", "test2"))
//                .Add(("b1", "test3.1"))
//                .Add(("b2", "test4"))
//    ;

//            var stageResultList2 = await group.DoIt(cache, options.Token);
//            var result2 = await stageResultList.Perform;
//            var cache2 = stageResultList.Cache;

//            Assert.IsTrue(stageResultList.HasChanges, "First Run");
//            Assert.IsTrue(stageResultList2.HasChanges, "Updated Data");
//        }
//        [TestMethod]
//        public async Task DocumentOrderChanged()
//        {
//            await using var context = new GeneratorContext();
//            var mok = new MultiStageMok(context)
//            {
//                Current = ImmutableList<(string id, string content)>.Empty
//                .Add(("a1", "test1"))
//                .Add(("a2", "test2"))
//                .Add(("b1", "test3"))
//                .Add(("b2", "test4"))
//            };

//            var lookup = new System.Collections.Concurrent.ConcurrentDictionary<char, MultiAssertStage<StartCache<ImmutableList<(string id, string content)>, char>>>();

//            var group = mok.GroupBy(x => x.Id[0],
//                (input, key) =>
//                {
//                    var stage = new MultiAssertStage<StartCache<ImmutableList<(string id, string content)>, char>>(input, input.Context);
//                    var success = lookup.TryAdd(key, stage);
//                    Assert.IsTrue(success, "Create pipline should only be called once for each key");
//                    return stage;
//                });
//            var options = new GenerationOptions()
//            {

//            };
//            var stageResultList = await group.DoIt(null, options.Token);
//            var result = await stageResultList.Perform;
//            var cache = stageResultList.Cache;

//            mok.Current = ImmutableList<(string id, string content)>.Empty
//                .Add(("a1", "test1"))
//                .Add(("a2", "test2"))
//                .Add(("b2", "test4"))
//                .Add(("b1", "test3"))
//    ;

//            var stageResultList2 = await group.DoIt(cache, options.Token);
//            var result2 = await stageResultList.Perform;
//            var cache2 = stageResultList.Cache;

//            Assert.IsTrue(stageResultList.HasChanges, "First Run");
//            Assert.IsTrue(stageResultList2.HasChanges, "Updated Data");
//        }

//        [TestMethod]
//        public async Task DocumentAdded()
//        {
//            await using var context = new GeneratorContext();
//            var mok = new MultiStageMok(context)
//            {
//                Current = ImmutableList<(string id, string content)>.Empty
//                .Add(("a1", "test1"))
//                .Add(("a2", "test2"))
//                .Add(("b1", "test3"))
//                .Add(("b2", "test4"))
//            };

//            var lookup = new System.Collections.Concurrent.ConcurrentDictionary<char, MultiAssertStage<StartCache<ImmutableList<(string id, string content)>, char>>>();

//            var group = mok.GroupBy(x => x.Id[0],
//                (input, key) =>
//                {
//                    var stage = new MultiAssertStage<StartCache<ImmutableList<(string id, string content)>, char>>(input, input.Context);
//                    var success = lookup.TryAdd(key, stage);
//                    Assert.IsTrue(success, "Create pipline should only be called once for each key");
//                    return stage;
//                });
//            var options = new GenerationOptions()
//            {

//            };
//            var stageResultList = await group.DoIt(null, options.Token);
//            var result = await stageResultList.Perform;
//            var cache = stageResultList.Cache;

//            mok.Current = ImmutableList<(string id, string content)>.Empty
//                .Add(("a1", "test1"))
//                .Add(("a2", "test2"))
//                .Add(("b1", "test3"))
//                .Add(("b2", "test4"))
//                .Add(("b3", "test5"))
//    ;

//            var stageResultList2 = await group.DoIt(cache, options.Token);
//            var result2 = await stageResultList.Perform;
//            var cache2 = stageResultList.Cache;

//            Assert.IsTrue(stageResultList.HasChanges, "First Run");
//            Assert.IsTrue(stageResultList2.HasChanges, "Updated Data");
//        }


//        [TestMethod]
//        public async Task DocumentRemoved()
//        {
//            await using var context = new GeneratorContext();
//            var mok = new MultiStageMok(context)
//            {
//                Current = ImmutableList<(string id, string content)>.Empty
//                .Add(("a1", "test1"))
//                .Add(("a2", "test2"))
//                .Add(("b1", "test3"))
//                .Add(("b2", "test4"))
//            };

//            var lookup = new System.Collections.Concurrent.ConcurrentDictionary<char, MultiAssertStage<StartCache<ImmutableList<(string id, string content)>, char>>>();

//            var group = mok.GroupBy(x => x.Id[0],
//                (input, key) =>
//                {
//                    var stage = new MultiAssertStage<StartCache<ImmutableList<(string id, string content)>, char>>(input, input.Context);
//                    var success = lookup.TryAdd(key, stage);
//                    Assert.IsTrue(success, "Create pipline should only be called once for each key");
//                    return stage;
//                });
//            var options = new GenerationOptions()
//            {

//            };
//            var stageResultList = await group.DoIt(null, options.Token);
//            var result = await stageResultList.Perform;
//            var cache = stageResultList.Cache;

//            mok.Current = ImmutableList<(string id, string content)>.Empty
//                .Add(("a1", "test1"))
//                .Add(("a2", "test2"))
//                .Add(("b2", "test4"))
//    ;

//            var stageResultList2 = await group.DoIt(cache, options.Token);
//            var result2 = await stageResultList.Perform;
//            var cache2 = stageResultList.Cache;

//            Assert.IsTrue(stageResultList.HasChanges, "First Run");
//            Assert.IsTrue(stageResultList2.HasChanges, "Updated Data");
//        }

//    }


//}
