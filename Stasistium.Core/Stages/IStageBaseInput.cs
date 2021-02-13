using Stasistium.Documents;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public interface IStageBaseInput<TIn> : IStageBase
    {
        Task DoIt(ImmutableList<IDocument<TIn>> cache, OptionToken options);
    }
    public interface IStageBaseInput<TIn1, TIn2> : IStageBase
    {
        Task DoIt1(ImmutableList<IDocument<TIn1>> in1, OptionToken options);
        Task DoIt2(ImmutableList<IDocument<TIn2>> in1, OptionToken options);
    }
}