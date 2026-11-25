using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Common;
using Game.Core;
using Game.Data;

namespace Game.UI.Icon
{
    public class Icon_Dungeon : MonoBehaviour
    {
        [SerializeField]
        private Image ImageBg;
        [SerializeField]
        private GameObject ObjFocus;
        [SerializeField]
        private GameObject ObjBlock;
        [SerializeField]
        private TextMeshProUGUI TextStage;
        [SerializeField]
        private Button ButtonSelect;

        private UI_Dungeon Parent;
        private int Lv = 0;

        public void Init(UI_Dungeon parent, BattleType type, int lv)
        {
            Parent = parent;
            Lv = lv;
            ImageBg.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common, Table.GetString(TableType.Dungeon, type.ToString(), "icon_bg"));

            TextStage.text = string.Format("{0} 스테이지", Lv);
            ObjBlock.SetActive(PlayerDataMgr.Instance.GetMaxStage(type) + 1 < lv);
        }

        private void Awake()
        {
            ButtonSelect.onClick.AddListener(() =>
            {
                SoundMgr.Instance.PlaySFXSound(SFXType.Ok);
                Parent.IconClicked(this, Lv);
            });
        }

        public void SetFocus(bool active)
        {
            ObjFocus.SetActive(active);
        }
    }
}
