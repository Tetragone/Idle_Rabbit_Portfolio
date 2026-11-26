using System;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Common;
using Game.Core;
using Game.Quests;
using Game.UI.Popup;

namespace Game.UI.Icon
{
    public class Icon_Quest : MonoBehaviour
    {
        [Header("표시할 항목만 연결")]
        [SerializeField]
        private TextMeshProUGUI TextName;
        [SerializeField]
        private TextMeshProUGUI TextDescription;
        [SerializeField]
        private Icon_SimpleItem Reward;
        [SerializeField]
        private TextMeshProUGUI TextAvailableCount;
        [SerializeField]
        private TextMeshProUGUI TextState;
        [SerializeField]
        [Tooltip("목표를 달성하고 미수령 보상이 있을 때 표시합니다.")]
        private GameObject ObjComplete;
        [SerializeField]
        [Tooltip("퀘스트 보상을 수령 완료했을 때 표시합니다. 반복 퀘스트는 완료 상태가 되지 않습니다.")]
        private GameObject ObjReceived;
        [SerializeField]
        private UI_Gauge Progress;
        [SerializeField]
        [Tooltip("아이콘 전체를 누르려면 아이콘 영역의 Button을 연결합니다. 비워두면 같은 오브젝트의 Button을 사용합니다.")]
        private Button ButtonReceive;

        public string QuestId { get; private set; } = "";

        private bool IsRefreshing;
        private float NextRefreshTime;

        private void Awake()
        {
            if (ObjComplete != null)
            {
                ObjComplete.SetActive(false);
            }

            if (ObjReceived != null)
            {
                ObjReceived.SetActive(false);
            }

            if (ButtonReceive == null)
            {
                ButtonReceive = GetComponent<Button>();
            }

            if (ButtonReceive != null)
            {
                ButtonReceive.onClick.AddListener(async () =>
                {
                    // 재사용된 아이콘에서도 클릭한 시점에 표시 중인 퀘스트를 수령한다.
                    QuestRule rule = QuestMgr.Instance.GetQuestInfo(QuestId);
                    bool received = QuestMgr.Instance.TryReceiveReward(QuestId);
                    Refresh();

                    if (received)
                    {
                        bool normalCurrency = Enum.TryParse(rule.RewardType, out Currency currency)
                            && Enum.IsDefined(typeof(Currency), currency);
                        var rewardPopup = await PopupMgr.Instance.Get<Popup_GetItems>(PopupType.GetItem);
                        rewardPopup.Init("퀘스트 보상 획득");
                        rewardPopup.AddItem(normalCurrency ? ItemType.Currency : ItemType.EventCurrency,
                            rule.RewardType, rule.RewardCount);
                        rewardPopup.Open();
                        return;
                    }

                    var popup = await PopupMgr.Instance.Get<Popup_Message>(PopupType.Message);
                    popup.Init("재화 보유 한도와 수령 조건을 확인해주세요.");
                    popup.Open();
                });
            }
        }

        public void SetQuest(string questId)
        {
            if (QuestMgr.Instance == null || QuestMgr.Instance.GetQuestInfo(questId) == null)
                throw new ArgumentException("표시할 퀘스트가 없습니다.", nameof(questId));

            QuestId = questId;
            Refresh();
        }

        private void OnEnable()
        {
            if (QuestMgr.Instance != null)
            {
                QuestMgr.Instance.Changed += OnQuestChanged;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (QuestMgr.Instance != null)
            {
                QuestMgr.Instance.Changed -= OnQuestChanged;
            }
        }

        private void OnQuestChanged()
        {
            Refresh();
        }

        private void Update()
        {
            if (Time.unscaledTime < NextRefreshTime)
                return;

            NextRefreshTime = Time.unscaledTime + 1f;
            Refresh();
        }

        public void Refresh()
        {
            if (IsRefreshing || string.IsNullOrEmpty(QuestId) || QuestMgr.Instance == null)
                return;

            IsRefreshing = true;

            try
            {
                QuestRule rule = QuestMgr.Instance.GetQuestInfo(QuestId);
                QuestState state = QuestMgr.Instance.GetQuestState(QuestId);

                if (rule == null || state == null)
                {
                    if (ObjComplete != null)
                    {
                        ObjComplete.SetActive(false);
                    }

                    if (ObjReceived != null)
                    {
                        ObjReceived.SetActive(false);
                    }

                    if (ButtonReceive != null)
                    {
                        ButtonReceive.interactable = false;
                    }

                    return;
                }

                SetInfo(rule, state);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void SetInfo(QuestRule rule, QuestState state)
        {
            if (TextName != null)
            {
                TextName.text = rule.Name;
            }

            if (TextDescription != null)
            {
                TextDescription.text = rule.FormatDescription(state.CurrentCount);
            }

            if (Reward != null)
            {
                if (rule.Category == "common")
                {
                    Reward.Init(ItemType.Currency, rule.RewardType, rule.RewardCount);
                }
                else
                {
                    Reward.Init(ItemType.EventCurrency, rule.RewardType, rule.RewardCount);
                }
            }

            if (TextAvailableCount != null)
            {
                TextAvailableCount.text = rule.Type == QuestType.Repeat && state.AvailableRewardCount > 0
                    ? string.Format("수령 가능 {0}회", state.AvailableRewardCount)
                    : "";
            }

            if (TextState != null)
            {
                TextState.text = GetStateText(state);
            }

            if (ObjComplete != null)
            {
                ObjComplete.SetActive(state.AvailableRewardCount > 0);
            }

            if (ObjReceived != null)
            {
                ObjReceived.SetActive(state.IsCompleted);
            }

            if (Progress != null)
            {
                Progress.SetGauge((double)state.CurrentCount, (double)rule.Goal);
            }

            if (ButtonReceive != null)
            {
                ButtonReceive.interactable = state.CanReceive;
            }
        }

        private string GetStateText(QuestState state)
        {
            if (!state.CanUse)
                return "현재 이용 불가";

            if (state.IsCompleted)
                return "수령 완료";

            return state.AvailableRewardCount > 0 ? "수령 가능" : "진행 중";
        }
    }
}
