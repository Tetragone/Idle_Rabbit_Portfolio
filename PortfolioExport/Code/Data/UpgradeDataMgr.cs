using System;
using System.Collections.Generic;

using Game.Calculate;
using Game.Common;
using Game.Core;
using Game.Quests;

namespace Game.Data
{
    public class UpgradeDataMgr : SingletonAllTime<UpgradeDataMgr>
    {
        public static event Action<string> ChangedUpgrade;

        private Dictionary<string, int> UpgradeLv = new Dictionary<string, int>();

        public int PlayerLv => MyData.PlayerLv;
        public float Exp => MyData.Exp;

        private UpgradeData MyData = new UpgradeData();

        public void SetData(UpgradeData data)
        {
            MyData.SetData(data);
            UpgradeLv.Clear();
            foreach (var value in MyData.UpgradeLvs)
            {
                UpgradeLv.Add(value.Key, value.Value);
            }
        }

        public UpgradeData GetData()
        {
            UpgradeData data = new UpgradeData();

            // 저장할 때만 List로 가지고 있으면 되니까 
            // Dictionary로 수정하고 List로 바꿔서 넣어주자
            // SerialKeyValue는 sturct 이기때문에 복사가 Dictionary에서 수정해도 MyData의 값이 안 바뀐다.
            MyData.UpgradeLvs.Clear();
            foreach (var item in UpgradeLv)
            {
                MyData.UpgradeLvs.Add(new SerialKeyValue<string, int>(item.Key, item.Value));
            }
            data.SetData(MyData);
            return data;
        }

        public void LevelUp(int count = 1)
        {
            if (count < 1)
            {
                return;
            }

            float sum = 0f;
            int idx = 0;

            for (; idx < count; idx++)
            {
                sum += Formula.NeedExp(PlayerLv + idx);

                if (sum > Exp)
                {
                    break;
                }
            }

            MyData.PlayerLv += idx;
            MyData.Exp -= sum;
            PlayerDataMgr.Instance.TryAddCurrency(Currency.MoonBless, 10 * idx);
            ChangedUpgrade.Invoke("lv");
        }

        public void AddExp(float exp, bool notifyChange = true)
        {
            if (exp < 0)
            {
                return;
            }

            MyData.Exp += exp;
            if (notifyChange) NotifyExpChanged();
        }

        public void NotifyExpChanged()
        {
            ChangedUpgrade?.Invoke("exp");
        }

        public bool IsUnlockUpgrade(string sid)
        {
            return UpgradeLv.ContainsKey(sid);
        }

        public int GetUpgradeLv(string sid)
        {
            if (!UpgradeLv.ContainsKey(sid))
            {
                return 0;
            }

            return UpgradeLv[sid];
        }

        public void UnlockUpgrade(string sid)
        {
            if (UpgradeLv.ContainsKey(sid))
            {
                return;
            }

            int unlockLv = Table.GetInt(TableType.Upgrade, sid, "unlock_lv");

            if (unlockLv <= PlayerLv)
            {
                UpgradeLv.Add(sid, 0);
                ChangedUpgrade?.Invoke(sid);
            }
        }

        public void UpgradeLvUp(string sid, int count = 1)
        {
            if (!UpgradeLv.ContainsKey(sid))
            {
                return;
            }

            if (count < 0)
            {
                return;
            }

            UpgradeLv[sid] += count;
            QuestMgr.Instance.ReportAction(QuestAction.StatUpgrade, count);
            ChangedUpgrade?.Invoke(sid);
        }
    }

    [Serializable]
    public class UpgradeData : IBaseData<UpgradeData>
    {
        public int PlayerLv = 0;
        public float Exp = 0f;

        public List<SerialKeyValue<string, int>> UpgradeLvs = new List<SerialKeyValue<string, int>>();

        public void SetData(UpgradeData beforeData)
        {
            PlayerLv = beforeData.PlayerLv;
            Exp = beforeData.Exp;
            UpgradeLvs.Clear();
            UpgradeLvs = new List<SerialKeyValue<string, int>>(beforeData.UpgradeLvs);
        }

        public void SetToDefault()
        {
            PlayerLv = 0;
            Exp = 0f;
            UpgradeLvs.Clear();
        }

        public static float GetValue(string sid, int lv)
        {
            return Table.GetFloat(TableType.Upgrade, sid, "base_pow") + Table.GetFloat(TableType.Upgrade, sid, "up_pow") * lv;
        }
    }
}
