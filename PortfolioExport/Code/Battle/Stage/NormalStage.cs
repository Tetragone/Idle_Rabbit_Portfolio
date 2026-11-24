using System;

using UnityEngine;

using Game.Battle.Unit;
using Game.Calculate;
using Game.Common;
using Game.Data;

namespace Game.Battle.Stage
{
    public class NormalStage : BaseStage
    {
        public override bool IsActived => true;

        private string CurStage = "";
        private const float RegenTime = 5f;

        public override void InitData(string sid)
        {
            Spawner = new MultiSpawner();
            CurStage = sid;
            Spawner.SetInfos(Table.GetString(TableType.Stage, CurStage, "eids").Split(';'), RegenTime);
        }

        public override string GetStageBgPath()
        {
            return Table.GetString(TableType.Stage, CurStage, "bg_res");
        }

        public override BGMType GetStageBGMType()
        {
            return (BGMType)Enum.Parse(typeof(BGMType), Table.GetString(TableType.Stage, CurStage, "bgm"));
        }

        public override BattleType GetBattleType()
        {
            return BattleType.Normal;
        }

        public override Vector3 GetStartPosition()
        {
            return new Vector3(-32, 6.5f, 0);
        }

        public override void DieEnemy(EnemyUnit unit)
        {
            Spawner.DieEnemy(unit);
            DropCalculator.Drop(BattleMgr.Instance.StageId);
        }

        public override void DiePlayer()
        {
            BattleMgr.Instance.ChangeBattleType(BattleType.Normal, CurStage);
        }

        public override void Update()
        {
            Spawner.SpawEnemy();
        }

        public override bool IsWaitTime()
        {
            return true;
        }
    }
}
