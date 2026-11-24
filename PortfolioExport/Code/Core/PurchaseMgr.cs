using System;
using System.Collections.Generic;

using Game.Calculate;
using Game.Common;
using Game.Data;
using Game.UI.Popup;
using Game.Events;

namespace Game.Core
{
    public class PurchaseMgr : SingletonAllTime<PurchaseMgr>
    {
        public event Action<string> SuccessAction;
        public event Action<string> FailureAction;

        private bool IsBuying;

        public ShopPurchaseResult Purchase(string eventId, string productId)
        {
            if (IsBuying)
                return ShopPurchaseResult.Busy;

            IsBuying = true;
            PopupMgr.Instance.SetActiveButtonLock(true);

            try
            {
                ShopPurchaseResult result = EventMgr.Instance.TryBuy(eventId, productId, TimeMgr.GetNowTimeNum());

                if (result == ShopPurchaseResult.Success)
                {
                    DataMgr.Instance.Save();
                }

                return result;
            }
            finally
            {
                PopupMgr.Instance.SetActiveButtonLock(false);
                IsBuying = false;
            }
        }

        public void Purchase(string sid)
        {
            if (IsBuying) return;
            IsBuying = true;
            PopupMgr.Instance.SetActiveButtonLock(true);
            try
            {
                Processing(sid);
            }
            finally
            {
                PopupMgr.Instance.SetActiveButtonLock(false);
                IsBuying = false;
            }
        }

        private void Processing(string sid)
        {
            // 확인 팝업을 연 뒤 잔액이나 제한이 바뀔 수 있으므로 실행 시점에 다시 검사한다.
            if (PlayerDataMgr.Instance.IsPurchasable(sid) && CanPay(sid) && TryGetItems(sid))
            {
                PlayerDataMgr.Instance.AddPurchase(sid);
                UseCurrency(sid);
                SuccessAction?.Invoke(sid);
            }
            else
            {
                FailureAction?.Invoke(sid);
            }
        }

        private bool CanPay(string sid)
        {
            string replace = Table.GetString(TableType.Shop, sid, "replace_type");

            if (replace != "x")
            {
                Currency alternative = (Currency)Enum.Parse(typeof(Currency), replace);
                int amount = Table.GetInt(TableType.Shop, sid, "replace_value");
                if (GetCurrencyValue(alternative) >= amount)
                    return true;
            }

            Currency currency = (Currency)Enum.Parse(typeof(Currency), Table.GetString(TableType.Shop, sid, "need_type"));
            return GetCurrencyValue(currency) >= Table.GetInt(TableType.Shop, sid, "need_value");
        }

        private static double GetCurrencyValue(Currency currency)
        {
            return currency == Currency.Gold ? PlayerDataMgr.Instance.GetGold() : PlayerDataMgr.Instance.GetCurrency(currency);
        }

        private bool TryGetItems(string sid)
        {
            Dictionary<Currency, int> getItems = new Dictionary<Currency, int>();
            Dictionary<string, int> gachas = new Dictionary<string, int>();

            for (int i = 0; ; i++)
            {
                string key = string.Format("item_{0}", i + 1);

                if (Table.IsValidColumn(TableType.Shop, key))
                {
                    string type = Table.GetString(TableType.Shop, sid, key);

                    if (type != "x")
                    {
                        int value = Table.GetInt(TableType.Shop, sid, string.Format("value_{0}", i + 1));
                        if (Enum.TryParse<Currency>(type, out Currency result))
                        {
                            getItems.Add(result, value);
                        }
                        else
                        {
                            gachas.Add(type, value);
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            if (getItems.Count > 0)
            {
                if (!CurrencyReward.TryGive(getItems)) return false;
                OpenItemPopup(getItems);
            }

            if (gachas.Count > 0)
            {
                if (gachas.ContainsKey("Gacha_Equip"))
                {
                    List<string> equips = new List<string>();
                    // addequipgachacount하는 순간 레벨이 바뀌기 때문
                    int equipLv = PlayerDataMgr.Instance.GetEquipGachaLv();
                    equips = GachaCalculator.GachaEquip(gachas["Gacha_Equip"], equipLv);
                    EquipDataMgr.Instance.AddEquip(equips);
                    PlayerDataMgr.Instance.AddEquipGachaCount(equips.Count);
                    OpenEquipGachaPopup(equips, equipLv);
                }
                else if (gachas.ContainsKey("Gacha_Skill"))
                {
                    List<string> skills = new List<string>();
                    skills = GachaCalculator.GachaSkill(gachas["Gacha_Skill"]);
                    SkillDataMgr.Instance.AddSkill(skills);
                    OpenSkillGachaPopup(skills);
                }
            }

            return true;
        }

        private void UseCurrency(string sid)
        {
            string replace = Table.GetString(TableType.Shop, sid, "replace_type");
            int replaceValue = replace == "x" ? int.MaxValue : Table.GetInt(TableType.Shop, sid, "replace_value");
            Currency replaceType = replace == "x" ? Currency.Gold : (Currency)Enum.Parse(typeof(Currency), replace);
            double nowReplaceValue = replace == "x" ? double.MinValue : GetCurrencyValue(replaceType);

            string need = Table.GetString(TableType.Shop, sid, "need_type");
            int needValue = Table.GetInt(TableType.Shop, sid, "need_value");
            Currency type = (Currency)Enum.Parse(typeof(Currency), need);
            double nowValue = GetCurrencyValue(type);

            if (nowReplaceValue >= replaceValue)
            {
                if (replaceType == Currency.Gold) PlayerDataMgr.Instance.UseGold(replaceValue);
                else PlayerDataMgr.Instance.UseCurrency(replaceType, replaceValue);
            }
            else if (nowValue >= needValue)
            {
                if (type == Currency.Gold) PlayerDataMgr.Instance.UseGold(needValue);
                else PlayerDataMgr.Instance.UseCurrency(type, needValue);
            }
        }

        private async void OpenItemPopup(Dictionary<Currency, int> items)
        {
            var popup = await PopupMgr.Instance.Get<Popup_GetItems>(PopupType.GetItem);
            popup.Init(items);
            popup.Open();
        }

        private async void OpenEquipGachaPopup(List<string> equips, int equipGachaLv)
        {
            var popup = await PopupMgr.Instance.Get<Popup_Gacha>(PopupType.Gacha);
            popup.InitEquip(equips, equipGachaLv);
            popup.Open();
        }

        private async void OpenSkillGachaPopup(List<string> skills)
        {
            var popup = await PopupMgr.Instance.Get<Popup_Gacha>(PopupType.Gacha);
            popup.InitSkill(skills);
            popup.Open();
        }
    }
}

