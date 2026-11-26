using System;

using Game.Common;
using Game.Core;

namespace Game.Events
{
    public sealed class EventSchedule
    {
        public long StartTimeNum { get; }
        public long EndTimeNum { get; }
        public long ExchangeEndTimeNum { get; }
        public bool HasShop { get; }

        public EventSchedule(long start, long end, bool hasShop)
        {
            if (end <= start)
            {
                throw new ArgumentException("이벤트 종료 시각은 시작 시각보다 늦어야 합니다.", nameof(end));
            }

            StartTimeNum = start;
            EndTimeNum = end;
            HasShop = hasShop;
            ExchangeEndTimeNum = hasShop ? TimeMgr.AddDays(EndTimeNum, Constant.EventShopExtraOpenDays) : EndTimeNum;
        }

        public EventPeriod GetPeriod(long now)
        {
            if (now < StartTimeNum) 
            {
                return EventPeriod.Upcoming;
            }
            else if (now < EndTimeNum)
            {
                return EventPeriod.Active;  
            } 
            else if (HasShop && now < ExchangeEndTimeNum)
            {
                return EventPeriod.ExchangeOnly;  
            } 
            else
            {
                return EventPeriod.Closed;
            }
        }

        public bool CanAcquire(long now) => GetPeriod(now) == EventPeriod.Active;
        public bool CanExchange(long now) => HasShop && now >= StartTimeNum && now < ExchangeEndTimeNum;
        public bool IsVisible(long now) => now >= StartTimeNum && now < ExchangeEndTimeNum;
    }
}
