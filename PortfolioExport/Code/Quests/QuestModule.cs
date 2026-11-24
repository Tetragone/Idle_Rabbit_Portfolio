using System;
using System.Collections.Generic;

using Game.Common;
using Game.Core;

namespace Game.Quests
{
    // 상시와 이벤트에서 같은 진행·초기화·수령 규칙을 사용한다. 파일 저장은 하지 않는다.
    public sealed class QuestModule
    {
        public event Action<string> Changed;
        public IReadOnlyList<QuestRule> Rules => RuleList.AsReadOnly();

        private readonly List<QuestRule> RuleList;
        private readonly Dictionary<string, QuestRule> ById = new Dictionary<string, QuestRule>();
        private readonly Dictionary<string, QuestProgressData> Progress = new Dictionary<string, QuestProgressData>();
        private List<QuestProgressData> SavedProgress;
        private bool IsReceiving;

        public QuestModule(IEnumerable<QuestRule> rules, List<QuestProgressData> progress)
        {
            RuleList = rules == null ? new List<QuestRule>() : new List<QuestRule>(rules);
            foreach (QuestRule rule in RuleList)
            {
                ById.Add(rule.Id, rule);
            }
            SetProgressData(progress);
        }

        public void SetProgressData(List<QuestProgressData> progress)
        {
            if (ReferenceEquals(SavedProgress, progress)) return;
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            Progress.Clear();
            foreach (QuestProgressData item in progress)
            {
                if (item == null || string.IsNullOrEmpty(item.QuestId) || item.TotalProgress < 0 || item.ClaimedCount < 0)
                    throw new ArgumentException("퀘스트 저장 기록이 잘못되었습니다.");
                Progress.Add(item.QuestId, item);
            }
            SavedProgress = progress;
        }

        public bool Refresh(long now)
        {
            bool changed = false;
            foreach (QuestRule rule in RuleList)
            {
                QuestProgressData progress = GetOrCreate(rule.Id);
                long period = GetPeriod(rule.Type, now);
                if (period > progress.PeriodStart)
                {
                    progress.PeriodStart = period;
                    progress.TotalProgress = 0;
                    progress.ClaimedCount = 0;
                    changed = true;
                    Changed?.Invoke(rule.Id);
                }
            }
            return changed;
        }

        private static long GetPeriod(QuestType type, long now)
        {
            if (type == QuestType.Daily) return TimeMgr.GetDayStartTimeNum(now);
            if (type == QuestType.Weekly) return TimeMgr.GetWeekStartTimeNum(now);
            return 0;
        }

        private QuestProgressData GetOrCreate(string id)
        {
            if (!Progress.TryGetValue(id, out QuestProgressData progress))
            {
                progress = new QuestProgressData { QuestId = id };
                Progress.Add(id, progress);
                SavedProgress.Add(progress);
            }
            return progress;
        }

        public bool ReportAction(QuestAction action, long count, long now)
        {
            if (count <= 0) return false;
            Refresh(now);

            bool changed = false;
            foreach (QuestRule rule in RuleList)
            {
                if (rule.Action != action) continue;
                QuestProgressData progress = GetOrCreate(rule.Id);
                if (now < progress.PeriodStart) continue;

                long limit = rule.Type == QuestType.Repeat ? long.MaxValue : rule.Goal;
                long added = Math.Min(count, limit - progress.TotalProgress);
                if (added <= 0) continue;

                progress.TotalProgress += added;
                changed = true;
                Changed?.Invoke(rule.Id);
            }

            return changed;
        }

        public QuestProgressData GetProgress(string id, long now)
        {
            if (!ById.ContainsKey(id)) return null;
            Refresh(now);
            return GetOrCreate(id).Copy();
        }

        public QuestProgressData GetSavedProgress(string id)
        {
            return Progress.TryGetValue(id, out QuestProgressData progress) ? progress.Copy() : null;
        }

        public long GetAvailableRewardCount(string id, long now)
        {
            if (!ById.TryGetValue(id, out QuestRule rule)) return 0;
            Refresh(now);
            QuestProgressData progress = GetOrCreate(id);
            if (now < progress.PeriodStart) return 0;
            return Math.Max(0, progress.TotalProgress / rule.Goal - progress.ClaimedCount);
        }

        public bool TryReceiveReward(string id, long count, long now, Func<QuestRule, long, bool> giveReward)
        {
            if (IsReceiving || count <= 0 || giveReward == null || count > GetAvailableRewardCount(id, now)) return false;
            QuestProgressData progress = GetOrCreate(id);
            IsReceiving = true;
            // 보상 알림에서 다시 수령을 요청해도 같은 횟수를 중복 지급하지 않도록 먼저 기록한다.
            progress.ClaimedCount += count;
            try
            {
                if (!giveReward(ById[id], count))
                {
                    progress.ClaimedCount -= count;
                    return false;
                }

                Changed?.Invoke(id);
                return true;
            }
            finally
            {
                // 지급 실패나 알림 중 예외가 발생해도 다음 수령 시도는 가능해야 한다.
                IsReceiving = false;
            }
        }
    }
}
