using System;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Common;
using Game.Core;
using Game.Data;
using Game.Events;

namespace Game.UI.Icon
{
    public class Icon_Currency : MonoBehaviour
    {
        public Currency Type => NowType;
        [SerializeField]
        private Image ImageCurrency;
        [SerializeField]
        private TextMeshProUGUI TextCurrency;

        private Currency NowType;
        private string EventId;
        private string EventCurrencyId;

        public void InitEvent(string eventId)
        {
            InitEvent(eventId, EventMgr.Instance.GetEventCurrencyId(eventId));
        }

        public void InitEvent(string eventId, string currencyId)
        {
            bool exists = false;

            foreach (string id in EventMgr.Instance.GetEventCurrencyIds(eventId))
            {
                if (id == currencyId)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                throw new ArgumentException("이벤트 재화가 없습니다.", nameof(eventId));
            }

            ClearEvent();
            EventId = eventId;
            EventCurrencyId = currencyId;
            EventMgr.Instance.Changed += OnEventChanged;

            ImageCurrency.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common,
                Table.GetString(TableType.EventCurrency, currencyId, "res"));

            Refresh();
        }

        private void ClearEvent()
        {
            if (EventId != null && EventMgr.Instance != null)
            {
                EventMgr.Instance.Changed -= OnEventChanged;
            }

            EventId = null;
            EventCurrencyId = null;
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
            ClearEvent();
        }

        public void Init(string currency)
        {
            bool exist = Enum.TryParse(currency, out NowType);

            if (!exist)
            {
                MyLogger.Log(LogType.Error, LogSituation.GetCurrency, ProcessType.Failed, "no type currency");
                return;
            }

            Init(NowType);
        }

        public void Init(Currency currency)
        {
            ClearEvent();
            NowType = currency;
            ImageCurrency.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common, Table.GetString(TableType.Currency, currency.ToString(), "res"));
            Refresh();
        }

        public void Refresh()
        {
            if (EventId != null)
            {
                TextCurrency.text = EventMgr.Instance.GetEventCurrencyAmount(EventId, EventCurrencyId).ToString();
                return;
            }

            switch (NowType)
            {
                case Currency.Gold:
                    double gold = PlayerDataMgr.Instance.GetTempGold();
                    TextCurrency.text = NumberFormatter.ToCompact(gold);
                    break;
                case Currency.Exp:
                    TextCurrency.text = UpgradeDataMgr.Instance.Exp.ToString("F0");
                    break;
                default:
                    TextCurrency.text = PlayerDataMgr.Instance.GetTempCurrency(NowType).ToString("F0");
                    break;
            }
        }

        public void SetValue(string currency, int value)
        {
            Currency type;
            bool exist = Enum.TryParse(currency, out type);

            if (!exist)
            {
                MyLogger.Log(LogType.Error, LogSituation.GetCurrency, ProcessType.Failed, string.Format("no type {0}", currency));
                return;
            }

            SetValue(type, value);
        }

        public void SetValue(Currency currency, int value)
        {
            ClearEvent();
            NowType = currency;
            ImageCurrency.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common, Table.GetString(TableType.Currency, currency.ToString(), "res"));
            TextCurrency.text = value.ToString();
        }
    }
}
