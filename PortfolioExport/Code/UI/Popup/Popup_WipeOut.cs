using System;
using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Calculate;
using Game.Common;
using Game.Core;
using Game.Data;
using Game.UI.Icon;
using Game.Quests;

namespace Game.UI.Popup
{
    public class Popup_WipeOut : BasePopup
    {
        [Space(15)]
        [SerializeField]
        private Icon_SimpleItem ResRewardItem;
        [SerializeField]
        private Transform RewardContent;

        [Header("Use Ticket")]
        [SerializeField]
        private Icon_SimpleItem Ticket;
        [SerializeField]
        private Button ButtonUp;
        [SerializeField]
        private Button ButtonDown;

        [Space(15)]
        [SerializeField]
        private Image ImageTicket;
        [SerializeField]
        private TextMeshProUGUI TextTicketCount;

        private List<Icon_SimpleItem> Rewards = new List<Icon_SimpleItem>();
        private BattleType NowType = BattleType.Normal;
        private Currency UseTicket = Currency.Gold;
        private int NowUseTictekCount = 1;
        private int NowLv = 0;

        public void Init(BattleType type)
        {
            NowType = type;
            NowLv = PlayerDataMgr.Instance.GetMaxStage(type);
            string ticket = Table.GetString(TableType.Dungeon, type.ToString(), "need");
            UseTicket = (Currency)Enum.Parse(typeof(Currency), ticket);
            TextTitle.text = string.Format("{0} {1}단계 소탕", Table.GetString(TableType.Dungeon, type.ToString(), "title"), NowLv);
            NowUseTictekCount = PlayerDataMgr.Instance.GetCurrency(UseTicket);
            ImageTicket.sprite = AtlasMgr.Instance.GetSprite(AtlasType.Common, Table.GetString(TableType.Currency, ticket, "res"));
            Refresh();
        }

        protected override void Awake()
        {
            base.Awake();
            InitCallback();
            InitText();
        }

        private void InitCallback()
        {
            ButtonUp.onClick.AddListener(() =>
            {
                NowUseTictekCount++;

                if (NowUseTictekCount >= PlayerDataMgr.Instance.GetCurrency(UseTicket))
                {
                    NowUseTictekCount = PlayerDataMgr.Instance.GetCurrency(UseTicket);
                }

                Refresh();
            });

            ButtonDown.onClick.AddListener(() =>
            {
                NowUseTictekCount--;

                if (NowUseTictekCount <= 1)
                {
                    NowUseTictekCount = 1;
                }

                Refresh();
            });

            ButtonYes.onClick.AddListener(async () =>
            {
                if (NowUseTictekCount <= PlayerDataMgr.Instance.GetCurrency(UseTicket))
                {
                    var reward = DropCalculator.DungeonClearReward(NowType, NowLv);
                    var popup = await PopupMgr.Instance.Get<Popup_GetItems>(PopupType.GetItem);
                    Dictionary<Currency, int> calRewards = new Dictionary<Currency, int>();

                    foreach (var key in reward.Keys)
                    {
                        calRewards.Add(key, reward[key] * NowUseTictekCount);
                    }

                    popup.Init(calRewards, "소탕 보상");
                    popup.Open();

                    foreach (var key in calRewards.Keys)
                    {
                        PlayerDataMgr.Instance.TryAddCurrency(key, calRewards[key]);
                    }
                    PlayerDataMgr.Instance.UseCurrency(UseTicket, NowUseTictekCount);
                    QuestMgr.Instance.ReportAction(QuestAction.ClearDungeon, NowUseTictekCount);
                }
                else
                {
                    var popup = await PopupMgr.Instance.Get<Popup_Message>(PopupType.Message);
                    popup.Init("재화가 부족합니다.");
                }

                Close();
            });
        }

        private void InitText()
        {
            TextYes.text = "소탕";
            TextNo.text = "취소";
        }

        private void Refresh()
        {
            Ticket.Init(ItemType.Currency, UseTicket.ToString(), NowUseTictekCount);
            TextTicketCount.text = NowUseTictekCount.ToString();

            ButtonDown.interactable = true;
            ButtonUp.interactable = true;

            if (NowUseTictekCount <= 1)
            {
                ButtonDown.interactable = false;
            }

            if (NowUseTictekCount >= PlayerDataMgr.Instance.GetCurrency(UseTicket))
            {
                ButtonUp.interactable = false;
            }

            var reward = DropCalculator.DungeonClearReward(NowType, NowLv);
            int idx = 0;

            foreach (var key in reward.Keys)
            {
                if (idx >= Rewards.Count)
                {
                    Icon_SimpleItem nowItem = Instantiate(ResRewardItem, RewardContent);
                    Rewards.Add(nowItem);
                }

                Rewards[idx].Init(ItemType.Currency, key.ToString(), reward[key] * NowUseTictekCount);
                Rewards[idx].gameObject.SetActive(true);
                idx++;
            }

            for (; idx < Rewards.Count; idx++)
            {
                Rewards[idx].gameObject.SetActive(false);
            }
        }
    }
}
