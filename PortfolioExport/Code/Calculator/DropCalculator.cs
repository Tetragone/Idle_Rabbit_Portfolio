using System;
using System.Collections.Generic;

using UnityEngine;

using Game.Common;
using Game.Core;
using Game.Data;

namespace Game.Calculate
{
    public static class DropCalculator
    {
        public const float GoldDropRate = 1.0f;
        public const float EquipUpDropRate = 0.1f;

        private static List<int> MaxStage = new List<int>();

        public static void Drop(string stageId)
        {
            int chapter = Table.GetInt(TableType.Stage, stageId, "chapter");
            int stage = Table.GetInt(TableType.Stage, stageId, "stage");

            if (MaxStage.Count == 0)
            {
                SetBeforeMaxStage();
            }

            int stageSum = stage + MaxStage[chapter];

            float goldDrop = UnityEngine.Random.Range(0, 1f);
            if (GoldDropRate > goldDrop)
            {
                double gold = Formula.DropGold(stageSum) * (100d + PowerMgr.GetSingleStat("gold_up")) * 0.01d;
                PlayerDataMgr.Instance.AddGold(gold);
            }

            float exp = Formula.DropExp(stageSum) * (100f + (float)PowerMgr.GetSingleStat("exp_up")) * 0.01f;
            UpgradeDataMgr.Instance.AddExp(exp);

            float equipUpDrop = UnityEngine.Random.Range(0, 1f);
            if (EquipUpDropRate > equipUpDrop)
            {
                float equipUp = Formula.DropEquipUp(stageSum) * (100f + (float)PowerMgr.GetSingleStat("equip_up_drop_value")) * 0.01f;
                PlayerDataMgr.Instance.TryAddCurrency(Currency.EquipUp, Mathf.RoundToInt(equipUp));
            }

            float equipDrop = UnityEngine.Random.Range(0, 1f);
            float equipDropRate = Table.GetFloat(TableType.Stage, stageId, "equip_drop_rate");
            if (equipDropRate > equipDrop)
            {
                string dropId = Table.GetString(TableType.Stage, stageId, "equip_drop");
                EquipDataMgr.Instance.AddEquip(dropId);
            }
        }

        private static void SetBeforeMaxStage()
        {
            var table = Table.GetTable(TableType.Stage);
            int maxChapter = 0;

            foreach (string key in table.Keys)
            {
                string sChapter = Table.GetString(TableType.Stage, key, "chapter");
                if (sChapter != "x")
                {
                    int chapter = Table.GetInt(TableType.Stage, key, "chapter");
                    maxChapter = Mathf.Max(chapter, maxChapter);
                }
            }

            MaxStage.Add(0);
            for (int i = 0; i < maxChapter; i++)
            {
                MaxStage.Add(0);
            }

            foreach (string key in table.Keys)
            {
                string sChapter = Table.GetString(TableType.Stage, key, "chapter");

                if (sChapter != "x")
                {
                    int chapter = Table.GetInt(TableType.Stage, key, "chapter");
                    int stage = Table.GetInt(TableType.Stage, key, "stage");

                    MaxStage[chapter] = Mathf.Max(stage, MaxStage[chapter]);
                }
            }
        }

        public static Dictionary<Currency, double> DropItems(string sid)
        {
            Dictionary<Currency, double> result = new Dictionary<Currency, double>();
            int chapter = Table.GetInt(TableType.Stage, sid, "chapter");
            int stage = Table.GetInt(TableType.Stage, sid, "stage");

            if (MaxStage.Count == 0)
            {
                SetBeforeMaxStage();
            }

            int stageSum = stage + MaxStage[chapter - 1];

            result.Add(Currency.Gold, Formula.DropGold(stageSum));
            result.Add(Currency.Exp, Formula.DropExp(stageSum));
            result.Add(Currency.EquipUp, Formula.DropEquipUp(stageSum));

            return result;
        }

        public static Dictionary<Currency, double> IdleCurrencyReward(string sid, long idleTime)
        {
            Dictionary<Currency, double> result = new Dictionary<Currency, double>();

            int chapter = Table.GetInt(TableType.Stage, sid, "chapter");
            int stage = Table.GetInt(TableType.Stage, sid, "stage");
            idleTime = Math.Max(0, Math.Min(idleTime, Constant.IdleReawedMaxSecond));

            if (MaxStage.Count == 0)
            {
                SetBeforeMaxStage();
            }

            int stageSum = stage + MaxStage[chapter];
            double gold = Formula.DropGold(stageSum) * (100d + PowerMgr.GetSingleStat("gold_up")) * 0.01d * GoldDropRate * idleTime;
            result.Add(Currency.Gold, gold);

            float exp = Formula.DropExp(stageSum) * (100f + (float)PowerMgr.GetSingleStat("exp_up")) * 0.01f * idleTime;
            result.Add(Currency.Exp, exp);

            float equipUp = Formula.DropEquipUp(stageSum) * (100f + (float)PowerMgr.GetSingleStat("equip_up_drop_value")) * 0.01f * EquipUpDropRate * idleTime;
            result.Add(Currency.EquipUp, equipUp);

            return result;
        }

        public static Dictionary<string, int> IdleEquipReward(string sid, long idleTime)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            idleTime = Math.Max(0, Math.Min(idleTime, Constant.IdleReawedMaxSecond));

            float equipDropRate = Table.GetFloat(TableType.Stage, sid, "equip_drop_rate");
            string dropId = Table.GetString(TableType.Stage, sid, "equip_drop");
            int count = Mathf.RoundToInt(equipDropRate * idleTime);
            result.Add(dropId, count);
            return result;
        }

        public static Dictionary<Currency, int> DungeonClearReward(BattleType type, int nowStage)
        {
            Dictionary<Currency, int> result = new Dictionary<Currency, int>();

            switch (type)
            {
                case BattleType.GoldDungeon:
                case BattleType.RogueDungeon:
                    {
                        int gemValue = 50 + nowStage * 50;
                        result.Add(Currency.Gem, gemValue);
                    }
                    break;
            }

            return result;
        }
    }

}
