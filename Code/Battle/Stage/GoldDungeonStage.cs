using System;

using UnityEngine;

using Game.Battle.Unit;
using Game.Calculate;
using Game.Common;
using Game.Data;
using Game.Quests;

namespace Game.Battle.Stage
{
    public class GoldDungeonStage : BaseStage, IGoalStage, IStageRequire
    {
        public override bool IsActived => !IsEnd;

        public Currency NeedTicket => Currency.DungeonTicket1;
        public int NeedCount => 1;

        private const string CurStage = "20001";
        private int NowLv = 0;

        private const float RegenTime = 5f;

        private const int ClearCount = 100;
        private int KillCount = 0;
        private bool IsEnd = false;

        private float WaitTime = 1.5f;
        private float ClearTime = 120f;
        private float Timer = 0f;

        public override void InitData(string nowStage)
        {
            NowLv = int.Parse(nowStage);
            Spawner = new MultiSpawner();
            Spawner.SetInfos(Table.GetString(TableType.Stage, CurStage, "eids").Split(';'), RegenTime);
            KillCount = 0;
            IsEnd = false;
            Timer = 0f;
        }

        public override string GetStageId()
        {
            return NowLv.ToString();
        }

        public override string GetStageBgPath()
        {
            return Table.GetString(TableType.Stage, CurStage, "bg_res");
        }

        public override BGMType GetStageBGMType()
        {
            return (BGMType)Enum.Parse(typeof(BGMType), Table.GetString(TableType.Stage, CurStage, "bgm"));
        }

        public override Vector3 GetStartPosition()
        {
            return new Vector3(0, 0, 0);
        }

        public override BattleType GetBattleType()
        {
            return BattleType.GoldDungeon;
        }

        public override bool DieEnemy(EnemyUnit unit)
        {
            Spawner.DieEnemy(unit);

            KillCount++;

            if (ClearCount <= KillCount)
            {
                Clear();
            }

            return true;
        }

        private void Clear()
        {
            if (IsEnd)
            {
                return;
            }

            IsEnd = true;
            ClearEnemy();
            PlayerDataMgr.Instance.SetMaxStage(GetBattleType(), NowLv.ToString());
            var reward = DropCalculator.DungeonClearReward(GetBattleType(), NowLv);
            foreach (var key in reward.Keys)
            {
                PlayerDataMgr.Instance.TryAddCurrency(key, reward[key]);
            }
            QuestMgr.Instance.ReportAction(QuestAction.ClearDungeon);
        }

        public override void DiePlayer()
        {
            // 시간이 지났는데 죽은 경우 패스
            if (IsEnd)
            {
                return;
            }

            IsEnd = true;
        }

        public override void Update()
        {
            if (!IsEnd)
            {
                if (IsWaitTime())
                {
                    Spawner.SpawEnemy();
                }

                Timer += Time.deltaTime;

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
