using System;
using System.Collections.Generic;

using UnityEngine;

using Game.Battle.Missile;
using Game.Common;
using Game.Core;
using Game.Pooling;
using Game.UI;

namespace Game.Data
{
    public class SkillDataMgr : SingletonAllTime<SkillDataMgr>
    {
        public static event Action ChangedSkillEquip;
        public static event Action<HashSet<string>> ChangedSkill;
        public IReadOnlyList<string> Equips => NowEquipSkill;
        public int EquipPreset => MySkill.NowPreset;

        private SkillData MySkill = new SkillData();
        private List<string> NowEquipSkill = new List<string>();
        private Dictionary<string, Skill> DicSkill = new Dictionary<string, Skill>();

        public void SetData(SkillData data)
        {
            MySkill.SetData(data);
            NowEquipSkill = new List<string>(MySkill.Preset[MySkill.NowPreset]);

            DicSkill.Clear();
            foreach (var skill in MySkill.Skills)
            {
                DicSkill.Add(skill.Id, skill);
            }
        }

        public SkillData GetData()
        {
            SkillData data = new SkillData();
            data.SetData(MySkill);
            return data;
        }

        public void PresetChange(int idx)
        {
            if (idx >= Constant.SkillPresetCount)
            {
                MyLogger.Log(LogType.Error, LogSituation.Skill, ProcessType.Failed, "wrong preset");
                return;
            }

            MySkill.NowPreset = idx;
            NowEquipSkill.Clear();
            NowEquipSkill = new List<string>(MySkill.Preset[MySkill.NowPreset]);
            for (int i = 0; i < NowEquipSkill.Count; i++)
            {
                string path = Table.GetString(TableType.Skill, NowEquipSkill[i], "missile_res");

                if (!PoolMgr.Instance.IsContain(BaseMissile.GetKey(path)))
                {
                    PoolMgr.Instance.Register<BaseMissile>(BaseMissile.GetKey(path), path, 2);
                }
            }
        }

        public void EquipSkill(string sid, int preset, int idx)
        {
            if (idx < 0 || idx >= Constant.SkillEquipCount)
            {
                MyLogger.Log(LogType.Error, LogSituation.Skill, ProcessType.Failed, "wrong idx");
                return;
            }

            if (preset < 0 || preset >= Constant.SkillPresetCount)
            {
                MyLogger.Log(LogType.Error, LogSituation.Skill, ProcessType.Failed, "wrong preset");
                return;
            }

            // Preset은 List.Add(Preset1)이런식으로 되어 있어서 같은 주소를 사용한다.
            // 그렇기에 Preset을 바꿔도 그 밑의 Preset1~6의 값이 바뀐다. 
            while (MySkill.Preset[preset].Count <= idx)
            {
                MySkill.Preset[preset].Add("");
            }

            MySkill.Preset[preset][idx] = sid;
            NowEquipSkill.Clear();
            NowEquipSkill = new List<string>(MySkill.Preset[MySkill.NowPreset]);

            string path = Table.GetString(TableType.Skill, sid, "missile_res");

            if (!PoolMgr.Instance.IsContain(BaseMissile.GetKey(path)))
            {
                PoolMgr.Instance.Register<BaseMissile>(BaseMissile.GetKey(path), path, 2);
            }

            ChangedSkillEquip?.Invoke();
        }

        public bool IsHave(string sid)
        {
            if (!DicSkill.ContainsKey(sid))
            {
                return false;
            }

            return !(DicSkill[sid].Count == 0 && DicSkill[sid].Star == 0);
        }

        public bool IsEquiped(string sid)
        {
            return NowEquipSkill.Contains(sid);
        }

        public int GetSkillCount(string sid)
        {
            if (!DicSkill.ContainsKey(sid))
            {
                return 0;
            }

            return DicSkill[sid].Count - Skill.StarToCount(DicSkill[sid].Star);
        }

        public int GetSkillLv(string sid)
        {
            if (!DicSkill.ContainsKey(sid))
            {
                return 0;
            }

            return DicSkill[sid].Level;
        }

        public int GetMaxSkillLv(string sid)
        {
            if (!DicSkill.ContainsKey(sid))
            {
                return 0;
            }

            return Constant.SkillMaxLv * (1 + DicSkill[sid].Star);
        }

        public int GetSkillStar(string sid)
        {
            if (!DicSkill.ContainsKey(sid))
            {
                return 0;
            }

            return DicSkill[sid].Star;
        }

        public List<string> GetSkillOnPresetIdx(int preset)
        {
            if (preset >= Constant.SkillPresetCount || preset < 0)
            {
                MyLogger.Log(LogType.Error, LogSituation.Skill, ProcessType.Failed, "wrong preset");
                return null;
            }

            return new List<string>(MySkill.Preset[preset]);
        }

        public void AddSkill(List<string> skills)
        {
            if (skills == null || skills.Count == 0)
            {
                return;
            }

            HashSet<string> changedSkills = new HashSet<string>();

            foreach (string skill in skills)
            {
                bool isHave = false;
                changedSkills.Add(skill);

                for (int i = 0; i < MySkill.Skills.Count; i++)
                {
                    if (MySkill.Skills[i].Id == skill)
                    {
                        MySkill.Skills[i].Count++;
                        isHave = true;
                        break;
                    }
                }

                if (!isHave)
                {
                    Skill newSkill = new Skill();
                    newSkill.Id = skill;
                    newSkill.Count = 1;
                    MySkill.Skills.Add(newSkill);
                    DicSkill.Add(skill, newSkill);
                }
            }

            ChangedSkill?.Invoke(changedSkills);
        }

        public void LevelUp(string sid, int count = 1)
        {
            if (count <= 0)
            {
                return;
            }

            for (int i = 0; i < MySkill.Skills.Count; i++)
            {
                if (MySkill.Skills[i].Id == sid)
                {
                    MySkill.Skills[i].Level += count;
                    ChangedSkill?.Invoke(new HashSet<string>() { sid });
                    return;
                }
            }
        }

        public void StarUp(string sid, int count = 1)
        {
            if (count <= 0)
            {
                return;
            }

            for (int i = 0; i < MySkill.Skills.Count; i++)
            {
                if (MySkill.Skills[i].Id == sid)
                {
                    int needCount = 0;

                    if (MySkill.Skills[i].Star + count > Constant.SkillMaxStar)
                    {
                        count = Constant.SkillMaxStar - MySkill.Skills[i].Star;
                    }

                    do
                    {
                        // 얻기 힘든 재화이고, 레벨업 계열이니, 나중에 레벨업에 필요한 재화량이 달라졌을때를 대비한 방식
                        needCount = Skill.StarToCount(MySkill.Skills[i].Star + count);

                        if (MySkill.Skills[i].Count > needCount)
                        {
                            MySkill.Skills[i].Star += count;
                            ChangedSkill?.Invoke(new HashSet<string>() { sid });
                            return;
                        }
                        count--;
                    }
                    while (count >= 1);
                }
            }
        }
    }

    [Serializable]
    public class SkillData : IBaseData<SkillData>
    {
        public List<Skill> Skills = new List<Skill>();
        public List<List<string>> Preset = new List<List<string>>();
        public List<string> Preset1 = new List<string>();
        public List<string> Preset2 = new List<string>();
        public List<string> Preset3 = new List<string>();
        public List<string> Preset4 = new List<string>();
        public List<string> Preset5 = new List<string>();
        public List<string> Preset6 = new List<string>();
        public int NowPreset = 0;

        public void SetData(SkillData beforeData)
        {
            Skills.Clear();
            Skills = new List<Skill>(beforeData.Skills);
            Preset1.Clear();
            Preset1 = new List<string>(beforeData.Preset1);
            Preset2.Clear();
            Preset2 = new List<string>(beforeData.Preset2);
            Preset3.Clear();
            Preset3 = new List<string>(beforeData.Preset3);
            Preset4.Clear();
            Preset4 = new List<string>(beforeData.Preset4);
            Preset5.Clear();
            Preset5 = new List<string>(beforeData.Preset5);
            Preset6.Clear();
            Preset6 = new List<string>(beforeData.Preset6);
            Preset.Clear();
            Preset.Add(Preset1);
            Preset.Add(Preset2);
            Preset.Add(Preset3);
            Preset.Add(Preset4);
            Preset.Add(Preset5);
            Preset.Add(Preset6);
            NowPreset = beforeData.NowPreset;
        }

        public void SetToDefault()
        {
            Skills.Clear();
            Preset.Clear();
            Preset1.Clear();
            Preset2.Clear();
            Preset3.Clear();
            Preset4.Clear();
            Preset5.Clear();
            Preset6.Clear();
            NowPreset = 0;
        }
    }

    [Serializable]
    public class Skill
    {
        public string Id = "1001";
        public int Level = 0;
        public int Count = 0;
        public int Star = 0;

        public static int StarToCount(int star)
        {
            int sum = 0;

            for (int i = 0; i < star; i++)
            {
                sum += Constant.SkillStarUpNeed[i];
            }

            return sum;
        }
    }
}
