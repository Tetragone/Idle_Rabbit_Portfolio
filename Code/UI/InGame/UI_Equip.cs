using System.Collections.Generic;

using TMPro;
using UnityEngine;

using Game.Common;
using Game.Data;
using Game.UI.Icon;

namespace Game.UI
{
    public class UI_Equip : SubUI
    {
        [SerializeField]
        private Icon_Equip ResIcon;
        [SerializeField]
        private Transform IconParent;
        [SerializeField]
        private Icon_Currency ResCurrency;
        [SerializeField]
        private Transform CurrencyParent;
        [SerializeField]
        private TextMeshProUGUI TextTitle;

        private List<Icon_Currency> IconCurrencies = new List<Icon_Currency>();
        private List<Icon_Equip> ListIcon = new List<Icon_Equip>();
        private string FocusId = "";

        public void IconClicked(string eid)
        {
            if (FocusId != "")
            {
                foreach (var icon in ListIcon)
                {
                    if (icon.Id == FocusId)
                    {
                        icon.OnFocus(false);
                    }
                }
            }

            FocusId = eid;
        }

        protected override void Awake()
        {
            base.Awake();
            var table = Table.GetTable(TableType.Equip);

            foreach (string key in table.Keys)
            {
                Icon_Equip icon = Instantiate(ResIcon, IconParent);
                icon.Init(key);
                icon.ButtonInit(this);
                ListIcon.Add(icon);
            }

            for (int i = 0; i < Constant.EquipUICurrency.Count; i++)
            {
                Icon_Currency icon = Instantiate(ResCurrency, CurrencyParent);
                icon.Init(Constant.EquipUICurrency[i]);
                IconCurrencies.Add(icon);
            }

            TextTitle.text = "장비";
        }

        private void OnEnable()
        {
            EquipDataMgr.ChangedEquip += OnEquipChanged;
            EquipDataMgr.ChangedEquipValue += OnEquipChanged;
            PlayerDataMgr.ChangedCurrency += OnCurrencyChanged;
            IconClicked("");
            OnEquipChanged();
        }

        private void OnDisable()
        {
            EquipDataMgr.ChangedEquip -= OnEquipChanged;
            EquipDataMgr.ChangedEquipValue -= OnEquipChanged;
            PlayerDataMgr.ChangedCurrency -= OnCurrencyChanged;
            IconClicked("");
        }

        private void OnEquipChanged()
        {
            foreach (var icon in ListIcon)
            {
                icon.Refresh();
            }
        }

        private void OnCurrencyChanged(Currency cur)
        {
            foreach (var icon in IconCurrencies)
            {
                if (icon.Type == cur)
                {
                    icon.Refresh();
                    break;
                }
            }
        }
    }
}
