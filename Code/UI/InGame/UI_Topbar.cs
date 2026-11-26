using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Common;
using Game.Core;
using Game.Data;
using Game.UI.Icon;

namespace Game.UI
{
    public class UI_Topbar : MonoBehaviour
    {
        [SerializeField]
        private Icon_Currency ResIcon;
        [SerializeField]
        private Transform CurrencyParent;
        [SerializeField]
        private Image ImagePortrait;
        [SerializeField]
        private TextMeshProUGUI TextNickname;
        [SerializeField]
        private TextMeshProUGUI TextPower;

        private List<Icon_Currency> Icons = new List<Icon_Currency>();

        private void Awake()
        {
            PlayerDataMgr.ChangedCurrency += OnCurrencyChanged;
            PlayerDataMgr.ChangedPortrait += ChangePortrait;
            PowerMgr.ChangedPower += PowerChange;

            foreach (var cur in Constant.TopUICurrency)
            {
                Icon_Currency icon = Instantiate(ResIcon, CurrencyParent);
                icon.Init(cur);
                Icons.Add(icon);
            }

            TextNickname.text = PlayerDataMgr.Instance.NickName;
        }

        private void OnDestroy()
        {
            PlayerDataMgr.ChangedCurrency -= OnCurrencyChanged;
            PlayerDataMgr.ChangedPortrait -= ChangePortrait;
            PowerMgr.ChangedPower -= PowerChange;
        }

        private void ChangePortrait()
        {
            ImagePortrait.sprite = PlayerDataMgr.Instance.Portrait;
        }

        private void OnCurrencyChanged(Currency cur)
        {
            foreach (var icon in Icons)
            {
                if (icon.Type == cur)
                {
                    icon.Refresh();
                }
            }
        }

        private void PowerChange()
        {
            TextPower.text = PowerMgr.NowPowerString;
        }
    }
}
