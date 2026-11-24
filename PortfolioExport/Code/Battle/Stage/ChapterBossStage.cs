using System;

using UnityEngine;

using Game.Battle.Unit;
using Game.Calculate;
using Game.Common;
using Game.Data;

namespace Game.Battle.Stage
{
    public class ChapterBossStage : BaseStage, IGoalStage
    {
        public override bool IsActived => !IsEnd;

        private string CurStage;
        private EnemyUnit Boss;

        private bool IsEnd = false;
        private float WaitTime = 3f;
        private float Timer = 0;
        private const float FailTime = 60f;

        public override void InitData(string nowStage)
        {
            Spawner = new SingleSpwaner();
            CurStage = nowStage;
            IsEnd = false;
            Spawner.SetInfos(Table.GetString(TableType.Stage, nowStage, "eids").Split('_'), 0);
        }

        public override string GetStageBgPath()
        {
            return Table.GetString(TableType.Stage, CurStage, "bg_res");
        }

        public override BGMType GetStageBGMType()
        {
            string sbgm = Table.GetString(TableType.Stage, CurStage, "bgm") + "Boss";
            BGMType result = (BGMType)Enum.Parse(typeof(BGMType), sbgm);
            return result;
        }

        public override Vector3 GetStartPosition()
        {
            return new Vector3(-5, 0, 0);
        }

        public override BattleType GetBattleType()
        {
            return BattleType.ChapterBoss;
        }

        public override void DieEnemy(EnemyUnit unit)
        {
            Spawner.DieEnemy(unit);
            Clear();
        }

        private void Clear()
        {
            if (IsEnd)
            {
                return;
            }

            IsEnd = true;
            // 챕터 보스이고 챕터 보스를 클리어 했는데 다음 챕터 1스테이지를 클리어해야하는 것은 조금 이상하니까
            PlayerDataMgr.Instance.SetMaxStage(BattleType.Normal, StageCalculator.GetNextStage(CurStage));
        }

        public override void DiePlayer()
        {
            IsEnd = true;
        }

        public override void Update()
        {
            if (!IsEnd)
            {
                if (IsReadied() && Boss == null)
                {
                    Spawner.SpawEnemy();
                    // SingleSpawner이기 때문에 적은 하나밖에 없다.
                    Boss = Spawner.GetClosestEnemy(Vector3.zero);
                }

                Timer += Time.deltaTime;

                if (Timer > FailTime + WaitTime)
                {
                    IsEnd = true;
                }
            }
        }

        public override bool IsWaitTime()
        {
            return WaitTime < Timer;
        }

        public bool IsEnded()
        {
            return IsEnd;
        }

        public bool IsClear()
        {
            return !Boss.IsAlived;
        }

        public float MaxTime()
        {
            return FailTime + WaitTime;
        }

        public float GetNowTime()
        {
            return Timer > WaitTime ? FailTime - Timer + WaitTime : FailTime;
        }

        public bool IsCountableGoal()
        {
            return false;
        }

        public int Goal()
        {
            return 0;
        }

        public int Progress()
        {
            return 0;
        }

        public float GoalPercent()
        {
            return (float)(Boss.NowHp / Boss.Stat.MaxHp);
        }
    }
}
