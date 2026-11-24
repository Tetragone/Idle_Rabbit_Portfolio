using System.Collections.Generic;

using UnityEngine;

using Game.Common;
using Game.Data;

namespace Game.Calculate
{
    public static class GachaCalculator
    {
        private static int EquipGachaMaxLv = 0;
        public static int MaxEquipGachaLv
        {
            get
            {
                if (EquipGachaMaxLv == 0)
                {
                    foreach (var gid in Table.GetTable(TableType.EquipGacha).Keys)
                    {
                        EquipGachaMaxLv = Mathf.Max(EquipGachaMaxLv, Table.GetInt(TableType.EquipGacha, gid, "lv"));
                    }
                }

                return EquipGachaMaxLv;
            }
        }

        public static int GachaIdx<T>(List<T> values, List<T> exception)
        {
            List<T> refined = new List<T>();

            for (int i = 0; i < values.Count; i++)
            {
                if (!exception.Contains(values[i]))
                {
                    refined.Add(values[i]);
                }
            }

            int rand = Random.Range(0, refined.Count);
            
            for (int i = 0; i < values.Count; i++)
            {
                if (refined[rand].Equals(values[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        public static List<int> GachaIdx(List<int> rates, int count)
        {
            List<int> ret = new List<int>();

            for (int i = 0; i < count; i++)
            {
                ret.Add(0);
            }

            int sum = 0;

            for (int i = 0; i < rates.Count; i++)
            {
                sum += rates[i];
            }

            List<int> rands = new List<int>();

            for (int i = 0; i < count; i++)
            {
                rands.Add(Random.Range(0, sum));
            }

            int now = 0;

            for (int i = 0; i < rates.Count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    if (now <= rands[j] && rands[j] < now + rates[i])
                    {
                        ret[j] = i;
                    }
                }

                now += rates[i];
            }

            return ret;
        }

        public static List<string> GachaEquip(int count, int lv)
        {
            List<string> result = new List<string>();
            string key = "";

            foreach (var gid in Table.GetTable(TableType.EquipGacha).Keys)
            {
                if (lv == Table.GetInt(TableType.EquipGacha, gid, "lv"))
                {
                    key = gid;
                    break;
                }
            }

            List<int> rates = new List<int>();

            for (int i = 0; ; i++)
            {
                var columnNum = GetColumnNum(i);
                string column = string.Format("grade_{0}_{1}", columnNum.grade, columnNum.order);

                if (Table.IsValidColumn(TableType.EquipGacha, column))
                {
                    rates.Add(Table.GetInt(TableType.EquipGacha, key, column));
                }
                else
                {
                    break;
                }
            }

            List<int> idxes = GachaIdx(rates, count);

            for (int i = 0; i < idxes.Count; i++)
            {
                result.Add((1001 + idxes[i]).ToString());
            }

            return result;
        }

        public static Dictionary<string, float> GetGachaEquipRate(int lv)
        {
            Dictionary<string, float> result = new Dictionary<string, float>();
            string key = "";

            foreach (var gid in Table.GetTable(TableType.EquipGacha).Keys)
            {
                if (lv == Table.GetInt(TableType.EquipGacha, gid, "lv"))
                {
                    key = gid;
                    break;
                }
            }

            int sum = 0;

            for (int i = 0; ; i++)
            {
                var columnNum = GetColumnNum(i);
                string column = string.Format("grade_{0}_{1}", columnNum.grade, columnNum.order);

                if (Table.IsValidColumn(TableType.EquipGacha, column))
                {
                    result.Add((1001 + i).ToString(), Table.GetInt(TableType.EquipGacha, key, column));
                    sum += Table.GetInt(TableType.EquipGacha, key, column);
                }
                else
                {
                    break;
                }
            }

            for (int i = 0; ; i++)
            {
                string eid = (1001 + i).ToString();
                if (result.ContainsKey(eid))
                {
                    result[eid] = result[eid] / (float)sum;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private static List<int> EquipColumnData = new List<int>();
        private static List<int> EquipColumns
        {
            get
            {
                if (EquipColumnData.Count == 0)
                {
                    EquipColumnData.Add(0);

                    for (int i = 0; ; i++)
                    {
                        string column = string.Format("grade_{0}_1", i + 1);

                        if (!Table.IsValidColumn(TableType.EquipGacha, column))
                        {
                            break;
                        }

                        EquipColumnData.Add(0);

                        for (int j = 0; ; j++)
                        {
                            column = string.Format("grade_{0}_{1}", i + 1, j + 1);

                            if (Table.IsValidColumn(TableType.EquipGacha, column))
                            {
                                EquipColumnData[i + 1]++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                return EquipColumnData;
            }
        }

        private static (int grade, int order) GetColumnNum(int idx)
        {
            for (int i = 1; i < EquipColumns.Count; i++)
            {
                if (idx < EquipColumns[i])
                {
                    return (i, idx + 1);
                }
                else
                {
                    idx -= EquipColumns[i];
                }
            }

            return (0, 0);
        }

        private static int GetIndex(int grade, int order)
        {
            int ret = 0;

            for (int i = 0; i <= grade; i++)
            {
                ret += EquipColumns[i];
            }

            return ret + order;
        }

        public static GachaGrade GetGachaGrade(GachaType type, string id, int lv)
        {
            switch (type)
            {
                case GachaType.Equip:
                    return GetEquipGachaGrade(id, lv);
                case GachaType.Skill:
                    return GetSkillGachaGrade(id);
            }

            return GachaGrade.Normal;
        }

        private static GachaGrade GetEquipGachaGrade(string id, int lv)
        {
            string key = "";

            foreach (var gid in Table.GetTable(TableType.EquipGacha).Keys)
            {
                if (lv == Table.GetInt(TableType.EquipGacha, gid, "lv"))
                {
                    key = gid;
                    break;
                }
            }

            int grade = Table.GetInt(TableType.Equip, id, "grade");
            int order = Table.GetInt(TableType.Equip, id, "order");
            int rate = Table.GetInt(TableType.EquipGacha, key, string.Format("grade_{0}_{1}", grade, order));
            int sum = 0;
            int endGrade = 0;
            int endOrder = 0;

            for (int i = 0; ; i++)
            {
                var columnNum = GetColumnNum(i);
                string column = string.Format("grade_{0}_{1}", columnNum.grade, columnNum.order);

                if (Table.IsValidColumn(TableType.EquipGacha, column))
                {
                    sum += Table.GetInt(TableType.EquipGacha, key, column);

                    if (Table.GetInt(TableType.EquipGacha, key, column) > 0)
                    {
                        endGrade = columnNum.grade;
                        endOrder = columnNum.order;
                    }
                }
                else
                {
                    break;
                }
            }

            bool lastFive = GetIndex(endGrade, endOrder) - 5 <= GetIndex(grade, order);
            bool lastTwo = GetIndex(endGrade, endOrder) - 2 <= GetIndex(grade, order);

            if (grade < 3)
            {
                if (lastTwo)
                {
                    return GachaGrade.Good;
                }
            }
            else
            {
                if (lastTwo)
                {
                    return GachaGrade.Great;
                }
                else if (lastFive)
                {
                    return GachaGrade.Good;
                }
            }

            return GachaGrade.Normal;
        }

        private static GachaGrade GetSkillGachaGrade(string id)
        {
            int grade = Table.GetInt(TableType.Skill, id, "grade");

            if (grade == 5)
            {
                return GachaGrade.Great;
            }
            else if (grade >= 3)
            {
                return GachaGrade.Good;
            }
            else
            {
                return GachaGrade.Normal;
            }
        }


        private static List<List<string>> SkillByGradeData = new List<List<string>>();
        private static List<List<string>> SkillByGrade
        {
            get
            {
                if (SkillByGradeData.Count == 0)
                {
                    int maxGrade = 0;
                    foreach (string sid in Table.GetTable(TableType.Skill).Keys)
                    {
                        int grade = Table.GetInt(TableType.Skill, sid, "grade");
                        maxGrade = Mathf.Max(maxGrade, grade);
                    }

                    for (int i = 0; i < maxGrade; i++)
                    {
                        SkillByGradeData.Add(new List<string>());
                    }

                    foreach (string sid in Table.GetTable(TableType.Skill).Keys)
                    {
                        int grade = Table.GetInt(TableType.Skill, sid, "grade");
                        SkillByGradeData[grade - 1].Add(sid);
                    }
                }

                return SkillByGradeData;
            }
        }

        public static List<string> GachaSkill(int count)
        {
            List<string> result = new List<string>();
            string key = "";

            foreach (string gid in Table.GetTable(TableType.Gachas).Keys)
            {
                if (Table.GetString(TableType.Gachas, gid, "type") == "skill")
                {
                    key = gid;
                    break;
                }
            }

            List<int> rates = new List<int>();

            for (int i = 0; ; i++)
            {
                string column = string.Format("grade_{0}", i + 1);

                if (Table.IsValidColumn(TableType.Gachas, column))
                {
                    rates.Add(Table.GetInt(TableType.Gachas, key, column));
                }
                else
                {
                    break;
                }
            }

            List<int> idxes = GachaIdx(rates, count);

            for (int i = 0; i < idxes.Count; i++)
            {
                int skillsCount = SkillByGrade[idxes[i]].Count;
                int rand = Random.Range(0, skillsCount);
                result.Add(SkillByGrade[idxes[i]][rand]);
            }

            return result;
        }
    }
}
