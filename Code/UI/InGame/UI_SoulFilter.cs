using Game.Common;

using System;
using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    // popup으로 하지 않은 이유는 생각이상으로 자주 사용될 듯 하니 굳이 popup으로 뺄 이유가 없어 보였음.
    public class UI_SoulFilter : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI TextTitle;

        [SerializeField]
        private TextMeshProUGUI TextGrade;
        [SerializeField]
        private List<Toggle> TogglesGrade;
        [SerializeField]
        private List<TextMeshProUGUI> TextsToggleGrade;
        [SerializeField]
        private TextMeshProUGUI TextLv;
        [SerializeField]
        private List<Toggle> TogglesLv;
        [SerializeField]
        private List<TextMeshProUGUI> TextsToggleLv;

        [SerializeField]
        private Button ButtonOk;
        [SerializeField]
        private TextMeshProUGUI TextOk;
        [SerializeField]
        private Button ButtonCancle;
        [SerializeField]
        private TextMeshProUGUI TextCancle;

        public int MinGrades => MyGrades;
        private int MyGrades = 0;

        public int MinLv => MyMinLv;
        private int MyMinLv = 0;

        private Action ButtonOkAction;
        private Action ButtonCancleAction;

        public void AddButtonOkListener(Action action)
        {
            ButtonOkAction = action;
        }
        
        private void OkAction()
        {
            ButtonOkAction?.Invoke();
        }

        public void AddButtonCancleListenner(Action action)
        {
            ButtonCancleAction = action;
        }

        private void NoAction()
        {
            ButtonCancleAction?.Invoke();
        }

        public void RefreshTooggle(int MinGrade, int MinLv)
        {
            MyGrades = MinGrade;
            MyMinLv = MinLv;

            for (int i = 0; i < TogglesGrade.Count; i++)
            {
                TogglesGrade[i].isOn = MinGrade == i;
            }

            for (int i = 0; i < TogglesLv.Count; i++)
            {
                TogglesLv[i].isOn = MinLv == i;
            }
        }

        private void Awake()
        {
            InitText();
            InitCallback();
        }

        private void InitText()
        {
            TextTitle.text = "소울 필터";
            TextOk.text = "결정";
            TextCancle.text = "취소";
            TextGrade.text = "최소 등급";
            TextLv.text = "최소 레벨";

            for (int i = 0; i < TextsToggleGrade.Count; i++)
            {
                TextsToggleGrade[i].text = string.Format("{0} 이상", Constant.GradeName[i]);
            }

            for (int i = 0; i < TextsToggleLv.Count; i++)
            {
                TextsToggleLv[i].text = string.Format("Lv. {0} 이상", Constant.SoulSubOptionAddedLvInterval * i);
            }
        }

        private void InitCallback()
        {
            ButtonOk.onClick.AddListener(OkAction);
            ButtonCancle.onClick.AddListener(NoAction);

            for (int i = 0; i < TogglesGrade.Count; i++)
            {
                int temp = i;
                TogglesGrade[i].isOn = true;

                TogglesGrade[i].onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        MyGrades = temp;
                    }
                });
            }

            for (int i = 0; i < TogglesLv.Count; i++)
            {
                int temp = i;

                TogglesLv[i].onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        MyMinLv = Constant.SoulSubOptionAddedLvInterval * temp;
                    }
                });
            }
        }
    }
}
