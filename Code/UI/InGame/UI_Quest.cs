using System;
using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Common;
using Game.Core;
using Game.Data;
using Game.Quests;
using Game.UI.Icon;
using Game.UI.Popup;

namespace Game.UI
{
    public class UI_Quest : SubUI
    {
        [Header("기본 UI")]
        [SerializeField]
        private TextMeshProUGUI TextTitle;
        [SerializeField]
        private Button ButtonReceiveAll;

        [Header("퀘스트 목록")]
        [SerializeField]
        private Icon_Quest ResQuestIcon;
        [SerializeField]
        private Transform QuestParent;

        [Header("보상 재화의 보유량")]
        [SerializeField]
        private Icon_Currency ResCurrency;
        [SerializeField]
        private Transform CurrencyParent;

        private readonly List<Icon_Quest> QuestIcons = new List<Icon_Quest>();
        private readonly List<Icon_Currency> CurrencyIcons = new List<Icon_Currency>();
        private List<QuestRule> QuestRules = new List<QuestRule>();
        private bool NeedsRefresh;

        protected override void Awake()
        {
            base.Awake();

            TextTitle.text = "퀘스트";
            ButtonReceiveAll.interactable = false;
            ButtonReceiveAll.onClick.AddListener(async () =>
            {
                QuestReceiveAllResult result = QuestMgr.Instance.ReceiveAllCommonRewards();
                NeedsRefresh = true;

                if (result.ReceivedQuestCount > 0)
                {
                    var rewardPopup = await PopupMgr.Instance.Get<Popup_GetItems>(PopupType.GetItem);
                    string title = result.FailedQuestCount > 0
                        ? string.Format("퀘스트 보상 획득 · {0}개 수령 실패", result.FailedQuestCount)
                        : "퀘스트 보상 획득";
                    rewardPopup.Init(title);
                    rewardPopup.AddItems(result.Rewards);
                    rewardPopup.Open();
                    return;
                }

                var popup = await PopupMgr.Instance.Get<Popup_Message>(PopupType.Message);
                popup.Init(GetResultText(result));
                popup.Open();
            });
        }

        private void OnEnable()
        {
            QuestMgr.Instance.Changed += OnQuestChanged;
            PlayerDataMgr.ChangedCurrency += OnCurrencyChanged;
            UpgradeDataMgr.ChangedUpgrade += OnUpgradeChanged;

            QuestRules = QuestMgr.Instance.GetRules("common");
            SetQuestIcons();
            SetCurrencyIcons();
            NeedsRefresh = true;
        }

        private void OnDisable()
        {
            if (QuestMgr.Instance != null)
            {
                QuestMgr.Instance.Changed -= OnQuestChanged;
            }

            PlayerDataMgr.ChangedCurrency -= OnCurrencyChanged;
            UpgradeDataMgr.ChangedUpgrade -= OnUpgradeChanged;
        }

        private void OnQuestChanged()
        {
            NeedsRefresh = true;
        }

        private void OnCurrencyChanged(Currency currency)
        {
            NeedsRefresh = true;
        }

        private void OnUpgradeChanged(string type)
        {
            if (type == "exp" || type == "lv")
            {
                NeedsRefresh = true;
            }
        }

        private void Update()
        {
            if (!NeedsRefresh || !QuestMgr.Instance.IsInitialized)
                return;

            // 조회 중 발생하는 변경 알림은 다음 프레임에 다시 처리한다.
            NeedsRefresh = false;
            RefreshQuests();

            foreach (Icon_Currency icon in CurrencyIcons)
            {
                if (icon.gameObject.activeSelf)
                {
                    icon.Refresh();
                }
            }
        }

        private void SetQuestIcons()
        {
            for (int index = 0; index < QuestRules.Count; index++)
            {
                if (index >= QuestIcons.Count)
                {
                    QuestIcons.Add(Instantiate(ResQuestIcon, QuestParent));
                }

                QuestIcons[index].SetQuest(QuestRules[index].Id);
                QuestIcons[index].gameObject.SetActive(true);
            }

            for (int index = QuestRules.Count; index < QuestIcons.Count; index++)
            {
                QuestIcons[index].gameObject.SetActive(false);
            }
        }

        private void SetCurrencyIcons()
        {
            var currencies = new List<Currency>();
            var addedCurrencies = new HashSet<Currency>();

            foreach (QuestRule rule in QuestRules)
            {
                if (!Enum.TryParse(rule.RewardType, out Currency currency)
                    || !Enum.IsDefined(typeof(Currency), currency))
                {
                    throw new ArgumentException("공용 퀘스트의 보상은 일반 재화여야 합니다: " + rule.Id);
                }

                // 완료 여부와 관계없이 공용 퀘스트의 모든 보상 종류를 한 번씩 표시한다.
                if (addedCurrencies.Add(currency))
                {
                    currencies.Add(currency);
                }
            }

            for (int index = 0; index < currencies.Count; index++)
            {
                if (index >= CurrencyIcons.Count)
                {
                    CurrencyIcons.Add(Instantiate(ResCurrency, CurrencyParent));
                }

                CurrencyIcons[index].Init(currencies[index]);
                CurrencyIcons[index].gameObject.SetActive(true);
            }

            for (int index = currencies.Count; index < CurrencyIcons.Count; index++)
            {
                CurrencyIcons[index].gameObject.SetActive(false);
            }
        }

        private void RefreshQuests()
        {
            bool canReceive = false;
            long now = TimeMgr.GetNowTimeNum();

            for (int index = 0; index < QuestRules.Count; index++)
            {
                QuestIcons[index].Refresh();
                QuestState state = QuestMgr.Instance.GetQuestState(QuestRules[index].Id, now);

                if (state != null && state.CanReceive)
                {
                    canReceive = true;
                }
            }

            ButtonReceiveAll.interactable = canReceive;
        }

        private static string GetResultText(QuestReceiveAllResult result)
        {
            if (!result.IsAccepted)
                return "현재 보상을 받을 수 없습니다.";

            if (result.FailedQuestCount > 0)
            {
                return string.Format("퀘스트 {0}개의 보상을 받지 못했습니다. 재화 보유 한도와 수령 조건을 확인해주세요.",
                    result.FailedQuestCount);
            }

            return "수령할 보상이 없습니다.";
        }
    }
}
