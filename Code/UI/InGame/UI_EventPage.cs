using System;
using System.Globalization;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Common;
using Game.Core;
using Game.Events;

namespace Game.UI
{
    public abstract class UI_EventPage : MonoBehaviour
    {
        [Header("공통 정보: 표시할 항목만 연결")]
        [SerializeField]
        private TextMeshProUGUI TextTitle;
        [SerializeField]
        private TextMeshProUGUI TextPeriod;
        [SerializeField]
        private TextMeshProUGUI TextDesc;
        [SerializeField]
        private Image ImageBackground;

        public string EventId { get; private set; } = "";
        public event Action<string> ShopRequested;

        private Sprite DefaultBackground;
        private bool IsBackgroundReady;
        private bool IsRefreshing;
        private float NextRefreshTime;

        protected virtual void Awake()
        {
            PrepareBackground();
        }

        public void SetEvent(string eventId)
        {
            if (EventMgr.Instance == null || !EventMgr.Instance.TryGetEventInfo(eventId, out EventInfo info))
                throw new ArgumentException("이벤트 페이지에 설정할 이벤트가 없습니다.", nameof(eventId));

            EventId = eventId;
            PrepareBackground();
            SetCommonInfo(info, TimeMgr.GetNowTimeNum());
            SetEventPage();
            Refresh();
        }

        protected virtual void SetEventPage()
        {
        }

        protected virtual void RefreshEventPage()
        {
        }

        protected virtual void OnEnable()
        {
            if (EventMgr.Instance != null)
            {
                EventMgr.Instance.Changed += OnEventChanged;
            }

            Refresh();
        }

        protected virtual void OnDisable()
        {
            if (EventMgr.Instance != null)
            {
                EventMgr.Instance.Changed -= OnEventChanged;
            }
        }

        private void OnEventChanged(string eventId)
        {
            if (eventId == EventId)
            {
                Refresh();
            }
        }

        protected virtual void Update()
        {
            if (Time.unscaledTime < NextRefreshTime)
                return;

            NextRefreshTime = Time.unscaledTime + 1f;
            Refresh();
        }

        public void Refresh()
        {
            if (IsRefreshing || string.IsNullOrEmpty(EventId) || EventMgr.Instance == null)
                return;

            IsRefreshing = true;

            try
            {
                if (EventMgr.Instance.TryGetEventInfo(EventId, out EventInfo info))
                {
                    SetCommonInfo(info, TimeMgr.GetNowTimeNum());
                }

                RefreshEventPage();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void PrepareBackground()
        {
            if (IsBackgroundReady)
                return;

            DefaultBackground = ImageBackground == null ? null : ImageBackground.sprite;
            IsBackgroundReady = true;
        }

        private void SetCommonInfo(EventInfo info, long now)
        {
            if (TextTitle != null)
            {
                TextTitle.text = info.Name;
            }

            if (TextDesc != null)
            {
                TextDesc.text = info.Description == "x" ? "" : info.Description;
            }

            if (TextPeriod != null)
            {
                string start = FormatDate(info.Schedule.StartTimeNum);
                string end = FormatDate(info.Schedule.EndTimeNum);
                TextPeriod.text = string.Format("이벤트 기간: {0} ~ {1}", start, end);

                if (info.Schedule.HasShop)
                {
                    string exchangeEnd = FormatDate(info.Schedule.ExchangeEndTimeNum);
                    TextPeriod.text += string.Format("\n교환 종료: {0}", exchangeEnd);
                }

                if (info.Schedule.GetPeriod(now) == EventPeriod.ExchangeOnly)
                {
                    TextPeriod.text += "\n재화 획득 종료 · 상점 교환 가능";
                }
            }

            if (ImageBackground != null)
            {
                Sprite background = DefaultBackground;

                if (!string.IsNullOrEmpty(info.Background) && info.Background != "x")
                {
                    background = AtlasMgr.Instance.GetSprite(AtlasType.Common, info.Background);

                    if (background == null)
                        throw new InvalidOperationException("이벤트 배경 이미지를 찾을 수 없습니다: " + info.Background);
                }

                ImageBackground.sprite = background;
            }
        }

        private static string FormatDate(long time)
        {
            return TimeMgr.GetNumToDate(time).ToString("yyyy.MM.dd HH:mm", CultureInfo.InvariantCulture);
        }

        protected void RequestShop()
        {
            if (EventMgr.Instance.IsShopOpen(EventId, TimeMgr.GetNowTimeNum()))
            {
                ShopRequested?.Invoke(EventId);
            }
        }
    }
}
