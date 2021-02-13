using Stasistium.Documents;
using System;

namespace Stasistium.Stages
{
    public static class SubPipeline
    {
        public static SubPiplineHelper<TInput, TResult> Create<TInput, TResult>(Func<IStageBaseOutput<TInput>, IStageBaseOutput<TResult>> createPipeline, IGeneratorContext context)
        {
            return SubPiplineHelper<TInput, TResult>.Create(createPipeline, context);
        }
    }

}
