using System;
using System.Collections.Generic;

using UnityEngine;

using Game.Common;
using Game.Core;
using Game.Quests;

namespace Game.Data
{
    public class EquipDataMgr : SingletonAllTime<EquipDataMgr>
    {
        public static event Action ChangedEquip;
        public static event Action ChangedEquipValue;

        private EquipData MyEquip = new EquipData();
        private Dictionary<string, Equip> DicEquip = new Dictionary<string, Equip>();

        public string EquipId => MyEquip.EquipId;

        public void Equip(string eid)
        {
            MyEquip.EquipId = eid;
            ChangedEquip?.Invoke();
        }

        public void SetData(EquipData data)
        {
            MyEquip.SetData(data);
            DicEquip.Clear();

            foreach (var equip in MyEquip.Equips)
            {
                DicEquip.Add(equip.Id, equip);
            }
        }

        public EquipData GetData()
        {
            EquipData data = new EquipData();
            data.SetData(MyEquip);
            return data;
        }

        public bool IsHaveEquip(string id)
        {
            if (!DicEquip.ContainsKey(id))
            {
                return false;
            }

            if (DicEquip[id].Count == 0 && DicEquip[id].Star == 0)
            {
                return false;
            }

            return true;
        }

        public int GetEquipCount(string id)
        {
            if (!DicEquip.ContainsKey(id))
            {
                return 0;
            }

            return DicEquip[id].Count;
        }

        public int GetEquipLv(string id)
        {
            if (!DicEquip.ContainsKey(id))
            {
                return 0;
            }

            return DicEquip[id].Lv;
        }

        public int GetEquipStar(string id)
        {
            if (!DicEquip.ContainsKey(id))
            {
                return 0;
            }

            return DicEquip[id].Star;
        }

        public void AddEquip(string id, int count = 1)
        {
            if (count < 0)
            {
                return;
            }

            for (int i = 0; i < MyEquip.Equips.Count; i++)
            {
                if (MyEquip.Equips[i].Id == id)
                {
                    MyEquip.Equips[i].Count += count;
                    ChangedEquipValue?.Invoke();
                    return;
                }
            }

            Equip newEquip = new Equip();
            newEquip.Id = id;
            newEquip.Count = count;
            MyEquip.Equips.Add(newEquip);
            DicEquip.Add(newEquip.Id, newEquip);
            ChangedEquipValue?.Invoke();
        }

        public void AddEquip(List<string> getEquips)
        {
            if (getEquips == null || getEquips.Count == 0)
            {
                return;
            }

            foreach (string equip in getEquips)
            {
                bool isHave = false;
                for (int i = 0; i < MyEquip.Equips.Count; i++)
                {
                    if (MyEquip.Equips[i].Id == equip)
                    {
                        MyEquip.Equips[i].Count++;
                        isHave = true;
                        break;
                    }
                }

                if (isHave)
                {
                    continue;
                }

                Equip newEquip = new Equip();
                newEquip.Id = equip;
                newEquip.Count = 1;
                MyEquip.Equips.Add(newEquip);
                DicEquip.Add(newEquip.Id, newEquip);
            }

            ChangedEquipValue?.Invoke();
        }

        public void LevelUpEquip(string id, int levelUp = 1)
        {
            if (levelUp < 0)
            {
                return;
            }

            for (int i = 0; i < MyEquip.Equips.Count; i++)
            {
                if (MyEquip.Equips[i].Id == id)
                {
                    MyEquip.Equips[i].Lv += levelUp;
                    QuestMgr.Instance.ReportAction(QuestAction.EquipUpgrade, levelUp);
                    ChangedEquipValue?.Invoke();
                    return;
                }
            }
        }

        public void GradeUpEquip(string id)
        {
            if (DicEquip[id].Count < Constant.EquipAdvanceCount)
            {
                return;
            }

            int nextEid = int.Parse(id) + 1;

            if (!Table.IsValidKey(TableType.Equip, nextEid.ToString()))
            {
                return;
            }

            for (int i = 0; i < MyEquip.Equips.Count; i++)
            {
                if (MyEquip.Equips[i].Id == id)
                {
                    int addedCount = MyEquip.Equips[i].Count / Constant.EquipAdvanceCount;
                    MyEquip.Equips[i].Count = MyEquip.Equips[i].Count % Constant.EquipAdvanceCount;
                    AddEquip(nextEid.ToString(), addedCount);
                    QuestMgr.Instance.ReportAction(QuestAction.EquipGradeUp, addedCount);
                }
            }
        }

        public bool IsValidStarUp(string id)
        {
            if (!DicEquip.ContainsKey(id)
                || DicEquip[id].Count < Constant.EquipStarUpCount)
            {
                return false;
            }

            if (Table.IsValidColumn(TableType.Equip, string.Format("star_stat_{0}", DicEquip[id].Star + 1)))
            {
                string stat = Table.GetString(TableType.Equip, id, string.Format("star_stat_{0}", DicEquip[id].Star + 1));
                return stat != "x";
            }
            else
            {
                return false;
            }
        }

        public void StarUp(string id)
        {
            if (!IsValidStarUp(id))
            {
                return;
            }

            for (int i = 0; i < MyEquip.Equips.Count; i++)
            {
                if (MyEquip.Equips[i].Id == id)
                {
                    string stat = Table.GetString(TableType.Equip, id, string.Format("star_stat_{0}", MyEquip.Equips[i].Star + 1));

                    if (stat != "x")
                    {
                        MyEquip.Equips[i].Star++;
                        MyEquip.Equips[i].Count -= Constant.EquipStarUpCount;
                        ChangedEquipValue?.Invoke();
                    }
                }
            }
        }
    }

    [Serializable]
    public class EquipData : IBaseData<EquipData>
    {
        public List<Equip> Equips = new List<Equip>();
        public string EquipId = "";

        public void SetData(EquipData beforeData)
        {
            Equips.Clear();
            Equips = new List<Equip>(beforeData.Equips);
            EquipId = beforeData.EquipId;
        }

        public void SetToDefault()
        {
            Equips.Clear();
            EquipId = "";
        }
    }

    [Serializable]
    public class Equip
    {
        public string Id = "1001";
        public int Count = 0;
        public int Star = 0;
        public int Lv = 0;

        public static int UpgradeNeed(string id, int lv)
        {
            return Mathf.RoundToInt(Table.GetInt(TableType.Equip, id, "need_value")
                * (Mathf.Pow(Table.GetFloat(TableType.Equip, id, "upgrade_rate"), lv)));
        }

        public static float UpgeadedStat(string id, float baseStat, int lv)
        {
            return baseStat + baseStat * (Table.GetFloat(TableType.Equip, id, "upgrade_rate") - 1) * lv;
        }
    }
}
