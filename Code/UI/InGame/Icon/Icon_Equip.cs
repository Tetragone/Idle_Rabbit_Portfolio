using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Common;
using Game.Core;
using Game.Data;

namespace Game.UI.Icon
{
    public class Icon_Equip : MonoBehaviour
    {
        public string Id => EId;

        [SerializeField]
        private Image ImageEquip;
        [SerializeField]
        private GameObject ObjBlock;
        [SerializeField]
        private GameObject ObjEquiped;
        [SerializeField]
        private UI_Gauge GaugeCount;
        [SerializeField]
        private Image ImageGradeBg;
        [SerializeField]
        private TextMeshProUGUI TextGrade;
        [SerializeField]
        private TextMeshProUGUI TextLv;
        [SerializeField]
        private Button ButtonInfo;

        [SerializeField]
        private GameObject ObjFocus;

        private string EId = "";
        private UI_Equip Parent;
        private bool IsButtonActived = false;

        public void Init(string eid)
        {
            EId = eid;
            ImageEquip.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Equip, Table.GetString(TableType.Equip, EId, "res"));
            int grade = Table.GetInt(TableType.Equip, EId, "grade");
            ImageGradeBg.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common, Constant.GradeBg[grade - 1]);
            TextGrade.text = Constant.GradeName[grade - 1];
            Refresh();
        }

        public void ButtonInit(UI_Equip parent)
        {
            Parent = parent;

            if (!IsButtonActived)
            {
                ButtonInfo.onClick.AddListener(() =>
                {
                    Parent.IconClicked(EId);
                    UI_EquipSub subEquip = UIMgr.Instance.Get<UI_EquipSub>(MainUIType.EquipSub);
                    subEquip.Open();
                    subEquip.Init(EId);
                    Parent.CloseAction += subEquip.Close;
                    subEquip.CloseAction += (useSound) => { OnFocus(false); };
                    OnFocus(true);
                });

                IsButtonActived = true;
            }
        }

        public void Refresh()
        {
            int count = EquipDataMgr.Instance.GetEquipCount(EId);
            GaugeCount.SetGauge(count, Constant.EquipAdvanceCount);
            TextLv.text = string.Format("Lv.{0}", EquipDataMgr.Instance.GetEquipLv(EId));
            ObjEquiped.SetActive(EquipDataMgr.Instance.EquipId == EId);
            ObjBlock.SetActive(!EquipDataMgr.Instance.IsHaveEquip(EId));
        }

        public void OnFocus(bool isFocus)
        {
            ObjFocus.SetActive(isFocus);
        }
    }
}
