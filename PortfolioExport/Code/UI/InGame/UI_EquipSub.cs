using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Common;
using Game.Core;
using Game.Data;
using Game.UI.Icon;
using Game.UI.Popup;

namespace Game.UI
{
    public class UI_EquipSub : SubUI
    {
        [Header("Selected")]
        [SerializeField]
        private TextMeshProUGUI TextTitle;
        [SerializeField]
        private Icon_Currency IconCurrency;
        [SerializeField]
        private Icon_Equip IconEquip;
        [SerializeField]
        private TextMeshProUGUI TextEquipName;
        [SerializeField]
        private TextMeshProUGUI TextEquipLv;
        [SerializeField]
        private ContinueButton UpgradeButton;
        [SerializeField]
        private TextMeshProUGUI TextUpgrade;
        [SerializeField]
        private Image ImageUpgradeNeed;
        [SerializeField]
        private TextMeshProUGUI TextUpgradeNeed;
        [SerializeField]
        private Button ButtonEquip;
        [SerializeField]
        private TextMeshProUGUI TextEquip;
        [SerializeField]
        private Button ButtonGradeUp;
        [SerializeField]
        private TextMeshProUGUI TextGradeUp;

        [Header("States")]
        [SerializeField]
        private TextMeshProUGUI TextEquipStatTitle;
        [SerializeField]
        private List<GameObject> ObjEquipStat;
        [SerializeField]
        private List<TextMeshProUGUI> TextEquipStat;
        [SerializeField]
        private List<TextMeshProUGUI> TextEquipStatBeforePow;
        [SerializeField]
        private List<TextMeshProUGUI> TextEquipStatAfterPow;
        [SerializeField]
        private TextMeshProUGUI TextPassiceStatTitle;
        [SerializeField]
        private List<GameObject> ObjPassiveStat;
        [SerializeField]
        private List<TextMeshProUGUI> TextPassiveStat;
        [SerializeField]
        private List<TextMeshProUGUI> TextPassiveStatBeforePow;
        [SerializeField]
        private List<TextMeshProUGUI> TextPassiveStatAfterPow;

        [Header("Stars")]
        [SerializeField]
        private GameObject ObjStar;
        [SerializeField]
        private TextMeshProUGUI TextStarTitle;
        [SerializeField]
        private TextMeshProUGUI TextStarStat;
        [SerializeField]
        private Button ButtonNextStar;
        [SerializeField]
        private Button ButtonPreStar;
        [SerializeField]
        private Button ButtonStarUp;
        [SerializeField]
        private TextMeshProUGUI TextStarUp;

        private string Eid;

        private int UpgradeCount = 0;
        private int UpgradeCurrenySum = 0;
        private int EquipLv = 0;
        private int NowStar = 0;

        public void Init(string eid)
        {
            Eid = eid;
            IconEquip.Init(Eid);
            EquipLv = EquipDataMgr.Instance.GetEquipLv(Eid);
            RefreshEquipText();
            RefreshStatText();
            RefreshUpgradeText();
            RefreshStarText();
            EquipChanged();
            string cid = Table.GetString(TableType.Equip, Eid, "need_type");
            ImageUpgradeNeed.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common, Table.GetString(TableType.Currency, cid, "res"));
            TextEquipName.text = Table.GetString(TableType.Equip, Eid, "name");
        }

        protected override void Awake()
        {
            base.Awake();
            InitText();
            InitCallback();
            IconCurrency.Init(Currency.EquipUp);
        }

        private void InitText()
        {
            TextTitle.text = "상세 장비 창";
            TextGradeUp.text = "등급 업";
            TextEquipStatTitle.text = "장착시 스탯";
            TextPassiceStatTitle.text = "패시브 스탯";
            TextStarTitle.text = "각성 스탯";
            TextStarUp.text = "각성";
            TextUpgrade.text = "강화";
        }

        private void InitCallback()
        {
            UpgradeButton.onHold.AddListener(async () =>
            {
                int need = Equip.UpgradeNeed(Eid, EquipLv + UpgradeCount);

                if (need <= PlayerDataMgr.Instance.GetTempCurrency(Currency.EquipUp)
                    && EquipLv + UpgradeCount < Constant.EquipMaxLevel)
                {
                    UpgradeCount++;
                    SoundMgr.Instance.PlaySFXSound(SFXType.Upgrade);
                    UpgradeCurrenySum += need;
                    PlayerDataMgr.Instance.SetTempCurrency(Currency.EquipUp, UpgradeCurrenySum);
                    RefreshStatText();
                    RefreshUpgradeText();
                }
                else
                {
                    if (UpgradeCount == 0)
                    {
                        Popup_Message message = await PopupMgr.Instance.Get<Popup_Message>(PopupType.Message);
                        SoundMgr.Instance.PlaySFXSound(SFXType.No);
                        message.Init("재화가 부족합니다.");
                        message.Open();
                    }

                    UpgradeButton.ForcedClickEnd();
                }
            });

            UpgradeButton.onClickEnd.AddListener(() =>
            {
                EquipDataMgr.Instance.LevelUpEquip(Eid, UpgradeCount);
                EquipLv = EquipDataMgr.Instance.GetEquipLv(Eid);
                PlayerDataMgr.Instance.UseCurrency(Currency.EquipUp, UpgradeCurrenySum);
                UpgradeCount = 0;
                UpgradeCurrenySum = 0;
                RefreshStatText();
                RefreshUpgradeText();
            });

            ButtonGradeUp.onClick.AddListener(async () =>
            {
                Popup_EquipGradeUp popup = await PopupMgr.Instance.Get<Popup_EquipGradeUp>(PopupType.EquipGradeUp);
                SoundMgr.Instance.PlaySFXSound(SFXType.Upgrade);
                popup.Init(Eid);
                popup.Open();
            });

            ButtonEquip.onClick.AddListener(() =>
            {
                EquipDataMgr.Instance.Equip(Eid);
                SoundMgr.Instance.PlaySFXSound(SFXType.Equip);
                RefreshEquipText();
            });

            ButtonStarUp.onClick.AddListener(() =>
            {
                if (EquipDataMgr.Instance.IsValidStarUp(Eid))
                {
                    SoundMgr.Instance.PlaySFXSound(SFXType.Upgrade);
                    EquipDataMgr.Instance.StarUp(Eid);
                }
            });

            ButtonNextStar.onClick.AddListener(() =>
            {
                NowStar++;
                SoundMgr.Instance.PlaySFXSound(SFXType.Ok);
                RefreshStarText();
            });

            ButtonPreStar.onClick.AddListener(() =>
            {
                NowStar--;
                SoundMgr.Instance.PlaySFXSound(SFXType.Ok);
                RefreshStarText();
            });
        }

        private void RefreshEquipText()
        {
            if (EquipDataMgr.Instance.EquipId == Eid)
            {
                TextEquip.text = "장착중";
                ButtonEquip.interactable = false;
            }
            else
            {
                TextEquip.text = "장착";
                ButtonEquip.interactable = true;
            }
        }

        private void RefreshStatText()
        {
            TextEquipLv.text = string.Format("Lv.{0}", (EquipLv + UpgradeCount + 1).ToString());
            int need = Equip.UpgradeNeed(Eid, EquipLv + UpgradeCount + 1);
            TextUpgradeNeed.text = Equip.UpgradeNeed(Eid, EquipLv + UpgradeCount + 1).ToString();
            TextUpgradeNeed.color = need <= PlayerDataMgr.Instance.GetTempCurrency(Currency.EquipUp)
                ? Color.white : Color.red;

            for (int i = 0; i < TextEquipStat.Count; i++)
            {
                string sid = "x";

                if (Table.IsValidColumn(TableType.Equip, string.Format("equip_stat_{0}", i + 1)))
                {
                    sid = Table.GetString(TableType.Equip, Eid, string.Format("equip_stat_{0}", i + 1));
                }

                if (sid != "x")
                {
                    ObjEquipStat[i].SetActive(true);
                    TextEquipStat[i].text = Table.GetString(TableType.Stat, sid, "desc");
                    float baseStat = Table.GetFloat(TableType.Equip, Eid, string.Format("equip_pow_{0}", i + 1));
                    TextEquipStatBeforePow[i].text = string.Format("{0:F2}%", Equip.UpgeadedStat(Eid, baseStat, EquipLv + UpgradeCount));

                    if (EquipLv + UpgradeCount < Constant.EquipMaxLevel)
                    {
                        TextEquipStatAfterPow[i].text = string.Format("{0:F2}%", Equip.UpgeadedStat(Eid, baseStat, EquipLv + UpgradeCount + 1));
                    }
                    else
                    {
                        TextEquipStatAfterPow[i].text = "";
                    }
                }
                else
                {
                    ObjEquipStat[i].SetActive(false);
                }

                sid = "x";

                if (Table.IsValidColumn(TableType.Equip, string.Format("passive_stat_{0}", i + 1)))
                {
                    sid = Table.GetString(TableType.Equip, Eid, string.Format("passive_stat_{0}", i + 1));
                }

                if (sid != "x")
                {
                    ObjPassiveStat[i].SetActive(true);
                    TextPassiveStat[i].text = Table.GetString(TableType.Stat, sid, "desc");
                    float baseStat = Table.GetFloat(TableType.Equip, Eid, string.Format("passive_pow_{0}", i + 1));
                    TextPassiveStatBeforePow[i].text = string.Format("{0:F2}%", Equip.UpgeadedStat(Eid, baseStat, EquipLv + UpgradeCount));

                    if (EquipLv + UpgradeCount < Constant.EquipMaxLevel)
                    {
                        TextPassiveStatAfterPow[i].text = string.Format("{0:F2}%", Equip.UpgeadedStat(Eid, baseStat, EquipLv + UpgradeCount + 1));
                    }
                    else
                    {
                        TextPassiveStatAfterPow[i].text = "";
                    }
                }
                else
                {
                    ObjPassiveStat[i].SetActive(false);
                }
            }
        }

        private void RefreshUpgradeText()
        {
            IconCurrency.Refresh();
        }

        private void RefreshStarText()
        {
            string sid = "x";
            // Button을 비활성화 시켜도 누르는 방법이 있기 때문에 이렇게 해준다
            if (!Table.IsValidColumn(TableType.Equip, string.Format("star_stat_{0}", NowStar + 1)))
            {
                if (NowStar <= 0)
                {
                    NowStar = 0;
                }
                else
                {
                    NowStar--;
                }
            }

            sid = Table.GetString(TableType.Equip, Eid, string.Format("star_stat_{0}", NowStar + 1));

            if (sid == "x")
            {
                ObjStar.SetActive(false);
            }
            else
            {
                ObjStar.SetActive(true);
                TextStarStat.text = string.Format(Table.GetString(TableType.Stat, sid, "desc_format")
                    , Table.GetFloat(TableType.Equip, Eid, string.Format("star_pow_{0}", NowStar + 1)));

                ButtonPreStar.gameObject.SetActive(NowStar != 0);

                string nextSid = "x";

                if (Table.IsValidColumn(TableType.Equip, string.Format("star_stat_{0}", NowStar + 2)))
                {
                    nextSid = Table.GetString(TableType.Equip, Eid, string.Format("star_stat_{0}", NowStar + 2));
                }

                ButtonNextStar.gameObject.SetActive(NowStar == 0 && nextSid != "x");
            }
        }

        private void OnEnable()
        {
            EquipDataMgr.ChangedEquipValue += EquipChanged;
            PlayerDataMgr.ChangedCurrency += CurrencyChanged;
        }

        private void OnDisable()
        {
            EquipDataMgr.ChangedEquipValue -= EquipChanged;
            PlayerDataMgr.ChangedCurrency -= CurrencyChanged;
        }

        private void EquipChanged()
        {
            IconEquip.Refresh();
            UpgradeButton.interactable = EquipDataMgr.Instance.IsHaveEquip(Eid);
            ButtonEquip.interactable = EquipDataMgr.Instance.IsHaveEquip(Eid);
            ButtonStarUp.interactable = EquipDataMgr.Instance.IsValidStarUp(Eid);
        }

        private void CurrencyChanged(Currency cur)
        {
            if (cur == Currency.EquipUp)
            {
                RefreshUpgradeText();
            }
        }
    }
}
