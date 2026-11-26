using System;
using System.Collections.Generic;

using Game.Common;
using Game.Core;
using Game.Data;

namespace Game.Quests
{
    // 등록된 퀘스트를 관리하며, 이벤트의 구체적인 구현은 알지 않는다.
    public sealed class QuestMgr : SingletonAllTime<QuestMgr>
    {
        public event Action Changed;
        public bool IsInitialized { get; private set; }
        public QuestModule CommonQuests { get; private set; }

        private readonly Dictionary<string, QuestRule> Rules = new Dictionary<string, QuestRule>();
        private readonly Dictionary<string, QuestRegistration> Registrations = new Dictionary<string, QuestRegistration>();
        private QuestData MyData = new QuestData();
        private bool IsReceiving;

        public void Initialize()
        {
            IsInitialized = false;
            // 기존 모듈의 알림 연결도 해제하여 재초기화 시 중복 전달을 막는다.
            foreach (string category in new List<string>(Registrations.Keys))
            {
                Unregister(category);
            }
            Rules.Clear();

            foreach (QuestRule rule in QuestRule.LoadAll()) 
            {
                Rules.Add(rule.Id, rule);
            }

            CommonQuests = new QuestModule(GetRules("common"), MyData.Progress);
            QuestModule common = CommonQuests;
            Register("common", common, now => common.Refresh(now), now => true,
                (rule, times, now) => TryGiveNormalReward(rule, times));
            IsInitialized = true;
        }

        public void Register(string category, QuestModule module, Action<long> updateState,
            Func<long, bool> canUse, Func<QuestRule, long, long, bool> giveReward)
        {
            if (string.IsNullOrEmpty(category) || module == null || updateState == null || canUse == null || giveReward == null)
                throw new ArgumentException("퀘스트 등록에는 소속, 모듈, 갱신·사용 조건·보상 함수가 필요합니다.");

            foreach (QuestRule rule in module.Rules)
            {
                if (rule.Category != category)
                    throw new ArgumentException("등록 소속과 퀘스트 설정의 소속이 일치하지 않습니다.", nameof(category));
            }

            Unregister(category);
            Registrations.Add(category, new QuestRegistration(module, updateState, canUse, giveReward));
            module.Changed += OnQuestChanged;
        }

        public void Unregister(string category)
        {
            if (!Registrations.TryGetValue(category, out QuestRegistration registration)) return;
            registration.Module.Changed -= OnQuestChanged;
            Registrations.Remove(category);
        }

        private void OnQuestChanged(string id)
        {
            Changed?.Invoke();
        }

        public List<QuestRule> GetRules(string category)
        {
            var result = new List<QuestRule>();
            foreach (QuestRule rule in Rules.Values)
            {
                if (rule.Category == category) result.Add(rule);
            }
            return result;
        }

        public QuestRule GetQuestInfo(string questId)
        {
            return IsInitialized && Rules.TryGetValue(questId, out QuestRule rule) ? rule : null;
        }

        public QuestState GetQuestState(string questId)
        {
            return GetQuestState(questId, TimeMgr.GetNowTimeNum());
        }

        public QuestState GetQuestState(string questId, long now)
        {
            QuestRule rule = GetQuestInfo(questId);

            if (rule == null)
                return null;

            QuestRegistration registration = FindRegistration(rule);

            if (registration == null)
                return null;

            registration.UpdateState(now);
            bool canUse = registration.CanUse(now);
            long available = canUse ? registration.Module.GetAvailableRewardCount(questId, now) : 0;

            // 종료된 이벤트의 기록을 오늘 날짜로 초기화하지 않고 마지막 진행량을 표시한다.
            return new QuestState(rule, registration.Module.GetSavedProgress(questId), available, canUse, IsReceiving);
        }

        public void SetData(QuestData data)
        {
            var next = new QuestData();
            next.SetData(data);
            MyData = next;
            if (CommonQuests != null) CommonQuests.SetProgressData(MyData.Progress);
        }

        public QuestData GetData()
        {
            var result = new QuestData();
            result.SetData(MyData);
            return result;
        }

        public void Refresh(long now)
        {
            if (!IsInitialized) return;

            foreach (QuestRegistration registration in Registrations.Values)
            {
                registration.UpdateState(now);
            }
        }

        public void ReportAction(QuestAction action, long count = 1)
        {
            if (!IsInitialized || count <= 0) return;

            long now = TimeMgr.GetNowTimeNum();
            foreach (QuestRegistration registration in Registrations.Values)
            {
                registration.UpdateState(now);
                if (registration.CanUse(now)) registration.Module.ReportAction(action, count, now);
            }
        }

        private QuestRegistration FindRegistration(QuestRule rule)
        {
            Registrations.TryGetValue(rule.Category, out QuestRegistration registration);
            return registration;
        }

        public QuestProgressData GetProgress(string id)
        {
            if (!IsInitialized || !Rules.TryGetValue(id, out QuestRule rule)) return null;
            
            QuestRegistration registration = FindRegistration(rule);
            if (registration == null) return null;
            
            long now = TimeMgr.GetNowTimeNum();
            registration.UpdateState(now);
            return registration.CanUse(now) ? registration.Module.GetProgress(id, now) : null;
        }

        public long GetAvailableRewardCount(string id)
        {
            if (!IsInitialized || !Rules.TryGetValue(id, out QuestRule rule)) return 0;
            QuestRegistration registration = FindRegistration(rule);
            
            if (registration == null) return 0;
            long now = TimeMgr.GetNowTimeNum();
            
            registration.UpdateState(now);
            return registration.CanUse(now) ? registration.Module.GetAvailableRewardCount(id, now) : 0;
        }

        public bool TryReceiveReward(string id, long count = 1)
        {
            if (!IsInitialized || IsReceiving || count <= 0 || !Rules.TryGetValue(id, out QuestRule rule))
                return false;

            IsReceiving = true;

            try
            {
                QuestRegistration registration = FindRegistration(rule);

                if (registration == null)
                    return false;

                long now = TimeMgr.GetNowTimeNum();
                registration.UpdateState(now);

                if (!registration.CanUse(now) || !TryReceiveReward(registration, id, count, now))
                    return false;

                DataMgr.Instance.Save();
            }
            finally
            {
                IsReceiving = false;
            }

            Changed?.Invoke();
            return true;
        }

        public bool TryReceiveRewardAll(string id)
        {
            if (!IsInitialized || IsReceiving)
                return false;

            return TryReceiveReward(id, GetAvailableRewardCount(id));
        }

        public QuestReceiveAllResult ReceiveAllCommonRewards()
        {
            if (!IsInitialized || IsReceiving)
                return new QuestReceiveAllResult(false, 0, 0);

            int receivedCount = 0;
            int failedCount = 0;
            var rewards = new Dictionary<Currency, double>();
            IsReceiving = true;

            try
            {
                long now = TimeMgr.GetNowTimeNum();

                foreach (QuestRule rule in GetRules("common"))
                {
                    QuestRegistration registration = FindRegistration(rule);

                    if (registration == null)
                        continue;

                    registration.UpdateState(now);

                    if (!registration.CanUse(now))
                        continue;

                    long count = registration.Module.GetAvailableRewardCount(rule.Id, now);

                    if (count <= 0)
                        continue;

                    // 일부 보상이 거절돼도 다른 퀘스트의 수령은 계속 진행한다.
                    if (TryReceiveReward(registration, rule.Id, count, now))
                    {
                        receivedCount++;

                        // 실제 지급에 성공한 보상만 합산하며, 팝업 수량은 int 합계 범위에 제한하지 않는다.
                        Currency currency = (Currency)Enum.Parse(typeof(Currency), rule.RewardType);
                        rewards.TryGetValue(currency, out double before);
                        rewards[currency] = before + (double)rule.RewardCount * count;
                    }
                    else
                    {
                        failedCount++;
                    }
                }

                if (receivedCount > 0)
                {
                    DataMgr.Instance.Save();
                }
            }
            finally
            {
                IsReceiving = false;
            }

            Changed?.Invoke();

            return new QuestReceiveAllResult(true, receivedCount, failedCount, rewards);
        }

        private bool TryReceiveReward(QuestRegistration registration, string id, long count, long now)
        {
            // 단일 수령과 모두 받기는 같은 기록·지급 처리를 사용하고 저장은 호출부에서 수행한다.
            return registration.Module.TryReceiveReward(id, count, now,
                (definition, times) => registration.GiveReward(definition, times, now));
        }

        public static bool TryGiveNormalReward(QuestRule rule, long times)
        {
            // 기존 재화 API는 int 수량을 받으므로 한 번의 수령량도 그 범위로 제한한다.
            if (times <= 0 || times > int.MaxValue / rule.RewardCount) return false;
            int amount = (int)times * rule.RewardCount;

            if (!Enum.TryParse(rule.RewardType, out Currency currency) || !Enum.IsDefined(typeof(Currency), currency))
                return false;

            return CurrencyReward.TryGive(new Dictionary<Currency, int> { { currency, amount } });
        }

        // 이 소속을 처리하는 방법만 보관하며, 이벤트 종류에 따른 분기는 두지 않는다.
        private sealed class QuestRegistration
        {
            public readonly QuestModule Module;
            public readonly Action<long> UpdateState;
            public readonly Func<long, bool> CanUse;
            public readonly Func<QuestRule, long, long, bool> GiveReward;

            public QuestRegistration(QuestModule module, Action<long> updateState,
                Func<long, bool> canUse, Func<QuestRule, long, long, bool> giveReward)
            {
                Module = module;
                UpdateState = updateState;
                CanUse = canUse;
                GiveReward = giveReward;
            }
        }
    }

    [Serializable]
    public sealed class QuestData : IBaseData<QuestData>
    {
        public List<QuestProgressData> Progress = new List<QuestProgressData>();
        public void SetToDefault() { Progress = new List<QuestProgressData>(); }
        public void SetData(QuestData source) { Progress = QuestProgressData.CopyList(source.Progress); }
    }

    [Serializable]
    public sealed class QuestProgressData
    {
        public string QuestId = "";
        public long PeriodStart;
        public long TotalProgress;
        public long ClaimedCount;

        public QuestProgressData Copy()
        {
            return new QuestProgressData
            {
                QuestId = QuestId, PeriodStart = PeriodStart, TotalProgress = TotalProgress, ClaimedCount = ClaimedCount
            };
        }

        public static List<QuestProgressData> CopyList(List<QuestProgressData> source)
        {
            var result = new List<QuestProgressData>();
            if (source != null)
            {
                foreach (QuestProgressData item in source)
                {
                    result.Add(item.Copy());
                }
            }
            return result;
        }
    }
}
