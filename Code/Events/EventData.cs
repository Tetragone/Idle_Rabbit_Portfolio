using System;
using System.Collections.Generic;

using Game.Data;
using Game.Quests;

namespace Game.Events
{
    [Serializable]
    public abstract class EventData
    {
        public string EventId = "";
        public long AppliedStartUtcTicks;
        public int SchemaVersion = 1;
    }

    // 재화와 사냥 기록은 이 이벤트의 저장 데이터에서만 관리한다.
    [Serializable]
    public sealed class CurrencyExchangeEventData : EventData, IBaseData<CurrencyExchangeEventData>
    {
        // 이전 저장 형식과의 호환용 기본 재화 수량. 실제 잔액은 Currencies에 ID별로 저장한다.
        public int CurrencyCount;
        public List<EventCurrencyData> Currencies = new List<EventCurrencyData>();
        public int HuntDate;
        public int DailyHuntCount;
        public int TotalHuntCount;
        public long LastOfflineRewardTime;
        public List<EventHuntDailyData> HuntDailyRecords = new List<EventHuntDailyData>();
        public List<QuestProgressData> Quests = new List<QuestProgressData>();
        public List<EventShopPurchaseData> Purchases = new List<EventShopPurchaseData>();

        public void SetToDefault()
        {
            EventId = "";
            AppliedStartUtcTicks = 0;
            SchemaVersion = 1;
            CurrencyCount = 0;
            Currencies = new List<EventCurrencyData>();
            HuntDate = 0;
            DailyHuntCount = 0;
            TotalHuntCount = 0;
            LastOfflineRewardTime = 0;
            HuntDailyRecords = new List<EventHuntDailyData>();
            Quests = new List<QuestProgressData>();
            Purchases = new List<EventShopPurchaseData>();
        }

        public void SetData(CurrencyExchangeEventData beforeData)
        {
            EventId = beforeData.EventId;
            AppliedStartUtcTicks = beforeData.AppliedStartUtcTicks;
            SchemaVersion = beforeData.SchemaVersion;
            CurrencyCount = beforeData.CurrencyCount;
            Currencies = new List<EventCurrencyData>();

            if (beforeData.Currencies != null)
            {
                foreach (EventCurrencyData currency in beforeData.Currencies)
                {
                    Currencies.Add(new EventCurrencyData { CurrencyId = currency.CurrencyId, Count = currency.Count });
                }
            }

            HuntDate = beforeData.HuntDate;
            DailyHuntCount = beforeData.DailyHuntCount;
            TotalHuntCount = beforeData.TotalHuntCount;
            LastOfflineRewardTime = beforeData.LastOfflineRewardTime;
            HuntDailyRecords = new List<EventHuntDailyData>();

            if (beforeData.HuntDailyRecords != null)
            {
                foreach (EventHuntDailyData record in beforeData.HuntDailyRecords)
                {
                    HuntDailyRecords.Add(new EventHuntDailyData
                    {
                        Date = record.Date,
                        HuntCount = record.HuntCount,
                        OfflineRemainder = record.OfflineRemainder
                    });
                }
            }

            // 이전 저장 형식의 일일 획득량도 날짜별 한도 계산에 사용한다.
            if (HuntDailyRecords.Count == 0 && HuntDate > 0)
            {
                HuntDailyRecords.Add(new EventHuntDailyData { Date = HuntDate, HuntCount = DailyHuntCount });
            }

            Quests = QuestProgressData.CopyList(beforeData.Quests);
            Purchases = new List<EventShopPurchaseData>();
            if (beforeData.Purchases != null)
            {
                foreach (EventShopPurchaseData purchase in beforeData.Purchases)
                {
                    Purchases.Add(new EventShopPurchaseData
                    {
                        ProductId = purchase.ProductId, PeriodStart = purchase.PeriodStart, Count = purchase.Count
                    });
                }
            }
        }
    }

    [Serializable]
    public sealed class EventCurrencyData
    {
        public string CurrencyId = "";
        public int Count;
    }

    [Serializable]
    public sealed class EventHuntDailyData
    {
        public int Date;
        public int HuntCount;
        public double OfflineRemainder;
    }

    [Serializable]
    public sealed class EventShopPurchaseData
    {
        public string ProductId = "";
        public long PeriodStart;
        public int Count;
    }
}
