using System;
using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Battle;
using Game.Calculate;
using Game.Common;
using Game.Core;
using Game.Data;
using Game.UI.Icon;
using Game.UI.Popup;

namespace Game.UI
{
    public class UI_Dungeon : SubUI
    {
        [SerializeField]
        private Icon_Dungeon ResIcon;
        [SerializeField]
        private Icon_Currency ResCurrency;
        [SerializeField]
        private ToggleTextSet ResToggle;
        [SerializeField]
        private Transform ParentIcons;
        [SerializeField]
        private Transform ParentCurrency;
        [SerializeField]
        private Transform ParentToggle;

        [Space(15)]
        [SerializeField]
        private TextMeshProUGUI TextTitle;

        [Space(15)]
        [SerializeField]
        private Icon_Mob ResEnemy;
        [SerializeField]
        private Icon_SimpleItem ResSimpleItem;
        [SerializeField]
        private TextMeshProUGUI TextEnemyTitle;
        [SerializeField]
        private Transform ParentEnemyIcon;
        [SerializeField]
        private TextMeshProUGUI TextDropTitle;
        [SerializeField]
        private Transform ParentDrops;

        [Space(15)]
        [SerializeField]
        private Button ButtonStart;
        [SerializeField]
        private TextMeshProUGUI TextStart;
        [SerializeField]
        private Image ImageNeed;
        [SerializeField]
        private TextMeshProUGUI TextNeed;

        private List<Icon_Dungeon> Icons = new List<Icon_Dungeon>();
        private List<Icon_Currency> Currecnices = new List<Icon_Currency>();
        private List<ToggleTextSet> Toggles = new List<ToggleTextSet>();

        private List<Icon_Mob> Enemys = new List<Icon_Mob>();
        private List<Icon_SimpleItem> Drops = new List<Icon_SimpleItem>();

        private BattleType NowType = BattleType.GoldDungeon;
        private Dictionary<BattleType, int> Lv = new Dictionary<BattleType, int>();
        private int NeedPlayCount = 1;
        private Dictionary<BattleType, Icon_Dungeon> BeforeClicked = new Dictionary<BattleType, Icon_Dungeon>();

        public void IconClicked(Icon_Dungeon icon, int lv)
        {
            if (BeforeClicked.ContainsKey(NowType))
            {
                if (BeforeClicked[NowType] != null)
                {
                    BeforeClicked[NowType].SetFocus(false);
                }
            }
            else
            {
                BeforeClicked.Add(NowType, icon);
                Lv.Add(NowType, lv);
            }

            BeforeClicked[NowType] = icon;

            if (icon == null)
            {
                return;
            }

            icon.SetFocus(true);
            Lv[NowType] = lv;
            SetDropItems();
            SetButtonText();
        }

        private void SetDropItems()
        {
            var drops = DropCalculator.DungeonClearReward(NowType, Lv[NowType]);
            int idx = 0;

            foreach (var dkey in drops.Keys)
            {
                if (idx < Drops.Count)
                {
                    Drops[idx].Init(ItemType.Currency, dkey.ToString(), drops[dkey]);
                }
                else
                {
                    var item = Instantiate(ResSimpleItem, ParentDrops);
                    item.Init(ItemType.Currency, dkey.ToString(), drops[dkey]);
                    Drops.Add(item);
                }

                Drops[idx].gameObject.SetActive(true);
                idx++;
            }

            for (int i = idx; i < Drops.Count; i++)
            {
                Drops[i].gameObject.SetActive(false);
            }
        }

        private void SetButtonText()
        {
            if (!Lv.ContainsKey(NowType))
            {
                ButtonStart.interactable = false;
                return;
            }

            int maxStage = PlayerDataMgr.Instance.GetMaxStage(NowType) + 1;
            TextStart.text = maxStage <= Lv[NowType] ? "도전하기" : "소탕하기";
            Currency currency = (Currency)Enum.Parse(typeof(Currency), Table.GetString(TableType.Dungeon, NowType.ToString(), "need"));
            ButtonStart.interactable = maxStage >= Lv[NowType] && PlayerDataMgr.Instance.GetCurrency(currency) >= NeedPlayCount;

            if (NowType == BattleType.RogueDungeon)
            {
                ButtonStart.interactable &= CurrencyReward.CanGive(DropCalculator.DungeonClearReward(NowType, Lv[NowType]));
            }
            string need = Table.GetString(TableType.Dungeon, NowType.ToString(), "need");
            ImageNeed.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common, Table.GetString(TableType.Currency, need, "res"));
            TextNeed.text = string.Format("x{0}", NeedPlayCount);
        }

        protected override void Awake()
        {
            base.Awake();
            HashSet<Currency> needs = new HashSet<Currency>();

            foreach (var key in Table.GetTable(TableType.Dungeon).Keys)
            {
                var toggle = Instantiate(ResToggle, ParentToggle);
                toggle.Text.text = Table.GetString(TableType.Dungeon, key, "title");
                toggle.Image.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common, Table.GetString(TableType.Dungeon, key, "toggle_bg"));
                toggle.Toggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        SoundMgr.Instance.PlaySFXSound(SFXType.Ok);
                        SetIcons(key);
                    }
                });
                toggle.gameObject.SetActive(true);
                Toggles.Add(toggle);

                Currency currency = (Currency)Enum.Parse(typeof(Currency), Table.GetString(TableType.Dungeon, key, "need"));

                if (needs.Add(currency))
                {
                    var newCurrency = Instantiate(ResCurrency, ParentCurrency);
                    newCurrency.Init(currency);
                    newCurrency.gameObject.SetActive(true);
                    Currecnices.Add(newCurrency);
                }
            }

            ButtonStart.onClick.AddListener(async () =>
            {
                if (!Lv.ContainsKey(NowType))
                {
                    return;
                }

                Currency currency = (Currency)Enum.Parse(typeof(Currency), Table.GetString(TableType.Dungeon, NowType.ToString(), "need"));

                if (PlayerDataMgr.Instance.GetMaxStage(NowType) + 1 == Lv[NowType])
                {
                    if (PlayerDataMgr.Instance.GetCurrency(currency) >= NeedPlayCount && ButtonStart.interactable)
                    {
                        BattleMgr.Instance.ChangeBattleType(NowType, Lv[NowType].ToString());
                        Close();
                    }
                }
                else if (PlayerDataMgr.Instance.GetMaxStage(NowType) + 1 > Lv[NowType])
                {
                    if (PlayerDataMgr.Instance.GetCurrency(currency) >= NeedPlayCount)
                    {
                        var popup = await PopupMgr.Instance.Get<Popup_WipeOut>(PopupType.WipeOut);
                        popup.Init(NowType);
                        popup.Open();
                    }
                }
            });

            InitText();
        }

        private void InitText()
        {
            TextTitle.text = "던전";
            TextEnemyTitle.text = "출현하는 적 종류";
            TextDropTitle.text = "획득 가능 아이템";
        }

        private void SetIcons(string key)
        {
            ParentIcons.gameObject.SetActive(true);
            ButtonStart.gameObject.SetActive(true);
            BattleType type = (BattleType)Enum.Parse(typeof(BattleType), key);
            NowType = type;
            int maxStage = 0;

            switch (type)
            {
                case BattleType.GoldDungeon:
                    maxStage = Constant.MaxGoldDunegonStage;
                    break;
                case BattleType.RogueDungeon:
                    maxStage = Constant.MaxRoguelikeStage;
                    break;
            }

            for (int i = 0; i < maxStage; i++)
            {
                if (i < Icons.Count)
                {
                    Icons[i].Init(this, type, i + 1);
                }
                else
                {
                    var icon = Instantiate(ResIcon, ParentIcons);
                    icon.Init(this, type, i + 1);
                    Icons.Add(icon);
                }

                Icons[i].gameObject.SetActive(true);
                Icons[i].SetFocus(false);

                if (!BeforeClicked.ContainsKey(type) && PlayerDataMgr.Instance.GetMaxStage(type) == i)
                {
                    IconClicked(Icons[i], i + 1);
                }
            }

            for (int i = maxStage; i < Icons.Count; i++)
            {
                Icons[i].gameObject.SetActive(false);
            }

            // 다른 탭에서 같은 아이콘 객체를 재사용하므로 선택 상태도 다시 연결한다.
            int selectedLevel = Lv.ContainsKey(type) ? Lv[type] : PlayerDataMgr.Instance.GetMaxStage(type) + 1;
            selectedLevel = Mathf.Clamp(selectedLevel, 1, maxStage);
            IconClicked(Icons[selectedLevel - 1], selectedLevel);

            string sid = Table.GetString(TableType.Dungeon, type.ToString(), "sid");
            string[] eids = Table.GetString(TableType.Stage, sid, "eids").Split(';');

            for (int i = 0; i < eids.Length; i++)
            {
                if (i < Enemys.Count)
                {
                    Enemys[i].Init(eids[i]);
                }
                else
                {
                    var enemy = Instantiate(ResEnemy, ParentEnemyIcon);
                    enemy.Init(eids[i]);
                    Enemys.Add(enemy);
                }

                Enemys[i].gameObject.SetActive(true);
            }

            for (int i = eids.Length; i < Enemys.Count; i++)
            {
                Enemys[i].gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            foreach (var cur in Currecnices)
            {
                cur.Refresh();
            }

            ParentIcons.gameObject.SetActive(false);
            ButtonStart.gameObject.SetActive(false);

            foreach (var icon in Icons)
            {
                icon.SetFocus(false);
            }

            BeforeClicked.Clear();
            Lv.Clear();

            foreach (var toggle in Toggles)
            {
                if (toggle.Toggle.group != null)
                {
                    toggle.Toggle.group.SetAllTogglesOff(false);
                }
                else
                {
                    toggle.Toggle.SetIsOnWithoutNotify(false);
                }
            }

            PlayerDataMgr.ChangedCurrency += ChangedCurrency;
        }

        private void OnDisable()
        {
            PlayerDataMgr.ChangedCurrency -= ChangedCurrency;
        }

        private void ChangedCurrency(Currency changedCur)
        {
            Currency currency = (Currency)Enum.Parse(typeof(Currency), Table.GetString(TableType.Dungeon, NowType.ToString(), "need"));

            if (changedCur == currency)
            {
                SetButtonText();
            }

            foreach (var cur in Currecnices)
            {
                cur.Refresh();
            }
        }
    }

}
