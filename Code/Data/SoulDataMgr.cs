using Game.Calculate;
using Game.Common;
using Game.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Data
{
    public class SoulDataMgr : SingletonAllTime<SoulDataMgr>
    {
        public enum SoulCondition
        {
            None, Lv, Grade
        }

        public static event Action AllSoulChanged;
        public static event Action<string> SoulChanged;

        private SoulData MyData = new SoulData();
        private Dictionary<string, Soul> MySouls = new Dictionary<string, Soul>();
        private List<string> Equiped = new List<string>();

        public void SetData(SoulData data)
        {
            MyData.SetData(data);
            
            MySouls.Clear();
            foreach (var soul in data.Souls) 
            {
                MySouls.Add(soul.Id, soul);
            }

            Equiped.Clear();
            Equiped.AddRange(data.EquipSoul);
        }

        public SoulData GetData()
        {
            List<Soul> souls = new List<Soul>();
            foreach (var soul in MySouls.Values)
            {
                souls.Add(soul);
            }
            MyData.Souls = souls;
            MyData.EquipSoul = new List<string>(Equiped);

            SoulData data = new SoulData();
            data.SetData(MyData);
            return data;
        }

        public void EquipSoul(string key)
        {
            string sid = MySouls[key].SId;
            int iSid = int.Parse(sid);
            int num = iSid / 1000;
            Equiped[num - 1] = key;
            AllSoulChanged?.Invoke();

        }

        public bool IsEquiped(string key)
        {
            for (int i = 0; i < Equiped.Count; i++)
            {
                if (Equiped[i] == key)
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerable<string> GetSoulsKeyByCondition(params (SoulCondition condition, int value)[] conditions)
        {
            foreach (var keyValue in MySouls)
            {
                bool isChecked = true;

                foreach (var data in conditions)
                {
                    switch (data.condition)
                    {
                        case SoulCondition.Grade:
                            if (keyValue.Value.Grade < data.value)
                            {
                                isChecked = false;
                            }
                            break;
                        case SoulCondition.Lv:
                            if (keyValue.Value.Level < data.value)
                            {
                                isChecked = false;
                            }
                            break;
                    }

                    if (!isChecked)
                    {
                        break;
                    }
                }

                if (isChecked)
                {
                    yield return keyValue.Key;
                }
            }
        }

        public string GetSoulID(string id)
        {
            return MySouls[id].SId;
        }
        
        public int GetSoulGrade(string id)
        {
            return MySouls[id].Grade;
        }

        public int GetSoulLv(string id)
        {
            return MySouls[id].Level;
        }

        public List<string> GetSoulSubOption(string id)
        {
            return new List<string>(MySouls[id].SubOptions);
        }

        public void AddSoul(List<string> newSoul, List<int> newGrade)
        {
            for (int i = 0; i < newSoul.Count; i++)
            {
                string id = Guid.NewGuid().ToString();
                Soul soul = new Soul(newSoul[i], id, newGrade[i]);
                MySouls.Add(id, soul);
            }

            AllSoulChanged?.Invoke();
        }

        public List<string> UpgradeSoul(string id, int lv = 1)
        {
            if (lv < 0)
            {
                Debug.LogError("upgrade fail, lv is upper then 0");
                return null;
            }

            if (!MySouls.ContainsKey(id))
            {
                Debug.LogError("upgrade fail, no key");
                return null;
            }

            int beforeLv = MySouls[id].Level;
            MySouls[id].Level += lv;
            int afterLv = MySouls[id].Level;
            SoulChanged?.Invoke(id);
            return AddedSubOptions(id, beforeLv, afterLv);
        }

        private List<string> AddedSubOptions(string id, int beforeLv, int afterLv)
        {
            int beforeAddedSubOptionCount = beforeLv / Constant.SoulSubOptionAddedLvInterval;
            int afterAddedSubOptionCount = afterLv / Constant.SoulSubOptionAddedLvInterval;
            List<string> result = new List<string>();

            for (int i = 0; i < afterAddedSubOptionCount - beforeAddedSubOptionCount; i++)
            {
                result.Add(MySouls[id].AddSubOption());
            }

            return result;
        }

        public void DecomposeSoul(HashSet<string> id) 
        {
            int sum = 0;

            foreach (string key in id)
            {
                sum += DecomposeSoul(key, false);
            }

            PlayerDataMgr.Instance.TryAddCurrency(Currency.SoulUp, sum);
            AllSoulChanged?.Invoke();
        }

        public int DecomposeSoul(string id, bool isUseAction = true)
        {
            int result = Soul.GetDecomposeValue(MySouls[id].Grade, MySouls[id].Level);
            
            // Action이 필요한 경우만 재화 변동이 일어나도록 하자.
            if (isUseAction)
            {
                PlayerDataMgr.Instance.TryAddCurrency(Currency.SoulUp, result);
                AllSoulChanged?.Invoke();
            }

            return result;
        }

        #region TestValue Only Use
        public void AddSoulOnlyTest(int grade, int count)
        {
            List<string> soulKeys = new List<string>(Table.GetTable(TableType.Soul).Keys);
            List<string> empty = new List<string>();

            for (int i = 0; i < count; i++)
            {
                string id = Guid.NewGuid().ToString();
                int idx = GachaCalculator.GachaIdx(soulKeys, empty);
                string sid = soulKeys[idx];
                Soul soul = new Soul(sid, id, grade);
                MySouls.Add(id, soul);
            }
        }

        public void RemoveAllSoul()
        {
            MySouls.Clear();
            for (int i = 0; i < Equiped.Count; i++)
            {
                Equiped[i] = "";
            }
        }
        #endregion
    }

    [Serializable]
    public class SoulData : IBaseData<SoulData>
    {
        public List<Soul> Souls = new List<Soul>();
        public List<string> EquipSoul = new List<string>();

        public void SetData(SoulData beforeData)
        {
            Souls.Clear();
            EquipSoul.Clear();
            Souls = new List<Soul>(beforeData.Souls);
            EquipSoul = new List<string>(beforeData.EquipSoul);
        }

        public void SetToDefault()
        {
            Souls.Clear();
            EquipSoul.Clear();
            // 3개만 사용할 거지만 넉넉하게 10개
            for (int i = 0; i < 10; i++)
            {
                EquipSoul.Add("");
            }
        }
    }

    [Serializable]
    public class Soul
    {
        public string Id = "";
        public string SId = "1001";
        public int Level = 0;
        public int Grade = 0;
        public List<string> SubOptions = new List<string>();

        public int ValidSubOptionCount
        {
            get
            {
                return Grade - 1 + Level / Constant.SoulSubOptionAddedLvInterval;
            }
        }

        public Soul(string sid, string id, int grade)
        {
            SId = sid;
            Id = id;
            Grade = grade;
        }

        public string AddSubOption()
        {
            if (SubOptions.Count < ValidSubOptionCount)
            {
                return "";
            }
            else
            {
                string added = "";
                List<string> subOptions = SubOptions.Distinct().ToList();

                if (subOptions.Count < Constant.SoulSubOptionMaxCount)
                {
                    List<string> keys = new(Table.GetTable(TableType.SoulSubOption).Keys);
                    int idx = GachaCalculator.GachaIdx(keys, subOptions);
                    added = keys[idx];
                }
                else
                {
                    int rand = UnityEngine.Random.Range(0, subOptions.Count);
                    added = subOptions[rand];
                }

                SubOptions.Add(added);

                return added;
            }
        }

        public static float GetSoulMainValue(int grade, int lv)
        {
            return 1f;
        }

        public static float GetSoulSubValue(int grade, int count)
        {
            return 1f;
        }

        public static int GetSoulLevelUpNeed(int grade, int lv)
        {
            return 1;
        }

        public static int GetDecomposeValue(int grade, int lv)
        {
            return 1;
        }
    }
}