using System.Collections.Generic;

using UnityEngine;

using Game.Battle.Unit;

namespace Game.Battle.Stage
{
    public interface IEnemySpawner
    {
        public IReadOnlyDictionary<string, EnemyUnit> Enemys { get; }
        public void SetInfos(string[] eids, float spawTime);
        public bool IsReadied();
        public void SpawEnemy();
        public EnemyUnit GetClosestEnemy(Vector3 position);
        public void DieEnemy(EnemyUnit unit);
        public void ClearEnemy();
    }
}
