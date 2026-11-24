using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Common;
using Game.Core;
using Game.Data;
using Game.Events;
using Game.Quests;
using Game.UI.Icon;

namespace Game.UI
{
    public class UI_EventHuntQuest : UI_EventPage
    {
        [Header("퀘스트")]
        [SerializeField]
        private Icon_Quest ResQuestIcon;
        [SerializeField]
        private Transform QuestParent;

        [Header("사냥: 필요한 항목만 연결")]
        [SerializeField]
        private Icon_SimpleItem HuntItem;
        [SerializeField]
        private TextMeshProUGUI TextHuntCount;

        [Header("상점")]
        [SerializeField]
        private Button ButtonShop;

        private readonly List<Icon_Quest> QuestIcons = new List<Icon_Quest>();

        protected override void Awake()
        {
            base.Awake();

            if (ButtonShop != null)
            {
                ButtonShop.onClick.AddListener(RequestShop);
            }
        }

        protected override void SetEventPage()
        {
            List<QuestRule> rules = QuestMgr.Instance.GetRules(EventId);

            for (int index = 0; index < rules.Count; index++)
            {
                if (index >= QuestIcons.Count)
                {
                    QuestIcons.Add(Instantiate(ResQuestIcon, QuestParent));
                }

                QuestIcons[index].SetQuest(rules[index].Id);
                QuestIcons[index].gameObject.SetActive(true);
            }

            for (int index = rules.Count; index < QuestIcons.Count; index++)
            {
                QuestIcons[index].gameObject.SetActive(false);
            }
        }

        protected override void RefreshEventPage()
        {
            EventHuntInfo hunt = EventMgr.Instance.GetHuntInfo(EventId, TimeMgr.GetNowTimeNum());

            if (HuntItem != null)
            {
                HuntItem.Init(ItemType.EventCurrency, hunt.CurrencyId, 0);
                TextHuntCount.text = hunt == null ? "" : FormatHuntCount(hunt);
            }

            if (ButtonShop != null)
            {
                ButtonShop.gameObject.SetActive(EventMgr.Instance.GetShopItems(EventId).Count > 0);
                ButtonShop.interactable = EventMgr.Instance.IsShopOpen(EventId, TimeMgr.GetNowTimeNum());
            }
        }

        private static string FormatHuntCount(EventHuntInfo hunt)
        {
            return hunt.DailyLimit < 0
                ? hunt.DailyCount.ToString()
                : string.Format("{0} / {1}", hunt.DailyCount, hunt.DailyLimit);
        }
    }
}
