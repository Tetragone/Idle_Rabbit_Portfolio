using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Battle;
using Game.Calculate;
using Game.Common;
using Game.Data;
using Game.UI.Icon;

namespace Game.UI.Popup
{
    public class Popup_StageSelect : BasePopup
    {
        [SerializeField]
        private Icon_Stage ResStage;
        [SerializeField]
        private Icon_Mob ResMob;
        [SerializeField]
        private Icon_SimpleItem ResItem;

        [Header("챕터 선택")]
        [SerializeField]
        private Button ButtonBeforeChapter;
        [SerializeField]
        private Button ButtonAfterChapter;
        [SerializeField]
        private TextMeshProUGUI TextNowChapter;
        [SerializeField]
        private Image ImageChapter;

        [Space(10)]
        [SerializeField]
        private Transform ParentStage;
        [SerializeField]
        private TextMeshProUGUI TextStageMobInfo;
        [SerializeField]
        private Transform ParentMobs;
        [SerializeField]
        private TextMeshProUGUI TextStageDropInfo;
        [SerializeField]
        private Transform ParentItem;

        [SerializeField]
        private Button ButtonStart;
        [SerializeField]
        private TextMeshProUGUI TextStart;

        private List<Icon_Stage> Stages = new List<Icon_Stage>();
        private List<Icon_Mob> Mobs = new List<Icon_Mob>();
        private List<Icon_SimpleItem> DropItems = new List<Icon_SimpleItem>();

        private int NowChapter;
        private int MinChapter = int.MaxValue;
        private int MaxChapter = int.MinValue;

        private string SelectedSid = "";

        public void SetSelected(string sid)
        {
            foreach (Icon_Stage stage in Stages)
            {
                if (stage.Sid == SelectedSid)
                {
                    stage.SetSelected(false);
                }
            }

            SelectedSid = sid;
            RefreshStageInfo();
            RefreshButton();
        }

        private void RefreshStageInfo()
        {
            string sid = SelectedSid;

            if (sid == "")
            {
                sid = PlayerDataMgr.Instance.GetCurStage(BattleType.Normal).ToString();
            }

            string[] eids = Table.GetString(TableType.Stage, sid, "eids").Split(';');

            for (int i = 0; i < Mobs.Count; i++)
            {
                Mobs[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < eids.Length; i++)
            {
                if (Mobs.Count > i)
                {
                    Mobs[i].Init(eids[i]);
                    Mobs[i].gameObject.SetActive(true);
                }
                else
                {
                    Icon_Mob mob = Instantiate(ResMob, ParentMobs);
                    mob.Init(eids[i]);
                    Mobs.Add(mob);
                }
            }

            string[] equips = Table.GetString(TableType.Stage, sid, "equip_drop").Split(';');
            int idx = 0;

            for (int i = 0; i < equips.Length; i++, idx++)
            {
                if (DropItems.Count > idx)
                {
                    DropItems[idx].Init(ItemType.Equip, equips[i], 1);
                }
                else
                {
                    Icon_SimpleItem item = Instantiate(ResItem, ParentItem);
                    item.Init(ItemType.Equip, equips[i], 1);
                    DropItems.Add(item);
                }
            }

            var drops = DropCalculator.DropItems(sid);

            foreach (Currency key in drops.Keys)
            {
                if (DropItems.Count > idx)
                {
                    DropItems[idx].Init(ItemType.Currency, key.ToString(), drops[key]);
                }
                else
                {
                    Icon_SimpleItem item = Instantiate(ResItem, ParentItem);
                    item.Init(ItemType.Currency, key.ToString(), drops[key]);
                    DropItems.Add(item);
                }

                idx++;
            }
        }

        private void RefreshButton()
        {
            int selected = int.Parse(SelectedSid);
            int vaildStage = StageCalculator.GetNextStageInt(PlayerDataMgr.Instance.GetMaxStage(BattleType.Normal));

            if (StageCalculator.IsLastChapterStage(selected.ToString()))
            {
                TextStart.text = vaildStage <= selected ? "도전 하기" : "";
                ButtonStart.interactable = vaildStage == selected;
            }
            else
            {
                TextStart.text = vaildStage <= selected ? "도전 하기" : "사냥 하기";
                ButtonStart.interactable = vaildStage >= selected;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            SetMinMaxValue();
            InitText();
            InitCallback();
        }

        private void SetMinMaxValue()
        {
            foreach (string key in Table.GetTable(TableType.Stage).Keys)
            {
                string sChapter = Table.GetString(TableType.Stage, key, "chapter");

                if (sChapter != "x")
                {
                    int chapter = Table.GetInt(TableType.Stage, key, "chapter");
                    MinChapter = Mathf.Min(MinChapter, chapter);
                    MaxChapter = Mathf.Max(MaxChapter, chapter);
                }
            }
        }

        private void InitText()
        {
            TextTitle.text = "스테이지 선택";
            TextStageMobInfo.text = "출현 적들";
            TextStageDropInfo.text = "드랍 정보";
        }

        private void InitCallback()
        {
            ButtonBeforeChapter.onClick.AddListener(() =>
            {
                NowChapter--;

                if (NowChapter <= MinChapter)
                {
                    NowChapter = MinChapter;
                }

                RefreshChapter();
            });

            ButtonAfterChapter.onClick.AddListener(() =>
            {
                NowChapter++;

                if (NowChapter >= MaxChapter)
                {
                    NowChapter = MaxChapter;
                }

                RefreshChapter();
            });

            ButtonStart.onClick.AddListener(() =>
            {
                if (StageCalculator.GetNextStage(PlayerDataMgr.Instance.GetMaxStage(BattleType.Normal)) == SelectedSid)
                {
                    BattleMgr.Instance.ChangeBattleType(BattleType.Challenge, SelectedSid);
                }
                else
                {
                    BattleMgr.Instance.ChangeBattleType(BattleType.Normal, SelectedSid);
                }

                Close();
            });
        }

        private void RefreshChapter()
        {
            int idx = 0;

            foreach (string key in Table.GetTable(TableType.Stage).Keys)
            {
                if (Table.GetString(TableType.Stage, key, "chapter") == NowChapter.ToString())
                {
                    if (Stages.Count > idx)
                    {
                        Stages[idx].Init(key);
                    }
                    else
                    {
                        Icon_Stage stage = Instantiate(ResStage, ParentStage);
                        stage.Init(key);
                        stage.SetParent(this);
                        Stages.Add(stage);
                    }

                    Stages[idx].gameObject.SetActive(true);
                    idx++;
                }
            }

            for (; idx < Stages.Count; idx++)
            {
                Stages[idx].gameObject.SetActive(false);
            }

            Stages.Sort((a, b) =>
            {
                return int.Parse(a.Sid) > int.Parse(b.Sid) ? 1 : -1;
            });

            ButtonBeforeChapter.gameObject.SetActive(NowChapter != MinChapter);
            ButtonAfterChapter.gameObject.SetActive(NowChapter != MaxChapter);
            TextNowChapter.text = string.Format("{0}챕터", NowChapter);
        }

        public override void OpenAction()
        {
            SetNowChapter();
            RefreshChapter();
            RefreshStageInfo();
            RefreshButton();
        }

        private void SetNowChapter()
        {
            int curStage = PlayerDataMgr.Instance.GetCurStage(BattleType.Normal);
            NowChapter = Table.GetInt(TableType.Stage, curStage.ToString(), "chapter");
            SelectedSid = PlayerDataMgr.Instance.GetCurStage(BattleType.Normal).ToString();
        }
    }
}
