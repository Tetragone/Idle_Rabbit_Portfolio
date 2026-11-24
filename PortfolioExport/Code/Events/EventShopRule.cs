using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Game.Common;
using Game.Core;
using Game.Data;

namespace Game.Events
{
    public sealed class EventShopRule
    {
        public string Id { get; }
        public string EventId { get; }
        public string Name { get; }
        public string CurrencyId { get; }
        public int Price { get; }
        public ShopLimitType LimitType { get; }
        public int LimitCount { get; }
        public string Description { get; }
        public string Resource { get; }
        public IReadOnlyDictionary<Currency, int> Rewards { get; }

        public EventShopRule(string id, string eventId, string name, string currencyId, int price,
            ShopLimitType limitType, int limitCount, string description, string resource, IDictionary<Currency, int> rewards)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(eventId) || string.IsNullOrEmpty(currencyId))
                throw new ArgumentException("이벤트 상품 ID, 이벤트 ID와 가격 재화는 비어 있을 수 없습니다.");
            
            if (price < 0 || limitCount < -1 || !Enum.IsDefined(typeof(ShopLimitType), limitType))
                throw new ArgumentException("이벤트 상품의 가격 또는 구매 제한이 잘못되었습니다.");
            
            if (rewards == null || rewards.Count == 0) 
                throw new ArgumentException("상품 보상이 비어 있습니다.");
            
            foreach (var reward in rewards)
            {
                if (!Enum.IsDefined(typeof(Currency), reward.Key) || reward.Value <= 0)
                    throw new ArgumentException("이벤트 상점 보상은 양수 수량의 일반 재화여야 합니다.");
            }
            
            Id = id;
            EventId = eventId;
            Name = name;
            CurrencyId = currencyId;
            Price = price;
            LimitType = limitType;
            LimitCount = limitCount;
            Description = description;
            Resource = resource;
            Rewards = new ReadOnlyDictionary<Currency, int>(new Dictionary<Currency, int>(rewards));
        }

        public static List<EventShopRule> Load(string eventId, string currencyId)
        {
            return Load(eventId, new[] { currencyId });
        }

        public static List<EventShopRule> Load(string eventId, IEnumerable<string> currencyIds)
        {
            var result = new List<EventShopRule>();
            var currencies = new HashSet<string>(currencyIds);

            foreach (string id in Table.GetTable(TableType.EventShop).Keys)
            {
                if (Table.GetString(TableType.EventShop, id, "category") != eventId) 
                    continue;
                
                string priceCurrency = Table.GetString(TableType.EventShop, id, "need_type");
                
                if (!currencies.Contains(priceCurrency))
                    throw new ArgumentException(string.Format("상품 '{0}'의 가격 재화가 이벤트와 다릅니다.", id));
                
                string limit = Table.GetString(TableType.EventShop, id, "limit_type");
                
                if (!Enum.TryParse(limit, true, out ShopLimitType limitType) || !Enum.IsDefined(typeof(ShopLimitType), limitType))
                    throw new ArgumentException(string.Format("상품 '{0}'의 구매 제한 종류가 잘못되었습니다.", id));
                
                string count = Table.GetString(TableType.EventShop, id, "limit_value");
                int limitCount = count == "x" ? -1 : Table.GetInt(TableType.EventShop, id, "limit_value");
                
                if (count != "x" && limitCount < 0) 
                    throw new ArgumentException("구매 제한 횟수는 0 이상 또는 x여야 합니다.");
                
                var rewards = new Dictionary<Currency, int>();
                
                for (int index = 1; ; index++)
                {
                    string column = string.Format("item_{0}", index);
                    
                    if (!Table.IsValidColumn(TableType.EventShop, column)) 
                        break;
                    
                    string type = Table.GetString(TableType.EventShop, id, column);
                    
                    if (type == "x") 
                        continue;
                    
                    if (!Enum.TryParse(type, out Currency reward) || !Enum.IsDefined(typeof(Currency), reward)
                        || !Table.IsValidKey(TableType.Currency, type))
                        throw new ArgumentException(string.Format("상품 '{0}'의 보상은 일반 재화만 지원합니다.", id));
                    
                    int amount = Table.GetInt(TableType.EventShop, id, string.Format("value_{0}", index));
                    
                    if (amount <= 0) 
                        throw new ArgumentException("상품 보상 수량은 양수여야 합니다.");
                    rewards.TryGetValue(reward, out int previous);
                    
                    if (previous > int.MaxValue - amount) 
                        throw new ArgumentException("상품 보상 수량이 정수 범위를 넘습니다.");
                    rewards[reward] = previous + amount;
                }
                result.Add(new EventShopRule(id, eventId, Table.GetString(TableType.EventShop, id, "name"), priceCurrency,
                    Table.GetInt(TableType.EventShop, id, "need_value"), limitType, limitCount,
                    Table.GetString(TableType.EventShop, id, "desc"), Table.GetString(TableType.EventShop, id, "res"), rewards));
            }
            return result;
        }

        public long GetPeriodStart(long now)
        {
            if (LimitType == ShopLimitType.Daily) return TimeMgr.GetDayStartTimeNum(now);
            if (LimitType == ShopLimitType.Week) return TimeMgr.GetWeekStartTimeNum(now);
            if (LimitType == ShopLimitType.Month)
            {
                DateTime date = TimeMgr.GetNumToDate(now);
                return TimeMgr.GetTimeNum(new DateTime(date.Year, date.Month, 1));
            }
            return 0;
        }
    }
}
