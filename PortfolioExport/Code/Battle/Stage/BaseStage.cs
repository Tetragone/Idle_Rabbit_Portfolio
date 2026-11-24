using Game.Battle.Unit;
using System.Collections.Generic;

using UnityEngine;

using Game.Common;

namespace Game.Battle.Stage
{
    public abstract class BaseStage : IStage
    {
        public abstract bool IsActived { get; }
        protected IEnemySpawner Spawner;
        
        private string CurStage = "";

        public virtual void ClearEnemy()
        {
            Spawner.ClearEnemy();
        }

        public virtual EnemyUnit GetClosestEnemy(Vector3 position)
        {
            return Spawner.GetClosestEnemy(position);
        }

        public virtual IReadOnlyDictionary<string, EnemyUnit> GetEnemies()
        {
            return Spawner.Enemys;
        }

        public virtual string GetStageId()
        {
            return CurStage;
        }

        public virtual Vector3 GetStartPosition()
        {
            return Vector3.zero;
        }

        public virtual void SetInfo(string nowStage)
        {
            CurStage = nowStage;
            InitData(nowStage);
        }

        public abstract void InitData(string nowStage);
        public abstract void DieEnemy(EnemyUnit unit);
        public abstract void DiePlayer();
        public abstract BattleType GetBattleType();
        public abstract void Update();
        public virtual bool IsReadied()
        {
            return Spawner.IsReadied();
        }
        public abstract bool IsWaitTime();
        public abstract BGMType GetStageBGMType();
        public abstract string GetStageBgPath();

    }
}
