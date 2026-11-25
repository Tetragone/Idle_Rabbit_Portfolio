using System;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Common;
using Game.Core;
using Game.Data;

namespace Game.UI.Icon
{
    public class Icon_RoguelikeEffect : MonoBehaviour
    {
        [SerializeField] private Image ImageIcon;
        [SerializeField] private TextMeshProUGUI TextTitle;
        [SerializeField] private TextMeshProUGUI TextDesc;
        [SerializeField] private Button ButtonSelect;

        private string EffectId;
        private Action<string> OnSelected;

        private void Awake()
        {
            ButtonSelect.onClick.AddListener(() => OnSelected?.Invoke(EffectId));
        }

        public void Init(string id, Action<string> onSelected)
        {
            EffectId = id;
            OnSelected = onSelected;

            string stat = Table.GetString(TableType.RoguelikeEffect, id, "stat");
            float value = Table.GetFloat(TableType.RoguelikeEffect, id, "value");

            TextTitle.text = Table.GetString(TableType.Stat, stat, "desc");
            TextDesc.text = string.Format(Table.GetString(TableType.Stat, stat, "desc_format"), value);
            ImageIcon.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Upgrade,
                Table.GetString(TableType.RoguelikeEffect, id, "icon"));
        }
    }
}
