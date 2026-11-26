using Game.Common;
using Game.Core;
using Game.Data;
using Game.UI.Icon;
using Game.UI.Popup;

using System.Collections.Generic;
using System.Threading.Tasks;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class UI_SoulSub : SubUI
    {
        [SerializeField]
        private Icon_Soul IconSoul;

        [SerializeField]
        private TextMeshProUGUI TextName;
        [SerializeField]
        private TextMeshProUGUI TextLv;
        [SerializeField]
        private TextMeshProUGUI TextGrade;

        [SerializeField]
        private Button ButtonEquip;
        [SerializeField]
        private TextMeshProUGUI TextEquip;
        [SerializeField]
        private ContinueButton ButtonLevelUp;
        [SerializeField]
        private TextMeshProUGUI TextLevelUp;
        [SerializeField]
        private Button ButtonDecompose;
        [SerializeField]
        private TextMeshProUGUI TextDecompose;

        [SerializeField]
        private TextMeshProUGUI TextSubOptionTitle;
        [SerializeField]
        private List<GameObject> ObjSubOptions;
        [SerializeField]
        private List<TextMeshProUGUI> TextSubOption;
        [SerializeField]
        private List<TextMeshProUGUI> TextSubOptionValue;
        [SerializeField]
        private TextMeshProUGUI TextNoneSubOption;

        private string Key = "";
        private int LevelUpCount = 0;
        private int Grade = 0;

        private Dictionary<string, int> MySubOptions = new Dictionary<string, int>();
        private List<string> MySubOptionKeys = new List<string>();

        public void Init(string key)
        {
            Key = key;
            SetOptionText();
            IconSoul.Init(Key);
        }

        private void SetOptionText()
        {
            string sid = SoulDataMgr.Instance.GetSoulID(Key);
            TextName.text = Table.GetString(TableType.Soul, sid, "name");
            RefreshLv();
            Grade = SoulDataMgr.Instance.GetSoulGrade(Key);
            TextGrade.text = Constant.GradeName[Grade];
            
            List<string> subtOptions = SoulDataMgr.Instance.GetSoulSubOption(Key);
            MySubOptions.Clear();
            
            foreach (var subOption in subtOptions)
            {
                if (MySubOptions.ContainsKey(subOption))
                {
                    MySubOptions[subOption]++;
                }
                else
                {
                    MySubOptions.Add(subOption, 1);
                    MySubOptionKeys.Add(subOption);
                }
            }

            TextNoneSubOption.gameObject.SetActive(MySubOptions.Count == 0);
            int idx = 0;

            foreach (var key in MySubOptionKeys)
            {
                ObjSubOptions[idx].SetActive(true);
                TextSubOption[idx].text = Table.GetString(TableType.Stat, key, "name");
                float value = Soul.GetSoulSubValue(Grade, MySubOptions[key]);
                TextSubOptionValue[idx].text = string.Format("{0}%", value);
                idx++;
            }

            for (; idx < ObjSubOptions.Count; idx++)
            {
                ObjSubOptions[idx].SetActive(false);
            }
            RefreshButton();
        }

        private void RefreshLv(List<string> addedSuboption = null)
        {
            TextLv.text = string.Format("Lv. {0}", SoulDataMgr.Instance.GetSoulLv(Key) + LevelUpCount);

            if (addedSuboption == null)
            {
                return;
            }

            HashSet<string> added = new HashSet<string>();

            for (int i = 0; i < addedSuboption.Count; i++)
            {
                added.Add(addedSuboption[i]);

                if (MySubOptions.ContainsKey(addedSuboption[i]))
                {
                    MySubOptions[addedSuboption[i]]++;
                }
                else
                {
                    MySubOptions.Add(addedSuboption[i], 1);
                    MySubOptionKeys.Add(addedSuboption[i]);
                }
            }

            foreach (string key in added)
            {
                for (int i = 0; i < MySubOptionKeys.Count; i++)
                {
                    if (key == MySubOptionKeys[i])
                    {
                        float value = Soul.GetSoulSubValue(Grade, MySubOptions[key]);
                        TextSubOptionValue[i].text = string.Format("{0}%", value);
                    }
                }
            }
        }

        private void RefreshButton()
        {
            ButtonLevelUp.interactable = SoulDataMgr.Instance.GetSoulLv(Key) < Constant.SoulMaxLevel;
            ButtonEquip.interactable = !SoulDataMgr.Instance.IsEquiped(Key);
        }

        protected override void Awake()
        {
            base.Awake();
            InitText();
            InitCallback();
        }

        private void InitText()
        {
            TextEquip.text = "장착";
            TextLevelUp.text = "강화";
            TextDecompose.text = "분해";
            TextSubOptionTitle.text = "서브 옵션";
            TextNoneSubOption.text = "서브 옵션이 없습니다.";
        }

        private void InitCallback()
        {
            ButtonEquip.onClick.AddListener(() =>
            {
                SoundMgr.Instance.PlaySFXSound(SFXType.Equip);
                SoulDataMgr.Instance.EquipSoul(Key);
            });

            ButtonDecompose.onClick.AddListener(() =>
            {
                _ = OpenConfirmPopup();
            });

            ButtonLevelUp.onHold.AddListener(() =>
            {
                if (LevelUpCount + SoulDataMgr.Instance.GetSoulLv(Key) >= Constant.SoulMaxLevel)
                {
                    ButtonLevelUp.ForcedClickEnd();
                    SoundMgr.Instance.PlaySFXSound(SFXType.No);
                    return;
                }

                SoundMgr.Instance.PlaySFXSound(SFXType.Upgrade);
                LevelUpCount++;
                RefreshLv();
            });

            ButtonLevelUp.onClickEnd.AddListener(() =>
            {
                var addedSubOption = SoulDataMgr.Instance.UpgradeSoul(Key, LevelUpCount);
                LevelUpCount = 0;
                RefreshLv(addedSubOption);
                RefreshButton();
            });
        }

        private async Task OpenConfirmPopup()
        {
            Popup_Common popup = await PopupMgr.Instance.Get<Popup_Common>(PopupType.Common);
            popup.TextTitle.text = "이 소울을 분해하겠습니까?";
            popup.IsUseYes(true);

            popup.ButtonYes.onClick.AddListener(() =>
            {
                _ = OpenGetItemPopup();
                popup.Close();
            });

            popup.ButtonNo.onClick.AddListener(() =>
            {
                popup.Close();
            });
        }

        private async Task OpenGetItemPopup()
        {
            int value = SoulDataMgr.Instance.DecomposeSoul(Key);
            Popup_GetItems getItems = await PopupMgr.Instance.Get<Popup_GetItems>(PopupType.GetItem);
            getItems.Init();
            getItems.AddItems(new Dictionary<Currency, double>() { { Currency.SoulUp, value } });
            getItems.Open();
        }
    }
}
