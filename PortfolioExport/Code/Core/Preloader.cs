using System.Collections.Generic;

using Game.Battle.Missile;
using Game.Battle.Unit;
using Game.Common;
using Game.Data;
using Game.Pooling;
using Game.UI.Battle;

namespace Game.Core
{
    public class Preloader
    {
        public float Percent
        {
            get
            {
                if (TotalCount == 0)
                {
                    return 100f;
                }
                else
                {
                    float totalPecent = 0f;

                    foreach (string key in Keys)
                    {
                        totalPecent += PoolMgr.Instance.PoolPercent(key);
                    }

                    return totalPecent / TotalCount;
                }
            }
        }

        public bool IsEnded
        {
            get
            {
                if (TotalCount == 0)
                {
                    return true;
                }
                else
                {
                    int count = 0;
                    FailedCount = 0;

                    foreach (string key in Keys)
                    {
                        ProcessType process = PoolMgr.Instance.GetProcess(key);

                        if (process == ProcessType.Done)
                        {
                            count++;
                        }
                        else if (process == ProcessType.Failed)
                        {
                            count++;
                            FailedCount++;
                        }
                    }

                    return count == TotalCount;
                }
            }
        }

        public List<string> Keys = new List<string>();
        public int FailedCount = 0;
        private int TotalCount = 0;

        public bool TryRegistEnemy(string eid, int count = 100, int missileCount = 150)
        {
            string key = string.Format(EnemyUnit.GetKey(eid));
            ProcessType process = PoolMgr.Instance.GetProcess(key);
            if (process == ProcessType.Doing || process == ProcessType.Done)
            {
                return false;
            }

            if (PoolMgr.Instance.IsContain(key))
            {
                return false;
            }

            PoolMgr.Instance.Register<EnemyUnit>(key, Table.GetString(TableType.Enemy, eid, "res"), count);
            Keys.Add(key);
            TotalCount++;

            string path = Table.GetString(TableType.Enemy, eid, "missile");
            RegistMissile(path, missileCount);
            return true;
        }

        public void RegistMissile(string path, int count = 150)
        {
            string key = string.Format(BaseMissile.GetKey(path));

            ProcessType process = PoolMgr.Instance.GetProcess(key);
            if (process == ProcessType.Doing || process == ProcessType.Done)
            {
                return;
            }

            PoolMgr.Instance.Register<BaseMissile>(key, path, count);
            Keys.Add(key);
            TotalCount++;
        }

        public void RegistPlayer()
        {
            ProcessType process = PoolMgr.Instance.GetProcess(Constant.PlayerKey);
            if (process == ProcessType.Doing || process == ProcessType.Done)
            {
                return;
            }

            PoolMgr.Instance.Register<PlayerUnit>(Constant.PlayerKey, Constant.PlayerPath, 2);
            Keys.Add(Constant.PlayerKey);
            TotalCount++;

            string eid = EquipDataMgr.Instance.EquipId;
            if (eid == "")
            {
                RegistMissile(Constant.BaseMissilePath);
            }
            else
            {
                string path = Table.GetString(TableType.Equip, eid, "missile_res");
                RegistMissile(path);
            }
        }

        public void RegistMiniMapObj()
        {
            ProcessType process = PoolMgr.Instance.GetProcess(Constant.MiniUnitKey);
            if (process == ProcessType.Doing || process == ProcessType.Done)
            {
                return;
            }

            PoolMgr.Instance.Register<Icon_MiniUnit>(Constant.MiniUnitKey, Constant.MiniUnitPath, 105);
            Keys.Add(Constant.MiniUnitKey);
            TotalCount++;
        }

        public void RegistEnemyHp()
        {
            ProcessType process = PoolMgr.Instance.GetProcess(Constant.EnemyHpKey);
            if (process == ProcessType.Doing || process == ProcessType.Done)
            {
                return;
            }

            PoolMgr.Instance.Register<UI_Hp>(Constant.EnemyHpKey, Constant.EnemyHpPath, 105);
            Keys.Add(Constant.EnemyHpKey);
            TotalCount++;
        }

        public void RegistDamage()
        {
            ProcessType process = PoolMgr.Instance.GetProcess(Constant.DamageUIKey);
            if (process == ProcessType.Doing || process == ProcessType.Done)
            {
                return;
            }

            PoolMgr.Instance.Register<UI_Damage>(Constant.DamageUIKey, Constant.DamageUIPath, 105);
            Keys.Add(Constant.DamageUIKey);
            TotalCount++;
        }
    }
}
