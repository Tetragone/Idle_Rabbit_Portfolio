using Game.Common;
using Game.Core;
using Game.Data;
using Game.UI.Icon;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class UI_Soul : MonoBehaviour
    {
        [SerializeField]
        private UI_VirtualList SoulList;
        [SerializeField]
        private TextMeshProUGUI TextNone;

        [Space(10)]
        [SerializeField]
        private UI_SoulFilter Filter;

        [Space(10)]
        [SerializeField]
        private Button ButtonStartDecompose;
        [SerializeField]
        private TextMeshProUGUI TextStartDecompose;
        [SerializeField]
        private Button ButtonFilter;
        [SerializeField]
        private TextMeshProUGUI TextFilter;

        [Space(10)]
        [SerializeField]
        private GameObject ObjSelectDecompse;
        [SerializeField]
        private Button ButtonDecomposeCancle;
        [SerializeField]
        private TextMeshProUGUI TextCancle;
        [SerializeField]
        private Button ButtonDecompose;
        [SerializeField]
        private TextMeshProUGUI TextDecompose;

        private HashSet<string> SetDecompose = new HashSet<string>();
        private List<string> SoulKeys = new List<string>();
        private string FocusKey;

        private bool IsDecompose = false;
        private int MinGrade = 0;
        private int MinLv = 0;

        private void Awake()
        {
            InitText();
            InitCallback();
            InitList();
        }

        private void InitText()
        {
            TextNone.text = "아직 소울이 존재 하지 않습니다.";
            TextStartDecompose.text = "일괄 분해";
            TextCancle.text = "취소";
            TextDecompose.text = "분해";
            TextFilter.text = "필터";
        }

        private void InitCallback()
        {
            ButtonStartDecompose.onClick.AddListener(() =>
            {
                SoundMgr.Instance.PlaySFXSound(SFXType.Ok);
                SetDecompose.Clear();
                ObjSelectDecompse.SetActive(true);
                IsDecompose = true;

                UIMgr.Instance.Get<UI_SoulSub>(MainUIType.SoulSub)?.Close();
                SoulList.RefreshVisibleItems();
            });

            ButtonDecomposeCancle.onClick.AddListener(() =>
            {
                SoundMgr.Instance.PlaySFXSound(SFXType.No);
                SetDecompose.Clear();
                ObjSelectDecompse.SetActive(false);
                IsDecompose = false;

                SoulList.RefreshVisibleItems();
            });

            ButtonDecompose.onClick.AddListener(() =>
            {
                SoundMgr.Instance.PlaySFXSound(SFXType.Ok);
                SoulDataMgr.Instance.DecomposeSoul(SetDecompose);
                SetDecompose.Clear();
                ObjSelectDecompse.SetActive(false);
                IsDecompose = false;

                SoulList.RefreshVisibleItems();
            });

            ButtonFilter.onClick.AddListener(() =>
            {
                Filter.gameObject.SetActive(true);
                Filter.RefreshTooggle(MinGrade, MinLv);
            });

            Filter.AddButtonOkListener(() =>
            {
                Filter.gameObject.SetActive(false);
                MinGrade = Filter.MinGrades;
                MinLv = Filter.MinLv;
                RefreshIcon(true);
            });

            Filter.AddButtonCancleListenner(() =>
            {
                Filter.gameObject.SetActive(false);
            });
        }

        private void InitList()
        {
            
            if (SoulList == null)
            {
                throw new InvalidOperationException("UI_Soul의 Soul List를 연결해야 합니다.");
            }

            SoulList.Init((cell) =>
            {
                Icon_Soul icon = cell.GetComponent<Icon_Soul>();

                if (icon == null)
                {
                    throw new InvalidOperationException("Soul List의 셀 프리팹 루트에 Icon_Soul이 필요합니다.");
                }

                icon.ButtonInit((clickedIcon) => OnIconClicked(clickedIcon));
            }
            , (cell, idx) =>
            {
                Icon_Soul icon = cell.GetComponent<Icon_Soul>();
                string key = SoulKeys[idx];

                icon.Init(key);
                icon.SetCheck(IsDecompose && SetDecompose.Contains(key));
                icon.SetFocus(key == FocusKey);
            });
        }

        private void RefreshIcon(bool resetPosition = false)
        {
            // 지연 열거 결과를 확정하여 스크롤 중 인덱스로 접근한다.
            SoulKeys.Clear();
            SoulKeys.AddRange(SoulDataMgr.Instance.GetSoulsKeyByCondition(
                (SoulDataMgr.SoulCondition.Grade, MinGrade),
                (SoulDataMgr.SoulCondition.Lv, MinLv)));

            TextNone.gameObject.SetActive(SoulKeys.Count == 0);
            SoulList.SetItemCount(SoulKeys.Count, resetPosition);
        }

        private void ClearFocus(bool useSound)
        {
            FocusKey = null;

            if (isActiveAndEnabled)
            {
                SoulList.RefreshVisibleItems();
            }
        }

        private void OnIconClicked(Icon_Soul icon)
        {
            if (IsDecompose)
            {
                SelectDecomposeIcon(icon);
            }
            else
            {
                OpenIconInfo(icon);
            }
        }

        private void SelectDecomposeIcon(Icon_Soul icon)
        {
            if (!SetDecompose.Add(icon.Key)) 
            {
                SoundMgr.Instance.PlaySFXSound(SFXType.Ok);
                SetDecompose.Remove(icon.Key);
                icon.SetCheck(false);
            } 
            else
            {
                SoundMgr.Instance.PlaySFXSound(SFXType.No);
                icon.SetCheck(true);
            }
        }

        private void OpenIconInfo(Icon_Soul icon)
        {
            FocusKey = icon.Key;
            SoulList.RefreshVisibleItems();

            UIMgr.Instance.Get<UI_SoulSub>(MainUIType.SoulSub).Init(FocusKey);
            UIMgr.Instance.Get<UI_SoulSub>(MainUIType.SoulSub).Open();

            // 다른 셀로 재사용된 객체를 닫기 콜백에서 참조하지 않는다.
            UIMgr.Instance.Get<UI_SoulSub>(MainUIType.SoulSub).CloseAction -= ClearFocus;
            UIMgr.Instance.Get<UI_SoulSub>(MainUIType.SoulSub).CloseAction += ClearFocus;
        }

        private void OnEnable()
        {
            SoulDataMgr.AllSoulChanged += ChangedAllSouls;
            SoulDataMgr.SoulChanged += ChangedSingleSoul;

            // Title에서 저장 데이터가 적용된 뒤 InGame UI가 활성화된다.
            // 비활성 중 놓친 변경도 다시 조회한다.
            RefreshIcon();
        }

        private void OnDisable()
        {
            SoulDataMgr.AllSoulChanged -= ChangedAllSouls;
            SoulDataMgr.SoulChanged -= ChangedSingleSoul;

            if (FocusKey != null)
            {
                UIMgr.Instance.Get<UI_SoulSub>(MainUIType.SoulSub).CloseAction -= ClearFocus;
                FocusKey = null;
            }
        }

        private void ChangedAllSouls()
        {
            RefreshIcon();
        }

        private void ChangedSingleSoul(string key)
        {
            SoulList.RefreshItem(SoulKeys.IndexOf(key));
        }
    }
}
