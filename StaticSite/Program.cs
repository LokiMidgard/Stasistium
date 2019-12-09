﻿using StaticSite.Documents;
using System;
using System.Threading.Tasks;

namespace StaticSite
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var context = new GeneratorContext();
            var startModule = Modules.FromResult("https://github.com/LokiMidgard/NDProperty.git", context);
            var generatorOptions = new GenerationOptions();
            var g = startModule
                .GitModul()
                .Where(x => Task.FromResult(x.FrindlyName == "origin/master"), x => x.Hash)
                .Single(x => Task.FromResult(x.Hash))
                .GitRefToFiles();

            var task = await g.DoIt(null, generatorOptions.Token).ConfigureAwait(false);
            Console.WriteLine($"First run changes: {task.HasChanges}");
            var result = await task.Perform;
            foreach (var item in result.result)
                Console.WriteLine($"\t{item.Id}");

            var data = BaseCache.Write(result.cache);

            Console.WriteLine(data.ToString());

            var cache = BaseCache.Load(data);

            var task2 = await g.DoIt(cache, generatorOptions.Token).ConfigureAwait(false);
            Console.WriteLine($"Seccond run changes: {task2.HasChanges}");

            Console.ReadKey(false);
            var result2 = await task.Perform;
            foreach (var item in result2.result)
                Console.WriteLine($"\t{item.Id}");

            //var g = new Git("https://github.com/nota-game/nota.git", new DirProvider());

        }
    }

}
