using System.Collections;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Common;
using Game.Data;
using Game.UI.Icon;

namespace Game.UI.Popup
{
    public class Popup_EquipGradeUp : BasePopup
    {
        [SerializeField]
        private Icon_Equip BeforeEquip;
        [SerializeField]
        private Icon_Equip AfterEquip;

        [SerializeField]
        private Button ButtonAuto;
        [SerializeField]
        private TextMeshProUGUI TextAuto;

        [SerializeField]
        private Animator GradeUpAni;

        private string Eid = "";

        public void Init(string eid)
        {
            Eid = eid;

            BeforeEquip.Init(Eid);

            int nextEid = int.Parse(Eid) + 1;
            if (Table.IsValidKey(TableType.Equip, nextEid.ToString()))
            {
                AfterEquip.Init(nextEid.ToString());
            }
        }
        protected override void Awake()
        {
            base.Awake();
            InitText();
            InitButton();
        }

        private void InitText()
        {
            TextTitle.text = "장비 등급업";
            TextContent.text = string.Format("하위 장비 {0}개를 사용해서 상위 장비를 만들 수 있습니다.", Constant.EquipAdvanceCount);
            TextYes.text = "등급 업";
            TextAuto.text = "자동 등급 업";
            TextNo.text = "취소";
        }

        private void InitButton()
        {
            ButtonYes.onClick.RemoveAllListeners();
            ButtonYes.onClick.AddListener(() =>
            {
                if (IsValidGradeUp())
                {
                    StartCoroutine(GradeUp(true));
                }
            });

            ButtonAuto.onClick.RemoveAllListeners();
            ButtonAuto.onClick.AddListener(() =>
            {
                if (IsValidGradeUp())
                {
                    StartCoroutine(GradeUp(false));
                }
            });

            ButtonNo.onClick.RemoveAllListeners();
            ButtonNo.onClick.AddListener(() =>
            {
                Close();
            });
        }

        private bool IsValidGradeUp()
        {
            return EquipDataMgr.Instance.GetEquipCount(Eid) >= Constant.EquipAdvanceCount;
        }

        private IEnumerator GradeUp(bool isSingle)
        {
            ButtonsInteractive(false);

            while (IsValidGradeUp())
            {
                GradeUpAni.SetTrigger("GradeUp");
                yield return null;
                while (GradeUpAni.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.99f)
                {
                    yield return null;
                }

                EquipDataMgr.Instance.GradeUpEquip(Eid);
                int nextEid = int.Parse(Eid) + 1;
                if (Table.IsValidKey(TableType.Equip, nextEid.ToString()) && !isSingle)
                {
                    Eid = nextEid.ToString();
                    Init(Eid);
                }
                else
                {
                    break;
                }
            }

            ButtonsInteractive(true);
            Close();
        }

        private void ButtonsInteractive(bool isActive)
        {
            ButtonYes.interactable = isActive;
            ButtonAuto.interactable = isActive;
            ButtonClose.interactable = isActive;
            ButtonBlock.interactable = isActive;
            ButtonNo.interactable = isActive;
        }
    }
}
