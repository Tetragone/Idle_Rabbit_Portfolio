using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Game.Battle.Unit;
using Game.Battle.Stage;
using Game.Calculate;
using Game.Common;
using Game.Core;
using Game.Data;
using Game.Pooling;
using Game.Events;
using Game.Quests;

namespace Game.Battle
{
    public class BattleMgr : SingletonAllTime<BattleMgr>
    {
        public static event Action StageChanged;
        public static event Action<BattleType, bool> StageEnded;
        public static event Action PlayerDie;
        public bool IsReadied => IsInit;

        public string StageId => StageInfo.GetStageId();
        public string StageBgPath => StageInfo.GetStageBgPath();
        public BGMType StageBGMType => StageInfo.GetStageBGMType();
        public BattleType NowBattleType => StageInfo.GetBattleType();

        public IGoalStage Goal => NowGoal;
        public IStageRequire StageRequire => NowStageRequire;
        public IStageResult StageResult => NowGoal;
        public IStageTimer StageTimer => NowGoal;
        public IStageProgress StageProgress => NowGoal;

        // Player 관련 public 변수
        public IReadOnlyList<float> SkillBaseCoolTimes => Player.BaseSkillCoolTimeValue;
        public IReadOnlyList<float> SkillNowCoolTimes => Player.NowSkillCoolTimeValue;
        public BattleStat PlayerStat => Player.Stat;
        public Vector3 PlayerPosition => Player.transform.position;
        public UnitState PlayerState => Player.NowState;
        public int PlayerUseSkill => Player.NowUseSkillIdx;
        public double PlayerNowHp => Player.NowHp;
        public float PlayerNowMp => Player.NowMp;

        // 어짜피 사용하는 곳에서 n으로 사용할거면 2n이나 n이나 같으니 이렇게 하자
        public IReadOnlyList<Vector3> EnemysPosition
        {
            get
            {
                EnemysPos.Clear();

                foreach (EnemyUnit enemy in StageCombat.GetEnemies().Values)
                {
                    EnemysPos.Add(enemy.transform.position);
                }

                return EnemysPos;
            }
        }

        private List<Vector3> EnemysPos = new List<Vector3>();
        private PlayerUnit Player;
        // 만약 시간이 걸려서 Init되는 경우를 위한 값
        private bool IsInit = false;

        private IStage NowStage = new NormalStage();
        private IStageLifecycle StageLifecycle => NowStage;
        private IStageInfo StageInfo => NowStage;
        private IStageCombat StageCombat => NowStage;
        private IGoalStage NowGoal = null;
        private IStageRequire NowStageRequire = null;
        private bool IsEnded = false;


        public void DieEnemy(EnemyUnit unit)
        {
            BattleType battleType = NowBattleType;
            StageCombat.DieEnemy(unit);
            EventMgr.Instance.OnEnemyKilled(battleType);
            QuestMgr.Instance.ReportAction(QuestAction.KillMonster);
        }

        public void ChangeBattleType(BattleType type, string sid)
        {
            SceneMgr.Instance.OpenLoading();
            IsInit = false;
            IsEnded = false;
            StartCoroutine(ReStartGame(type, sid));
        }

        private IEnumerator ReStartGame(BattleType type, string sid)
        {
            yield return new WaitForSeconds(0.1f);
            StageCombat.ClearEnemy();

            switch (type)
            {
                case BattleType.Normal:
                    if (StageCalculator.IsLastChapterStage(sid))
                    {
                        sid = StageCalculator.GetNextStage(sid);
                    }

                    NowStage = new NormalStage();
                    PlayerDataMgr.Instance.SetCurStage(type, sid);
                    break;
                case BattleType.Challenge:
                    if (StageCalculator.IsLastChapterStage(sid))
                    {
                        NowStage = new ChapterBossStage();
                    }
                    else
                    {
                        NowStage = new ChallengeStage();
                    }
                    break;
                case BattleType.ChapterBoss: 
                    NowStage = new ChallengeStage();
                    break;
                case BattleType.GoldDungeon:
                    NowStage = new GoldDungeonStage();
                    break;
            }

            StageLifecycle.SetInfo(sid);

            if (NowStage is IGoalStage goal)
            {
                NowGoal = goal;
            }
            else
            {
                NowGoal = null;
            }

            if (NowStage is IStageRequire require)
            {
                NowStageRequire = require;
            }
            else
            {
                NowStageRequire = null;
            }

            yield return null;
            StageChanged?.Invoke();

            while (!Map.IsReadied || !StageLifecycle.IsReadied())
            {
                yield return null;
            }

            InitUnits();
            StageLifecycle.Update();
            yield return ChangeIsInit(false);
            SceneMgr.Instance.CloseLoading();
        }

        private IEnumerator ChangeIsInit(bool isStageInvoke = true)
        {
            while (!IsStageReady())
            {
                yield return null;
            }

            IsInit = true;

            if (isStageInvoke)
            {
                StageChanged?.Invoke();
            }
        }

        private bool IsStageReady()
        {
            return StageLifecycle.IsActived && Player.IsReadied == ProcessType.Ready;
        }

        public EnemyUnit GetCloestEnemy(Vector3 position)
        {
            return StageCombat.GetClosestEnemy(position);
        }

        public void DiePlayer()
        {
            StageCombat.DiePlayer();
            PlayerDie?.Invoke();
        }

        private void Start()
        {
            IsInit = false;
            InitInfo();
            InitUnits();
            StartCoroutine(ChangeIsInit());
        }

        private void InitInfo()
        {
            StageLifecycle.SetInfo(PlayerDataMgr.Instance.GetCurStage(BattleType.Normal).ToString());
        }

        private void InitUnits()
        {
            if (Player == null)
            {
                Player = PoolMgr.Instance.Get<PlayerUnit>(Constant.PlayerKey);
            }
            Player.InitUnit();
            Player.gameObject.transform.position = StageInfo.GetStartPosition();
        }

        private void Update()
        {
            if (IsInit)
            {
                StageLifecycle.Update();

                if (StageLifecycle.IsWaitTime() && Player.IsReadied == ProcessType.Ready)
                {
                    Player.UnitStart();
                }

                if (StageResult != null)
                { 
                    if (!IsEnded && StageResult.IsEnded())
                    {
                        StageEnded.Invoke(NowBattleType, StageResult.IsClear());
                        IsEnded = true;
                    }
                }
            }
        }
    }
}
