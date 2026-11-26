namespace Game.Battle.Stage
{
    // 기존 UI 호출부와 스테이지 구현이 함께 사용하는 목표 계약이다.
    public interface IGoalStage : IStageResult, IStageTimer, IStageProgress
    {
    }
}
