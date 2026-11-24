using System;

using Game.Common;

namespace Game.Quests
{
    // 표시용 복사본으로 실제 진행 데이터와 보상 수령 처리를 분리한다.
    public sealed class QuestState
    {
        public long CurrentCount { get; }
        public long AvailableRewardCount { get; }
        public bool CanUse { get; }
        public bool CanReceive { get; }
        public bool IsCompleted { get; }

        public QuestState(QuestRule rule, QuestProgressData progress, long availableCount, bool canUse, bool isReceiving)
        {
            long total = progress == null ? 0 : progress.TotalProgress;
            long received = progress == null ? 0 : progress.ClaimedCount;

            CurrentCount = rule.Type == QuestType.Repeat ? total % rule.Goal : Math.Min(total, rule.Goal);
            AvailableRewardCount = availableCount;
            CanUse = canUse;
            CanReceive = canUse && !isReceiving && availableCount > 0;
            IsCompleted = rule.Type != QuestType.Repeat && received > 0;
        }
    }
}
