using System.Collections.Generic;

using UnityEngine;

using Game.Battle.Unit;
using Game.Core;
using Game.Pooling;

namespace Game.Battle.Stage
{
    public class SingleSpwaner : IEnemySpawner
    {
        public IReadOnlyDictionary<string, EnemyUnit> Enemys => NowEnemys;

        private string Eid;
        private EnemyUnit Unit;
        private Dictionary<string, EnemyUnit> NowEnemys = new Dictionary<string, EnemyUnit>();
        private Preloader Loader = new Preloader();

        public void SetInfos(string[] eids, float spawTime)
        {
            Eid = eids[0];
            Loader.TryRegistEnemy(Eid, 2, 2);
        }

        public bool IsReadied()
        {
            return Loader.IsEnded;
        }

        public void SpawEnemy()
        {
            if (Unit == null)
            {
                Unit = PoolMgr.Instance.Get<EnemyUnit>(EnemyUnit.GetKey(Eid));
                Unit.Eid = Eid;
                Unit.InitUnit();
                Unit.gameObject.SetActive(true);
                Unit.transform.position = new Vector3(5, 0, 0);
                NowEnemys.Add(Eid, Unit);
            }
        }

        public void ClearEnemy()
        {
            foreach (var unit in NowEnemys.Values)
            {
                unit.SetForcedDie();
            }

            NowEnemys.Clear();
            Unit = null;
        }

        public void DieEnemy(EnemyUnit unit)
        {
            NowEnemys.Remove(unit.Eid);
        }

        public EnemyUnit GetClosestEnemy(Vector3 position)
        {
            return Unit;
        }
    }
}

