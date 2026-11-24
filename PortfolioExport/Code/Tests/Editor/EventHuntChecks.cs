using System;

using UnityEditor;
using UnityEngine;

using Game.Common;
using Game.Core;
using Game.Events;

namespace Game.Tests.Editor
{
    public static class EventHuntChecks
    {
        [MenuItem("Tools/Event Tests/Log Current Hunt State")]
        public static void LogCurrentState()
        {
            if (!Application.isPlaying || EventMgr.Instance == null || !EventMgr.Instance.IsInitialized)
            {
                Debug.LogWarning("Title 씬에서 게임을 실행하고 이벤트 초기화가 끝난 뒤 확인해주세요.");
                return;
            }

            long now = TimeMgr.GetNowTimeNum();
            foreach (IGameEvent gameEvent in EventMgr.Instance.Events)
            {
                if (gameEvent is CurrencyExchangeEvent exchangeEvent)
                {
                    Debug.LogFormat("이벤트: {0}, 상태: {1}, 재화: {2}, 잔액: {3}, 일일 획득: {4}, 전체 획득: {5}",
                        exchangeEvent.Id, exchangeEvent.Schedule.GetPeriod(now), exchangeEvent.CurrencyId,
                        exchangeEvent.GetCurrency(), exchangeEvent.DailyHuntCount, exchangeEvent.TotalHuntCount);
                }
            }
        }

        [MenuItem("Tools/Event Tests/Run Hunt Checks")]
        public static void Run()
        {
            CheckRewardAndLimits();
            CheckPeriodAndReload();
            CheckUnlimitedAndOverflow();
            CheckInvalidSettings();
            Debug.Log("이벤트 사냥 검사 통과: 지급, 일일·전체 한도, 날짜 변경, 재로드, 회차 초기화, 수량 범위, 설정 검사.");
        }

        private static void CheckRewardAndLimits()
        {
            long start = TimeMgr.ParseTableDate("2026.11.17");
            var rule = new EventHuntRule("token", BattleType.Normal, 1f, 3, 5, 8, true);
            var instance = CreateEvent("test", start, rule);
            int notifications = 0;
            instance.ChangedData += () => notifications++;

            Require(instance.TryGetHuntReward(BattleType.Normal, start - 1) == 0, "시작 전에 지급했습니다.");
            Require(instance.TryGetHuntReward(BattleType.Challenge, start) == 0, "다른 전투 종류에서 지급했습니다.");
            Require(instance.TryGetHuntReward(BattleType.Normal, start) == 3, "확률 1의 지급에 실패했습니다.");
            Require(instance.TryGetHuntReward(BattleType.Normal, start) == 2, "일일 한도의 남은 수량만 지급해야 합니다.");
            Require(instance.TryGetHuntReward(BattleType.Normal, start) == 0, "일일 한도를 초과했습니다.");
            Require(instance.GetCurrency() == 5 && instance.DailyHuntCount == 5 && instance.TotalHuntCount == 5,
                "잔액과 사냥 기록이 일치하지 않습니다.");
            Require(instance.CurrencyId == "token" && notifications > 0, "재화 정보 또는 변경 알림이 누락됐습니다.");

            // 잔액을 사용한 상황을 재현해도 사냥 획득 한도는 돌아오지 않아야 한다.
            var spent = instance.GetData();
            spent.CurrencyCount = 0;
            spent.Currencies[0].Count = 0;
            instance.SetData(spent);
            Require(instance.TryGetHuntReward(BattleType.Normal, start) == 0, "잔액 감소로 획득 한도가 복구됐습니다.");

            long nextDay = TimeMgr.AddDays(start, 1);
            Require(instance.TryGetHuntReward(BattleType.Normal, nextDay) == 3, "날짜 변경 후 일일 한도가 갱신되지 않았습니다.");
            Require(instance.GetCurrency() == 3 && instance.DailyHuntCount == 3 && instance.TotalHuntCount == 8,
                "일일 갱신이 전체 획득량 또는 잔액을 잘못 변경했습니다.");
            Require(instance.TryGetHuntReward(BattleType.Normal, TimeMgr.AddDays(nextDay, 1)) == 0,
                "날짜가 바뀌어도 전체 한도는 유지해야 합니다.");

            var zeroRate = CreateEvent("zero", start, new EventHuntRule("token", BattleType.Normal, 0f, 1, -1, -1, false));
            Require(zeroRate.TryGetHuntReward(BattleType.Normal, start) == 0, "확률 0에서 지급했습니다.");
            var zeroLimit = CreateEvent("zero_limit", start, new EventHuntRule("token", BattleType.Normal, 1f, 1, 0, -1, false));
            Require(zeroLimit.TryGetHuntReward(BattleType.Normal, start) == 0, "획득 한도 0에서 지급했습니다.");
        }

        private static void CheckPeriodAndReload()
        {
            long start = TimeMgr.ParseTableDate("2026.11.17");
            var rule = new EventHuntRule("token", BattleType.Normal, 1f, 1, -1, -1, false);
            var instance = CreateEvent("test", start, rule);
            instance.TryGetHuntReward(BattleType.Normal, start);
            var saved = instance.GetData();
            var reloaded = CreateEvent("test", start, rule);
            reloaded.SetData(saved);
            reloaded.Refresh(start);
            Require(reloaded.GetCurrency() == 1 && reloaded.DailyHuntCount == 1 && reloaded.TotalHuntCount == 1,
                "같은 회차 재로드에서 잔액이나 획득 기록이 사라졌습니다.");
            saved.CurrencyCount = 999;
            Require(reloaded.GetCurrency() == 1, "외부 저장 데이터 변경이 내부 잔액을 변경했습니다.");

            Require(reloaded.TryGetHuntReward(BattleType.Normal, reloaded.Schedule.EndTimeNum) == 0,
                "교환만 가능한 기간에 사냥 재화를 지급했습니다.");
            Require(reloaded.GetCurrency() == 1 && reloaded.TotalHuntCount == 1, "획득 종료 시 재화나 누적 기록이 사라졌습니다.");

            long nextStart = TimeMgr.AddDays(start, 30);
            var next = CreateEvent("test", nextStart, rule);
            next.SetData(reloaded.GetData());
            next.Refresh(nextStart - 1);
            Require(next.GetCurrency() == 1, "다음 회차 시작 전에 재화를 초기화했습니다.");
            next.Refresh(nextStart);
            Require(next.GetCurrency() == 0 && next.DailyHuntCount == 0 && next.TotalHuntCount == 0,
                "새 회차에서 잔액과 획득 기록이 초기화되지 않았습니다.");

            var other = CreateEvent("other", start, rule);
            Require(other.GetCurrency() == 0 && instance.GetCurrency() == 1, "이벤트 간 잔액이 섞였습니다.");
        }

        private static void CheckUnlimitedAndOverflow()
        {
            long start = TimeMgr.ParseTableDate("2026.11.17");
            var rule = new EventHuntRule("token", BattleType.Normal, 1f, 2, -1, -1, false);
            var instance = CreateEvent("test", start, rule);
            Require(instance.TryGetHuntReward(BattleType.Normal, start) == 2, "제한 없음 설정에서 지급하지 않았습니다.");
            instance.SetData(new CurrencyExchangeEventData
            {
                EventId = "test", AppliedStartUtcTicks = start, HuntDate = 20261117,
                CurrencyCount = int.MaxValue - 1, DailyHuntCount = int.MaxValue - 1, TotalHuntCount = int.MaxValue - 1
            });
            Require(instance.TryGetHuntReward(BattleType.Normal, start) == 1, "정수 범위 안의 남은 수량만 지급해야 합니다.");
            Require(instance.TryGetHuntReward(BattleType.Normal, start) == 0 && instance.GetCurrency() == int.MaxValue,
                "정수 범위를 초과했습니다.");
        }

        private static void CheckInvalidSettings()
        {
            Expect<ArgumentException>(() => new EventHuntRule("", BattleType.Normal, 1f, 1, -1, -1, false));
            Expect<ArgumentException>(() => new EventHuntRule("token", BattleType.Normal, 1.1f, 1, -1, -1, false));
            Expect<ArgumentException>(() => new EventHuntRule("token", BattleType.Normal, float.NaN, 1, -1, -1, false));
            Expect<ArgumentException>(() => new EventHuntRule("token", BattleType.Normal, 1f, 0, -1, -1, false));
            Expect<ArgumentException>(() => new EventHuntRule("token", BattleType.Normal, 1f, 1, -2, -1, false));
            long start = TimeMgr.ParseTableDate("2026.11.17");
            var instance = CreateEvent("test", start, null);
            Require(instance.TryGetHuntReward(BattleType.Normal, start) == 0, "사냥 설정 없는 이벤트에서 지급했습니다.");
            Expect<ArgumentException>(() => instance.SetData(new CurrencyExchangeEventData { CurrencyCount = -1 }));
        }

        [MenuItem("Tools/Event Tests/Check Hunt JSON Round Trip")]
        public static void CheckSerialization()
        {
            long start = TimeMgr.ParseTableDate("2026.11.17");
            var rule = new EventHuntRule("token", BattleType.Normal, 1f, 3, 5, 8, false);
            var instance = CreateEvent("test", start, rule);
            instance.TryGetHuntReward(BattleType.Normal, start);
            string json = JsonUtility.ToJson(instance.GetData());
            var restored = JsonUtility.FromJson<CurrencyExchangeEventData>(json);
            var reloaded = CreateEvent("test", start, rule);
            reloaded.SetData(restored);
            reloaded.Refresh(start);
            Require(reloaded.GetCurrency() == 3 && reloaded.DailyHuntCount == 3
                && reloaded.TotalHuntCount == 3 && reloaded.GetData().HuntDate == 20261117,
                "JSON 복원 후 재화 또는 획득 기록이 일치하지 않습니다.");
            Require(reloaded.TryGetHuntReward(BattleType.Normal, start) == 2, "JSON 복원 후 남은 한도가 잘못되었습니다.");
            Debug.Log("이벤트 사냥 JSON 왕복 검사 통과.");
        }

        private static CurrencyExchangeEvent CreateEvent(string id, long start, EventHuntRule rule)
        {
            return new CurrencyExchangeEvent(id, id, "", new EventSchedule(start, TimeMgr.AddDays(start, 7), true),
                rule, rule == null ? "" : rule.CurrencyId);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void Expect<T>(Action action) where T : Exception
        {
            try { action(); }
            catch (T) { return; }
            throw new InvalidOperationException("예상한 예외가 발생하지 않았습니다: " + typeof(T).Name);
        }
    }
}
