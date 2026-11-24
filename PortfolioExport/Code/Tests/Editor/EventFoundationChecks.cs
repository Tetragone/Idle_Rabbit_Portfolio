using System;

using UnityEditor;
using UnityEngine;

using Game.Common;
using Game.Core;
using Game.Data;
using Game.Events;

namespace Game.Tests.Editor
{
    public static class EventFoundationChecks
    {
        [MenuItem("Tools/Event Tests/Run Foundation Checks")]
        public static void Run()
        {
            CheckSchedule();
            CheckOccurrence();
            CheckDataCopy();
            CheckInvalidSettings();
            Debug.Log("이벤트 기반 검사 통과: 시간 경계, 회차 초기화, 데이터 재적용, 이벤트 격리, 복사, 잘못된 설정.");
        }

        private static void CheckSchedule()
        {
            long start = TimeMgr.ParseTableDate("2026.11.17");
            long end = TimeMgr.ParseTableDate("2026.11.24");
            Require(TimeMgr.GetNumToDate(start) == new DateTime(2026, 11, 17), "테이블 날짜는 한국 시간 00:00이어야 합니다.");
            var schedule = new EventSchedule(start, end, true);
            long exchangeEnd = TimeMgr.AddDays(end, Constant.EventShopExtraOpenDays);
            Require(schedule.ExchangeEndTimeNum == exchangeEnd, "상점 추가 운영일 적용에 실패했습니다.");
            Require(schedule.GetPeriod(start - 1) == EventPeriod.Upcoming && !schedule.IsVisible(start - 1), "시작 전 판정에 실패했습니다.");
            Require(schedule.CanAcquire(start) && schedule.CanExchange(start) && schedule.IsVisible(start), "시작 시각은 포함해야 합니다.");
            Require(schedule.CanAcquire(end - 1), "종료 직전에는 획득할 수 있어야 합니다.");
            Require(!schedule.CanAcquire(end) && schedule.CanExchange(end), "획득 종료 시각에는 교환만 가능해야 합니다.");
            Require(schedule.GetPeriod(exchangeEnd - 1) == EventPeriod.ExchangeOnly, "상점 추가 운영 기간 판정에 실패했습니다.");
            Require(schedule.GetPeriod(exchangeEnd) == EventPeriod.Closed && !schedule.CanExchange(exchangeEnd)
                && !schedule.IsVisible(exchangeEnd), "상점 종료 시각은 포함하지 않아야 합니다.");
            var noShop = new EventSchedule(start, end, false);
            Require(!noShop.CanExchange(start) && noShop.ExchangeEndTimeNum == end && !noShop.IsVisible(end),
                "상점이 없는 이벤트에 추가 운영 기간이 적용됐습니다.");
        }

        private static void CheckOccurrence()
        {
            long start = TimeMgr.ParseTableDate("2026.11.17");
            var schedule = new EventSchedule(start, TimeMgr.AddDays(start, 7), true);
            var instance = new ProgressEvent("test", schedule);
            instance.SetData(new ProgressData
            {
                EventId = "test", AppliedStartUtcTicks = TimeMgr.AddDays(start, -30), Count = 42
            });
            instance.Refresh(start - 1);
            Require(instance.Count == 42, "새 회차 시작 전에는 기존 데이터를 유지해야 합니다.");
            Require(instance.Refresh(start), "새 회차 시작을 변경으로 알려야 합니다.");
            Require(instance.Count == 0 && instance.GetData().AppliedStartUtcTicks == start,
                "새 회차 시작 시 기본 데이터와 시작 시각을 적용해야 합니다.");
            instance.Count = 7;
            Require(!instance.Refresh(TimeMgr.AddDays(start, 1)) && instance.Count == 7, "같은 회차를 반복 초기화했습니다.");

            // 파일 입출력 대신 저장 시 조회한 복사본을 새 객체에 적용하여 로드 순서를 확인한다.
            ProgressData stored = instance.GetData();
            var reloaded = new ProgressEvent("test", schedule);
            reloaded.SetData(stored);
            reloaded.Refresh(TimeMgr.AddDays(start, 2));
            Require(reloaded.Count == 7, "같은 회차의 데이터를 다시 적용하면 진행도를 유지해야 합니다.");
            reloaded.Refresh(schedule.EndTimeNum);
            reloaded.Refresh(schedule.ExchangeEndTimeNum);
            Require(reloaded.Count == 7, "획득 종료나 상점 종료 시 진행도를 지우면 안 됩니다.");

            var unrelated = new ProgressEvent("other", schedule);
            unrelated.SetData(new ProgressData { EventId = "other", AppliedStartUtcTicks = start, Count = 99 });
            long nextStart = TimeMgr.AddDays(start, 60);
            var next = new ProgressEvent("test", new EventSchedule(nextStart, TimeMgr.AddDays(nextStart, 7), true));
            next.SetData(stored);
            next.Refresh(TimeMgr.AddDays(nextStart, 2));
            Require(next.Count == 0 && unrelated.Count == 99, "늦게 접속해도 해당 이벤트의 회차만 초기화해야 합니다.");
            var extended = new ProgressEvent("test", new EventSchedule(start, TimeMgr.AddDays(start, 10), true));
            extended.SetData(stored);
            extended.Refresh(TimeMgr.AddDays(start, 8));
            Require(extended.Count == 7, "종료일 연장으로 진행도를 초기화하면 안 됩니다.");
        }

        private static void CheckDataCopy()
        {
            long start = TimeMgr.ParseTableDate("2026.11.17");
            var instance = new ProgressEvent("test", new EventSchedule(start, TimeMgr.AddDays(start, 7), true));
            var source = new ProgressData { EventId = "test", AppliedStartUtcTicks = start, Count = 10 };
            instance.SetData(source);
            source.Count = 20;
            Require(instance.Count == 10, "SetData는 외부 객체의 참조를 그대로 보관하면 안 됩니다.");
            ProgressData copy = instance.GetData();
            copy.Count = 30;
            Require(instance.Count == 10, "GetData로 받은 복사본이 내부 데이터를 변경하면 안 됩니다.");
        }

        private static void CheckInvalidSettings()
        {
            long start = TimeMgr.ParseTableDate("2026.11.17");
            var schedule = new EventSchedule(start, TimeMgr.AddDays(start, 7), true);
            Expect<ArgumentException>(() => new ProgressEvent("invalid/id", schedule));
            Expect<ArgumentException>(() => new ProgressEvent("", schedule));
            Expect<ArgumentException>(() => new ProgressEvent(null, schedule));
            Expect<ArgumentException>(() => new EventSchedule(start, start, true));
            Expect<ArgumentException>(() => new EventSchedule(start, start - 1, true));
            Expect<FormatException>(() => TimeMgr.ParseTableDate("2026.02.30"));
            Require(new ProgressEvent("test_event-1", schedule).Id == "test_event-1", "허용된 이벤트 ID를 거부했습니다.");
        }

        [MenuItem("Tools/Event Tests/Check Unity JSON Round Trip")]
        public static void CheckSerialization()
        {
            long start = TimeMgr.ParseTableDate("2026.11.17");
            var schedule = new EventSchedule(start, TimeMgr.AddDays(start, 7), true);
            var data = new ProgressData { EventId = "test", AppliedStartUtcTicks = start, Count = 42 };
            var restored = JsonUtility.FromJson<ProgressData>(JsonUtility.ToJson(data));
            Require(restored != null && restored.EventId == "test" && restored.AppliedStartUtcTicks == start
                && restored.SchemaVersion == 1 && restored.Count == 42,
                "공통 정보와 개별 데이터의 JSON 변환에 실패했습니다.");
            var instance = new ProgressEvent("test", schedule);
            instance.SetData(restored);
            instance.Refresh(TimeMgr.AddDays(start, 1));
            Require(instance.Count == 42, "JSON으로 복원한 같은 회차의 진행도가 유지되지 않았습니다.");

            var actual = new CurrencyExchangeEvent("hunt_quest", "테스트", "", schedule);
            actual.Refresh(start);
            var actualData = JsonUtility.FromJson<CurrencyExchangeEventData>(JsonUtility.ToJson(actual.GetData()));
            var actualReloaded = new CurrencyExchangeEvent("hunt_quest", "테스트", "", schedule);
            actualReloaded.SetData(actualData);
            actualReloaded.Refresh(start);
            Require(actualReloaded.GetData().EventId == "hunt_quest"
                && actualReloaded.GetData().AppliedStartUtcTicks == start,
                "실제 이벤트 데이터의 JSON 복원에 실패했습니다.");
            Debug.Log("이벤트 Unity JSON 왕복 검사 통과: 공통 정보, 개별 데이터, 복원 후 회차 유지.");
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

        [Serializable]
        public sealed class ProgressData : EventData, IBaseData<ProgressData>
        {
            public int Count;
            public void SetToDefault() { EventId = ""; AppliedStartUtcTicks = 0; SchemaVersion = 1; Count = 0; }
            public void SetData(ProgressData source)
            {
                EventId = source.EventId;
                AppliedStartUtcTicks = source.AppliedStartUtcTicks;
                SchemaVersion = source.SchemaVersion;
                Count = source.Count;
            }
        }

        private sealed class ProgressEvent : GameEvent<ProgressData>
        {
            public int Count { get => Data.Count; set => Data.Count = value; }
            public ProgressEvent(string id, EventSchedule schedule) : base(id, id, "", schedule) { }
        }
    }
}
