using Game.Common;
using Game.Core;
using Game.Data;

using System;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Icon
{
    public class Icon_Soul : MonoBehaviour
    {
        [SerializeField]
        private Image ImageIcon;
        [SerializeField]
        private Image ImageGrade;
        [SerializeField]
        private TextMeshProUGUI TextLv;
        [SerializeField]
        private TextMeshProUGUI TextGrade;
        [SerializeField]
        private GameObject ObjEquip;
        [SerializeField]
        private Button ButtonClick;
        [SerializeField]
        private GameObject ObjCheck;
        [SerializeField]
        private GameObject ObjFocus;

        public string Key => MyKey;
        private string MyKey = "";

        public bool IsChecked => Checking;
        private bool Checking = false;

        private Action<Icon_Soul> OnActoin;

        public void Init(string key)
        {
            MyKey = key;
            //ImageIcon.sprite = SoulDataMgr.Instance.GetSoulID(Key);
            ImageGrade.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common,
                Constant.GradeBg[SoulDataMgr.Instance.GetSoulGrade(Key)]);
            ObjEquip.SetActive(SoulDataMgr.Instance.IsEquiped(Key));
            TextGrade.text = Constant.GradeName[SoulDataMgr.Instance.GetSoulGrade(Key)];
            RefreshText();
        }

        public void RefreshText()
        {
            TextLv.text = string.Format("Lv. {0}", SoulDataMgr.Instance.GetSoulLv(Key));
        }

        public void ButtonInit(Action<Icon_Soul> action)
        {
            OnActoin = action;

            ButtonClick.onClick.RemoveAllListeners();
            ButtonClick.onClick.AddListener(() => OnClicked());
        }

        private void OnClicked()
        {
            OnActoin?.Invoke(this);
        }

        public void SetFocus(bool isFocus)
        {
            ObjFocus.SetActive(isFocus);
        }

        public void SetCheck(bool isChecked)
        {
            Checking = isChecked;
            ObjCheck.SetActive(isChecked);
        }
    }
}
