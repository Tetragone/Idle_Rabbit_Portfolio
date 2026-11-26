using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Common;
using Game.Core;
using Game.Data;
using Game.UI.Icon;
using Game.Events;

namespace Game.UI
{
    public class UI_Shop : SubUI
    {
        [SerializeField]
        private Icon_Shop ResSmall;
        [SerializeField]
        private Icon_Shop ResMiddle;
        [SerializeField]
        private Icon_Shop ResBig;
        [SerializeField]
        private List<Icon_Currency> Currencies;
        [SerializeField]
        private ToggleTextSet ResToggleItem;
        [SerializeField]
        private Transform ParentToggle;
        [SerializeField]
        private Transform ParentSmallIcons;
        [SerializeField]
        private Transform ParentBigIcons;

        [Header("텍스트")]
        [SerializeField]
        private TextMeshProUGUI TextTitle;

        private List<Toggle> Toggles = new List<Toggle>();
        private List<TextMeshProUGUI> ToggleTitles = new List<TextMeshProUGUI>();
        private List<string> ToggleCategory = new List<string>();
        private List<string> EventIds = new List<string>();
        private Dictionary<string, int> EventToggleIndexes = new Dictionary<string, int>();

        private List<Icon_Shop> SmallIcons = new List<Icon_Shop>();
        private List<Icon_Shop> MiddileIcons = new List<Icon_Shop>();
        private List<Icon_Shop> BigIcons = new List<Icon_Shop>();

        private int NowCategory = 0;
        private float NextRefreshTime;
        private int EventInitializeCount = -1;

        protected override void Awake()
        {
            base.Awake();
            InitToggle();
            RefreshEventToggles();

            if (Toggles[NowCategory].isOn)
            {
                SetIcons(NowCategory);
            }
            else
            {
                Toggles[NowCategory].isOn = true;
            }
        }

        private void InitToggle()
        {
            var categoryTable = Table.GetTable(TableType.ShopCategory);

            foreach (var key in categoryTable.Keys)
            {
                AddToggle(key, Table.GetString(TableType.ShopCategory, key, "name"), null);
            }
        }

        private void AddToggle(string category, string title, string eventId)
        {
            var toggle = Instantiate(ResToggleItem, ParentToggle);
            toggle.Toggle.isOn = false;
            toggle.gameObject.SetActive(true);

            int index = Toggles.Count;
            Toggles.Add(toggle.Toggle);
            ToggleTitles.Add(toggle.Text);
            ToggleCategory.Add(category);
            EventIds.Add(eventId);

            toggle.Text.text = title;
            toggle.Toggle.onValueChanged.AddListener(isOn =>
            {
                if (!isOn)
                    return;

                SoundMgr.Instance.PlaySFXSound(SFXType.Ok);
                NowCategory = index;
                SetIcons(index);
            });
        }

        public override void Open(bool useSound = true)
        {
            base.Open(useSound);
            RefreshEventToggles();
            SetIcons(NowCategory);
        }

        public bool OpenEventShop(string eventId)
        {
            if (!EventMgr.Instance.IsShopOpen(eventId, TimeMgr.GetNowTimeNum()))
                return false;

            Open();

            if (!EventToggleIndexes.TryGetValue(eventId, out int index))
                return false;

            NowCategory = index;
            Toggles[index].SetIsOnWithoutNotify(true);
            SetIcons(index);

            return true;
        }

        private void Update()
        {
            if (Time.unscaledTime < NextRefreshTime)
                return;

            NextRefreshTime = Time.unscaledTime + 1f;
            RefreshEventToggles();

            foreach (Icon_Shop icon in SmallIcons)
            {
                if (icon.gameObject.activeInHierarchy)
                {
                    icon.Refresh();
                }
            }

            foreach (Icon_Currency currency in Currencies)
            {
                if (currency.gameObject.activeInHierarchy)
                {
                    currency.Refresh();
                }
            }
        }

        private void RefreshEventToggles()
        {

            if (EventMgr.Instance == null || !EventMgr.Instance.IsInitialized)
                return;

            long now = TimeMgr.GetNowTimeNum();
            bool initialized = EventInitializeCount != EventMgr.Instance.InitializeCount;
            EventInitializeCount = EventMgr.Instance.InitializeCount;

            foreach (string eventId in EventMgr.Instance.GetShopEventIds())
            {
                if (EventToggleIndexes.ContainsKey(eventId))
                    continue;

                EventToggleIndexes.Add(eventId, Toggles.Count);
                AddToggle(eventId, EventMgr.Instance.GetShopName(eventId), eventId);
            }

            foreach (var item in EventToggleIndexes)
            {
                string eventId = item.Key;
                int index = item.Value;
                bool visible = EventMgr.Instance.IsShopOpen(eventId, now);

                Toggles[index].gameObject.SetActive(visible);
                ToggleTitles[index].text = EventMgr.Instance.GetShopName(eventId);

                if (index != NowCategory)
                    continue;

                if (!visible)
                {
                    Toggles[index].isOn = false;

                    for (int fallback = 0; fallback < Toggles.Count; fallback++)
                    {
                        if (!Toggles[fallback].gameObject.activeSelf)
                            continue;

                        NowCategory = fallback;
                        Toggles[fallback].isOn = true;
                        break;
                    }
                }
                else if (initialized)
                {
                    // 이벤트가 다시 초기화되면 현재 상품과 재화 표시도 다시 연결한다.
                    SetIcons(index);
                }
            }
        }

        private void SetIcons(int idx)
        {
            foreach (var icon in SmallIcons)
            {
                icon.SetActive(false);
            }

            foreach (var icon in MiddileIcons)
            {
                icon.SetActive(false);
            }

            foreach (var icon in BigIcons)
            {
                icon.SetActive(false);
            }

            string eventId = EventIds[idx];

            if (eventId != null)
            {
                SetEventIcons(eventId);
                return;
            }

            string type = Table.GetString(TableType.ShopCategory, ToggleCategory[idx], "size");

            switch (type)
            {
                case "Big":
                    ParentSmallIcons.gameObject.SetActive(false);
                    SetIconsInList(BigIcons, ResBig, ParentBigIcons);
                    break;
                case "Middle":
                    ParentSmallIcons.gameObject.SetActive(false);
                    SetIconsInList(MiddileIcons, ResMiddle, ParentBigIcons);
                    break;
                case "Small":
                default:
                    ParentBigIcons.gameObject.SetActive(false);
                    SetIconsInList(SmallIcons, ResSmall, ParentSmallIcons);
                    break;
            }

            TextTitle.text = Table.GetString(TableType.ShopCategory, ToggleCategory[idx], "name");

            for (int i = 0; i < Currencies.Count; i++)
            {
                string currency = Table.GetString(TableType.ShopCategory, ToggleCategory[idx], string.Format("currency_{0}", i + 1));
                if (currency == "x")
                {
                    Currencies[i].gameObject.SetActive(false);
                }
                else
                {
                    Currencies[i].Init(currency);
                    Currencies[i].gameObject.SetActive(true);
                }
            }
        }

        private void SetIconsInList(List<Icon_Shop> icons, Icon_Shop res, Transform parent)
        {
            HashSet<string> ids = new HashSet<string>();
            int idx = -1;
            parent.gameObject.SetActive(true);

            foreach (var key in Table.GetTable(TableType.Shop).Keys)
            {
                if (Table.GetString(TableType.Shop, key, "category") != ToggleCategory[NowCategory])
                {
                    continue;
                }

                string realKey = key.Split('_')[0];
                if (!ids.Contains(realKey))
                {
                    ids.Add(realKey);
                    idx++;
                }

                if (icons.Count <= idx)
                {
                    var icon = Instantiate(res, parent);
                    icon.AddSid(key);
                    icons.Add(icon);
                }
                else
                {
                    icons[idx].AddSid(key);
                }
            }

            // 이전 분류의 남는 아이콘은 활성화하지 않는다.
            for (int i = 0; i <= idx; i++)
            {
                icons[i].SetActive(true);
            }
        }

        private void SetEventIcons(string eventId)
        {
            IReadOnlyList<EventShopRule> products = EventMgr.Instance.GetShopItems(eventId);

            ParentBigIcons.gameObject.SetActive(false);
            ParentSmallIcons.gameObject.SetActive(true);
            TextTitle.text = EventMgr.Instance.GetShopName(eventId);

            for (int i = 0; i < products.Count; i++)
            {
                if (i >= SmallIcons.Count)
                {
                    SmallIcons.Add(Instantiate(ResSmall, ParentSmallIcons));
                }

                SmallIcons[i].SetEventProduct(eventId, products[i]);
                SmallIcons[i].SetActive(true);
            }

            IReadOnlyList<string> currencyIds = EventMgr.Instance.GetEventCurrencyIds(eventId);

            if (currencyIds.Count > 0 && Currencies.Count == 0)
                throw new System.InvalidOperationException("상점 상단 재화 아이콘을 연결해주세요.");

            while (Currencies.Count < currencyIds.Count)
            {
                Icon_Currency source = Currencies[0];
                Currencies.Add(Instantiate(source, source.transform.parent));
            }

            for (int i = 0; i < Currencies.Count; i++)
            {
                bool visible = i < currencyIds.Count;
                Currencies[i].gameObject.SetActive(visible);

                if (visible)
                {
                    Currencies[i].InitEvent(eventId, currencyIds[i]);
                }
            }
        }
    }
}
