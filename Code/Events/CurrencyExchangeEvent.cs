using System;
using System.Collections.Generic;

using UnityEngine;

using Game.Common;
using Game.Core;
using Game.Quests;

namespace Game.Events
{
    public sealed class CurrencyExchangeEvent : GameEvent<CurrencyExchangeEventData>, IEventOfflineHunt, IEventShop
    {
        public EventHuntRule HuntRule { get; }

        // 기존 단일 재화 API의 기본 재화이며, 다중 재화 조회는 CurrencyIds와 ID별 GetCurrency를 사용한다.
        public string CurrencyId { get; }
        public IReadOnlyList<string> CurrencyIds => EventCurrencies.AsReadOnly();
        public QuestModule Quests { get; }
        public int DailyHuntCount => Data.DailyHuntCount;
        public int TotalHuntCount => Data.TotalHuntCount;
        public IReadOnlyList<EventShopRule> ShopItems => ShopRules.AsReadOnly();

        private readonly List<EventShopRule> ShopRules;
        private readonly List<string> EventCurrencies = new List<string>();
        private readonly Dictionary<string, EventShopRule> Products = new Dictionary<string, EventShopRule>();
        private bool IsBuying;
        private bool IsReceivingOfflineReward;

        public CurrencyExchangeEvent(string id, string name, string description, EventSchedule schedule,
            EventHuntRule huntRule = null, string currencyId = "", IEnumerable<QuestRule> questRules = null,
            IEnumerable<EventShopRule> shopRules = null, IEnumerable<string> currencyIds = null)
            : base(id, name, description, schedule)
        {
            HuntRule = huntRule;

            if (!string.IsNullOrEmpty(currencyId))
            {
                EventCurrencies.Add(currencyId);
            }

            if (currencyIds != null)
            {
                foreach (string value in currencyIds)
                {
                    if (string.IsNullOrEmpty(value))
                        throw new ArgumentException("이벤트 재화 ID가 비어 있습니다.", nameof(currencyIds));

                    if (!EventCurrencies.Contains(value))
                    {
                        EventCurrencies.Add(value);
                    }
                }
            }

            CurrencyId = EventCurrencies.Count == 0 ? "" : EventCurrencies[0];

            if (huntRule != null && !EventCurrencies.Contains(huntRule.CurrencyId))
                throw new ArgumentException("이벤트 재화와 사냥 지급 재화가 일치하지 않습니다.", nameof(currencyId));
            Quests = new QuestModule(questRules, Data.Quests);
            Quests.Changed += _ => NotifyChangedData();
            ShopRules = shopRules == null ? new List<EventShopRule>() : new List<EventShopRule>(shopRules);
            foreach (EventShopRule product in ShopRules)
            {
                if (product.EventId != id || !EventCurrencies.Contains(product.CurrencyId))
                    throw new ArgumentException("이벤트와 상품의 소속 또는 재화가 일치하지 않습니다.");
                Products.Add(product.Id, product);
            }
        }

        public int GetCurrency()
        {
            return string.IsNullOrEmpty(CurrencyId) ? Data.CurrencyCount : GetCurrency(CurrencyId);
        }

        public int GetCurrency(string currencyId)
        {
            foreach (EventCurrencyData currency in Data.Currencies)
            {
                if (currency.CurrencyId == currencyId)
                    return currency.Count;
            }

            return Data.Currencies.Count == 0 && currencyId == CurrencyId ? Data.CurrencyCount : 0;
        }

        private void ChangeCurrency(string currencyId, int amount)
        {
            EventCurrencyData target = Data.Currencies.Find(currency => currency.CurrencyId == currencyId);

            if (target == null)
            {
                target = new EventCurrencyData { CurrencyId = currencyId, Count = GetCurrency(currencyId) };
                Data.Currencies.Add(target);
            }

            target.Count += amount;

            // 이전 단일 재화 조회와 저장 형식도 기본 재화의 현재 수량을 유지한다.
            if (currencyId == CurrencyId)
            {
                Data.CurrencyCount = target.Count;
            }
        }

        public void RegisterQuests()
        {
            QuestMgr.Instance.Register(Id, Quests,
                updateState: now => Refresh(now),
                canUse: Schedule.CanAcquire,
                giveReward: GiveQuestReward);
        }

        public EventHuntInfo GetHuntInfo(long now)
        {
            Refresh(now);

            if (HuntRule == null)
                return null;

            return new EventHuntInfo(HuntRule.CurrencyId, Data.DailyHuntCount, HuntRule.MaxDailyGet,
                Data.TotalHuntCount, HuntRule.MaxGet);
        }

        private bool GiveQuestReward(QuestRule rule, long times, long now)
        {
            if (!Schedule.CanAcquire(now)) return false;
            if (!EventCurrencies.Contains(rule.RewardType)) return QuestMgr.TryGiveNormalReward(rule, times);
            if (times <= 0 || times > int.MaxValue / rule.RewardCount) return false;
            return TryAddQuestReward(rule.RewardType, (int)times * rule.RewardCount, now);
        }

        public override void SetData(CurrencyExchangeEventData data)
        {
            if (data == null || data.CurrencyCount < 0 || data.DailyHuntCount < 0
                || data.TotalHuntCount < data.DailyHuntCount || data.LastOfflineRewardTime < 0)
                throw new ArgumentException("이벤트 재화와 사냥 획득 기록이 잘못되었습니다.", nameof(data));

            ValidateDailyRecords(data);
            ValidateCurrencies(data);

            var productIds = new HashSet<string>();
            if (data.Purchases != null)
            {
                foreach (EventShopPurchaseData purchase in data.Purchases)
                {
                    if (purchase == null || string.IsNullOrEmpty(purchase.ProductId) || purchase.Count < 0
                        || !productIds.Add(purchase.ProductId))
                        throw new ArgumentException("이벤트 상점 구매 기록이 잘못되었습니다.", nameof(data));
                }
            }

            base.SetData(data);

            if (!string.IsNullOrEmpty(CurrencyId) && Data.Currencies.Count == 0)
            {
                ChangeCurrency(CurrencyId, 0);
            }

            if (!string.IsNullOrEmpty(CurrencyId))
            {
                Data.CurrencyCount = GetCurrency(CurrencyId);
            }

            Quests.SetProgressData(Data.Quests);
            NotifyChangedData();
        }

        private static void ValidateCurrencies(CurrencyExchangeEventData data)
        {
            if (data.Currencies == null)
                return;

            var ids = new HashSet<string>();

            foreach (EventCurrencyData currency in data.Currencies)
            {
                if (currency == null || string.IsNullOrEmpty(currency.CurrencyId) || currency.Count < 0
                    || !ids.Add(currency.CurrencyId))
                {
                    throw new ArgumentException("이벤트 재화 기록이 잘못되었습니다.", nameof(data));
                }
            }
        }

        private static void ValidateDailyRecords(CurrencyExchangeEventData data)
        {
            if (data.HuntDailyRecords == null)
                return;

            var dates = new HashSet<int>();

            foreach (EventHuntDailyData record in data.HuntDailyRecords)
            {
                if (record == null || record.Date <= 0 || record.HuntCount < 0
                    || record.HuntCount > data.TotalHuntCount || !dates.Add(record.Date)
                    || double.IsNaN(record.OfflineRemainder) || double.IsInfinity(record.OfflineRemainder)
                    || record.OfflineRemainder < 0 || record.OfflineRemainder >= 1)
                {
                    throw new ArgumentException("날짜별 이벤트 사냥 기록이 잘못되었습니다.", nameof(data));
                }
            }
        }

        private EventHuntDailyData GetDailyRecord(int date)
        {
            foreach (EventHuntDailyData record in Data.HuntDailyRecords)
            {
                if (record.Date == date)
                    return record;
            }

            var created = new EventHuntDailyData { Date = date };
            Data.HuntDailyRecords.Add(created);

            return created;
        }

        private void AddHuntReward(EventHuntDailyData record, int count)
        {
            ChangeCurrency(HuntRule.CurrencyId, count);
            Data.TotalHuntCount += count;
            record.HuntCount += count;

            if (record.Date == Data.HuntDate)
            {
                Data.DailyHuntCount = record.HuntCount;
            }
        }

        protected override bool RefreshData(long now)
        {
            bool changed = false;

            // 새 회차나 로드로 데이터 객체가 교체되면 퀘스트도 새 기록을 사용한다.
            Quests.SetProgressData(Data.Quests);

            if (Schedule.CanAcquire(now))
            {
                changed = Quests.Refresh(now) || changed;
            }

            int today = TimeMgr.GetDateNum(TimeMgr.GetNumToDate(now));

            // 새 회차가 시작되기 전에는 이전 회차의 기록을 그대로 유지한다.
            if (now >= Schedule.StartTimeNum && today > Data.HuntDate)
            {
                Data.HuntDate = today;
                Data.DailyHuntCount = GetDailyRecord(today).HuntCount;
                changed = true;
            }

            return changed;
        }

        public int TryGetHuntReward(BattleType battleType, long now)
        {
            // 날짜 변경 알림이 늦더라도 실제 지급 전에 회차와 일일 기록을 갱신한다.
            Refresh(now);

            if (HuntRule == null || battleType != HuntRule.HuntType || !Schedule.CanAcquire(now))
                return 0;

            int date = TimeMgr.GetDateNum(TimeMgr.GetNumToDate(now));

            if (date < Data.HuntDate)
                return 0;

            EventHuntDailyData record = GetDailyRecord(date);
            int count = HuntRule.GetDropCount(record.HuntCount, Data.TotalHuntCount, GetCurrency(HuntRule.CurrencyId));

            if (count <= 0 || HuntRule.DropRate <= 0f)
                return 0;

            if (HuntRule.DropRate < 1f && UnityEngine.Random.Range(0f, 1f) >= HuntRule.DropRate)
                return 0;

            AddHuntReward(record, count);

            NotifyChangedData();
            return count;
        }

        public int ReceiveOfflineHuntReward(long start, long end, long now)
        {
            if (start < 0 || end < start || now < end)
                throw new ArgumentException("오프라인 정산 시간 구간이 잘못되었습니다.");

            if (IsReceivingOfflineReward || now < Schedule.StartTimeNum)
                return 0;

            int received = 0;
            IsReceivingOfflineReward = true;

            try
            {
                // 현재 회차를 먼저 적용하되, 전날 기록은 정산이 끝날 때까지 남겨 둔다.
                Refresh(now);

                int today = TimeMgr.GetDateNum(TimeMgr.GetNumToDate(now));

                if (now <= Data.LastOfflineRewardTime || today < Data.HuntDate)
                    return 0;

                long cappedEnd = start + Math.Min(end - start, Constant.IdleReawedMaxSecond);
                long rewardStart = Math.Max(Math.Max(start, Data.LastOfflineRewardTime), Schedule.StartTimeNum);
                long rewardEnd = Math.Min(cappedEnd, Schedule.EndTimeNum);

                if (HuntRule != null && HuntRule.IsGetOffline && HuntRule.HuntType == BattleType.Normal)
                {
                    received = ReceiveOfflinePeriod(rewardStart, rewardEnd);
                }

                // 한도 초과나 지급 불가인 구간도 다시 요청하지 않도록 복귀 시각까지 완료한다.
                SetOfflineHuntCheckTime(now);
            }
            finally
            {
                IsReceivingOfflineReward = false;
            }

            NotifyChangedData();

            return received;
        }

        public void CompleteOfflineHuntCheck(long now)
        {
            if (now < 0)
                throw new ArgumentException("오프라인 정산 완료 시각이 잘못되었습니다.", nameof(now));

            if (IsReceivingOfflineReward || now < Schedule.StartTimeNum)
                return;

            IsReceivingOfflineReward = true;

            try
            {
                Refresh(now);

                int today = TimeMgr.GetDateNum(TimeMgr.GetNumToDate(now));

                if (now <= Data.LastOfflineRewardTime || today < Data.HuntDate)
                    return;

                SetOfflineHuntCheckTime(now);
            }
            finally
            {
                IsReceivingOfflineReward = false;
            }

            NotifyChangedData();
        }

        private void SetOfflineHuntCheckTime(long now)
        {
            Data.LastOfflineRewardTime = now;
            Data.HuntDailyRecords.RemoveAll(record => record.Date < Data.HuntDate);
        }

        private int ReceiveOfflinePeriod(long start, long end)
        {
            int received = 0;

            while (start < end)
            {
                long nextDay = TimeMgr.AddDays(TimeMgr.GetDayStartTimeNum(start), 1);
                long dayEnd = Math.Min(end, nextDay);

                received += ReceiveOfflineDayReward(start, dayEnd);
                start = dayEnd;
            }

            return received;
        }

        private int ReceiveOfflineDayReward(long start, long end)
        {
            int date = TimeMgr.GetDateNum(TimeMgr.GetNumToDate(start));
            EventHuntDailyData record = GetDailyRecord(date);
            int available = HuntRule.GetAvailableCount(record.HuntCount, Data.TotalHuntCount, GetCurrency(HuntRule.CurrencyId));

            // 실수 드랍 확률을 그대로 곱할 때 정수 경계에서 1개가 줄어드는 오차를 피한다.
            decimal expected = HuntRule.GetOfflineExpectedCount(end - start, record.OfflineRemainder);
            decimal wholeCount = decimal.Floor(expected);
            int count = (int)Math.Min(wholeCount, available);

            // 한도로 버린 보상은 재화를 사용한 뒤 다시 받을 수 없도록 남기지 않는다.
            record.OfflineRemainder = wholeCount >= available ? 0 : (double)(expected - wholeCount);
            AddHuntReward(record, count);

            return count;
        }

        public bool ReportQuestAction(QuestAction action, long count, long now)
        {
            Refresh(now);
            return Schedule.CanAcquire(now) && Quests.ReportAction(action, count, now);
        }

        public bool TryAddQuestReward(string currencyId, int count, long now)
        {
            Refresh(now);
            if (!Schedule.CanAcquire(now) || !EventCurrencies.Contains(currencyId) || count <= 0
                || GetCurrency(currencyId) > int.MaxValue - count) return false;

            // 퀘스트 보상은 사냥 획득 한도를 소비하지 않는다.
            ChangeCurrency(currencyId, count);
            NotifyChangedData();
            return true;
        }

        private EventShopPurchaseData GetPurchaseData(EventShopRule product, long now)
        {
            long period = product.GetPeriodStart(now);
            foreach (EventShopPurchaseData purchase in Data.Purchases)
            {
                if (purchase.ProductId != product.Id) continue;
                if (period > purchase.PeriodStart)
                {
                    purchase.PeriodStart = period;
                    purchase.Count = 0;
                }
                return purchase;
            }
            var created = new EventShopPurchaseData { ProductId = product.Id, PeriodStart = period };
            Data.Purchases.Add(created);
            return created;
        }

        public int GetPurchaseCount(string productId, long now)
        {
            Refresh(now);
            return Products.TryGetValue(productId, out EventShopRule product) ? GetPurchaseData(product, now).Count : 0;
        }

        public bool TryGetShopItem(string productId, out EventShopRule product)
        {
            return Products.TryGetValue(productId, out product);
        }

        public ShopPurchaseResult GetPurchaseResult(string productId, long now)
        {
            Refresh(now);
            if (IsBuying)
                return ShopPurchaseResult.Busy;
            if (!Products.TryGetValue(productId, out EventShopRule product))
                return ShopPurchaseResult.InvalidProduct;
            if (!Schedule.CanExchange(now))
                return ShopPurchaseResult.EventClosed;

            EventShopPurchaseData purchase = GetPurchaseData(product, now);

            if (now < purchase.PeriodStart || purchase.Count == int.MaxValue
                || (product.LimitCount >= 0 && purchase.Count >= product.LimitCount))
                return ShopPurchaseResult.LimitReached;
            if (GetCurrency(product.CurrencyId) < product.Price)
                return ShopPurchaseResult.NotEnoughCurrency;

            return ShopPurchaseResult.Success;
        }

        public ShopPurchaseResult TryBuy(string productId, long now, Func<IReadOnlyDictionary<Currency, int>, bool> giveReward)
        {
            ShopPurchaseResult result = GetPurchaseResult(productId, now);
            if (result != ShopPurchaseResult.Success) return result;
            if (giveReward == null) throw new ArgumentNullException(nameof(giveReward));
            EventShopRule product = Products[productId];
            EventShopPurchaseData purchase = GetPurchaseData(product, now);
            IsBuying = true;
            // 보상 알림 중 재요청이 들어와도 같은 잔액과 구매 횟수를 사용하지 못하게 먼저 반영한다.
            ChangeCurrency(product.CurrencyId, -product.Price);
            purchase.Count++;
            try
            {
                if (!giveReward(product.Rewards))
                {
                    ChangeCurrency(product.CurrencyId, product.Price);
                    purchase.Count--;
                    return ShopPurchaseResult.RewardLimitReached;
                }
            }
            finally
            {
                IsBuying = false;
            }
            NotifyChangedData();
            return ShopPurchaseResult.Success;
        }
    }
}
