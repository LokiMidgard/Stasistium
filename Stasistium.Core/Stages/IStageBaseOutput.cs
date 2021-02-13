namespace Stasistium.Stages
{
    public interface IStageBaseOutput<TResult> : IStageBase
    {
        public event StagePerform<TResult>? PostStages;
    }
}