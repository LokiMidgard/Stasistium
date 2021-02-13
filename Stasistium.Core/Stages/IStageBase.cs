using Stasistium.Documents;

namespace Stasistium.Stages
{
    public interface IStageBase
    {
        IGeneratorContext Context { get; }
        string Name { get; }
    }
}