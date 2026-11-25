using System;
using System.Collections.Generic;

using UnityEngine;

using Game.Common;
using Game.Data;
using Game.Battle;

namespace Game.Calculate
{
    public static class StatCalculator
    {
        public static void UpdateStatValue(Dictionary<string, float> statsAdd, Dictionary<string, double> statsMulti, EquipData equips, string equipId, SkillData skills, UpgradeData upgrades)
        {
            statsAdd.Clear();
            statsMulti.Clear();

            foreach (string key in Table.GetTable(TableType.Stat).Keys)
            {
                if (Table.GetString(TableType.Stat, key, "calculate_type") == "add")
                {
                    statsAdd.Add(key, 0f);
                }
                else
                {
                    statsMulti.Add(key, 1d);
                }
            }

            #region Equip Calculate
            foreach (Equip equip in equips.Equips)
            {
                string key = equip.Id;

                // 장착 스탯
                if (key == equipId)
                {
                    for (int i = 0; ; i++)
                    {
                        string colume = string.Format("equip_stat_{0}", i + 1);

                        if (!Table.IsValidColumn(TableType.Equip, colume))
                        {
                            break;
                        }

                        string statType = Table.GetString(TableType.Equip, key, colume);

                        if (statType == "x")
                        {
                            break;
                        }

                        float baseValue = Table.GetFloat(TableType.Equip, key, string.Format("equip_pow_{0}", i + 1));
                        float value = Equip.UpgeadedStat(key, baseValue, equip.Lv);
                        UpdateDicValue(statType, value, statsAdd, statsMulti);
                    }
                }


                // 패시브 스탯
                for (int i = 0; ; i++)
                {
                    string colume = string.Format("passive_stat_{0}", i + 1);

                    if (!Table.IsValidColumn(TableType.Equip, colume))
                    {
                        break;
                    }

                    string statType = Table.GetString(TableType.Equip, key, colume);

                    if (statType == "x")
                    {
                        break;
                    }

                    float baseValue = Table.GetFloat(TableType.Equip, key, string.Format("passive_pow_{0}", i + 1));
                    float value = Equip.UpgeadedStat(key, baseValue, equip.Lv);
                    UpdateDicValue(statType, value, statsAdd, statsMulti);
                }

                // 각성 스탯
                string starColumn = string.Format("star_stat_{0}", equip.Star);

                if (Table.IsValidColumn(TableType.Equip, starColumn))
                {
                    string statType = Table.GetString(TableType.Equip, key, starColumn);
                    if (statType != "x")
                    {
                        float value = Table.GetFloat(TableType.Equip, key, string.Format("star_pow_{0}", equip.Star));
                        UpdateDicValue(statType, value, statsAdd, statsMulti);
                    }
                }
            }
            #endregion

            #region Skill Calculate
            foreach (Skill skill in skills.Skills)
            {
                string key = skill.Id;

                string statType = Table.GetString(TableType.Skill, key, "passive_stat");
                if (statType != "x")
                {
                    float value = Table.GetFloat(TableType.Skill, key, "passive_value");
                    UpdateDicValue(statType, value, statsAdd, statsMulti);
                }

                string starStatType = Table.GetString(TableType.Skill, key, "star_stat");
                if (starStatType != "x")
                {
                    float talbeValue = Table.GetFloat(TableType.Skill, key, "star_value");
                    float value = Formula.SkillStarValue(key, skill.Star);
                    UpdateDicValue(starStatType, value, statsAdd, statsMulti);
                }
            }
            #endregion

            #region Upgrade Calculate
            foreach (var data in upgrades.UpgradeLvs)
            {
                string statType = Table.GetString(TableType.Upgrade, data.Key, "type");
                float value = UpgradeData.GetValue(data.Key, data.Value);
                UpdateDicValue(statType, value, statsAdd, statsMulti);
            }
            #endregion
        }

        private static void UpdateDicValue(string type, float value, Dictionary<string, float> add, Dictionary<string, double> multi)
        {
            if (Table.GetString(TableType.Stat, type, "calculate_type") == "add")
            {
                if (!add.ContainsKey(type))
                {
                    add.Add(type, 0f);
                }

                add[type] += value;
            }
            else
            {
                if (!multi.ContainsKey(type))
                {
                    multi.Add(type, 1d);
                }

                multi[type] *= (1d + value * 0.01d);
            }
        }

        public static BattleStat CalculatePlayer(Dictionary<string, float> statAdd, Dictionary<string, double> statMulti)
        {
            return new BattleStat
            {
                Atk = Constant.BaseAtk + statAdd["atk_add"] * statMulti["atk_multi"],
                MaxHp = Constant.BaseHp + statAdd["hp_add"] * statMulti["hp_multi"],
                Def = 0f,
                Cri = Constant.BaseCri + statAdd["cri"],
                CriDamage = Constant.BaseCriDam + (float)(statAdd["cridam_add"] * statMulti["cridam_multi"]),
                Spd = Constant.BaseSpd * (1 + statAdd["spd"]),
                AtkSpd = Constant.BaseAtkSpd * (1 + statAdd["atk_spd"]),
                MaxMp = Constant.BaseMp,
                CoolTimeReduce = Constant.BaseCooltimeReduce,
            };
        }

        public static BattleStat ApplyRoguelikeEffects(BattleStat baseStat, IReadOnlyList<string> effectIds)
        {
            var values = new Dictionary<string, double>();

            foreach (string id in effectIds)
            {
                string stat = Table.GetString(TableType.RoguelikeEffect, id, "stat");
                double value = Table.GetFloat(TableType.RoguelikeEffect, id, "value");
                values.TryGetValue(stat, out double current);
                values[stat] = current + value;
            }

            // 선택 효과는 기본 전투 스탯에 적용하며, 같은 능력치의 증가율은 합산한다.
            foreach (var pair in values)
            {
                switch (pair.Key)
                {
                    case "atk_multi": baseStat.Atk *= 1 + pair.Value / 100d; break;
                    case "hp_multi": baseStat.MaxHp *= 1 + pair.Value / 100d; break;
                    case "atk_spd": baseStat.AtkSpd *= 1 + (float)pair.Value / 100f; break;
                    case "cri": baseStat.Cri = Mathf.Clamp(baseStat.Cri + (float)pair.Value, 0, 100); break;
                    case "cridam_add": baseStat.CriDamage += (float)pair.Value; break;
                    default: throw new InvalidOperationException($"지원하지 않는 로그라이크 능력치: {pair.Key}");
                }
            }

            return baseStat;
        }

        public static BattleStat CalculateEnemy(BattleType type, string sid, string eid)
        {
            switch (type)
            {
                case BattleType.Normal:
                case BattleType.Challenge:
                    return CalculateNormalEnmey(sid, eid);
                case BattleType.GoldDungeon:
                    return CalculateGoldDungeonEnemy(sid, eid);
                case BattleType.RogueDungeon:
                    BattleStat stat = CalculateGoldDungeonEnemy(sid, eid);

                    if (!BattleMgr.Instance.TryGetRoguelike(out var stage))
                    {
                        throw new InvalidOperationException("로그라이크 적의 웨이브 정보를 찾을 수 없습니다.");
                    }

                    stat.Atk *= stage.CurrentWave.AtkMulti;
                    stat.MaxHp *= stage.CurrentWave.HpMulti;
                    return stat;
                default:
                    return CalculateNormalEnmey(sid, eid);
            }
        }

        private static BattleStat CalculateNormalEnmey(string sid, string eid)
        {
            int nowStage = Table.GetInt(TableType.Stage, sid, "stage");
            int nowChapter = Table.GetInt(TableType.Stage, sid, "chapter");
            double atk = Math.Pow(10000f, nowChapter - 1) * Math.Pow(1.5f, nowStage) * Table.GetFloat(TableType.Enemy, eid, "atk");
            double maxHp = Math.Pow(10000f, nowChapter - 1) * Math.Pow(1.5f, nowStage) * Table.GetFloat(TableType.Enemy, eid, "hp");
            float def = 0f;
            float cri = Table.GetFloat(TableType.Enemy, eid, "cri");
            float criDamage = Table.GetFloat(TableType.Enemy, eid, "cri_dam");
            float spd = Table.GetFloat(TableType.Enemy, eid, "spd");

            return new BattleStat
            {
                Atk = atk,
                MaxHp = maxHp,
                Def = def,
                Cri = cri,
                CriDamage = criDamage,
                Spd = spd,
                AtkSpd = Constant.BaseAtkSpd,
            };
        }

        private static BattleStat CalculateGoldDungeonEnemy(string sid, string eid)
        {
            int lv = int.Parse(sid);
            double atk = Math.Pow(1.5f, lv) * Table.GetFloat(TableType.Enemy, eid, "atk");
            double maxHp = Math.Pow(1.5f, lv) * Table.GetFloat(TableType.Enemy, eid, "hp");
            float def = 0f;
            float cri = Table.GetFloat(TableType.Enemy, eid, "cri");
            float criDamage = Table.GetFloat(TableType.Enemy, eid, "cri_dam");
            float spd = Table.GetFloat(TableType.Enemy, eid, "spd");

            return new BattleStat
            {
                Atk = atk,
                MaxHp = maxHp,
                Def = def,
                Cri = cri,
                CriDamage = criDamage,
                Spd = spd,
                AtkSpd = Constant.BaseAtkSpd,
            };
        }

        public static double CalculateBattlePower(BattleStat stat)
        {
            float cri = stat.Cri > 100f ? 100f : stat.Cri;
            double result = (stat.Atk + stat.Atk * cri * stat.CriDamage) / stat.AtkSpd + stat.MaxHp;
            return result;
        }
    }
}
