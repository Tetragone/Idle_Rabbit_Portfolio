using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using Game.Common;
using Game.Core;
using Game.Events;

namespace Game.Tests.Editor
{
    public static class EventOfflineChecks
    {
        [MenuItem("Tools/Event Tests/Run Offline Checks")]
        public static void Run()
        {
            CheckRewardPeriod();
            CheckEventPeriod();
            CheckDailyLimits();
            CheckTotalAndIntegerLimits();
            CheckRemainder();
            CheckReloadAndDuplicate();
            CheckDisabledAndNewOccurrence();
            CheckEmptyPeriodAndClock();
            CheckCompletionWithoutReward();
            CheckDataAndNotifications();

            Debug.Log("이벤트 오프라인 검사 통과: 시간 구간, 날짜별 한도, 소수점, 재로드, 중복 요청, 회차, 데이터 복사.");
        }

        private static long Start => TimeMgr.ParseTableDate("2026.11.17");

        private static CurrencyExchangeEvent Create(float rate = 1f, int daily = -1, int total = -1,
            bool offline = true, BattleType battleType = BattleType.Normal)
        {
            var rule = new EventHuntRule("token", battleType, rate, 1, daily, total, offline);

            return new CurrencyExchangeEvent("test", "검사", "",
                new EventSchedule(Start, TimeMgr.AddDays(Start, 7), true), rule, "token");
        }

        private static void CheckRewardPeriod()
        {
            Require(!TimeMgr.TryGetIdleRewardPeriod(0, Start, out _, out _), "최초 접속에서 방치 구간을 만들었습니다.");
            Require(!TimeMgr.TryGetIdleRewardPeriod(Start, Start - 1, out _, out _), "시간 역행에서 정산했습니다.");
            Require(!TimeMgr.TryGetIdleRewardPeriod(Start, Start + 600, out _, out _), "10분 이하에서 지급했습니다.");
            Require(TimeMgr.TryGetIdleRewardPeriod(Start, Start + 601, out long begin, out long end)
                && begin == Start && end == Start + 601, "최소 시간 경계가 잘못되었습니다.");
            Require(TimeMgr.TryGetIdleRewardPeriod(Start, Start + 36000, out begin, out end)
                && begin == Start && end == Start + 18000, "첫 5시간만 계산해야 합니다.");

            var instance = Create();
            Require(instance.ReceiveOfflineHuntReward(begin, end, Start + 36000) == 18000,
                "정산 시간과 지급 구간을 구분하지 못했습니다.");
            Require(instance.ReceiveOfflineHuntReward(Start, Start + 36000, Start + 36001) == 0,
                "최대 시간을 넘는 나머지 구간을 나중에 지급했습니다.");
        }

        private static void CheckEventPeriod()
        {
            var instance = Create();
            Require(instance.ReceiveOfflineHuntReward(Start - 1200, Start + 600, Start + 600) == 600,
                "시작 전 시간을 포함했습니다.");

            instance = Create();
            long finish = instance.Schedule.EndTimeNum;
            Require(instance.ReceiveOfflineHuntReward(finish - 600, finish + 600, finish + 600) == 600,
                "종료 전 유효 시간의 보상을 놓치거나 종료 이후까지 지급했습니다.");
            Require(instance.DailyHuntCount == 0 && instance.TotalHuntCount == 600,
                "과거 날짜의 보상이 오늘 일일 기록에 포함됐습니다.");

            instance = Create();
            Require(instance.ReceiveOfflineHuntReward(finish, finish + 1200, finish + 1200) == 0,
                "이벤트 획득 종료 후 시간을 지급했습니다.");
        }

        private static void CheckDailyLimits()
        {
            long midnight = TimeMgr.AddDays(Start, 1);
            var instance = Create(0.1f, 300);
            instance.SetData(new CurrencyExchangeEventData
            {
                EventId = "test", AppliedStartUtcTicks = Start, HuntDate = 20261117,
                CurrencyCount = 280, DailyHuntCount = 280, TotalHuntCount = 280
            });

            // 로드와 UI 조회가 먼저 날짜를 갱신해도 전날의 획득 한도는 유지돼야 한다.
            instance.Refresh(midnight + 1800);
            Require(instance.ReceiveOfflineHuntReward(midnight - 1800, midnight + 1800, midnight + 1800) == 200,
                "전날 20개와 오늘 180개를 구분하지 못했습니다.");
            Require(instance.GetCurrency() == 480 && instance.DailyHuntCount == 180 && instance.TotalHuntCount == 480,
                "날짜별 합계나 오늘 표시 값이 잘못되었습니다.");

            instance = Create(1f, 100);
            Require(instance.TryGetHuntReward(BattleType.Normal, Start) == 1, "온라인 지급에 실패했습니다.");
            Require(instance.ReceiveOfflineHuntReward(Start, Start + 1000, Start + 1000) == 99,
                "온라인과 오프라인이 일일 한도를 공유하지 않습니다.");
            Require(instance.TryGetHuntReward(BattleType.Normal, Start + 1001) == 0,
                "오프라인 지급 후 온라인에서 한도를 넘었습니다.");
        }

        private static void CheckTotalAndIntegerLimits()
        {
            var instance = Create(1f, 3, 4);
            Require(instance.ReceiveOfflineHuntReward(Start, Start + 1000, Start + 1000) == 3,
                "일일 한도 적용에 실패했습니다.");
            long nextDay = TimeMgr.AddDays(Start, 1);
            Require(instance.ReceiveOfflineHuntReward(nextDay, nextDay + 1000, nextDay + 1000) == 1,
                "전체 획득 한도 적용에 실패했습니다.");

            instance = Create();
            instance.SetData(new CurrencyExchangeEventData
            {
                EventId = "test", AppliedStartUtcTicks = Start, HuntDate = 20261117,
                CurrencyCount = int.MaxValue - 1, DailyHuntCount = int.MaxValue - 1,
                TotalHuntCount = int.MaxValue - 1
            });
            Require(instance.ReceiveOfflineHuntReward(Start, Start + 1000, Start + 1000) == 1
                && instance.GetCurrency() == int.MaxValue && instance.TotalHuntCount == int.MaxValue,
                "저장 정수 범위를 넘었습니다.");
        }

        private static void CheckRemainder()
        {
            var instance = Create(0.1f);
            Require(instance.ReceiveOfflineHuntReward(Start, Start + 3, Start + 3) == 0, "소수 보상을 미리 지급했습니다.");
            var restored = Create(0.1f);
            restored.SetData(instance.GetData());
            Require(restored.ReceiveOfflineHuntReward(Start, Start + 10, Start + 10) == 1,
                "구간 중복 제외 또는 소수점 잔여량 저장에 실패했습니다.");

            instance = Create(0.7f);
            int received = instance.ReceiveOfflineHuntReward(Start, Start + 600, Start + 600);
            Require(received == 420, string.Format("실수 오차로 정수 보상이 줄었습니다. 지급: {0}, 확률: {1}",
                received, (decimal)instance.HuntRule.DropRate));

            instance = Create(0.1f);
            long midnight = TimeMgr.AddDays(Start, 1);
            Require(instance.ReceiveOfflineHuntReward(midnight - 5, midnight, midnight) == 0,
                "날짜 경계의 소수 계산이 잘못되었습니다.");
            Require(instance.ReceiveOfflineHuntReward(midnight, midnight + 5, midnight + 5) == 0,
                "전날의 소수점 잔여량을 다음 날로 넘겼습니다.");
        }

        private static void CheckReloadAndDuplicate()
        {
            var instance = Create();
            Require(instance.ReceiveOfflineHuntReward(Start, Start + 1000, Start + 1000) == 1000,
                "첫 지급에 실패했습니다.");
            var restored = Create();
            restored.SetData(instance.GetData());
            Require(restored.ReceiveOfflineHuntReward(Start, Start + 1000, Start + 1000) == 0,
                "재로드 후 같은 구간을 다시 지급했습니다.");
            Require(restored.ReceiveOfflineHuntReward(Start, Start + 1500, Start + 1500) == 500,
                "겹친 구간에서 새 시간만 계산하지 못했습니다.");
            Require(restored.ReceiveOfflineHuntReward(Start, Start + 1000, Start + 1000) == 0,
                "이전 시각의 요청을 다시 처리했습니다.");
        }

        private static void CheckDisabledAndNewOccurrence()
        {
            var instance = Create(offline: false);
            Require(instance.ReceiveOfflineHuntReward(Start, Start + 1000, Start + 1000) == 0
                && instance.GetData().LastOfflineRewardTime == Start + 1000, "오프라인 금지 처리가 잘못되었습니다.");
            instance = Create(battleType: BattleType.Challenge);
            Require(instance.ReceiveOfflineHuntReward(Start, Start + 1000, Start + 1000) == 0,
                "일반 사냥 외의 설정에 방치 보상을 지급했습니다.");

            instance = Create();
            instance.ReceiveOfflineHuntReward(Start, Start + 1000, Start + 1000);
            long nextStart = TimeMgr.AddDays(Start, 30);
            var next = new CurrencyExchangeEvent("test", "검사", "",
                new EventSchedule(nextStart, TimeMgr.AddDays(nextStart, 7), true), instance.HuntRule, "token");
            next.SetData(instance.GetData());
            Require(next.ReceiveOfflineHuntReward(nextStart - 1000, nextStart + 1000, nextStart + 1000) == 1000
                && next.TotalHuntCount == 1000, "이전 회차 기록을 새 회차에 합산했습니다.");
        }

        private static void CheckDataAndNotifications()
        {
            var instance = Create();
            int nestedReward = -1;
            instance.ChangedData += () => nestedReward = instance.ReceiveOfflineHuntReward(Start, Start + 1000, Start + 1000);
            Require(instance.ReceiveOfflineHuntReward(Start, Start + 1000, Start + 1000) == 1000 && nestedReward == 0,
                "알림 중 재진입으로 중복 지급했습니다.");

            var copy = instance.GetData();
            copy.HuntDailyRecords[0].HuntCount = 9999;
            Require(instance.GetData().HuntDailyRecords[0].HuntCount == 1000, "날짜별 기록이 깊은 복사가 아닙니다.");

            bool failed = false;

            try
            {
                copy = instance.GetData();
                copy.HuntDailyRecords[0].OfflineRemainder = double.NaN;
                instance.SetData(copy);
            }
            catch (ArgumentException)
            {
                failed = true;
            }

            Require(failed, "잘못된 소수점 기록을 허용했습니다.");
        }

        private static void CheckEmptyPeriodAndClock()
        {
            var instance = Create();
            Require(instance.ReceiveOfflineHuntReward(Start + 1000, Start + 1000, Start + 1000) == 0,
                "지급 대상이 없는 구간에서 보상을 지급했습니다.");
            Require(instance.ReceiveOfflineHuntReward(Start, Start + 1500, Start + 1500) == 500,
                "지급 대상이 없던 과거 구간을 다시 계산했습니다.");

            instance = Create();
            long nextDay = TimeMgr.AddDays(Start, 1);
            instance.TryGetHuntReward(BattleType.Normal, nextDay);
            Require(instance.ReceiveOfflineHuntReward(Start, Start + 1000, Start + 1000) == 0
                && instance.TryGetHuntReward(BattleType.Normal, Start + 1000) == 0,
                "온라인 활동 날짜보다 이전으로 시간을 돌려 보상을 받았습니다.");

            bool failed = false;

            try
            {
                instance.ReceiveOfflineHuntReward(Start + 1000, Start, Start + 1000);
            }
            catch (ArgumentException)
            {
                failed = true;
            }

            Require(failed, "뒤집힌 정산 구간을 허용했습니다.");
        }

        private static void CheckCompletionWithoutReward()
        {
            var instance = Create();
            instance.TryGetHuntReward(BattleType.Normal, Start);
            instance.CompleteOfflineHuntCheck(Start + 1000);

            Require(instance.GetCurrency() == 1 && instance.DailyHuntCount == 1 && instance.TotalHuntCount == 1
                && instance.GetData().LastOfflineRewardTime == Start + 1000,
                "보상 없는 완료 처리에서 재화나 획득량이 늘었습니다.");
            Require(instance.ReceiveOfflineHuntReward(Start, Start + 1500, Start + 1500) == 500,
                "완료한 구간을 다음 지급 요청에 다시 포함했습니다.");

            var restored = Create();
            restored.SetData(instance.GetData());
            restored.CompleteOfflineHuntCheck(Start + 1000);
            Require(restored.GetData().LastOfflineRewardTime == Start + 1500 && restored.GetCurrency() == 501,
                "재로드 후 이전 완료 시각으로 되돌렸습니다.");

            long nextDay = TimeMgr.AddDays(Start, 1);
            restored.ChangedData += () => restored.CompleteOfflineHuntCheck(nextDay);
            restored.CompleteOfflineHuntCheck(nextDay);

            var data = restored.GetData();
            Require(data.HuntDailyRecords.Count == 1 && data.HuntDailyRecords[0].Date == 20261118
                && data.DailyHuntCount == 0 && data.CurrencyCount == 501 && data.TotalHuntCount == 501
                && data.LastOfflineRewardTime == nextDay,
                "완료 처리의 날짜 정리 또는 알림 중 재진입 처리가 잘못되었습니다.");
        }

        [MenuItem("Tools/Event Tests/Check Offline JSON Round Trip")]
        public static void CheckSerialization()
        {
            var instance = Create(0.1f);
            instance.ReceiveOfflineHuntReward(Start, Start + 3, Start + 3);
            var saved = JsonUtility.FromJson<CurrencyExchangeEventData>(JsonUtility.ToJson(instance.GetData()));
            var restored = Create(0.1f);
            restored.SetData(saved);

            Require(restored.ReceiveOfflineHuntReward(Start, Start + 10, Start + 10) == 1,
                "JSON 복원 후 정산 시각 또는 소수점 잔여량이 사라졌습니다.");

            var oldData = JsonUtility.FromJson<CurrencyExchangeEventData>(
                "{\"EventId\":\"test\",\"AppliedStartUtcTicks\":" + Start
                + ",\"HuntDate\":20261117,\"DailyHuntCount\":20,\"TotalHuntCount\":20}");
            restored.SetData(oldData);
            Require(restored.GetData().HuntDailyRecords[0].HuntCount == 20, "이전 저장 형식의 일일 기록이 사라졌습니다.");

            Debug.Log("이벤트 오프라인 JSON 검사 통과.");
        }

        private static void Require(bool value, string message)
        {
            if (!value)
                throw new InvalidOperationException(message);
        }
    }
}
