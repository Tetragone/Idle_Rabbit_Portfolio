using System.Collections.Generic;

using UnityEngine;

using Game.Battle.Unit;
using Game.Common;
using Game.Core;
using Game.Pooling;

namespace Game.Battle.Stage
{
    public class MultiSpawner : IEnemySpawner
    {
        public IReadOnlyDictionary<string, EnemyUnit> Enemys => NowEnemys;

        private string[] CurEids;
        private Dictionary<string, EnemyUnit> NowEnemys = new Dictionary<string, EnemyUnit>();
        private List<Dictionary<string, EnemyUnit>> RegionEnemys = new List<Dictionary<string, EnemyUnit>>();
        private List<float> RegionTimer = new List<float>();
        private float RegenTime = 5f;
        private Preloader Loader = new Preloader();


        public void SetInfos(string[] eids, float spawTime)
        {
            CurEids = eids;
            RegenTime = spawTime;

            foreach (string eid in CurEids)
            {
                Loader.TryRegistEnemy(eid);
            }
        }

        public bool IsReadied()
        {
            return Loader.IsEnded;
        }

        public void SpawEnemy()
        {
            if (RegionEnemys.Count == 0)
            {
                for (int i = 0; i * Constant.MaxRegionSpawCount < Constant.MaxEnemySpawCount; i++)
                {
                    RegionEnemys.Add(new Dictionary<string, EnemyUnit>());
                    RegionTimer.Add(RegenTime);
                }
            }

            // 적이 일정 숫자 이하로 떨어진 "특정 지역"이 있다면 그 지역만 리젠
            // + 시간도 체크
            for (int i = 0; i < RegionEnemys.Count; i++)
            {
                if (RegionEnemys[i].Count <= Constant.RegionSpawStartCount && RegionTimer[i] > RegenTime)
                {
                    while (RegionEnemys[i].Count <= Constant.MaxRegionSpawCount)
                    {
                        SpawSingleEnemy(i, CurEids);
                    }

                    RegionTimer[i] = 0f;
                }
            }

            for (int i = 0; i < RegionTimer.Count; i++)
            {
                RegionTimer[i] += Time.deltaTime;
            }
        }

        private void SpawSingleEnemy(int rIdx, string[] eids)
        {
            int random = Random.Range(0, eids.Length);
            string eid = eids[random];
            EnemyUnit enemy = PoolMgr.Instance.Get<EnemyUnit>(EnemyUnit.GetKey(eid));
            enemy.Eid = eid;
            enemy.InitUnit();
            enemy.gameObject.SetActive(true);
            RegionEnemys[rIdx].Add(enemy.ID, enemy);
            NowEnemys.Add(enemy.ID, enemy);

            int count = RegionEnemys[rIdx].Count - 1;
            int value = rIdx / 3;
            int quotient = rIdx % 3;
            float x = value * 13.75f + count * 0.8f - 25;
            float y = 6 - quotient * 6 + (count % 2) * 0.9f;
            enemy.transform.position = new Vector3(x, y, 0);
        }

        public void ClearEnemy()
        {
            foreach (var unit in NowEnemys.Values)
            {
                unit.SetForcedDie();
            }

            NowEnemys.Clear();
            RegionEnemys.Clear();
        }

        public void DieEnemy(EnemyUnit unit)
        {
            string id = unit.ID;
            NowEnemys.Remove(id);

            for (int i = 0; i < RegionEnemys.Count; i++)
            {
                if (RegionEnemys[i].ContainsKey(id))
                {
                    RegionEnemys[i].Remove(id);
                    // 죽었을때 초기화해야 바로바로 안나오니까
                    RegionTimer[i] = 0f;
                }
            }
        }

        public EnemyUnit GetClosestEnemy(Vector3 position)
        {
            EnemyUnit enemy = null;
            float dis = float.MaxValue;

            foreach (EnemyUnit enemyUnit in Enemys.Values)
            {
                // 무적 시간이 있으니, 무적시간에 낭비가 되지 않도록
                if (!enemyUnit.CanTakeDamage)
                {
                    continue;
                }

                float tempDis = Vector2.Distance(position, enemyUnit.gameObject.transform.position);

                if (tempDis < dis)
                {
                    dis = tempDis;
                    enemy = enemyUnit;
                }
            }

            return enemy;
        }
    }

}
