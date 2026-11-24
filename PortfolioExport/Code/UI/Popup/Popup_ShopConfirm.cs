using System;
using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Common;
using Game.Core;
using Game.Data;
using Game.UI.Icon;
using Game.Events;

namespace Game.UI.Popup
{
    public class Popup_ShopConfirm : BasePopup
    {
        [SerializeField]
        private Image ImageNeed;
        [SerializeField]
        private TextMeshProUGUI TextNeed;
        [SerializeField]
        private Icon_Currency ResItem;
        [SerializeField]
        private Transform ParentItem;

        private List<Icon_Currency> Currencies = new List<Icon_Currency>();
        private string NowSid = "";
        private string EventId;
        private EventShopRule EventProduct;
        private int EventInitializeCount;

        protected override void Awake()
        {
            base.Awake();
            InitCallback();
            InitText();
        }

        private void InitCallback()
        {
            ButtonYes.onClick.AddListener(async () =>
            {
                if (IsEnd)
                    return;

                if (EventId != null)
                {
                    EventShopRule product = EventProduct;
                    bool current = EventInitializeCount == EventMgr.Instance.InitializeCount
                        && EventMgr.Instance.TryGetShopItem(EventId, product.Id, out EventShopRule currentProduct)
                        && ReferenceEquals(product, currentProduct);

                    ShopPurchaseResult result = current
                        ? PurchaseMgr.Instance.Purchase(EventId, product.Id)
                        : ShopPurchaseResult.EventClosed;

                    Close();

                    if (result == ShopPurchaseResult.Success)
                    {
                        var popup = await PopupMgr.Instance.Get<Popup_GetItems>(PopupType.GetItem);
                        popup.Init(new Dictionary<Currency, int>(product.Rewards));
                        popup.Open();
                    }
                    else
                    {
                        var popup = await PopupMgr.Instance.Get<Popup_Message>(PopupType.Message);
                        popup.Init(GetFailureMessage(result));
                        popup.Open();
                    }

                    return;
                }

                PurchaseMgr.Instance.Purchase(NowSid);
            });

            ButtonNo.onClick.AddListener(() =>
            {
                Close();
            });
        }

        private void InitText()
        {
            TextTitle.text = "구매 확인";
            TextYes.text = "구매";
            TextNo.text = "취소";
        }

        public override void OpenAction()
        {
            PurchaseMgr.Instance.SuccessAction += PurchaseAction;
            PurchaseMgr.Instance.FailureAction += PurchaseFailure;
        }

        public override void CloseAction()
        {
            PurchaseMgr.Instance.SuccessAction -= PurchaseAction;
            PurchaseMgr.Instance.FailureAction -= PurchaseFailure;
            EventId = null;
            EventProduct = null;
        }

        private void PurchaseAction(string sid)
        {
            if (EventId == null && sid == NowSid)
            {
                Close();
            }
        }

        private async void PurchaseFailure(string sid)
        {
            if (EventId != null || sid != NowSid)
                return;

            Close();

            var popup = await PopupMgr.Instance.Get<Popup_Message>(PopupType.Message);
            popup.Init("재화, 구매 횟수 또는 보상 보유 한도를 확인해주세요.");
            popup.Open();
        }

        public void Init(string sid)
        {
            EventId = null;
            EventProduct = null;
            int idx = 0;
            NowSid = sid;

            for (; ; idx++)
            {
                string key = string.Format("item_{0}", idx + 1);

                if (!Table.IsValidColumn(TableType.Shop, key))
                {
                    break;
                }

                string type = Table.GetString(TableType.Shop, sid, key);

                if (type == "x")
                {
                    break;
                }

                if (type == "Gacha_Equip" || type == "Gacha_Skill")
                {
                    continue;
                }

                int value = Table.GetInt(TableType.Shop, sid, string.Format("value_{0}", idx + 1));

                if (idx < Currencies.Count)
                {
                    Currencies[idx].SetValue(type, value);
                    Currencies[idx].gameObject.SetActive(true);
                }
                else
                {
                    Icon_Currency icon = Instantiate(ResItem, ParentItem);
                    icon.SetValue(type, value);
                    icon.gameObject.SetActive(true);
                    Currencies.Add(icon);
                }
            }

            for (; idx < Currencies.Count; idx++)
            {
                Currencies[idx].gameObject.SetActive(false);
            }

            string need = Table.GetString(TableType.Shop, sid, "need_type");
            int needValue = Table.GetInt(TableType.Shop, sid, "need_value");

            string replace = Table.GetString(TableType.Shop, sid, "replace_type");
            int replaceValue = replace == "x" ? int.MaxValue : Table.GetInt(TableType.Shop, sid, "replace_value");
            int nowReplaceValue = replace == "x" ? int.MinValue : PlayerDataMgr.Instance.GetCurrency((Currency)Enum.Parse(typeof(Currency), replace));

            ImageNeed.sprite = replace != "x" && nowReplaceValue >= replaceValue
                ? AtlasMgr.Instance.GetSprite(AtlasType.Common, Table.GetString(TableType.Currency, replace, "res"))
                : AtlasMgr.Instance.GetSprite(AtlasType.Common, Table.GetString(TableType.Currency, need, "res"));
            TextNeed.text = replace != "x" && nowReplaceValue >= replaceValue
                ? string.Format("x{0}", replaceValue)
                : string.Format("x{0}", needValue);
        }

        public void Init(string eventId, EventShopRule product)
        {
            EventId = eventId;
            EventProduct = product;
            EventInitializeCount = EventMgr.Instance.InitializeCount;
            NowSid = product.Id;

            int index = 0;

            foreach (var reward in product.Rewards)
            {
                if (index >= Currencies.Count)
                {
                    Currencies.Add(Instantiate(ResItem, ParentItem));
                }

                Currencies[index].SetValue(reward.Key, reward.Value);
                Currencies[index].gameObject.SetActive(true);
                index++;
            }
            for (; index < Currencies.Count; index++)
            {
                Currencies[index].gameObject.SetActive(false);
            }

            string currencyId = product.CurrencyId;

            ImageNeed.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common,
                Table.GetString(TableType.EventCurrency, currencyId, "res"));

            TextNeed.text = string.Format("x{0}", product.Price);
        }

        private static string GetFailureMessage(ShopPurchaseResult result)
        {
            switch (result)
            {
                case ShopPurchaseResult.EventClosed: return "이벤트 교환 기간이 종료되었습니다.";
                case ShopPurchaseResult.NotEnoughCurrency: return "이벤트 재화가 부족합니다.";
                case ShopPurchaseResult.LimitReached: return "구매 가능 횟수를 모두 사용했습니다.";
                case ShopPurchaseResult.RewardLimitReached: return "보상 재화의 보유 한도를 초과합니다.";
                case ShopPurchaseResult.Busy: return "구매를 처리 중입니다.";
                default: return "구매할 수 없는 상품입니다.";
            }
        }
    }
}
