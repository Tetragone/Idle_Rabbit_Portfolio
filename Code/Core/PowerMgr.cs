using System;
using System.Collections.Generic;

using UnityEngine;

using Game.Calculate;
using Game.Common;
using Game.Data;

namespace Game.Core
{
    public class PowerMgr : MonoBehaviour
    {
        public static event Action ChangedPower;
        public static string NowPowerString => BattlePowerString;
        public static double NowPowerNum => BattlePower;

        public static BattleStat PlayerStat => StatData;
        private static BattleStat StatData;

        private EquipData NowEquip;
        private string EquipId;
        private SkillData NowSkill;
        private UpgradeData NowUpgrade;

        private static double BeforeBattlePower = 0d;
        private static double BattlePower = 0d;
        private static string BattlePowerString;

        private static Dictionary<string, float> StatAdded = new Dictionary<string, float>();
        private static Dictionary<string, double> StatMulti = new Dictionary<string, double>();

        public static double GetSingleStat(string uid)
        {
            if (!Table.IsValidKey(TableType.Stat, uid))
            {
                return 0d;
            }

            if (Table.GetString(TableType.Stat, uid, "calculate_type") == "add")
            {
                if (!StatAdded.ContainsKey(uid))
                {
                    StatAdded.Add(uid, 0f);
                }

                return StatAdded[uid];
            }
            else
            {
                if (!StatMulti.ContainsKey(uid))
                {
                    StatMulti.Add(uid, 1d);
                }

                return StatMulti[uid];
            }
        }

        private void Awake()
        {
            NowUpgrade = UpgradeDataMgr.Instance.GetData();
            NowEquip = EquipDataMgr.Instance.GetData();
            EquipId = EquipDataMgr.Instance.EquipId;
            NowSkill = SkillDataMgr.Instance.GetData();
            SkillDataMgr.ChangedSkill += SkillChanged;
            EquipDataMgr.ChangedEquip += EquipChanged;
            EquipDataMgr.ChangedEquipValue += EquipChanged;
            UpgradeDataMgr.ChangedUpgrade += UpgradeChange;
            CalBattlePower();
        }

        private void OnDestroy()
        {
            SkillDataMgr.ChangedSkill -= SkillChanged;
            EquipDataMgr.ChangedEquip -= EquipChanged;
            EquipDataMgr.ChangedEquipValue -= EquipChanged;
            UpgradeDataMgr.ChangedUpgrade -= UpgradeChange;
        }

        private void UpgradeChange(string type)
        {
            NowUpgrade = UpgradeDataMgr.Instance.GetData();
            CalBattlePower();
        }

        private void EquipChanged()
        {
            NowEquip = EquipDataMgr.Instance.GetData();
            EquipId = EquipDataMgr.Instance.EquipId;
            CalBattlePower();
        }

        private void SkillChanged(HashSet<string> sid)
        {
            NowSkill = SkillDataMgr.Instance.GetData();
            CalBattlePower();
        }

        private void CalBattlePower()
        {
            StatCalculator.UpdateStatValue(StatAdded, StatMulti, NowEquip, EquipId, NowSkill, NowUpgrade);
            StatData = StatCalculator.CalculatePlayer(StatAdded, StatMulti);
            BattlePower = StatCalculator.CalculateBattlePower(StatData);
            BattlePowerString = NumberFormatter.ToCompact(BattlePower);

            if (BattlePower != BeforeBattlePower && BeforeBattlePower != 0d)
            {
                ChangedPower?.Invoke();
            }

            BeforeBattlePower = BattlePower;
        }
    }
}
