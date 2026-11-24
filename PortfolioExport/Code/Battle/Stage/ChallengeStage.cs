using System;

using UnityEngine;

using Game.Battle.Unit;
using Game.Common;
using Game.Data;

namespace Game.Battle.Stage
{
    public class ChallengeStage : BaseStage, IGoalStage
    {
        public override bool IsActived => !IsEnd;
        private string CurStage = "";

        private const int ClearCount = 65;
        private int KillCount = 0;
        private bool IsEnd = false;

        private float WaitTime = 1.5f;
        private float ClearTime = 120f;
        private float Timer = 0f;

        public override void InitData(string nowStage)
        {
            CurStage = nowStage;
            Spawner = new MultiSpawner();
            Spawner.SetInfos(Table.GetString(TableType.Stage, CurStage, "eids").Split(';'), 2f);
            KillCount = 0;
            IsEnd = false;
            Timer = 0f;
        }

        public override string GetStageBgPath()
        {
            return Table.GetString(TableType.Stage, CurStage, "bg_res") + "_challenge";
        }

        public override BGMType GetStageBGMType()
        {
            string sbgm = Table.GetString(TableType.Stage, CurStage, "bgm") + "Challenge";
            BGMType result = (BGMType)Enum.Parse(typeof(BGMType), sbgm);
            return result;
        }

        public override Vector3 GetStartPosition()
        {
            return new Vector3(-5, 0, 0);
        }

        public override BattleType GetBattleType()
        {
            return BattleType.Challenge;
        }

        public override void DieEnemy(EnemyUnit unit)
        {
            Spawner.DieEnemy(unit);

            KillCount++;

            if (ClearCount <= KillCount)
            {
                Clear();
            }
        }

        private void Clear()
        {
            if (IsEnd)
            {
                return;
            }

            IsEnd = true;
            ClearEnemy();
            PlayerDataMgr.Instance.SetMaxStage(BattleType.Normal, CurStage);
        }

        public override void DiePlayer()
        {
            IsEnd = true;
        }

        public override void Update()
        {
            if (!IsEnd)
            {
                Timer += Time.deltaTime;

                if (Timer > WaitTime)
                {
                    Spawner.SpawEnemy();
                }

                if (Timer > ClearTime + WaitTime)
                {
                    IsEnd = true;
                }
            }
        }

        public override bool IsWaitTime()
        {
            return Timer > WaitTime;
        }

        public bool IsEnded()
        {
            return IsEnd;
        }

        public bool IsClear()
        {
            return ClearCount <= KillCount && Timer < ClearTime + WaitTime;
        }

        public float MaxTime()
        {
            return ClearTime;
        }

        public float GetNowTime()
        {
            return Timer < WaitTime ? ClearTime : ClearTime + WaitTime - Timer;
        }

        public bool IsCountableGoal()
        {
            return true;
        }

        public int Goal()
        {
            return ClearCount;
        }

        public int Progress()
        {
            return KillCount;
        }

        public float GoalPercent()
        {
            return (ClearCount - KillCount) / (float)ClearCount;
        }
    }
}
