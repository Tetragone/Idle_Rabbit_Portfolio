using System;
using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Common;
using Game.Core;
using Game.Data;
using Game.UI.Popup;
using Game.Events;

namespace Game.UI.Icon
{
    public class Icon_Shop : MonoBehaviour
    {
        public TextMeshProUGUI TextContent;
        public TextMeshProUGUI TextCount;
        public Image ImageItem;

        public Icon_GachaDetail GachaDetail;

        [Space(10)]
        public List<TextMeshProUGUI> TextTitles;
        public List<Button> Buttons;
        public List<GameObject> ObjSoldOuts;
        public List<GameObject> ObjLacks;
        public List<Image> ImagePrices;
        public List<TextMeshProUGUI> TextPrices;
        public List<TextMeshProUGUI> TextLimits;
        public List<TextMeshProUGUI> TextRefill;

        private List<string> Sids = new List<string>();
        private string EventId;
        private EventShopRule EventProduct;
        private bool IsLoadingConfirm;

        public void SetEventProduct(string eventId, EventShopRule product)
        {
            if (product == null || !EventMgr.Instance.TryGetShopItem(eventId, product.Id, out EventShopRule current)
                || !ReferenceEquals(product, current))
            {
                throw new ArgumentException("이벤트 상점이 없습니다.", nameof(eventId));
            }

            EventId = eventId;
            EventProduct = product;
        }

        public void AddSid(string sid)
        {
            if (!Sids.Contains(sid))
            {
                Sids.Add(sid);
            }
        }

        public void SetActive(bool isActive)
        {
            if (isActive)
            {
                Init();
                gameObject.SetActive(true);
            }
            else
            {
                ResetValue();
                gameObject.SetActive(false);
            }
        }

        private void Init()
        {
            if (EventId != null)
            {
                InitEventProduct();
                return;
            }

            string sid = Sids[0];
            PurchaseMgr.Instance.SuccessAction += PurchaseSucess;
            ImageItem.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common, Table.GetString(TableType.Shop, sid, "res"));

            string desc = Table.GetString(TableType.Shop, sid, "desc");
            string count = Table.GetString(TableType.Shop, sid, "value_1");

            if (TextCount != null)
            {
                TextCount.text = desc == "x" ? string.Format("x{0}", count) : "";
            }

            if (TextContent != null)
            {
                TextContent.text = desc == "x" ? "" : desc;
            }

            int idx = 0;

            for (; idx < Sids.Count; idx++)
            {
                string nowSid = Sids[idx];
                string need = Table.GetString(TableType.Shop, nowSid, "need_type");
                int needValue = Table.GetInt(TableType.Shop, nowSid, "need_value");

                Currency type = (Currency)Enum.Parse(typeof(Currency), need);
                int nowValue = PlayerDataMgr.Instance.GetCurrency(type);

                string replace = Table.GetString(TableType.Shop, nowSid, "replace_type");
                int replaceValue = replace == "x" ? int.MaxValue : Table.GetInt(TableType.Shop, nowSid, "replace_value");
                Currency replaceType = replace == "x" ? Currency.Gold : (Currency)Enum.Parse(typeof(Currency), replace);
                int nowReplaceValue = replace == "x" ? int.MinValue : PlayerDataMgr.Instance.GetCurrency(replaceType);

                bool isOverValue = nowReplaceValue >= replaceValue || nowValue >= needValue;

                if (idx < TextTitles.Count)
                {
                    TextTitles[idx].text = Table.GetString(TableType.Shop, nowSid, "name");
                }

                if (idx < Buttons.Count)
                {
                    Buttons[idx].gameObject.SetActive(true);
                    Buttons[idx].onClick.RemoveAllListeners();
                    Buttons[idx].onClick.AddListener(async () =>
                    {
                        if (PlayerDataMgr.Instance.IsPurchasable(nowSid))
                        {
                            if (isOverValue)
                            {
                                var popup = await PopupMgr.Instance.Get<Popup_ShopConfirm>(PopupType.ShopConfirm);
                                popup.Init(nowSid);
                                popup.Open();
                            }
                        }
                    });
                }

                if (idx < TextPrices.Count)
                {
                    TextPrices[idx].text = needValue.ToString();
                }
            }

            Refresh();
        }

        private void PurchaseSucess(string sid)
        {
            Refresh();
        }

        private void ResetValue()
        {
            Sids.Clear();

            if (PurchaseMgr.Instance != null)
            {
                PurchaseMgr.Instance.SuccessAction -= PurchaseSucess;
            }

            if (EventId != null && EventMgr.Instance != null)
            {
                EventMgr.Instance.Changed -= OnEventChanged;
            }

            PlayerDataMgr.ChangedCurrency -= OnCurrencyChanged;
            EventId = null;
            EventProduct = null;
        }

        private void OnEventChanged(string eventId)
        {
            if (eventId == EventId)
            {
                Refresh();
            }
        }

        private void OnDestroy()
        {
            ResetValue();
        }

        private void OnCurrencyChanged(Currency currency)
        {
            if (EventProduct != null && EventProduct.Rewards.ContainsKey(currency))
            {
                Refresh();
            }
        }

        private void InitEventProduct()
        {
            EventMgr.Instance.Changed += OnEventChanged;
            PlayerDataMgr.ChangedCurrency += OnCurrencyChanged;
            ImageItem.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common, EventProduct.Resource);

            if (TextContent != null)
                TextContent.text = EventProduct.Description == "x" ? "" : EventProduct.Description;

            if (TextCount != null)
            {
                TextCount.text = "";
                if (EventProduct.Description == "x")
                {
                    foreach (var reward in EventProduct.Rewards)
                    {
                        TextCount.text = string.Format("x{0}", reward.Value);
                        break;
                    }
                }
            }

            if (GachaDetail != null)
                GachaDetail.gameObject.SetActive(false);

            for (int i = 0; i < Buttons.Count; i++)
            {
                Buttons[i].gameObject.SetActive(i == 0);
                Buttons[i].onClick.RemoveAllListeners();
            }

            if (TextTitles.Count > 0)
                TextTitles[0].text = EventProduct.Name;
            if (Buttons.Count > 0)
                Buttons[0].onClick.AddListener(OpenEventConfirm);

            RefreshEventProduct();
        }

        private async void OpenEventConfirm()
        {
            if (IsLoadingConfirm)
                return;

            string eventId = EventId;
            EventShopRule product = EventProduct;
            int initializeCount = EventMgr.Instance.InitializeCount;

            IsLoadingConfirm = true;
            PopupMgr.Instance.SetActiveButtonLock(true);

            try
            {
                var popup = await PopupMgr.Instance.Get<Popup_ShopConfirm>(PopupType.ShopConfirm);

                // 팝업 로드 중 표시할 상품이 바뀌거나 화면이 닫혔다면 대기 등록을 해제한다.
                if (eventId != EventId || !ReferenceEquals(product, EventProduct)
                    || initializeCount != EventMgr.Instance.InitializeCount || !gameObject.activeInHierarchy)
                {
                    PopupMgr.Instance.Close(PopupType.ShopConfirm);
                    return;
                }

                popup.Init(eventId, product);
                popup.Open();
            }
            finally
            {
                PopupMgr.Instance.SetActiveButtonLock(false);
                IsLoadingConfirm = false;
            }
        }

        private void RefreshEventProduct()
        {
            long now = TimeMgr.GetNowTimeNum();
            ShopPurchaseResult result = EventMgr.Instance.GetShopPurchaseResult(EventId, EventProduct.Id, now);
            bool canBuy = result == ShopPurchaseResult.Success;

            if (Buttons.Count > 0)
            {
                Buttons[0].interactable = canBuy;
            }

            if (TextPrices.Count > 0)
            {
                TextPrices[0].text = EventProduct.Price.ToString();
                TextPrices[0].color = canBuy ? Color.white : Color.red;
            }

            string currencyId = EventProduct.CurrencyId;

            if (ImagePrices.Count > 0 && !string.IsNullOrEmpty(currencyId))
            {
                ImagePrices[0].sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common,
                    Table.GetString(TableType.EventCurrency, currencyId, "res"));
            }

            if (ObjLacks.Count > 0)
            {
                ObjLacks[0].SetActive(result == ShopPurchaseResult.NotEnoughCurrency);
            }

            if (ObjSoldOuts.Count > 0)
            {
                ObjSoldOuts[0].SetActive(result == ShopPurchaseResult.LimitReached);
            }

            if (TextLimits.Count > 0)
            {
                TextLimits[0].gameObject.SetActive(EventProduct.LimitCount >= 0);
                TextLimits[0].text = string.Format("구매 가능 횟수 {0}회 남음",
                    Math.Max(0, EventProduct.LimitCount - EventMgr.Instance.GetShopPurchaseCount(EventId, EventProduct.Id, now)));
            }

            if (TextRefill.Count > 0)
            {
                TextRefill[0].gameObject.SetActive(EventProduct.LimitCount >= 0);

                switch (EventProduct.LimitType)
                {
                    case ShopLimitType.Daily: TextRefill[0].text = "매일 초기화"; break;
                    case ShopLimitType.Week: TextRefill[0].text = "매주 월요일 초기화"; break;
                    case ShopLimitType.Month: TextRefill[0].text = "매월 초기화"; break;
                    default: TextRefill[0].text = "다음 이벤트 회차에 초기화"; break;
                }

                if (result == ShopPurchaseResult.RewardLimitReached)
                {
                    TextRefill[0].gameObject.SetActive(true);
                    TextRefill[0].text = "보상 보유 한도 초과";
                }
            }
        }

        public void Refresh()
        {
            if (EventId != null)
            {
                RefreshEventProduct();
                return;
            }

            bool isHaveGacha = false;
            GachaType gachaType = GachaType.Equip;
            int idx = 0;

            for (; idx < Sids.Count; idx++)
            {
                string nowSid = Sids[idx];
                string need = Table.GetString(TableType.Shop, nowSid, "need_type");
                int needValue = Table.GetInt(TableType.Shop, nowSid, "need_value");

                Currency type = (Currency)Enum.Parse(typeof(Currency), need);
                int nowValue = PlayerDataMgr.Instance.GetCurrency(type);

                string replace = Table.GetString(TableType.Shop, nowSid, "replace_type");
                int replaceValue = replace == "x" ? int.MaxValue : Table.GetInt(TableType.Shop, nowSid, "replace_value");
                Currency replaceType = replace == "x" ? Currency.Gold : (Currency)Enum.Parse(typeof(Currency), replace);
                int nowReplaceValue = replace == "x" ? int.MinValue : PlayerDataMgr.Instance.GetCurrency(replaceType);

                bool isOverValue = nowReplaceValue >= replaceValue || nowValue >= needValue;

                if (idx < ImagePrices.Count)
                {
                    string needRes = replaceValue <= nowReplaceValue
                        ? Table.GetString(TableType.Currency, replace, "res")
                        : Table.GetString(TableType.Currency, need, "res");
                    ImagePrices[idx].sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common, needRes);
                }

                if (idx < Buttons.Count)
                {
                    Buttons[idx].interactable = PlayerDataMgr.Instance.IsPurchasable(nowSid) && isOverValue;
                }

                if (idx < TextPrices.Count)
                {
                    TextPrices[idx].color = isOverValue
                        ? Color.white
                        : Color.red;
                }

                if (idx < ObjLacks.Count)
                {
                    ObjLacks[idx].SetActive(!isOverValue);
                }

                if (idx < ObjSoldOuts.Count)
                {
                    ObjSoldOuts[idx].SetActive(!PlayerDataMgr.Instance.IsPurchasable(nowSid));
                }

                if (idx < TextLimits.Count)
                {
                    string limit = Table.GetString(TableType.Shop, nowSid, "limit_type");
                    ShopLimitType limitType = (ShopLimitType)Enum.Parse(typeof(ShopLimitType), limit, true);

                    if (limitType != ShopLimitType.None)
                    {
                        TextLimits[idx].gameObject.SetActive(true);
                        TextRefill[idx].gameObject.SetActive(true);
                        int purchaseCount = PlayerDataMgr.Instance.GetPurcahsedCount(nowSid);
                        int limitCount = Table.GetInt(TableType.Shop, nowSid, "limit_value");
                        TextLimits[idx].text = string.Format("구매 가능 횟수 {0}회 남음", limitCount - purchaseCount);

                        switch (limitType)
                        {
                            case ShopLimitType.Daily:
                                TextRefill[idx].text = "매일 초기화";
                                break;
                            case ShopLimitType.Week:
                                TextRefill[idx].text = "주간 초기화";
                                break;
                            case ShopLimitType.Month:
                                TextRefill[idx].text = "매월 초기화";
                                break;
                        }
                    }
                    else
                    {
                        TextLimits[idx].gameObject.SetActive(false);
                        TextRefill[idx].gameObject.SetActive(false);
                    }
                }

                for (int i = 1; ; i++)
                {
                    string col = string.Format("item_{0}", i);

                    if (!Table.IsValidColumn(TableType.Shop, col))
                    {
                        break;
                    }

                    string itemType = Table.GetString(TableType.Shop, nowSid, col);

                    if (itemType == "Gacha_Equip")
                    {
                        isHaveGacha = true;
                        gachaType = GachaType.Equip;
                    }

                    if (itemType == "Gacha_Skill")
                    {
                        isHaveGacha = true;
                        gachaType = GachaType.Skill;
                    }
                }
            }

            if (isHaveGacha)
            {
                GachaDetail.gameObject.SetActive(true);
                GachaDetail.Init(gachaType);
            }
            else
            {
                if (GachaDetail != null)
                {
                    GachaDetail.gameObject.SetActive(false);
                }
            }

            for (; idx < Buttons.Count; idx++)
            {
                Buttons[idx].gameObject.SetActive(false);
            }
        }
    }
}
