using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using UnityEngine;

using Game.Battle;
using Game.Calculate;
using Game.Common;
using Game.Data;
using Game.UI.Popup;
using Game.Events;
using Game.Quests;

namespace Game.Core
{
    public class TimeMgr : MonoBehaviour
    {
        private const long TickToNum = 10000000;
        private const int OffsetTimeHour = 9;

        public static DateTime GetNowDateTime()
        {
            DateTime time = DateTime.UtcNow;
            return time.AddHours(OffsetTimeHour);
        }

        public static int GetNowDateNum()
        {
            DateTime time = DateTime.UtcNow;
            time = time.AddHours(OffsetTimeHour);
            return GetDateNum(time);
        }

        public static int GetDateNum(DateTime time)
        {
            return time.Year * 10000 + time.Month * 100 + time.Day;
        }

        public static long GetNowTimeNum()
        {
            DateTime time = DateTime.UtcNow;
            time = time.AddHours(OffsetTimeHour);
            return GetTimeNum(time);
        }

        public static long GetTimeNum(DateTime time)
        {
            return time.Ticks / TickToNum;
        }

        public static DateTime GetNumToDate(long num)
        {
            return new DateTime(num * TickToNum);
        }
        
        public static long ParseTableDate(string value)
        {
            return GetTimeNum(DateTime.ParseExact(value, "yyyy.MM.dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None));
        }

        public static long AddDays(long before, int addDay)
        {
            DateTime beforeDate = GetNumToDate(before);
            beforeDate = beforeDate.AddDays(addDay);
            return GetTimeNum(beforeDate);
        }

        public static long GetDayStartTimeNum(long timeNum)
        {
            return GetTimeNum(GetNumToDate(timeNum).Date);
        }

        public static long GetWeekStartTimeNum(long timeNum)
        {
            DateTime date = GetNumToDate(timeNum).Date;
            int days = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return GetTimeNum(date.AddDays(-days));
        }

        public static bool TryGetIdleRewardPeriod(long lastTime, long now, out long start, out long end)
        {
            start = now;
            end = now;

            if (lastTime <= 0 || now <= lastTime || now - lastTime <= Constant.IdleRewardMinSecond)
                return false;

            start = lastTime;
            end = lastTime + Math.Min(now - lastTime, Constant.IdleReawedMaxSecond);

            return true;
        }

        private int LastDayNum;
        private bool IsFocused = true;
        private bool IsPaused;
        private bool IsTimeReady;
        private bool IsCheckingTime;
        private float NextActivityCheck;
        private Coroutine ReadyCoroutine;
        private Coroutine NextDayCoroutine;

        private bool IsActive => IsFocused && !IsPaused;

        private bool IsGameReady => BattleMgr.Instance != null && BattleMgr.Instance.IsReadied;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            WaitForGameReady();
        }

        private void Update()
        {
            if (!IsTimeReady || !IsActive || !IsGameReady || Time.unscaledTime < NextActivityCheck)
                return;

            NextActivityCheck = Time.unscaledTime + 1f;
            long now = GetNowTimeNum();

            if (GetDateNum(GetNumToDate(now)) != LastDayNum)
            {
                CheckTime(false);
            }
            else
            {
                // 정상 플레이한 시간이 다음 실행에서 방치 시간으로 계산되지 않도록 한다.
                PlayerDataMgr.Instance.SetLastTimeNum(now);
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            bool wasActive = IsActive;
            IsFocused = focus;

            CheckApplicationState(wasActive);
        }

        private void OnApplicationPause(bool pause)
        {
            bool wasActive = IsActive;
            IsPaused = pause;

            CheckApplicationState(wasActive);
        }

        private void CheckApplicationState(bool wasActive)
        {
            if (wasActive == IsActive)
                return;

            if (IsCheckingTime)
                return;

            if (!IsTimeReady)
            {
                WaitForGameReady();
                return;
            }

            if (IsActive)
            {
                CheckTime(true);
            }
            else
            {
                StopNextDayCheck();
                PlayerDataMgr.Instance.SetLastTimeNum(GetNowTimeNum());
                DataMgr.Instance.Save();
            }
        }

        private void WaitForGameReady()
        {
            if (ReadyCoroutine == null)
            {
                ReadyCoroutine = StartCoroutine(CheckTimeAfterSetting());
            }
        }

        private IEnumerator CheckTimeAfterSetting()
        {
            // 대기 코루틴 참조를 등록한 다음 프레임부터 준비 상태를 확인한다.
            yield return null;

            while (!IsGameReady || !IsActive)
            {
                yield return null;
            }

            ReadyCoroutine = null;
            CheckTime(true);
        }

        private void CheckTime(bool giveIdleReward)
        {
            if (!IsGameReady)
            {
                WaitForGameReady();
                return;
            }

            if (IsCheckingTime || !IsActive)
                return;

            Dictionary<Currency, double> rewardCurrency = null;
            Dictionary<string, int> rewardEquip = null;
            IsCheckingTime = true;

            try
            {
                long lastTime = PlayerDataMgr.Instance.GetLastNum();
                long now = GetNowTimeNum();
                long start = now;
                long end = now;

                bool canReceive = giveIdleReward
                    && TryGetIdleRewardPeriod(lastTime, now, out start, out end);

                if (canReceive)
                {
                    GiveIdleReward(end - start, out rewardCurrency, out rewardEquip);

                    var eventRewards = EventMgr.Instance.ReceiveOfflineHuntRewards(start, end, now);

                    foreach (var reward in eventRewards)
                    {
                        Debug.LogFormat("이벤트 방치 보상: {0}, {1}개", reward.Key, reward.Value);
                    }
                }
                else
                {
                    EventMgr.Instance.CompleteOfflineHuntCheck(now);
                }

                GetDateChangedReward(lastTime, now);
                EventMgr.Instance.Refresh(now);
                QuestMgr.Instance.Refresh(now);
                PlayerDataMgr.Instance.SetLastTimeNum(now);
                DataMgr.Instance.Save();

                LastDayNum = GetDateNum(GetNumToDate(now));
                IsTimeReady = true;

                if (IsActive)
                {
                    SetNextCheck(now);
                }
            }
            finally
            {
                IsCheckingTime = false;
            }

            // 비동기 팝업 로드는 지급과 정산 시각 저장이 모두 끝난 다음 시작한다.
            if (rewardCurrency != null)
            {
                OpenIdleRewardPopup(rewardCurrency, rewardEquip);
            }
        }

        private void StopNextDayCheck()
        {
            if (NextDayCoroutine == null)
                return;

            StopCoroutine(NextDayCoroutine);
            NextDayCoroutine = null;
        }

        private void SetNextCheck(long now)
        {
            StopNextDayCheck();

            long nextDay = AddDays(GetDayStartTimeNum(now), 1);
            NextDayCoroutine = StartCoroutine(NextCheckTimeOnSeconds(nextDay - now + 1));
        }

        private IEnumerator NextCheckTimeOnSeconds(float waitingTime)
        {
            yield return new WaitForSecondsRealtime(waitingTime);
            NextDayCoroutine = null;
            CheckTime(false);
        }

        private void GetDateChangedReward(long lastTimeNum, long nowTimeNum)
        {
            if (nowTimeNum <= lastTimeNum)
                return;

            DateTime lastTime = GetNumToDate(lastTimeNum);
            DateTime nowTime = GetNumToDate(nowTimeNum);

            if (lastTime.Year != nowTime.Year || lastTime.Month != nowTime.Month)
            {
                PlayerDataMgr.Instance.GetDateChangedReward(ShopLimitType.Month);
            }

            long lastWeekStart = GetWeekStartTimeNum(lastTimeNum);
            long nowWeekStart = GetWeekStartTimeNum(nowTimeNum);

            if (nowWeekStart > lastWeekStart)
            {
                PlayerDataMgr.Instance.GetDateChangedReward(ShopLimitType.Week);
            }

            if (nowTime.Date > lastTime.Date)
            {
                PlayerDataMgr.Instance.GetDateChangedReward(ShopLimitType.Daily);
            }
        }

        private void GiveIdleReward(long seconds, out Dictionary<Currency, double> rewardCurrency,
            out Dictionary<string, int> rewardEquip)
        {
            string stageId = PlayerDataMgr.Instance.GetMaxStage(BattleType.Normal).ToString();
            rewardCurrency = DropCalculator.IdleCurrencyReward(stageId, seconds);
            rewardEquip = DropCalculator.IdleEquipReward(stageId, seconds);

            foreach (Currency key in rewardCurrency.Keys)
            {
                if (key == Currency.Gold)
                {
                    PlayerDataMgr.Instance.AddGold(rewardCurrency[key]);
                }
                else if (key == Currency.Exp)
                {
                    UpgradeDataMgr.Instance.AddExp((float)rewardCurrency[key]);
                }
                else
                {
                    PlayerDataMgr.Instance.TryAddCurrency(key, Mathf.RoundToInt((float)rewardCurrency[key]));
                }
            }

            foreach (string key in rewardEquip.Keys)
            {
                EquipDataMgr.Instance.AddEquip(key, rewardEquip[key]);
            }
        }

        private async void OpenIdleRewardPopup(Dictionary<Currency, double> rewardCurrency, Dictionary<string, int> rewardEquip)
        {
            var popup = await PopupMgr.Instance.Get<Popup_GetItems>(PopupType.GetItem);
            popup.Init("방치 보상");
            popup.AddItems(rewardCurrency);
            popup.AddEquips(rewardEquip);
            popup.Open();
        }
    }
}
