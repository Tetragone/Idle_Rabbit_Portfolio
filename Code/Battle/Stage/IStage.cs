using System.Collections.Generic;
using UnityEngine;
using Game.Battle.Unit;
using Game.Common;

namespace Game.Battle.Stage
{
    // 공통 스테이지 계약이며 목표와 입장 조건은 선택적인 역할로 분리한다.
    public interface IStage : IStageLifecycle, IStageInfo, IStageCombat
    {
    }

    public interface IStageLifecycle
    {
        public bool IsActived { get; }
        public void SetInfo(string nowStage);
        public void Update();
        public bool IsReadied();
        public bool IsWaitTime();
    }

    public interface IStageInfo
    {
        public string GetStageId();
        public BattleType GetBattleType();
        public string GetStageBgPath();
        public BGMType GetStageBGMType();
        public Vector3 GetStartPosition();
    }

    public interface IStageCombat
    {
        public void ClearEnemy();
        public IReadOnlyDictionary<string, EnemyUnit> GetEnemies();
        public EnemyUnit GetClosestEnemy(Vector3 position);
        public bool DieEnemy(EnemyUnit unit);
        public void DiePlayer();
    }
}
