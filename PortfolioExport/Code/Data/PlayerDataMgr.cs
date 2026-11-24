using System;
using System.Collections.Generic;

using UnityEngine;

using Game.Calculate;
using Game.Common;
using Game.Core;
using Game.UI;

namespace Game.Data
{
    public class PlayerDataMgr : SingletonAllTime<PlayerDataMgr>
    {
        public static event Action<Currency> ChangedCurrency;
        public static event Action ChangedPortrait;

        public string NickName => MyData.NickName;
        public Sprite Portrait
        {
            get
            {
                return NowPortrait;
            }
            set
            {
                NowPortrait = value;
                ChangedPortrait?.Invoke();
            }
        }

        private PlayerData MyData = new PlayerData();
        private Sprite NowPortrait = null;

        private Dictionary<string, int> Currencies = new Dictionary<string, int>();

        private Dictionary<BattleType, int> CurStages = new Dictionary<BattleType, int>();
        private Dictionary<BattleType, int> MaxStages = new Dictionary<BattleType, int>();

        private Dictionary<string, int> DailyPurchase = new Dictionary<string, int>();
        private Dictionary<string, int> WeeklyPurchase = new Dictionary<string, int>();
        private Dictionary<string, int> MonthlyPurchase = new Dictionary<string, int>();
        private Dictionary<string, int> PurchaseAll = new Dictionary<string, int>();

        // 꾹 눌러서 업글하는 경우 계산은 한번에 하기 위해서 넣은 변수
        private double TempGold = 0d;
        private Dictionary<string, int> TempCurrency = new Dictionary<string, int>();

        public void SetData(PlayerData data)
        {
            MyData.SetData(data);
            Currencies.Clear();
            foreach (var item in MyData.Currencies)
            {
                Currencies.Add(item.Key, item.Value);
            }

            CurStages.Clear();
            MaxStages.Clear();

            foreach (var item in MyData.CurStages)
            {
                BattleType type = (BattleType)Enum.Parse(typeof(BattleType), item.Key);
                CurStages.Add(type, item.Value);
            }

            foreach (var item in MyData.MaxStages)
            {
                BattleType type = (BattleType)Enum.Parse(typeof(BattleType), item.Key);
                MaxStages.Add(type, item.Value);
            }

            DailyPurchase.Clear();
            WeeklyPurchase.Clear();
            MonthlyPurchase.Clear();
            PurchaseAll.Clear();

            foreach (var item in MyData.DailyPurchase)
            {
                DailyPurchase.Add(item.Key, item.Value);
            }

            foreach (var item in MyData.WeeklyPurchase)
            {
                WeeklyPurchase.Add(item.Key, item.Value);
            }

            foreach (var item in MyData.MonthlyPurchase)
            {
                MonthlyPurchase.Add(item.Key, item.Value);
            }

            foreach (var item in MyData.PurchaseAll)
            {
                PurchaseAll.Add(item.Key, item.Value);
            }
        }

        public PlayerData GetData()
        {
            PlayerData data = new PlayerData();

            // 저장할 때만 List로 가지고 있으면 되니까 
            // Dictionary로 수정하고 List로 바꿔서 넣어주자
            // SerialKeyValue는 sturct 이기때문에 복사가 Dictionary에서 수정해도 MyData의 값이 안 바뀐다
            MyData.Currencies.Clear();
            foreach (string key in Currencies.Keys)
            {
                MyData.Currencies.Add(new SerialKeyValue<string, int>(key, Currencies[key]));
            }

            MyData.CurStages.Clear();
            foreach (BattleType key in CurStages.Keys)
            {
                MyData.CurStages.Add(new SerialKeyValue<string, int>(key.ToString(), CurStages[key]));
            }

            MyData.MaxStages.Clear();
            foreach (BattleType key in MaxStages.Keys)
            {
                MyData.MaxStages.Add(new SerialKeyValue<string, int>(key.ToString(), MaxStages[key]));
            }

            MyData.DailyPurchase.Clear();
            foreach (string key in DailyPurchase.Keys)
            {
                MyData.DailyPurchase.Add(new SerialKeyValue<string, int>(key, DailyPurchase[key]));
            }

            MyData.WeeklyPurchase.Clear();
            foreach (string key in WeeklyPurchase.Keys)
            {
                MyData.WeeklyPurchase.Add(new SerialKeyValue<string, int>(key, WeeklyPurchase[key]));
            }

            MyData.MonthlyPurchase.Clear();
            foreach (string key in MonthlyPurchase.Keys)
            {
                MyData.MonthlyPurchase.Add(new SerialKeyValue<string, int>(key, MonthlyPurchase[key]));
            }

            MyData.PurchaseAll.Clear();
            foreach (string key in PurchaseAll.Keys)
            {
                MyData.PurchaseAll.Add(new SerialKeyValue<string, int>(key, PurchaseAll[key]));
            }

            data.SetData(MyData);
            return data;
        }

        public void GetDateChangedReward(ShopLimitType type)
        {
            switch (type)
            {
                case ShopLimitType.Week:
                    WeeklyPurchase.Clear();
                    break;
                case ShopLimitType.Month:
                    MonthlyPurchase.Clear();
                    break;
                case ShopLimitType.Daily:
                    DailyPurchase.Clear();

                    foreach (Currency key in Constant.DailyAttendReward.Keys)
                    {
                        TryAddCurrency(key, Constant.DailyAttendReward[key]);
                    }
                    break;
            }
        }

        #region currency
        public void AddGold(double added, bool notifyChange = true)
        {
            if (added < 0)
            {
                return;
            }

            MyData.Gold += added;
            if (notifyChange) NotifyCurrencyChanged(Currency.Gold);
        }

        public void UseGold(double use)
        {
            if (use < 0)
            {
                return;
            }

            MyData.Gold -= use;
            TempGold = 0;
            ChangedCurrency?.Invoke(Currency.Gold);
        }

        public double GetGold()
        {
            return MyData.Gold;
        }

        public void SetTempGold(double use)
        {
            if (use < 0)
            {
                return;
            }

            TempGold = use;
            ChangedCurrency?.Invoke(Currency.Gold);
        }

        public double GetTempGold()
        {
            return MyData.Gold - TempGold;
        }

        public bool TryAddCurrency(Currency cur, int added, bool notifyChange = true)
        {
            if (added < 0)
            {
                MyLogger.Log(LogType.Warning, LogSituation.AddCurrency, ProcessType.Failed, "AddCurrency Must Be Used Only Add");
                return false;
            }

            switch (cur)
            {
                case Currency.Gold:
                    MyLogger.Log(LogType.Warning, LogSituation.AddCurrency, ProcessType.Failed, string.Format("{0} is not use this", cur.ToString()));
                    return false;
                default:
                    string sCur = cur.ToString();

                    if (!Currencies.ContainsKey(sCur))
                    {
                        Currencies.Add(sCur, 0);
                    }

                    Currencies[sCur] = Mathf.Min(Currencies[sCur], int.MaxValue - added);
                    Currencies[sCur] += added;
                    if (notifyChange) NotifyCurrencyChanged(cur);
                    return true;
            }
        }

        public void NotifyCurrencyChanged(Currency currency)
        {
            ChangedCurrency?.Invoke(currency);
        }

        public void UseCurrency(Currency cur, int use)
        {
            if (use < 0)
            {
                MyLogger.Log(LogType.Warning, LogSituation.UseCurreny, ProcessType.Failed, "UseCurrency Must Be Used Only Use");
                return;
            }

            switch (cur)
            {
                case Currency.Gold:
                    MyLogger.Log(LogType.Warning, LogSituation.UseCurreny, ProcessType.Failed, string.Format("{0} is not use this", cur.ToString()));
                    break;
                default:
                    string sCur = cur.ToString();

                    if (!Currencies.ContainsKey(sCur))
                    {
                        Currencies.Add(sCur, 0);
                    }

                    if (!TempCurrency.ContainsKey(sCur))
                    {
                        TempCurrency.Add(sCur, 0);
                    }

                    TempCurrency[sCur] = 0;
                    Currencies[sCur] -= use;
                    ChangedCurrency?.Invoke(cur);
                    break;
            }
        }

        public int GetCurrency(Currency cur)
        {
            switch (cur)
            {
                case Currency.Gold:
                    MyLogger.Log(LogType.Warning, LogSituation.GetCurrency, ProcessType.Failed, string.Format("{0} is not use this", cur.ToString()));
                    return 0;
                default:
                    string sCur = cur.ToString();

                    if (!Currencies.ContainsKey(sCur))
                    {
                        Currencies.Add(sCur, 0);
                    }

                    return Currencies[cur.ToString()];
            }
        }

        // 꾹 눌러서 사용하는 경우 UI에서는 값이 줄어야
        // 하지만 계산은 한번에 하기 위해 사용하는 함수
        public void SetTempCurrency(Currency cur, int use)
        {
            if (use < 0)
            {
                MyLogger.Log(LogType.Warning, LogSituation.UseCurreny, ProcessType.Failed, "UseCurrency Must Be Used Only Use");
                return;
            }

            switch (cur)
            {
                case Currency.Gold:
                    MyLogger.Log(LogType.Warning, LogSituation.UseCurreny, ProcessType.Failed, string.Format("{0} is not use this", cur.ToString()));
                    break;
                default:
                    string sCur = cur.ToString();

                    if (!TempCurrency.ContainsKey(sCur))
                    {
                        TempCurrency.Add(sCur, 0);
                    }

                    TempCurrency[sCur] = use;
                    ChangedCurrency?.Invoke(cur);
                    break;
            }
        }

        // 꾹 눌러서 사용할때 UI 표기를 해주기 위한 함수
        public int GetTempCurrency(Currency cur)
        {
            switch (cur)
            {
                case Currency.Gold:
                    MyLogger.Log(LogType.Warning, LogSituation.GetCurrency, ProcessType.Failed, string.Format("{0} is not use this", cur.ToString()));
                    return 0;
                default:
                    string sCur = cur.ToString();

                    if (!Currencies.ContainsKey(sCur))
                    {
                        Currencies.Add(sCur, 0);
                    }

                    if (!TempCurrency.ContainsKey(sCur))
                    {
                        TempCurrency.Add(sCur, 0);
                    }

                    return Currencies[sCur] - TempCurrency[sCur];
            }
        }
        #endregion

        #region stage
        public void SetCurStage(BattleType type, string sid)
        {
            if (!CurStages.ContainsKey(type))
            {
                CurStages.Add(type, 0);
            }

            CurStages[type] = int.Parse(sid);
        }

        public int GetCurStage(BattleType type)
        {
            if (!CurStages.ContainsKey(type))
            {
                CurStages.Add(type, StageCalculator.GetFirstStage(type));
            }

            return CurStages[type];
        }

        public void SetMaxStage(BattleType type, string sid)
        {
            if (!MaxStages.ContainsKey(type))
            {
                MaxStages.Add(type, 1);
            }

            MaxStages[type] = int.Parse(sid);
        }

        public int GetMaxStage(BattleType type)
        {
            if (!MaxStages.ContainsKey(type))
            {
                MaxStages.Add(type, StageCalculator.GetFirstStage(type));
            }

            return MaxStages[type];
        }
        #endregion

        #region purchase
        public void AddPurchase(string sid)
        {
            string limitType = Table.GetString(TableType.Shop, sid, "limit_type");

            switch (limitType)
            {
                case "Daily":
                    if (DailyPurchase.ContainsKey(sid))
                    {
                        DailyPurchase[sid]++;
                    }
                    else
                    {
                        DailyPurchase.Add(sid, 1);
                    }
                    break;
                case "Weekly":
                    if (WeeklyPurchase.ContainsKey(sid))
                    {
                        WeeklyPurchase[sid]++;
                    }
                    else
                    {
                        WeeklyPurchase.Add(sid, 1);
                    }
                    break;
                case "Monthly":
                    if (MonthlyPurchase.ContainsKey(sid))
                    {
                        MonthlyPurchase[sid]++;
                    }
                    else
                    {
                        MonthlyPurchase.Add(sid, 1);
                    }
                    break;

            }

            if (PurchaseAll.ContainsKey(sid))
            {
                PurchaseAll[sid]++;
            }
            else
            {
                PurchaseAll.Add(sid, 1);
            }
        }

        public bool IsPurchasable(string sid)
        {
            string limitType = Table.GetString(TableType.Shop, sid, "limit_type");
            string sLimitValue = Table.GetString(TableType.Shop, sid, "limit_value");

            if (sLimitValue == "x")
            {
                return true;
            }

            int limitValue = Table.GetInt(TableType.Shop, sid, "limit_value");
            int nowValue = 0;

            switch (limitType)
            {
                case "Daily":
                    if (!DailyPurchase.ContainsKey(sid))
                    {
                        return true;
                    }
                    nowValue = DailyPurchase[sid];
                    break;
                case "Weekly":
                    if (!WeeklyPurchase.ContainsKey(sid))
                    {
                        return true;
                    }
                    nowValue = WeeklyPurchase[sid];
                    break;
                case "Monthly":
                    if (!MonthlyPurchase.ContainsKey(sid))
                    {
                        return true;
                    }
                    nowValue = MonthlyPurchase[sid];
                    break;
                default:
                    if (!PurchaseAll.ContainsKey(sid))
                    {
                        return true;
                    }
                    nowValue = PurchaseAll[sid];
                    break;

            }

            return nowValue < limitValue;
        }

        public int GetPurcahsedCount(string sid)
        {
            string limitType = Table.GetString(TableType.Shop, sid, "limit_type");

            switch (limitType)
            {
                case "Daily":
                    if (DailyPurchase.ContainsKey(sid))
                    {
                        return DailyPurchase[sid];
                    }
                    else
                    {
                        return 0;
                    }
                case "Weekly":
                    if (WeeklyPurchase.ContainsKey(sid))
                    {
                        return WeeklyPurchase[sid];
                    }
                    else
                    {
                        return 0;
                    }
                case "Monthly":
                    if (MonthlyPurchase.ContainsKey(sid))
                    {
                        return MonthlyPurchase[sid];
                    }
                    else
                    {
                        return 0;
                    }
            }

            if (PurchaseAll.ContainsKey(sid))
            {
                return PurchaseAll[sid];
            }
            else
            {
                return 0;
            }
        }
        #endregion

        public int GetEquipGachaLv()
        {
            int sum = 0;
            int idx = 0;

            for (; ; idx++)
            {
                string key = (1001 + idx).ToString();

                if (Table.IsValidKey(TableType.EquipGacha, key))
                {
                    sum += Table.GetInt(TableType.EquipGacha, key, "need_count");

                    if (MyData.EquipGachaCount <= sum)
                    {
                        return Table.GetInt(TableType.EquipGacha, key, "lv");
                    }
                }
                else
                {
                    break;
                }
            }

            return idx;
        }

        public int GetEquipGachaCount()
        {
            int sum = 0;
            int idx = 0;

            for (; ; idx++)
            {
                string key = (1001 + idx).ToString();

                if (Table.IsValidKey(TableType.Shop, key))
                {
                    if (sum < MyData.EquipGachaCount && MyData.EquipGachaCount <= sum + Table.GetInt(TableType.Shop, key, "need_count"))
                    {
                        return MyData.EquipGachaCount - sum;
                    }

                    sum += Table.GetInt(TableType.Shop, key, "need_count");
                }
                else
                {
                    break;
                }
            }

            return MyData.EquipGachaCount - sum;
        }

        public void AddEquipGachaCount(int count = 1)
        {
            MyData.EquipGachaCount += count;
        }

        public long GetLastNum()
        {
            return MyData.LastTimeNum;
        }

        public void SetLastTimeNum(long num, bool isCheat = false)
        {
            if (num > MyData.LastTimeNum || isCheat)
            {
                MyData.LastTimeNum = num;
            }
        }
    }

    [Serializable]
    public class PlayerData : IBaseData<PlayerData>
    {
        public double Gold = 0d;
        public int CurStage = 0;
        public int MaxStage = 0;
        public int EquipGachaCount = 0;
        public string NickName = "";
        public long LastTimeNum = 0;

        public List<SerialKeyValue<string, int>> Currencies = new List<SerialKeyValue<string, int>>();

        public List<SerialKeyValue<string, int>> CurStages = new List<SerialKeyValue<string, int>>();
        public List<SerialKeyValue<string, int>> MaxStages = new List<SerialKeyValue<string, int>>();

        public List<SerialKeyValue<string, int>> DailyPurchase = new List<SerialKeyValue<string, int>>();
        public List<SerialKeyValue<string, int>> WeeklyPurchase = new List<SerialKeyValue<string, int>>();
        public List<SerialKeyValue<string, int>> MonthlyPurchase = new List<SerialKeyValue<string, int>>();
        public List<SerialKeyValue<string, int>> PurchaseAll = new List<SerialKeyValue<string, int>>();

        public void SetData(PlayerData beforeData)
        {
            Gold = beforeData.Gold;
            CurStage = beforeData.CurStage;
            MaxStage = beforeData.MaxStage;
            NickName = beforeData.NickName;
            EquipGachaCount = beforeData.EquipGachaCount;
            LastTimeNum = beforeData.LastTimeNum;
            Currencies = new List<SerialKeyValue<string, int>>(beforeData.Currencies);
            CurStages = new List<SerialKeyValue<string, int>>(beforeData.CurStages);
            MaxStages = new List<SerialKeyValue<string, int>>(beforeData.MaxStages);
            DailyPurchase = new List<SerialKeyValue<string, int>>(beforeData.DailyPurchase);
            WeeklyPurchase = new List<SerialKeyValue<string, int>>(beforeData.WeeklyPurchase);
            MonthlyPurchase = new List<SerialKeyValue<string, int>>(beforeData.MonthlyPurchase);
            PurchaseAll = new List<SerialKeyValue<string, int>>(beforeData.PurchaseAll);
        }

        public void SetToDefault()
        {
            Gold = 0d;
            CurStage = 0;
            MaxStage = 0;
            EquipGachaCount = 0;
            LastTimeNum = 0L;
            NickName = RandomWord.Generate();
            Currencies.Clear();
            CurStages.Clear();
            MaxStages.Clear();
            DailyPurchase.Clear();
            WeeklyPurchase.Clear();
            MonthlyPurchase.Clear();
            PurchaseAll.Clear();
        }
    }
}
