using System;
using System.Collections.Generic;

using UnityEngine;

using Game.Battle.Unit;
using Game.Common;
using Game.Core;
using Game.Pooling;

namespace Game.Battle.Stage
{
    public class WaveSpawner : IEnemySpawner
    {
        public IReadOnlyDictionary<string, EnemyUnit> Enemys => AliveEnemies;
        public int KillCount { get; private set; }
        public bool IsSpawnComplete => !Wave.RepeatSpawn && SpawnCount >= Wave.TotalCount;

        private readonly Dictionary<string, EnemyUnit> AliveEnemies = new Dictionary<string, EnemyUnit>();
        // 사망 연출 중인 적도 다음 웨이브 전에 회수하기 위해 생성 당시 ID를 함께 보관한다.
        private readonly Dictionary<string, EnemyUnit> OwnedEnemies = new Dictionary<string, EnemyUnit>();
        private readonly Preloader Loader = new Preloader();

        private RoguelikeWave Wave;
        private int SpawnCount;
        private float SpawnTimer;

        public void SetInfos(string[] eids, float spawTime)
        {
            // 생성 간격은 SetWave에서 설정하므로 여기서는 리소스만 등록한다.
            foreach (string id in eids)
            {
                Loader.TryRegistEnemy(id);
            }
        }

        public void SetWave(RoguelikeWave wave)
        {
            Wave = wave;
            SpawnCount = 0;
            KillCount = 0;
            SpawnTimer = wave.SpawnInterval;
        }

        public bool IsReadied()
        {
            bool isEnded = Loader.IsEnded;

            if (Loader.FailedCount > 0)
            {
                throw new InvalidOperationException("로그라이크 적 리소스 준비에 실패했습니다.");
            }

            return isEnded;
        }

        public void SpawEnemy()
        {
            SpawnTimer += Time.deltaTime;

            if (IsSpawnComplete || AliveEnemies.Count >= Wave.MaxAlive || SpawnTimer < Wave.SpawnInterval)
            {
                return;
            }

            SpawnTimer = 0;

            // 한 번의 생성 주기에 남은 수량과 동시 생존 한도만큼 생성한다.
            int count = Wave.MaxAlive - AliveEnemies.Count;

            if (!Wave.RepeatSpawn)
            {
                count = Math.Min(count, Wave.TotalCount - SpawnCount);
            }

            for (int i = 0; i < count; i++)
            {
                string eid = GetEnemyId(SpawnCount % Wave.TotalCount);
                EnemyUnit enemy = PoolMgr.Instance.Get<EnemyUnit>(EnemyUnit.GetKey(eid));
                enemy.Eid = eid;
                enemy.InitUnit();

                Vector3 position;

                if (Wave.EnemyType == WaveEnemyType.Boss)
                {
                    position = new Vector3(5, 0, 0);
                }
                else
                {
                    Vector2 offset = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(5f, 10f);
                    position = BattleMgr.Instance.PlayerPosition + new Vector3(offset.x, offset.y, 0);
                    position.x = Mathf.Clamp(position.x, -Constant.NormalMapMaxX, Constant.NormalMapMaxX);
                    position.y = Mathf.Clamp(position.y, -Constant.NormalMapMaxY, Constant.NormalMapMaxY);
                }

                enemy.transform.position = position;

                AliveEnemies.Add(enemy.ID, enemy);
                OwnedEnemies[enemy.ID] = enemy;
                SpawnCount++;
            }
        }

        private string GetEnemyId(int index)
        {
            for (int i = 0; i < Wave.EnemyIds.Length; i++)
            {
                if (index < Wave.EnemyCounts[i])
                {
                    return Wave.EnemyIds[i];
                }

                index -= Wave.EnemyCounts[i];
            }

            throw new InvalidOperationException("웨이브 적 생성 순서가 잘못되었습니다.");
        }

        public bool Contains(EnemyUnit unit)
        {
            return AliveEnemies.TryGetValue(unit.ID, out var current) && current == unit;
        }

        public void DieEnemy(EnemyUnit unit)
        {
            if (Contains(unit))
            {
                AliveEnemies.Remove(unit.ID);
                KillCount++;
            }
        }

        public void ClearEnemy()
        {
            foreach (var pair in OwnedEnemies)
            {
                // 풀에서 재사용된 객체와 이미 반환된 객체는 다시 반환하지 않는다.
                if (pair.Value.ID == pair.Key && pair.Value.gameObject.activeSelf)
                {
                    pair.Value.SetForcedDie();
                }
            }

            AliveEnemies.Clear();
            OwnedEnemies.Clear();
        }

        public EnemyUnit GetClosestEnemy(Vector3 position)
        {
            EnemyUnit closest = null;
            float distance = float.MaxValue;

            foreach (var enemy in AliveEnemies.Values)
            {
                if (!enemy.CanTakeDamage)
                {
                    continue;
                }

                float next = (enemy.transform.position - position).sqrMagnitude;

                if (next < distance)
                {
                    distance = next;
                    closest = enemy;
                }
            }

            return closest;
        }
    }
}
