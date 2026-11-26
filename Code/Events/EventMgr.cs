using System;
using System.Collections.Generic;

using UnityEngine;

using Game.Common;
using Game.Data;
using Game.Core;
using Game.Quests;

namespace Game.Events
{
    public sealed class EventMgr : SingletonAllTime<EventMgr>
    {
        public event Action<string> Changed;
        public bool IsInitialized { get; private set; }
        public int InitializeCount { get; private set; }
        public IReadOnlyList<IGameEvent> Events => EventList.AsReadOnly();

        private readonly List<IGameEvent> EventList = new List<IGameEvent>();
        private readonly Dictionary<string, IGameEvent> ById = new Dictionary<string, IGameEvent>();
        private readonly Dictionary<string, Action> ChangeActions = new Dictionary<string, Action>();
        private readonly Dictionary<string, Func<string, string, string, EventSchedule, IGameEvent>> Factories =
            new Dictionary<string, Func<string, string, string, EventSchedule, IGameEvent>>
            {
                { "hunt_quest", CreateCurrencyExchangeEvent }
            };

        private static IGameEvent CreateCurrencyExchangeEvent(string id, string name, string description, EventSchedule schedule)
        {
            List<string> currencyIds = FindCurrencyIds(id);
            EventHuntRule huntRule = EventHuntRule.Load(id);
            string currencyId = huntRule == null ? currencyIds[0] : huntRule.CurrencyId;

            if (!currencyIds.Contains(currencyId))
                throw new ArgumentException("사냥 지급 재화가 해당 이벤트에 속하지 않습니다.", nameof(id));

            var instance = new CurrencyExchangeEvent(id, name, description, schedule, huntRule, currencyId,
                QuestMgr.Instance.GetRules(id), EventShopRule.Load(id, currencyIds), currencyIds);

            instance.RegisterQuests();

            return instance;
        }

        public void Initialize()
        {
            // 이벤트가 교체되기 전에 이전 객체를 참조하는 퀘스트 등록을 해제한다.
            foreach (IGameEvent oldEvent in EventList)
            {
                QuestMgr.Instance.Unregister(oldEvent.Id);

                if (ChangeActions.TryGetValue(oldEvent.Id, out Action action))
                {
                    oldEvent.ChangedData -= action;
                }
            }

            ChangeActions.Clear();
            EventList.Clear();
            ById.Clear();
            IsInitialized = false;

            foreach (string productId in Table.GetTable(TableType.EventShop).Keys)
            {
                string category = Table.GetString(TableType.EventShop, productId, "category");
                if (!Table.IsValidKey(TableType.Event, category))
                    throw new ArgumentException(string.Format("상품 '{0}'의 이벤트 '{1}'가 없습니다.", productId, category));
            }

            foreach (string id in Table.GetTable(TableType.Event).Keys)
            {
                if (!Factories.TryGetValue(id, out var factory))
                {
                    Debug.LogErrorFormat("Event '{0}' disabled", id);
                    continue;
                }

                bool hasShop = false;

                foreach (string sid in Table.GetTable(TableType.EventShop).Keys)
                {
                    if (Table.GetString(TableType.EventShop, sid, "category") == id)
                    {
                        hasShop = true;
                        break;
                    }
                }

                var schedule = new EventSchedule(
                    TimeMgr.ParseTableDate(Table.GetString(TableType.Event, id, "start")),
                    TimeMgr.ParseTableDate(Table.GetString(TableType.Event, id, "end")), hasShop);
                IGameEvent instance = factory(id,
                    Table.GetString(TableType.Event, id, "name"),
                    Table.GetString(TableType.Event, id, "desc"), schedule);
                EventList.Add(instance);
                ById.Add(id, instance);

                ConnectChangedEvent(instance);
            }

            IsInitialized = true;
            InitializeCount++;
        }

        private void ConnectChangedEvent(IGameEvent instance)
        {
            // 외부에서는 구현 객체 대신 관리자의 이벤트 ID 알림을 사용한다.
            Action action = () => Changed?.Invoke(instance.Id);
            ChangeActions.Add(instance.Id, action);
            instance.ChangedData += action;
        }

        public void RegisterDataHandlers<T>(Action<string, Func<T>, Action<T>> register)
            where T : EventData, IBaseData<T>, new()
        {
            // 저장 담당자는 이벤트 구현 객체 없이 읽기와 쓰기 함수만 등록한다.
            foreach (IGameEvent gameEvent in EventList)
            {
                if (gameEvent is GameEvent<T> savedEvent)
                {
                    register(savedEvent.Id, savedEvent.GetData, savedEvent.SetData);
                }
            }
        }

        private static List<string> FindCurrencyIds(string eventId)
        {
            var result = new List<string>();

            foreach (string currencyId in Table.GetTable(TableType.EventCurrency).Keys)
            {
                if (Table.GetString(TableType.EventCurrency, currencyId, "event_id") == eventId)
                {
                    result.Add(currencyId);
                }
            }

            if (result.Count == 0)
                throw new ArgumentException(string.Format("이벤트 '{0}'의 재화가 없습니다.", eventId));

            return result;
        }

        public bool TryGetEvent(string id, out IGameEvent instance)
        {
            return ById.TryGetValue(id, out instance);
        }

        public string GetEventName(string eventId)
        {
            return ById.TryGetValue(eventId, out IGameEvent instance) ? instance.Name : "";
        }

        public bool TryGetEventInfo(string eventId, out EventInfo info)
        {
            info = null;

            if (!IsInitialized || !ById.TryGetValue(eventId, out IGameEvent instance))
                return false;

            string background = Table.IsValidColumn(TableType.Event, "background")
                ? Table.GetString(TableType.Event, eventId, "background")
                : "x";

            info = new EventInfo(instance.Id, instance.Name, instance.Description, background, instance.Schedule);
            return true;
        }

        public EventHuntInfo GetHuntInfo(string eventId, long now)
        {
            if (!IsInitialized || !ById.TryGetValue(eventId, out IGameEvent instance)
                || !(instance is IEventHunt hunt))
                return null;

            return hunt.GetHuntInfo(now);
        }

        public bool IsEventVisible(string eventId, long now)
        {
            return IsInitialized && ById.TryGetValue(eventId, out IGameEvent instance)
                && instance.Schedule.IsVisible(now);
        }

        public List<string> GetShopEventIds()
        {
            var ids = new List<string>();

            foreach (IGameEvent gameEvent in EventList)
            {
                if (gameEvent is IEventShop shop && shop.ShopItems.Count > 0)
                {
                    ids.Add(gameEvent.Id);
                }
            }

            return ids;
        }

        public string GetShopName(string eventId)
        {
            return ById.TryGetValue(eventId, out IGameEvent gameEvent) ? gameEvent.Name : "";
        }

        public bool IsShopOpen(string eventId, long now)
        {
            return TryGetShop(eventId, out IEventShop shop)
                && shop.ShopItems.Count > 0
                && ById[eventId].Schedule.CanExchange(now);
        }

        public IReadOnlyList<EventShopRule> GetShopItems(string eventId)
        {
            if (TryGetShop(eventId, out IEventShop shop))
            {
                return shop.ShopItems;
            }

            return Array.Empty<EventShopRule>();
        }

        private bool TryGetShop(string eventId, out IEventShop shop)
        {
            shop = null;

            if (!IsInitialized || !ById.TryGetValue(eventId, out IGameEvent gameEvent))
                return false;

            shop = gameEvent as IEventShop;
            return shop != null;
        }

        private bool TryGetCurrency(string eventId, out IEventCurrency currency)
        {
            currency = null;

            if (!IsInitialized || !ById.TryGetValue(eventId, out IGameEvent gameEvent))
                return false;

            currency = gameEvent as IEventCurrency;
            return currency != null;
        }

        public string GetEventCurrencyId(string eventId)
        {
            IReadOnlyList<string> ids = GetEventCurrencyIds(eventId);

            return ids.Count == 0 ? "" : ids[0];
        }

        public IReadOnlyList<string> GetEventCurrencyIds(string eventId)
        {
            return TryGetCurrency(eventId, out IEventCurrency currency) ? currency.CurrencyIds : Array.Empty<string>();
        }

        public int GetEventCurrencyAmount(string eventId)
        {
            return GetEventCurrencyAmount(eventId, GetEventCurrencyId(eventId));
        }

        public int GetEventCurrencyAmount(string eventId, string currencyId)
        {
            return TryGetCurrency(eventId, out IEventCurrency currency) ? currency.GetCurrency(currencyId) : 0;
        }

        public bool TryGetShopItem(string eventId, string productId, out EventShopRule product)
        {
            product = null;

            return TryGetShop(eventId, out IEventShop shop)
                && shop.TryGetShopItem(productId, out product);
        }

        public int GetShopPurchaseCount(string eventId, string productId, long now)
        {
            return TryGetShop(eventId, out IEventShop shop)
                ? shop.GetPurchaseCount(productId, now)
                : 0;
        }

        public ShopPurchaseResult GetShopPurchaseResult(string eventId, string productId, long now)
        {
            if (!TryGetShop(eventId, out IEventShop shop))
                return ShopPurchaseResult.EventClosed;

            ShopPurchaseResult result = shop.GetPurchaseResult(productId, now);

            if (result != ShopPurchaseResult.Success)
                return result;

            shop.TryGetShopItem(productId, out EventShopRule product);

            return CurrencyReward.CanGive(product.Rewards)
                ? ShopPurchaseResult.Success
                : ShopPurchaseResult.RewardLimitReached;
        }

        public ShopPurchaseResult TryBuy(string eventId, string productId, long now)
        {
            if (!TryGetShop(eventId, out IEventShop shop))
                return ShopPurchaseResult.EventClosed;

            return shop.TryBuy(productId, now, CurrencyReward.TryGive);
        }

        public void Refresh(long now)
        {
            foreach (IGameEvent instance in EventList)
            {
                instance.Refresh(now);
            }
        }

        public void OnEnemyKilled(BattleType battleType)
        {
            if (!IsInitialized) return;

            long now = TimeMgr.GetNowTimeNum();
            foreach (IGameEvent gameEvent in EventList)
            {
                if (gameEvent is IEventHunt hunt)
                {
                    hunt.TryGetHuntReward(battleType, now);
                }
            }
        }

        public Dictionary<string, int> ReceiveOfflineHuntRewards(long start, long end, long now)
        {
            var rewards = new Dictionary<string, int>();

            if (!IsInitialized)
                return rewards;

            foreach (IGameEvent gameEvent in EventList)
            {
                if (!(gameEvent is IEventOfflineHunt hunt))
                    continue;

                int count = hunt.ReceiveOfflineHuntReward(start, end, now);

                if (count > 0)
                {
                    rewards.Add(gameEvent.Id, count);
                }
            }

            return rewards;
        }

        public void CompleteOfflineHuntCheck(long now)
        {
            if (!IsInitialized)
                return;

            foreach (IGameEvent gameEvent in EventList)
            {
                if (gameEvent is IEventOfflineHunt hunt)
                {
                    hunt.CompleteOfflineHuntCheck(now);
                }
            }
        }

    }
}
