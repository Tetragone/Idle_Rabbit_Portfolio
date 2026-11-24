using System;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Common;
using Game.Core;
using Game.Data;

namespace Game.UI.Icon
{
    public class Icon_SimpleItem : MonoBehaviour
    {
        [SerializeField]
        private Image ImageItem;
        [SerializeField]
        private Image ImageBg;
        [SerializeField]
        private Button ButtonoInfo;
        [SerializeField]
        private TextMeshProUGUI TextCount;

        private ItemType Type;

        public void Init(ItemType type, string id, double count)
        {
            Type = type;

            switch (Type)
            {
                case ItemType.Equip:
                    ImageItem.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Equip, Table.GetString(TableType.Equip, id, "res"));
                    ImageBg.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common, Constant.GradeBg[Table.GetInt(TableType.Equip, id, "grade") - 1]);
                    break;
                case ItemType.Skill:
                    ImageItem.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Skill, Table.GetString(TableType.Skill, id, "image_res"));
                    ImageBg.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common, Constant.GradeBg[Table.GetInt(TableType.Skill, id, "grade") - 1]);
                    break;
                case ItemType.EventCurrency:
                    ImageItem.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common, Table.GetString(TableType.EventCurrency, id, "res"));
                    ImageBg.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common, Constant.GradeBg[0]);
                    break;
                case ItemType.Currency:
                default:
                    ImageItem.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common, Table.GetString(TableType.Currency, id, "res"));
                    ImageBg.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common, Constant.GradeBg[0]);
                    break;
            }

            count = Math.Round(count);
            TextCount.text = NumberFormatter.ToCompact(count);
        }

        private void Awake()
        {
            ButtonoInfo.onClick.AddListener(() =>
            {

            });
        }
    }
}
