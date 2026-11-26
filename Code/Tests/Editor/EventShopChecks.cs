using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using Game.Common;
using Game.Core;
using Game.Events;

namespace Game.Tests.Editor
{
    public static class EventShopChecks
    {
        [MenuItem("Tools/Event Tests/Run Shop Checks")]
        public static void Run()
        {
            CheckPurchase();
            CheckFailures();
            CheckPeriodsAndReload();
            CheckDuplicateRequest();
            CheckSettings();
            Debug.Log("이벤트 상점 검사 통과: 차감·지급·횟수, 실패 복구, 구매 제한, 기간, 재로드, 중복 요청.");
        }

        private static EventShopRule Product(string eventId = "test", ShopLimitType type = ShopLimitType.Week, int limit = 3)
        {
            return new EventShopRule("1001", eventId, "테스트 상품", "token", 30, type, limit, "x", "Silver Nugget",
                new Dictionary<Currency, int> { { Currency.EquipUp, 50 }, { Currency.SkillUp, 10 } });
        }

        private static CurrencyExchangeEvent Create(long start, EventShopRule product, int balance = 300)
        {
            var instance = new CurrencyExchangeEvent(product.EventId, "테스트", "",
                new EventSchedule(start, TimeMgr.AddDays(start, 14), true), null, "token", null, new[] { product });
            instance.SetData(new CurrencyExchangeEventData
            {
                EventId = product.EventId, AppliedStartUtcTicks = start, CurrencyCount = balance
            });
            return instance;
        }

        private static void CheckPurchase()
        {
            long now = TimeMgr.ParseTableDate("2026.11.17");
            var instance = Create(now, Product());
            int received = 0;
            Require(instance.TryBuy("1001", now, rewards => { received += rewards[Currency.EquipUp]; return true; })
                == ShopPurchaseResult.Success, "정상 구매에 실패했습니다.");
            Require(instance.GetCurrency() == 270 && received == 50 && instance.GetPurchaseCount("1001", now) == 1,
                "가격·보상·구매 횟수가 일치하지 않습니다.");
            Require(instance.DailyHuntCount == 0 && instance.TotalHuntCount == 0, "구매가 사냥 획득량을 변경했습니다.");
            instance.TryBuy("1001", now, rewards => true);
            instance.TryBuy("1001", now, rewards => true);
            Require(instance.TryBuy("1001", now, rewards => true) == ShopPurchaseResult.LimitReached,
                "주간 구매 한도를 초과했습니다.");
            Require(instance.GetCurrency() == 210 && instance.GetPurchaseCount("1001", now) == 3, "제한 초과 요청이 상태를 변경했습니다.");
        }

        private static void CheckFailures()
        {
            long now = TimeMgr.ParseTableDate("2026.11.17");
            var instance = Create(now, Product());
            Require(instance.TryBuy("1001", now, rewards => false) == ShopPurchaseResult.RewardLimitReached,
                "보상 지급 거절을 구매 성공으로 처리했습니다.");
            Require(instance.GetCurrency() == 300 && instance.GetPurchaseCount("1001", now) == 0, "지급 거절 후 차감과 구매 기록을 복구하지 않았습니다.");
            Require(instance.TryBuy("1001", now, rewards => true) == ShopPurchaseResult.Success, "지급 거절 후 구매 잠금이 해제되지 않았습니다.");
            bool called = false;
            var poor = Create(now, Product(), 29);
            Require(poor.TryBuy("1001", now, rewards => { called = true; return true; }) == ShopPurchaseResult.NotEnoughCurrency
                && !called && poor.GetCurrency() == 29, "잔액 부족 요청이 보상이나 잔액을 변경했습니다.");
            Require(instance.TryBuy("missing", now, rewards => true) == ShopPurchaseResult.InvalidProduct, "없는 상품을 구매했습니다.");
            Require(instance.TryBuy("1001", now - 1, rewards => true) == ShopPurchaseResult.EventClosed, "시작 전에 구매했습니다.");
        }

        private static void CheckPeriodsAndReload()
        {
            long start = TimeMgr.ParseTableDate("2026.11.17");
            var product = Product();
            var instance = Create(start, product);
            instance.TryBuy("1001", start, rewards => true);
            var saved = instance.GetData();
            var restored = Create(start, product);
            restored.SetData(saved);
            Require(restored.GetPurchaseCount("1001", start) == 1 && restored.GetCurrency() == 270, "재로드 후 기록이 유지되지 않았습니다.");
            saved.Purchases[0].Count = 99;
            Require(restored.GetPurchaseCount("1001", start) == 1, "구매 기록을 깊게 복사하지 않았습니다.");
            long monday = TimeMgr.ParseTableDate("2026.11.23");
            Require(restored.GetPurchaseCount("1001", monday) == 0 && restored.GetCurrency() == 270, "주간 초기화가 잔액을 변경했습니다.");
            Require(restored.TryBuy("1001", restored.Schedule.EndTimeNum, rewards => true) == ShopPurchaseResult.Success,
                "획득 종료 후 추가 교환 기간에 구매하지 못했습니다.");
            Require(restored.TryBuy("1001", restored.Schedule.ExchangeEndTimeNum, rewards => true) == ShopPurchaseResult.EventClosed,
                "교환 종료 시각에 구매했습니다.");

            long nextStart = TimeMgr.AddDays(start, 40);
            var next = Create(nextStart, product);
            next.SetData(restored.GetData());
            next.Refresh(nextStart);
            Require(next.GetCurrency() == 0 && next.GetPurchaseCount("1001", nextStart) == 0, "새 회차에서 구매 기록과 재화가 초기화되지 않았습니다.");

            var daily = Create(start, Product(type: ShopLimitType.Daily, limit: 1));
            daily.TryBuy("1001", start, rewards => true);
            Require(daily.GetPurchaseCount("1001", TimeMgr.AddDays(start, 1)) == 0, "일일 구매 제한이 초기화되지 않았습니다.");
            var monthly = Product(type: ShopLimitType.Month);
            Require(monthly.GetPeriodStart(TimeMgr.ParseTableDate("2027.01.15")) == TimeMgr.ParseTableDate("2027.01.01"),
                "월간 구간 시작 계산이 잘못되었습니다.");
        }

        private static void CheckDuplicateRequest()
        {
            long now = TimeMgr.ParseTableDate("2026.11.17");
            var instance = Create(now, Product(type: ShopLimitType.None, limit: -1));
            ShopPurchaseResult nested = ShopPurchaseResult.Success;
            instance.TryBuy("1001", now, rewards =>
            {
                nested = instance.TryBuy("1001", now, other => true);
                return true;
            });
            Require(nested == ShopPurchaseResult.Busy && instance.GetCurrency() == 270
                && instance.GetPurchaseCount("1001", now) == 1, "보상 지급 중 중복 요청을 처리했습니다.");
            Require(instance.TryBuy("1001", now, rewards => true) == ShopPurchaseResult.Success, "정상 구매 후 잠금이 남았습니다.");
            var otherEvent = Create(now, Product("other"));
            Require(otherEvent.GetPurchaseCount("1001", now) == 0 && otherEvent.GetCurrency() == 300,
                "같은 상품 ID의 다른 이벤트 기록이 섞였습니다.");
        }

        private static void CheckSettings()
        {
            bool threw = false;
            try
            {
                new EventShopRule("bad", "test", "", "token", -1, ShopLimitType.None, -1, "x", "",
                    new Dictionary<Currency, int> { { Currency.Gem, 10 } });
            }
            catch (ArgumentException) { threw = true; }
            Require(threw, "음수 가격을 허용했습니다.");
            var rewards = new Dictionary<Currency, int> { { Currency.Gem, 10 } };
            var rule = new EventShopRule("free", "test", "", "token", 0, ShopLimitType.None, -1, "x", "", rewards);
            rewards[Currency.Gem] = 999;
            Require(rule.Rewards[Currency.Gem] == 10, "외부 목록 변경이 상품 보상을 변경했습니다.");
            long now = TimeMgr.ParseTableDate("2026.11.17");
            var free = Create(now, rule, 0);
            Require(free.TryBuy("free", now, values => true) == ShopPurchaseResult.Success && free.GetCurrency() == 0,
                "무료 상품이 잔액을 요구하거나 변경했습니다.");
        }

        [MenuItem("Tools/Event Tests/Log Current Shop State")]
        public static void LogCurrentState()
        {
            if (!Application.isPlaying || EventMgr.Instance == null || !EventMgr.Instance.IsInitialized)
            {
                Debug.LogWarning("Title 씬에서 게임을 실행한 뒤 확인해주세요.");
                return;
            }
            long now = TimeMgr.GetNowTimeNum();
            foreach (IGameEvent gameEvent in EventMgr.Instance.Events)
            {
                if (!(gameEvent is CurrencyExchangeEvent owner)) continue;
                foreach (EventShopRule product in owner.ShopItems)
                {
                    Debug.LogFormat("이벤트: {0}, 상품: {1}, 잔액: {2}, 가격: {3}, 구간 구매 횟수: {4}, 구매 상태: {5}",
                        owner.Id, product.Id, owner.GetCurrency(), product.Price, owner.GetPurchaseCount(product.Id, now),
                        owner.GetPurchaseResult(product.Id, now));
                }
            }
        }

        [MenuItem("Tools/Event Tests/Check Shop JSON Round Trip")]
        public static void CheckSerialization()
        {
            long now = TimeMgr.ParseTableDate("2026.11.17");
            var product = Product();
            var instance = Create(now, product);
            instance.TryBuy("1001", now, rewards => true);
            var data = JsonUtility.FromJson<CurrencyExchangeEventData>(JsonUtility.ToJson(instance.GetData()));
            var restored = Create(now, product);
            restored.SetData(data);
            Require(restored.GetCurrency() == 270 && restored.GetPurchaseCount("1001", now) == 1, "구매 기록의 JSON 복원에 실패했습니다.");
            var oldData = JsonUtility.FromJson<CurrencyExchangeEventData>("{\"EventId\":\"test\",\"CurrencyCount\":5}");
            var upgraded = new CurrencyExchangeEventData();
            upgraded.SetData(oldData);
            Require(upgraded.CurrencyCount == 5 && upgraded.Purchases.Count == 0, "기존 저장 파일을 읽지 못했습니다.");
            Debug.Log("이벤트 상점 JSON 왕복 검사 통과.");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
