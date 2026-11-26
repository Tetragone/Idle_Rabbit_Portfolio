using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Calculate;
using Game.Common;
using Game.Data;

namespace Game.UI.Popup
{
    public class Popup_GachaDetail : BasePopup
    {
        [Space(15)]
        [SerializeField]
        private GachaRateSet ResSet;
        [SerializeField]
        private Transform RateContent;

        [Header("Level Objs")]
        [SerializeField]
        private GameObject ObjLvs;
        [SerializeField]
        private Button ButtonUp;
        [SerializeField]
        private Button ButtonDown;
        [SerializeField]
        private TextMeshProUGUI TextNowLv;

        private List<GachaRateSet> RateSets = new List<GachaRateSet>();
        private GachaType NowType = GachaType.Equip;
        private int NowLv = 0;

        public void Init(GachaType type)
        {
            NowType = type;

            switch (NowType)
            {
                case GachaType.Equip:
                    ObjLvs.SetActive(true);
                    NowLv = PlayerDataMgr.Instance.GetEquipGachaLv();
                    TextTitle.text = "장비 뽑기 확률표";
                    TextContent.gameObject.SetActive(false);
                    break;
                case GachaType.Skill:
                    ObjLvs.SetActive(false);
                    TextTitle.text = "스킬 뽑기 확률표";
                    TextContent.gameObject.SetActive(true);
                    TextContent.text = "같은 등급의 스킬은 동일한 확률로 얻을 수 있습니다.";
                    break;
            }

            Refresh();
        }

        protected override void Awake()
        {
            base.Awake();

            ButtonUp.onClick.AddListener(() =>
            {
                NowLv++;

                if (NowLv >= GachaCalculator.MaxEquipGachaLv)
                {
                    NowLv = GachaCalculator.MaxEquipGachaLv;
                }

                Refresh();
            });

            ButtonDown.onClick.AddListener(() =>
            {
                NowLv--;

                if (NowLv <= 1)
                {
                    NowLv = 1;
                }

                Refresh();
            });

            ButtonOk.onClick.AddListener(Close);
            TextOk.text = "확인";
        }

        private void Refresh()
        {
            int idx = 0;

            switch (NowType)
            {
                case GachaType.Equip:
                    idx = 0;

                    var rates = GachaCalculator.GetGachaEquipRate(NowLv);
                    TextNowLv.text = string.Format("Lv. {0}", NowLv);
                    ButtonDown.interactable = true;
                    ButtonUp.interactable = true;

                    if (NowLv <= 1)
                    {
                        ButtonDown.interactable = false;
                    }

                    if (NowLv >= GachaCalculator.MaxEquipGachaLv)
                    {
                        ButtonUp.interactable = false;
                    }

                    List<KeyValuePair<string, float>> listRate = new List<KeyValuePair<string, float>>();
                    foreach (var key in rates.Keys)
                    {
                        listRate.Add(new KeyValuePair<string, float>(key, rates[key]));
                    }

                    listRate.Sort((a, b) =>
                    {
                        return a.Key.CompareTo(b.Key);
                    });

                    for (; idx < listRate.Count; idx++)
                    {
                        if (idx >= RateSets.Count)
                        {
                            var newRate = Instantiate(ResSet, RateContent);
                            RateSets.Add(newRate);
                        }

                        int grade = Table.GetInt(TableType.Equip, listRate[idx].Key, "grade");
                        RateSets[idx].gameObject.SetActive(true);
                        RateSets[idx].TextName.text = string.Format("<color=#{0}>{1}</color>", Constant.GradeColor[grade - 1], Table.GetString(TableType.Equip, listRate[idx].Key, "name"));
                        RateSets[idx].TextRate.text = (listRate[idx].Value * 100f).ToString("F3") + "%";
                    }

                    for (; idx < RateSets.Count; idx++)
                    {
                        RateSets[idx].gameObject.SetActive(false);
                    }

                    break;
                case GachaType.Skill:
                    string gid = "";
                    idx = 0;

                    foreach (var key in Table.GetTable(TableType.Gachas).Keys)
                    {
                        if (Table.GetString(TableType.Gachas, key, "type") == "skill")
                        {
                            gid = key;
                            break;
                        }
                    }

                    int sum = 0;

                    List<int> skillRates = new List<int>();

                    for (int i = 1; ; i++)
                    {
                        string col = string.Format("grade_{0}", i);

                        if (Table.IsValidColumn(TableType.Gachas, col))
                        {
                            sum += Table.GetInt(TableType.Gachas, gid, col);
                            skillRates.Add(Table.GetInt(TableType.Gachas, gid, col));
                        }
                        else
                        {
                            break;
                        }
                    }

                    for (; idx < skillRates.Count; idx++)
                    {
                        if (idx >= RateSets.Count)
                        {
                            var newRate = Instantiate(ResSet, RateContent);
                            RateSets.Add(newRate);
                        }

                        RateSets[idx].gameObject.SetActive(true);
                        RateSets[idx].TextName.text = string.Format("<color=#{0}>{1}</color>", Constant.GradeColor[idx], Constant.GradeName[idx]);
                        RateSets[idx].TextRate.text = (skillRates[idx] / (float)sum * 100f).ToString("F3") + "%";
                    }

                    for (; idx < RateSets.Count; idx++)
                    {
                        RateSets[idx].gameObject.SetActive(false);
                    }
                    break;
            }
        }
    }
}
