using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using Game.Common;
using Game.Core;
using Game.Events;

namespace Game.Tests.Editor
{
    public static class EventCurrencyChecks
    {
        [MenuItem("Tools/Event Tests/Run Currency Checks")]
        public static void Run()
        {
            CheckSeparateBalances();
            CheckShopCurrency();
            CheckReloadAndLegacy();
            CheckInvalidRecords();

            Debug.Log("이벤트 다중 재화 검사 통과: 재화별 지급·한도, 상품 결제, 거절 복구, 저장 복사, 이전 형식과 새 회차.");
        }

        private static long Start => TimeMgr.ParseTableDate("2026.11.17");

        private static CurrencyExchangeEvent Create()
        {
            var product = new EventShopRule("item", "test", "검사", "second", 100, ShopLimitType.None, 3, "x", "",
                new Dictionary<Currency, int> { { Currency.Gem, 1 } });

            return new CurrencyExchangeEvent("test", "검사", "",
                new EventSchedule(Start, TimeMgr.AddDays(Start, 7), true),
                new EventHuntRule("first", BattleType.Normal, 1f, 1, -1, -1, true), "first", null,
                new[] { product }, new[] { "first", "second" });
        }

        private static void CheckSeparateBalances()
        {
            var instance = Create();
            Require(instance.CurrencyIds.Count == 2, "이벤트 재화 목록이 누락됐습니다.");
            Require(instance.TryAddQuestReward("first", 10, Start)
                && instance.TryAddQuestReward("second", 20, Start), "재화별 지급에 실패했습니다.");
            Require(instance.GetCurrency("first") == 10 && instance.GetCurrency("second") == 20,
                "서로 다른 재화의 잔액이 섞였습니다.");
            Require(instance.TryGetHuntReward(BattleType.Normal, Start) == 1
                && instance.GetCurrency("first") == 11 && instance.GetCurrency("second") == 20,
                "사냥에서 지정한 재화 외의 잔액을 변경했습니다.");
            Require(instance.ReceiveOfflineHuntReward(Start, Start + 1000, Start + 1000) == 1000
                && instance.GetCurrency("first") == 1011 && instance.GetCurrency("second") == 20,
                "오프라인 사냥의 지급 재화가 잘못되었습니다.");
            Require(instance.TryAddQuestReward("second", int.MaxValue - 20, Start + 1001)
                && !instance.TryAddQuestReward("second", 1, Start + 1001)
                && instance.TryAddQuestReward("first", 1, Start + 1001),
                "한 재화의 보유 한도가 다른 재화에 영향을 줍니다.");
        }

        private static void CheckShopCurrency()
        {
            var instance = Create();
            instance.TryAddQuestReward("first", 300, Start);
            Require(instance.TryBuy("item", Start, _ => true) == ShopPurchaseResult.NotEnoughCurrency,
                "기본 재화 잔액으로 다른 재화 상품을 구매했습니다.");

            instance.TryAddQuestReward("second", 150, Start);
            Require(instance.TryBuy("item", Start, _ => true) == ShopPurchaseResult.Success
                && instance.GetCurrency("first") == 300 && instance.GetCurrency("second") == 50,
                "상품이 지정한 재화에서 가격을 차감하지 않았습니다.");

            instance.TryAddQuestReward("second", 60, Start);
            Require(instance.TryBuy("item", Start, _ => false) == ShopPurchaseResult.RewardLimitReached
                && instance.GetCurrency("first") == 300 && instance.GetCurrency("second") == 110
                && instance.GetPurchaseCount("item", Start) == 1,
                "보상 거절 시 결제 재화와 구매 기록을 복구하지 못했습니다.");
        }

        private static void CheckReloadAndLegacy()
        {
            var instance = Create();
            instance.SetData(new CurrencyExchangeEventData
            {
                EventId = "test", AppliedStartUtcTicks = Start, CurrencyCount = 42, Currencies = null
            });
            Require(instance.GetCurrency("first") == 42 && instance.GetCurrency("second") == 0,
                "이전 단일 재화 저장을 잘못 변환했습니다.");

            instance.TryAddQuestReward("second", 77, Start);
            var saved = instance.GetData();
            var restored = Create();
            restored.SetData(saved);
            saved.Currencies[0].Count = 999;
            Require(restored.GetCurrency("first") == 42 && restored.GetCurrency("second") == 77,
                "재화별 저장 기록이 유지되지 않거나 깊은 복사가 아닙니다.");

            long nextStart = TimeMgr.AddDays(Start, 30);
            var next = new CurrencyExchangeEvent("test", "검사", "",
                new EventSchedule(nextStart, TimeMgr.AddDays(nextStart, 7), true), currencyIds: new[] { "first", "second" });
            next.SetData(restored.GetData());
            next.Refresh(nextStart);
            Require(next.GetCurrency("first") == 0 && next.GetCurrency("second") == 0,
                "새 회차에서 모든 재화를 초기화하지 않았습니다.");

            var changedDefault = new CurrencyExchangeEvent("test", "검사", "", instance.Schedule,
                currencyIds: new[] { "second", "first" });
            changedDefault.SetData(new CurrencyExchangeEventData
            {
                EventId = "test", AppliedStartUtcTicks = Start, CurrencyCount = 42,
                Currencies = new List<EventCurrencyData> { new EventCurrencyData { CurrencyId = "first", Count = 42 } }
            });
            Require(changedDefault.GetCurrency("second") == 0 && changedDefault.GetCurrency("first") == 42,
                "기본 재화 순서 변경으로 저장 잔액을 다른 재화에 복제했습니다.");
        }

        private static void CheckInvalidRecords()
        {
            var instance = Create();
            bool failed = false;

            try
            {
                instance.SetData(new CurrencyExchangeEventData
                {
                    Currencies = new List<EventCurrencyData>
                    {
                        new EventCurrencyData { CurrencyId = "first", Count = 1 },
                        new EventCurrencyData { CurrencyId = "first", Count = 2 }
                    }
                });
            }
            catch (ArgumentException)
            {
                failed = true;
            }

            Require(failed, "같은 재화 ID의 중복 저장 기록을 허용했습니다.");
            Require(!instance.TryAddQuestReward("unknown", 1, Start), "등록하지 않은 재화를 지급했습니다.");
        }

        [MenuItem("Tools/Event Tests/Check Currency JSON Round Trip")]
        public static void CheckSerialization()
        {
            var instance = Create();
            instance.TryAddQuestReward("first", 12, Start);
            instance.TryAddQuestReward("second", 34, Start);
            var data = JsonUtility.FromJson<CurrencyExchangeEventData>(JsonUtility.ToJson(instance.GetData()));
            var restored = Create();
            restored.SetData(data);
            Require(restored.GetCurrency("first") == 12 && restored.GetCurrency("second") == 34,
                "JSON 복원 후 재화별 잔액이 달라졌습니다.");

            Debug.Log("이벤트 다중 재화 JSON 검사 통과.");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
