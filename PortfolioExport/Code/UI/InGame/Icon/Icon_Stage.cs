using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Calculate;
using Game.Common;
using Game.Core;
using Game.Data;
using Game.UI.Popup;

namespace Game.UI.Icon
{
    public class Icon_Stage : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI TextTitle;
        [SerializeField]
        private Image ImageBg;
        [SerializeField]
        private Button ButtonSelect;
        [SerializeField]
        private GameObject ObjSelected;
        [SerializeField]
        private GameObject ObjNow;
        [SerializeField]
        private GameObject ObjNotYet;

        public string Sid => nowSid;
        private string nowSid = "";
        private Popup_StageSelect Parent;

        public void SetParent(Popup_StageSelect parent)
        {
            Parent = parent;
        }

        public void Init(string sid)
        {
            nowSid = sid;
            TextTitle.text = Table.GetString(TableType.Stage, nowSid, "name");
            ObjNotYet.SetActive(StageCalculator.GetNextStageInt(PlayerDataMgr.Instance.GetMaxStage(BattleType.Normal)) < int.Parse(nowSid));
            string path = Table.GetString(TableType.Stage, nowSid, "bg_res");
            if (StageCalculator.GetNextStageInt(PlayerDataMgr.Instance.GetMaxStage(BattleType.Normal)) <= int.Parse(nowSid))
            {
                path += "_challenge";
            }
            ImageBg.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common, path);
            Refresh();
            SetSelected(PlayerDataMgr.Instance.GetCurStage(BattleType.Normal) == int.Parse(nowSid));
        }

        public void SetSelected(bool selcted)
        {
            ObjSelected.SetActive(selcted);
        }

        public void Refresh()
        {
            ObjNow.SetActive(PlayerDataMgr.Instance.GetCurStage(BattleType.Normal) == int.Parse(nowSid));
        }

        private void Awake()
        {
            InitCallback();
        }

        private void InitCallback()
        {
            ButtonSelect.onClick.AddListener(() =>
            {
                SoundMgr.Instance.PlaySFXSound(SFXType.Ok);
                Parent.SetSelected(nowSid);
                SetSelected(true);
            });
        }
    }
}
