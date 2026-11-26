using System;
using System.Collections.Generic;

using Game.Common;
using Game.Data;

namespace Game.Core
{
    // 퀘스트와 상점이 일반 재화를 지급할 때 사용하는 검사와 지급 처리다.
    public static class CurrencyReward
    {
        private static bool IsGiving;

        public static bool CanGive(IReadOnlyDictionary<Currency, int> rewards)
        {
            if (IsGiving || rewards == null || rewards.Count == 0) return false;
            foreach (var reward in rewards)
            {
                if (reward.Value <= 0 || !Enum.IsDefined(typeof(Currency), reward.Key)) return false;
                if (reward.Key == Currency.Gold)
                {
                    double gold = PlayerDataMgr.Instance.GetGold() + reward.Value;
                    if (double.IsNaN(gold) || double.IsInfinity(gold)) return false;
                }
                else if (reward.Key == Currency.Exp)
                {
                    float exp = UpgradeDataMgr.Instance.Exp + reward.Value;
                    if (float.IsNaN(exp) || float.IsInfinity(exp)) return false;
                }
                else if (PlayerDataMgr.Instance.GetCurrency(reward.Key) > int.MaxValue - reward.Value)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool TryGive(IReadOnlyDictionary<Currency, int> rewards)
        {
            // 한 상품의 보상을 전부 받을 수 있는지 먼저 검사한다.
            if (!CanGive(rewards)) return false;
            IsGiving = true;
            try
            {
                foreach (var reward in rewards)
                {
                    if (reward.Key == Currency.Gold) PlayerDataMgr.Instance.AddGold(reward.Value, false);
                    else if (reward.Key == Currency.Exp) UpgradeDataMgr.Instance.AddExp(reward.Value, false);
                    else if (!PlayerDataMgr.Instance.TryAddCurrency(reward.Key, reward.Value, false))
                    {
                        throw new InvalidOperationException("사전 검사를 통과한 일반 재화의 지급에 실패했습니다.");
                    }
                }
                // 여러 보상의 상태 반영이 끝난 뒤 알림을 보내 중간 상태가 노출되지 않도록 한다.
                foreach (var reward in rewards)
                {
                    if (reward.Key == Currency.Exp) UpgradeDataMgr.Instance.NotifyExpChanged();
                    else PlayerDataMgr.Instance.NotifyCurrencyChanged(reward.Key);
                }
                return true;
            }
            finally
            {
                IsGiving = false;
            }
        }
    }
}
