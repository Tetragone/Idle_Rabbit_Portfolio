namespace Game.Battle.Stage
{
    public interface IStageResult
    {
        public bool IsEnded();
        public bool IsClear();
    }

    public interface IStageTimer
    {
        public float MaxTime();
        public float GetNowTime();
    }

    public interface IStageProgress
    {
        public bool IsCountableGoal();
        public int Goal();
        public int Progress();
        public float GoalPercent();
    }
}
