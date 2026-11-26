using System;
using System.Collections.Generic;

using Game.Common;

namespace Game.Events
{
    // 상점 유무와 관계없이 이벤트 전용 재화를 조회할 때 사용한다.
    public interface IEventCurrency
    {
        public IReadOnlyList<string> CurrencyIds { get; }

        public int GetCurrency(string currencyId);
    }

    // 몬스터 처치 시 처리할 기능이 있는 이벤트에서 구현한다.
    public interface IEventHunt
    {
        public int TryGetHuntReward(BattleType battleType, long now);

        // 표시할 사냥 설정이 없으면 null을 반환한다.
        public EventHuntInfo GetHuntInfo(long now);
    }

    // 온라인 사냥과 별도로 오프라인 정산을 지원하는 이벤트에서만 구현한다.
    public interface IEventOfflineHunt : IEventHunt, IEventCurrency
    {
        // start와 end는 최소 시간 검사를 마친 지급 구간이며, now는 정산 완료 시각이다.
        public int ReceiveOfflineHuntReward(long start, long end, long now);

        // 보상 지급 없이 정산 완료 시각을 갱신하고 지난 날짜 기록을 정리한다.
        public void CompleteOfflineHuntCheck(long now);
    }

    // 상품마다 지정한 이벤트 재화로 일반 재화를 구매한다.
    public interface IEventShop : IEventCurrency
    {
        public IReadOnlyList<EventShopRule> ShopItems { get; }

        public bool TryGetShopItem(string productId, out EventShopRule product);

        public int GetPurchaseCount(string productId, long now);

        public ShopPurchaseResult GetPurchaseResult(string productId, long now);

        public ShopPurchaseResult TryBuy(string productId, long now,
            Func<IReadOnlyDictionary<Currency, int>, bool> giveReward);
    }
}
