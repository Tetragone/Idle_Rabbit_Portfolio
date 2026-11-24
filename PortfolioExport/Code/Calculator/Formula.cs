using System;
using System.Collections.Generic;

using UnityEngine;

using Game.Common;
using Game.Data;

namespace Game.Calculate
{
    public static class Formula
    {
        private static readonly List<int> SkillBaseUpgradeNeed = new List<int> { 100, 200, 300, 400, 500 };
        private static readonly List<float> SkillMultiUpgradeNeed = new List<float> { 1.01f, 1.02f, 1.03f, 1.04f, 1.05f };

        public static int SkillUpgradeNeed(int grade, int lv)
        {
            return Mathf.RoundToInt(SkillBaseUpgradeNeed[grade - 1] * Mathf.Pow(SkillMultiUpgradeNeed[grade - 1], lv));
        }

        public static float SkillManaNeed(string sid, int lv)
        {
            float baseValue = Table.GetFloat(TableType.Skill, sid, "need_mp");
            return baseValue + baseValue * (Table.GetFloat(TableType.Skill, sid, "multi") - 1) * lv;
        }

        public static float SkillValueOnLv(string sid, string column, int lv)
        {
            return Table.GetString(TableType.Skill, sid, column) == "x"
                ? 0f
                : Table.GetFloat(TableType.Skill, sid, column) * Mathf.Pow(Table.GetFloat(TableType.Skill, sid, "multi"), lv);
        }

        public static float SkillStarValue(string sid, int lv)
        {
            return Table.GetString(TableType.Skill, sid, "star_stat") == "x"
                ? 0f : Table.GetFloat(TableType.Skill, sid, "star_value") * lv;
        }

        public static double DropGold(int stageSum)
        {
            return 100 * Math.Pow(1.8d, stageSum);
        }

        public static float DropExp(int stageSum)
        {
            return 20 * Mathf.Pow(1.2f, stageSum);
        }

        public static float NeedExp(int lv)
        {
            return 2000 * (1 + 1 * lv) * Mathf.Pow(1.2f, lv);
        }

        public static float DropEquipUp(int stageSum)
        {
            return 10 * Mathf.Pow(1.1f, stageSum);
        }

        public static double UpgradeGoldNeed(string uid, int lv, int count)
        {
            float needMulti = Table.GetFloat(TableType.Upgrade, uid, "need_multi");
            double need1 = Table.GetFloat(TableType.Upgrade, uid, "need_value") * Math.Pow(needMulti, lv);
            needMulti = needMulti == 1 ? count : needMulti * (1 - Mathf.Pow(needMulti, count)) / (1 - needMulti);
            double realNeed = need1 * needMulti;
            return realNeed;
        }

        public static int UpgradeNeed(string uid, int lv, int count)
        {
            float needMulti = Table.GetFloat(TableType.Upgrade, uid, "need_multi");
            float need1 = Table.GetFloat(TableType.Upgrade, uid, "need_value") * Mathf.Pow(needMulti, lv);
            needMulti = needMulti == 1 ? count : needMulti * (1 - Mathf.Pow(needMulti, count)) / (1 - needMulti);
            int realNeed = Mathf.RoundToInt(need1 * needMulti);
            return realNeed;
        }
    }
}
