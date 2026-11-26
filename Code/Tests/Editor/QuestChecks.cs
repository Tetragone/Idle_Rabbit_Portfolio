using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using Game.Common;
using Game.Core;
using Game.Events;
using Game.Quests;

namespace Game.Tests.Editor
{
    public static class QuestChecks
    {
        [MenuItem("Tools/Event Tests/Run Quest Checks")]
        public static void Run()
        {
            CheckRepeat();
            CheckReset();
            CheckClaims();
            CheckReceiveStateRelease();
            CheckEvent();
            CheckCopyAndOverflow();
            CheckDisplayState();
            Debug.Log("퀘스트 검사 통과: 누적 반복, 일일·주간·영구, 수령 검증, 이벤트 기간·회차, 데이터 복사.");
        }

        private static QuestRule Rule(string id, QuestType type, QuestAction action = QuestAction.KillMonster)
        {
            return new QuestRule(id, "common", type, action, 100, "Gem", 10);
        }

        private static void CheckRepeat()
        {
            long now = TimeMgr.ParseTableDate("2026.11.17");
            var module = new QuestModule(new[] { Rule("repeat", QuestType.Repeat) }, new List<QuestProgressData>());
            module.ReportAction(QuestAction.KillMonster, 350, now);
            Require(module.GetAvailableRewardCount("repeat", now) == 3, "350회 달성 시 3회 수령할 수 있어야 합니다.");
            long rewards = 0;
            Require(module.TryReceiveReward("repeat", 2, now, (rule, count) => { rewards += count * rule.RewardCount; return true; }),
                "반복 퀘스트 부분 수령에 실패했습니다.");
            Require(module.GetAvailableRewardCount("repeat", now) == 1, "부분 수령 후 남은 수령 횟수가 잘못됐습니다.");
            module.TryReceiveReward("repeat", 1, now, (rule, count) => { rewards += count * rule.RewardCount; return true; });
            QuestProgressData data = module.GetProgress("repeat", now);
            Require(data.TotalProgress == 350 && data.ClaimedCount == 3 && data.TotalProgress % 100 == 50 && rewards == 30,
                "수령 시 누적 진행량이나 나머지를 잃었습니다.");
            module.ReportAction(QuestAction.KillMonster, 50, now);
            Require(module.GetAvailableRewardCount("repeat", now) == 1, "나머지 50회에 추가 50회가 합산되지 않았습니다.");
        }

        private static void CheckReset()
        {
            long sunday = TimeMgr.ParseTableDate("2026.11.22");
            long monday = TimeMgr.AddDays(sunday, 1);
            var rules = new[] { Rule("daily", QuestType.Daily), Rule("weekly", QuestType.Weekly), Rule("none", QuestType.None),
                Rule("stat", QuestType.Daily, QuestAction.StatUpgrade) };
            var module = new QuestModule(rules, new List<QuestProgressData>());
            module.ReportAction(QuestAction.KillMonster, 100, sunday);
            Require(module.GetProgress("stat", sunday).TotalProgress == 0, "다른 행동의 퀘스트를 집계했습니다.");
            Require(module.TryReceiveReward("daily", 1, sunday, (rule, count) => true), "일일 수령에 실패했습니다.");
            Require(!module.TryReceiveReward("daily", 1, sunday, (rule, count) => true), "일일 보상을 중복 수령했습니다.");
            module.Refresh(monday);
            Require(module.GetProgress("daily", monday).TotalProgress == 0 && module.GetProgress("daily", monday).ClaimedCount == 0,
                "날짜 변경 시 일일 기록을 초기화하지 않았습니다.");
            Require(module.GetProgress("weekly", monday).TotalProgress == 0, "월요일에 주간 기록을 초기화하지 않았습니다.");
            Require(module.GetProgress("none", monday).TotalProgress == 100, "영구 퀘스트 진행도를 초기화했습니다.");
            module.ReportAction(QuestAction.KillMonster, 20, monday);
            module.Refresh(TimeMgr.AddDays(monday, 1));
            Require(module.GetProgress("weekly", TimeMgr.AddDays(monday, 1)).TotalProgress == 20,
                "같은 주에 주간 진행도를 초기화했습니다.");
            Require(module.GetProgress("none", monday).TotalProgress == 100, "일회성 퀘스트를 목표 이상 집계했습니다.");
        }

        private static void CheckClaims()
        {
            long now = TimeMgr.ParseTableDate("2026.11.17");
            var module = new QuestModule(new[] { Rule("repeat", QuestType.Repeat) }, new List<QuestProgressData>());
            module.ReportAction(QuestAction.KillMonster, 100, now);
            Require(!module.TryReceiveReward("repeat", 2, now, (rule, count) => true), "달성 횟수보다 많이 수령했습니다.");
            Require(!module.TryReceiveReward("repeat", 1, now, (rule, count) => false), "지급 실패를 수령 성공으로 처리했습니다.");
            Require(module.GetAvailableRewardCount("repeat", now) == 1, "지급 거절 시 수령 기록을 복구하지 않았습니다.");
            bool nested = true;
            Require(module.TryReceiveReward("repeat", 1, now, (rule, count) =>
            {
                nested = module.TryReceiveReward("repeat", 1, now, (other, times) => true);
                return true;
            }), "정상 수령에 실패했습니다.");
            Require(!nested && module.GetAvailableRewardCount("repeat", now) == 0, "보상 지급 중 재진입으로 중복 수령했습니다.");
        }

        private static void CheckReceiveStateRelease()
        {
            long now = TimeMgr.ParseTableDate("2026.11.17");
            var module = new QuestModule(new[] { Rule("repeat", QuestType.Repeat) }, new List<QuestProgressData>());
            module.ReportAction(QuestAction.KillMonster, 500, now);

            Require(module.TryReceiveReward("repeat", 1, now, (rule, count) => true), "첫 수령에 실패했습니다.");
            Require(module.TryReceiveReward("repeat", 1, now, (rule, count) => true), "성공 후 수령 잠금이 해제되지 않았습니다.");
            Require(!module.TryReceiveReward("repeat", 1, now, (rule, count) => false), "지급 실패를 성공으로 처리했습니다.");
            Require(module.TryReceiveReward("repeat", 1, now, (rule, count) => true), "실패 후 수령 잠금이 해제되지 않았습니다.");

            bool threw = false;
            try
            {
                module.TryReceiveReward("repeat", 1, now, (rule, count) =>
                {
                    throw new InvalidOperationException("테스트용 지급 예외");
                });
            }
            catch (InvalidOperationException)
            {
                threw = true;
            }
            Require(threw, "지급 함수의 예외가 호출부로 전달되지 않았습니다.");
            // 이 검사는 잠금 해제만 확인하며 예외 발생 전의 부분 지급 복구를 가정하지 않는다.
            Require(module.TryReceiveReward("repeat", 1, now, (rule, count) => true), "예외 후 수령 잠금이 해제되지 않았습니다.");
        }

        private static void CheckEvent()
        {
            long start = TimeMgr.ParseTableDate("2026.11.17");
            var rule = new QuestRule("event", "test", QuestType.Repeat, QuestAction.KillMonster, 100, "token", 10);
            var schedule = new EventSchedule(start, TimeMgr.AddDays(start, 7), true);
            // 사냥 설정 없이도 이벤트 재화와 퀘스트를 사용할 수 있어야 한다.
            var instance = new CurrencyExchangeEvent("test", "테스트", "", schedule, null, "token", new[] { rule });
            Require(!instance.ReportQuestAction(QuestAction.KillMonster, 100, start - 1), "시작 전 이벤트를 집계했습니다.");
            instance.ReportQuestAction(QuestAction.KillMonster, 350, start);
            Require(instance.Quests.GetAvailableRewardCount("event", start) == 3, "이벤트 반복 퀘스트 집계에 실패했습니다.");
            Require(instance.Quests.TryReceiveReward("event", 3, start,
                (definition, count) => instance.TryAddQuestReward(definition.RewardType, (int)count * definition.RewardCount, start)),
                "사냥 없는 이벤트의 재화 보상 지급에 실패했습니다.");
            Require(instance.GetCurrency() == 30 && instance.DailyHuntCount == 0 && instance.TotalHuntCount == 0,
                "퀘스트 보상이 사냥 한도를 소비했습니다.");
            Require(!instance.ReportQuestAction(QuestAction.KillMonster, 100, schedule.EndTimeNum)
                && !instance.TryAddQuestReward("token", 10, schedule.EndTimeNum), "획득 종료 후 집계하거나 보상 지급했습니다.");
            var saved = instance.GetData();
            var restored = new CurrencyExchangeEvent("test", "테스트", "", schedule, null, "token", new[] { rule });
            restored.SetData(saved);
            restored.Refresh(start);
            Require(restored.Quests.GetProgress("event", start).ClaimedCount == 3 && restored.GetCurrency() == 30,
                "같은 회차 재적용에서 진행도나 수령 기록이 사라졌습니다.");
            saved.Quests[0].TotalProgress = 999;
            Require(restored.Quests.GetProgress("event", start).TotalProgress == 350, "이벤트 퀘스트 목록을 깊게 복사하지 않았습니다.");
            long nextStart = TimeMgr.AddDays(start, 30);
            var next = new CurrencyExchangeEvent("test", "테스트", "",
                new EventSchedule(nextStart, TimeMgr.AddDays(nextStart, 7), true), null, "token", new[] { rule });
            next.SetData(restored.GetData());
            next.Refresh(nextStart);
            Require(next.GetCurrency() == 0 && next.Quests.GetProgress("event", nextStart).TotalProgress == 0,
                "새 회차의 퀘스트와 잔액이 초기화되지 않았습니다.");
        }

        private static void CheckCopyAndOverflow()
        {
            long now = TimeMgr.ParseTableDate("2026.11.17");
            var state = new QuestData();
            state.Progress.Add(new QuestProgressData { QuestId = "repeat", TotalProgress = long.MaxValue - 1 });
            var module = new QuestModule(new[] { Rule("repeat", QuestType.Repeat) }, state.Progress);
            module.ReportAction(QuestAction.KillMonster, 100, now);
            Require(module.GetProgress("repeat", now).TotalProgress == long.MaxValue, "누적 진행량이 정수 범위를 넘었습니다.");
            var copy = new QuestData();
            copy.SetData(state);
            copy.Progress[0].TotalProgress = 0;
            Require(state.Progress[0].TotalProgress == long.MaxValue, "상시 퀘스트 저장 목록을 깊게 복사하지 않았습니다.");
        }

        [MenuItem("Tools/Event Tests/Check Quest JSON Round Trip")]
        public static void CheckSerialization()
        {
            var data = new QuestData();
            data.Progress.Add(new QuestProgressData { QuestId = "repeat", TotalProgress = 350, ClaimedCount = 2 });
            var restored = JsonUtility.FromJson<QuestData>(JsonUtility.ToJson(data));
            long now = TimeMgr.ParseTableDate("2026.11.17");
            var module = new QuestModule(new[] { Rule("repeat", QuestType.Repeat) }, restored.Progress);
            Require(module.GetAvailableRewardCount("repeat", now) == 1 && module.GetProgress("repeat", now).TotalProgress == 350,
                "누적 진행량과 수령 횟수의 JSON 복원에 실패했습니다.");
            var eventData = new CurrencyExchangeEventData { CurrencyCount = 30 };
            eventData.Quests.Add(data.Progress[0].Copy());
            var eventRestored = JsonUtility.FromJson<CurrencyExchangeEventData>(JsonUtility.ToJson(eventData));
            Require(eventRestored.CurrencyCount == 30 && eventRestored.Quests[0].ClaimedCount == 2,
                "이벤트 재화와 퀘스트 목록의 JSON 복원에 실패했습니다.");
            var oldData = JsonUtility.FromJson<CurrencyExchangeEventData>("{\"EventId\":\"test\",\"CurrencyCount\":5}");
            var upgraded = new CurrencyExchangeEventData();
            upgraded.SetData(oldData);
            Require(upgraded.CurrencyCount == 5 && upgraded.Quests != null, "이전 저장 데이터에서 퀘스트 목록을 추가하지 못했습니다.");
            Debug.Log("퀘스트 JSON 왕복 검사 통과.");
        }

        [MenuItem("Tools/Event Tests/Log Current Quest State")]
        public static void LogCurrentState()
        {
            if (!Application.isPlaying || !QuestMgr.Instance.IsInitialized)
            {
                Debug.LogWarning("Title 씬에서 게임을 실행한 뒤 확인해주세요.");
                return;
            }
            LogCategory("common");
            if (EventMgr.Instance != null)
            {
                foreach (IGameEvent instance in EventMgr.Instance.Events)
                {
                    LogCategory(instance.Id);
                }
            }
        }

        private static void LogCategory(string category)
        {
            foreach (QuestRule rule in QuestMgr.Instance.GetRules(category))
            {
                QuestProgressData progress = QuestMgr.Instance.GetProgress(rule.Id);
                if (progress == null) continue;
                Debug.LogFormat("퀘스트: {0}, 소속: {1}, 누적: {2}, 목표: {3}, 수령: {4}, 추가 수령 가능: {5}",
                    rule.Id, category, progress.TotalProgress, rule.Goal, progress.ClaimedCount,
                    QuestMgr.Instance.GetAvailableRewardCount(rule.Id));
            }
        }

        [MenuItem("Tools/Event Tests/Claim First Available Quest Reward")]
        public static void ClaimFirstReward()
        {
            if (!Application.isPlaying || !QuestMgr.Instance.IsInitialized)
            {
                Debug.LogWarning("Title 씬에서 게임을 실행한 뒤 수령해주세요.");
                return;
            }
            var rules = QuestMgr.Instance.GetRules("common");
            if (EventMgr.Instance != null)
            {
                foreach (IGameEvent instance in EventMgr.Instance.Events)
                {
                    rules.AddRange(QuestMgr.Instance.GetRules(instance.Id));
                }
            }
            foreach (QuestRule rule in rules)
            {
                if (QuestMgr.Instance.GetAvailableRewardCount(rule.Id) <= 0) continue;
                Debug.LogFormat("퀘스트 '{0}' 1회 보상 수령 결과: {1}", rule.Id, QuestMgr.Instance.TryReceiveReward(rule.Id));
                return;
            }
            Debug.Log("수령할 수 있는 퀘스트가 없습니다.");
        }

        private static void CheckDisplayState()
        {
            var rule = new QuestRule("display", "common", QuestType.Repeat, QuestAction.KillMonster,
                100, "Gem", 10, "몬스터 처치", "몬스터 처치 {0} / {1}", "Gem");
            var progress = new QuestProgressData { TotalProgress = 350, ClaimedCount = 1 };
            var state = new QuestState(rule, progress, 2, true, false);

            Require(state.CurrentCount == 50 && state.AvailableRewardCount == 2 && state.CanReceive
                && rule.FormatDescription(state.CurrentCount) == "몬스터 처치 50 / 100",
                "반복 퀘스트의 설명과 수령 가능 횟수를 구분하지 못했습니다.");

            state = new QuestState(rule, progress, 0, false, false);
            Require(state.CurrentCount == 50 && !state.CanUse && !state.CanReceive,
                "이용 불가 상태에서 진행량이 사라지거나 수령이 허용됐습니다.");

            var once = new QuestState(Rule("once", QuestType.None),
                new QuestProgressData { TotalProgress = 100, ClaimedCount = 1 }, 0, true, false);
            Require(once.CurrentCount == 100 && once.IsCompleted && !once.CanReceive,
                "일회성 퀘스트의 수령 완료 상태가 잘못되었습니다.");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
