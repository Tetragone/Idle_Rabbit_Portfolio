using System;
using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Common;
using Game.Core;
using Game.Events;
using Game.UI.Icon;

namespace Game.UI
{
    public class UI_Event : SubUI
    {
        [Serializable]
        private sealed class EventPageSetting
        {
            public string EventId = "";

            [Tooltip("처음 선택할 때 생성할 이벤트 페이지 프리팹")]
            public UI_EventPage Prefab = null;

            [NonSerialized]
            public ToggleTextSet ToggleItem;
        }

        [Header("상단")]
        [SerializeField]
        private TextMeshProUGUI TextTitle;
        [SerializeField]
        private List<Icon_Currency> EventCurrencies = new List<Icon_Currency>();

        [Header("왼쪽 이벤트 토글")]
        [SerializeField]
        private ToggleTextSet ResToggleItem;
        [SerializeField]
        private Transform ParentToggle;
        [SerializeField]
        private ToggleGroup GroupToggle;

        [Header("이벤트별 페이지")]
        [SerializeField]
        private Transform ParentPage;
        [SerializeField]
        private List<EventPageSetting> Pages = new List<EventPageSetting>();

        public string SelectedEventId => SelectedPage == null ? "" : SelectedPage.EventId;

        private readonly Dictionary<string, UI_EventPage> CreatedPages = new Dictionary<string, UI_EventPage>();
        private EventPageSetting SelectedPage;
        private Transform PageCreateRoot;
        private int EventInitializeCount = -1;
        private float NextRefreshTime;
        private bool IsRefreshingPages;

        protected override void Awake()
        {
            ValidatePages();
            base.Awake();

            TextTitle.text = "이벤트";
            foreach (Icon_Currency currency in EventCurrencies)
            {
                currency.gameObject.SetActive(false);
            }
            GroupToggle.allowSwitchOff = false;

            // 프리팹이 활성 상태여도 ID 설정 전에 OnEnable이 실행되지 않게 생성한다.
            var createRoot = new GameObject("EventPageCreateRoot", typeof(RectTransform));
            createRoot.SetActive(false);
            createRoot.transform.SetParent(ParentPage, false);
            PageCreateRoot = createRoot.transform;

            CreateToggles();
        }

        private void ValidatePages()
        {
            var ids = new HashSet<string>();
            if (ParentPage == null)
                throw new ArgumentException("이벤트 페이지를 생성할 부모를 연결해주세요.");

            foreach (EventPageSetting page in Pages)
            {
                if (page == null || string.IsNullOrEmpty(page.EventId) || page.Prefab == null)
                {
                    throw new ArgumentException("이벤트 ID와 페이지 프리팹을 연결해주세요.");
                }

                if (!ids.Add(page.EventId))
                {
                    throw new ArgumentException("이벤트 ID는 중복해서 등록할 수 없습니다.");
                }
            }
        }

        private void CreateToggles()
        {
            foreach (EventPageSetting page in Pages)
            {
                ToggleTextSet item = Instantiate(ResToggleItem, ParentToggle);
                item.gameObject.SetActive(false);

                page.ToggleItem = item;

                item.Toggle.onValueChanged.AddListener(isOn =>
                {
                    if (!isOn || IsRefreshingPages)
                        return;

                    SelectEvent(page.EventId);
                });
            }
        }

        private void OnEnable()
        {
            RefreshPages();
        }

        public override void Open(bool useSound = true)
        {
            base.Open(useSound);
            RefreshPages();
        }

        private void Update()
        {
            if (Time.unscaledTime < NextRefreshTime)
                return;

            NextRefreshTime = Time.unscaledTime + 1f;
            RefreshPages();
        }

        private void RefreshPages()
        {
            bool ready = EventMgr.Instance != null && EventMgr.Instance.IsInitialized;
            long now = TimeMgr.GetNowTimeNum();
            EventPageSetting firstVisiblePage = null;

            IsRefreshingPages = true;

            try
            {
                foreach (EventPageSetting page in Pages)
                {
                    bool visible = ready && EventMgr.Instance.IsEventVisible(page.EventId, now);
                    page.ToggleItem.gameObject.SetActive(visible);

                    if (!visible)
                        continue;

                    page.ToggleItem.Text.text = EventMgr.Instance.GetEventName(page.EventId);

                    if (firstVisiblePage == null)
                    {
                        firstVisiblePage = page;
                    }
                }
            }
            finally
            {
                IsRefreshingPages = false;
            }

            EventPageSetting nextPage = SelectedPage;

            if (nextPage == null || !ready || !EventMgr.Instance.IsEventVisible(nextPage.EventId, now))
            {
                nextPage = firstVisiblePage;
            }

            int initializeCount = ready ? EventMgr.Instance.InitializeCount : -1;

            if (SelectedPage != nextPage || EventInitializeCount != initializeCount)
            {
                EventInitializeCount = initializeCount;
                ShowPage(nextPage, true);
            }
        }

        public void SelectEvent(string eventId)
        {

            if (EventMgr.Instance == null || !EventMgr.Instance.IsEventVisible(eventId, TimeMgr.GetNowTimeNum()))
            {
                RefreshPages();
                return;
            }

            foreach (EventPageSetting page in Pages)
            {
                if (page.EventId != eventId)
                    continue;

                bool changed = SelectedPage != page;
                ShowPage(page);

                if (changed)
                {
                    SoundMgr.Instance.PlaySFXSound(SFXType.Ok);
                }

                return;
            }
        }

        private void ShowPage(EventPageSetting selected, bool reload = false)
        {
            SelectedPage = selected;
            UI_EventPage currentPage = null;

            if (selected != null)
            {
                currentPage = GetOrCreatePage(selected, reload);
                selected.ToggleItem.Toggle.SetIsOnWithoutNotify(true);
            }
            else
            {
                GroupToggle.SetAllTogglesOff(false);
            }

            foreach (UI_EventPage page in CreatedPages.Values)
            {
                page.gameObject.SetActive(page == currentPage);
            }

            ShowCurrencies(selected);
        }

        private UI_EventPage GetOrCreatePage(EventPageSetting setting, bool reload)
        {
            if (!CreatedPages.TryGetValue(setting.EventId, out UI_EventPage page))
            {
                page = Instantiate(setting.Prefab, PageCreateRoot);
                page.gameObject.SetActive(false);
                page.transform.SetParent(ParentPage, false);
                page.ShopRequested += OpenEventShop;
                page.SetEvent(setting.EventId);

                CreatedPages.Add(setting.EventId, page);
            }
            else if (reload)
            {
                page.SetEvent(setting.EventId);
            }

            return page;
        }

        private void OpenEventShop(string eventId)
        {
            if (!gameObject.activeInHierarchy || SelectedEventId != eventId)
                return;

            UI_Shop shop = UIMgr.Instance.Get<UI_Shop>(MainUIType.Shop);

            if (shop != null && shop.OpenEventShop(eventId))
            {
                Close(false);
            }
        }

        private void ShowCurrencies(EventPageSetting selected)
        {
            IReadOnlyList<string> ids = selected == null
                ? Array.Empty<string>()
                : EventMgr.Instance.GetEventCurrencyIds(selected.EventId);

            if (ids.Count > 0 && EventCurrencies.Count == 0)
            {
                throw new InvalidOperationException("상단 재화 아이콘을 하나 이상 연결해주세요.");
            }

            // 연결한 슬롯보다 재화가 많으면 첫 아이콘을 원본으로 필요한 만큼 추가한다.
            while (EventCurrencies.Count < ids.Count)
            {
                Icon_Currency source = EventCurrencies[0];
                EventCurrencies.Add(Instantiate(source, source.transform.parent));
            }

            for (int index = 0; index < EventCurrencies.Count; index++)
            {
                bool visible = index < ids.Count;
                EventCurrencies[index].gameObject.SetActive(visible);

                if (visible)
                {
                    EventCurrencies[index].InitEvent(selected.EventId, ids[index]);
                }
            }
        }
    }
}
