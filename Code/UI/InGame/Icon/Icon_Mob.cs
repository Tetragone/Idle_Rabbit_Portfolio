using UnityEngine;
using UnityEngine.UI;

using Game.Common;
using Game.Core;
using Game.Data;

namespace Game.UI.Icon
{
    public class Icon_Mob : MonoBehaviour
    {
        [SerializeField]
        private Image ImagePortrait;
        [SerializeField]
        private Button ButtonoInfo;

        public void Init(string eid)
        {
            ImagePortrait.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Enemys, Table.GetString(TableType.Enemy, eid, "p_res"));
        }

        private void Awake()
        {
            ButtonoInfo.onClick.AddListener(() =>
            {

            });
        }
    }
}
