using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Common;
using Game.Core;
using Game.Data;
using Game.UI.Popup;

namespace Game.UI.Icon
{
    public class Icon_GachaDetail : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI TextLv;
        [SerializeField]
        private Image ImageGacha;
        [SerializeField]
        private Button ButtonPopup;
        [SerializeField]
        private UI_Gauge Gauge;

        private GachaType Type = GachaType.Equip;

        public void Init(GachaType type)
        {
            Type = type;

            switch (Type)
            {
                case GachaType.Equip:
                    ImageGacha.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common, Table.GetString(TableType.Currency, "EquipTicket", "res"));
                    ImageGacha.gameObject.SetActive(true);
                    int gachaLv = PlayerDataMgr.Instance.GetEquipGachaLv();
                    int gachaedCount = PlayerDataMgr.Instance.GetEquipGachaCount();
                    int needCount = 0;

                    foreach (var key in Table.GetTable(TableType.EquipGacha).Keys)
                    {
                        if (Table.GetInt(TableType.EquipGacha, key, "lv") == gachaLv)
                        {
                            needCount = Table.GetInt(TableType.EquipGacha, key, "need_count");
                            break;
                        }
                    }

                    Gauge.SetGauge(gachaedCount, needCount);
                    Gauge.gameObject.SetActive(true);
                    TextLv.text = string.Format("Lv. {0}", gachaLv);
                    TextLv.gameObject.SetActive(true);
                    break;
                case GachaType.Skill:
                    if (TextLv != null)
                    {
                        TextLv.gameObject.SetActive(false);
                    }

                    if (Gauge != null)
                    {
                        Gauge.gameObject.SetActive(false);
                    }

                    if (ImageGacha != null)
                    {
                        ImageGacha.gameObject.SetActive(false);
                    }
                    break;
            }
        }

        private void Awake()
        {
            ButtonPopup.onClick.AddListener(async () =>
            {
                var popup = await PopupMgr.Instance.Get<Popup_GachaDetail>(PopupType.GachaDetail);
                popup.Init(Type);
                popup.Open();
            });
        }
    }
}
