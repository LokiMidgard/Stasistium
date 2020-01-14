using Stasistium.Documents;
using Stasistium.Stages;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class OrderByStage<T, TItemCache, TPreviousCache, TKey> : GeneratedHelper.Multiple.Simple.OutputMultiSimpleInputSingle0List1StageBase<T, TItemCache, TPreviousCache, T>
        where TItemCache : class
        where TPreviousCache : class
    {
        private readonly Func<IDocument<T>, TKey> keySelector;

        public OrderByStage(StagePerformHandler<T, TItemCache, TPreviousCache> inputList0, Func<IDocument<T>, TKey> keySelector, IGeneratorContext context, string? name) : base(inputList0, context, name)
        {
            this.keySelector = keySelector;
        }

        protected override Task<ImmutableList<IDocument<T>>> Work(ImmutableList<IDocument<T>> input, OptionToken options) => Task.FromResult(input.OrderBy(this.keySelector).ToImmutableList());
    }
}

namespace Stasistium
{
    public static partial class StageExtensions
    {
        public static OrderByStage<T, TItemCache, TPreviousCache, TKey> OrderBy<T, TItemCache, TPreviousCache, TKey>(this MultiStageBase<T, TItemCache, TPreviousCache> input, Func<IDocument<T>, TKey> keySelector, string? name = null)
        where TItemCache : class
        where TPreviousCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (keySelector is null)
                throw new ArgumentNullException(nameof(keySelector));
            return new OrderByStage<T, TItemCache, TPreviousCache, TKey>(input.DoIt, keySelector, input.Context, name);
        }
    }
}
