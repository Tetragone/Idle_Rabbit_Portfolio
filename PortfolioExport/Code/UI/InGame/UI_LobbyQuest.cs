using System;

using UnityEngine;

using Game.Common;
using Game.Core;
using Game.Quests;
using Game.UI.Icon;

namespace Game.UI
{
    public class UI_LobbyQuest : MonoBehaviour
    {
        [SerializeField]
        private Icon_Quest QuestIcon;

        private bool NeedsRefresh;

        private void Awake()
        {
            if (QuestIcon == null)
                throw new ArgumentException("로비에 배치한 퀘스트 아이콘을 연결해주세요.");

            if (QuestIcon.gameObject == gameObject || transform.IsChildOf(QuestIcon.transform))
                throw new ArgumentException("로비 퀘스트 컨트롤러는 비활성화할 아이콘 바깥에 배치해주세요.");

            QuestIcon.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            QuestIcon.gameObject.SetActive(false);
            NeedsRefresh = true;
            QuestMgr.Instance.Changed += OnQuestChanged;
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
            // 알림 중 바로 조회하지 않고 같은 프레임의 변경을 한 번에 반영한다.
            NeedsRefresh = true;
        }

        private void Update()
        {
            if (!NeedsRefresh || !QuestMgr.Instance.IsInitialized)
                return;

            // 조회 중 추가 알림이 오면 다음 프레임의 갱신 예약을 유지한다.
            NeedsRefresh = false;
            RefreshIcon();
        }

        private void RefreshIcon()
        {
            string questId = FindAvailableQuestId();

            if (string.IsNullOrEmpty(questId))
            {
                QuestIcon.gameObject.SetActive(false);
                return;
            }

            if (QuestIcon.QuestId != questId)
            {
                QuestIcon.SetQuest(questId);
            }
            else
            {
                QuestIcon.Refresh();
            }

            QuestIcon.gameObject.SetActive(true);
        }

        private string FindAvailableQuestId()
        {
            long now = TimeMgr.GetNowTimeNum();

            foreach (QuestRule rule in QuestMgr.Instance.GetRules("common"))
            {
                if (rule.Type != QuestType.Daily && rule.Type != QuestType.Weekly)
                    continue;

                QuestState state = QuestMgr.Instance.GetQuestState(rule.Id, now);

                if (state == null || !state.CanUse || state.AvailableRewardCount <= 0)
                    continue;

                // 여러 퀘스트가 완료돼 있어도 첫 항목 하나만 표시한다.
                return rule.Id;
            }

            return "";
        }
    }
}
