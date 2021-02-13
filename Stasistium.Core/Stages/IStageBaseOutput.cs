namespace Stasistium.Stages
{
    public interface IStageBaseOutput<TResult>
    {
        public event StagePerform<TResult>? PostStages;
    }
}