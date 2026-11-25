using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Game.Battle.Unit;
using Game.Calculate;
using Game.Common;
using Game.Core;
using Game.Data;
using Game.Quests;

namespace Game.Battle.Stage
{
    public class RoguelikeStage : BaseStage, IGoalStage, IStageRequire
    {
        public const int ChoiceCount = 3;

        public override bool IsActived => State != RoguelikeState.Ended;
        public Currency NeedTicket => (Currency)Enum.Parse(typeof(Currency), Table.GetString(TableType.Dungeon, "RogueDungeon", "need"));
        public int NeedCount => 1;

        public RoguelikeState State { get; private set; }
        public int WaveNumber => WaveIndex + 1;
        public int WaveCount => Waves.Count;
        public RoguelikeWave CurrentWave => Waves[WaveIndex];
        public IReadOnlyList<string> EffectIds => SelectedEffects;
        public IReadOnlyList<string> ChoiceIds => Choices;
        public IReadOnlyDictionary<Currency, int> Rewards => ClearRewards;

        private readonly List<string> SelectedEffects = new List<string>();
        private readonly List<string> Choices = new List<string>();
        private Dictionary<Currency, int> ClearRewards;
        private IReadOnlyList<RoguelikeWave> Waves;
        private WaveSpawner WaveSpawner;

        private int Level;
        private int WaveIndex;
        private float Timer;
        private float WaitTimer;
        private bool ClearResult;
        private const float WaitTime = 1.5f;

        private string MapId => Table.GetString(TableType.Dungeon, "RogueDungeon", "sid");

        public override void InitData(string nowStage)
        {
            Level = int.Parse(nowStage);
            Waves = RoguelikeWave.Load(Level);
            ValidateEffects();

            SelectedEffects.Clear();
            Choices.Clear();
            ClearRewards = DropCalculator.DungeonClearReward(GetBattleType(), Level);
            WaveIndex = 0;
            ClearResult = false;

            WaveSpawner = new WaveSpawner();
            Spawner = WaveSpawner;

            var enemyIds = new HashSet<string>();

            foreach (var wave in Waves)
            {
                enemyIds.UnionWith(wave.EnemyIds);
            }

            Spawner.SetInfos(enemyIds.ToArray(), 0);
            PrepareWave();
        }

        private void ValidateEffects()
        {
            long totalStack = 0;

            foreach (string id in Table.GetTable(TableType.RoguelikeEffect).Keys)
            {
                string stat = Table.GetString(TableType.RoguelikeEffect, id, "stat");
                float value = Table.GetFloat(TableType.RoguelikeEffect, id, "value");
                int weight = Table.GetInt(TableType.RoguelikeEffect, id, "weight");
                int maxStack = Table.GetInt(TableType.RoguelikeEffect, id, "max_stack");

                if (!Table.IsValidKey(TableType.Stat, stat) || float.IsNaN(value) || float.IsInfinity(value) 
                    || value <= 0 || weight <= 0 || maxStack <= 0 ||
                    string.IsNullOrWhiteSpace(Table.GetString(TableType.RoguelikeEffect, id, "icon")))
                {
                    throw new InvalidOperationException($"로그라이크 효과 {id}의 설정이 잘못되었습니다.");
                }

                // 설명 문자열의 잘못된 자리 표시자도 입장 전에 확인한다.
                string.Format(Table.GetString(TableType.Stat, stat, "desc_format"), value);
                totalStack += maxStack;
            }

            if (totalStack < Waves.Count - 1)
            {
                throw new InvalidOperationException("웨이브 수에 비해 선택 가능한 로그라이크 효과 중첩 수가 부족합니다.");
            }
        }

        private void PrepareWave()
        {
            Timer = 0;
            WaitTimer = 0;
            WaveSpawner.SetWave(CurrentWave);
            State = RoguelikeState.Preparing;
        }

        public override void Update()
        {
            if (State == RoguelikeState.RewardPending)
            {
                TryCompleteReward();
                return;
            }

            if (State == RoguelikeState.Preparing)
            {
                if (!BattleMgr.Instance.IsReadied || !IsReadied())
                {
                    return;
                }

                WaitTimer += Time.deltaTime;

                if (WaitTimer >= WaitTime)
                {
                    State = RoguelikeState.Fighting;
                    BattleMgr.Instance.NotifyRoguelikeChanged();
                }

                return;
            }

            if (State != RoguelikeState.Fighting)
            {
                return;
            }

            if (BattleMgr.Instance.PlayerNowHp <= 0)
            {
                EndRun(false);
                return;
            }

            Timer += Time.deltaTime;

            // 버티기는 목표 시간이 끝나면 추가 생성 없이 통과한다.
            if (CurrentWave.ClearType == WaveClearType.Survive && Timer >= CurrentWave.TimeLimit)
            {
                ClearWave();
                return;
            }

            if (CurrentWave.ClearType == WaveClearType.KillAll &&
                WaveSpawner.IsSpawnComplete && WaveSpawner.Enemys.Count == 0)
            {
                ClearWave();
                return;
            }

            if (CurrentWave.ClearType == WaveClearType.KillAll &&
                CurrentWave.TimeLimit > 0 && Timer >= CurrentWave.TimeLimit)
            {
                EndRun(false);
                return;
            }

            WaveSpawner.SpawEnemy();
        }

        public override bool DieEnemy(EnemyUnit unit)
        {
            if (State != RoguelikeState.Fighting || !WaveSpawner.Contains(unit))
            {
                return false;
            }

            WaveSpawner.DieEnemy(unit);
            return true;
        }

        public override void DiePlayer()
        {
            if (State == RoguelikeState.Fighting)
            {
                EndRun(false);
            }
        }

        private void ClearWave()
        {
            State = RoguelikeState.Preparing;
            BattleMgr.Instance.ClearWaveCombat();
            ClearEnemy();

            if (WaveIndex == Waves.Count - 1)
            {
                EndRun(true);
                return;
            }

            CreateChoices();
            State = RoguelikeState.Selecting;
            BattleMgr.Instance.NotifyRoguelikeChanged();
        }

        private void CreateChoices()
        {
            var available = new List<string>();
            var weights = new List<int>();

            foreach (string id in Table.GetTable(TableType.RoguelikeEffect).Keys)
            {
                int count = SelectedEffects.FindAll(selected => selected == id).Count;

                if (count < Table.GetInt(TableType.RoguelikeEffect, id, "max_stack"))
                {
                    available.Add(id);
                    weights.Add(Table.GetInt(TableType.RoguelikeEffect, id, "weight"));
                }
            }

            if (available.Count == 0)
            {
                throw new InvalidOperationException("선택 가능한 로그라이크 효과가 없습니다.");
            }

            Choices.Clear();

            while (Choices.Count < ChoiceCount && available.Count > 0)
            {
                int index = GachaCalculator.GachaIdx(weights, 1)[0];
                Choices.Add(available[index]);

                // 선택한 ID와 가중치를 함께 제거하여 다음 추첨에서 중복되지 않게 한다.
                available.RemoveAt(index);
                weights.RemoveAt(index);
            }
        }

        public bool TrySelectEffect(string id)
        {
            if (State != RoguelikeState.Selecting || !Choices.Contains(id))
            {
                return false;
            }

            State = RoguelikeState.Preparing;
            SelectedEffects.Add(id);
            Choices.Clear();

            BattleMgr.Instance.RefreshRoguelikeStat(Table.GetString(TableType.RoguelikeEffect, id, "stat") == "hp_multi");

            WaveIndex++;
            PrepareWave();
            BattleMgr.Instance.NotifyRoguelikeChanged();
            return true;
        }

        private void EndRun(bool isClear)
        {
            if (State == RoguelikeState.Ended || State == RoguelikeState.RewardPending)
            {
                return;
            }

            ClearResult = isClear;
            State = isClear ? RoguelikeState.RewardPending : RoguelikeState.Ended;
            BattleMgr.Instance.ClearWaveCombat();
            ClearEnemy();

            if (isClear)
            {
                TryCompleteReward();
            }

            BattleMgr.Instance.NotifyRoguelikeChanged();
        }

        private void TryCompleteReward()
        {
            // 보유 한도로 지급하지 못하면 종료 결과를 내보내지 않고 정산을 기다린다.
            if (!CurrencyReward.TryGive(ClearRewards))
            {
                return;
            }

            State = RoguelikeState.Ended;
            int maxStage = PlayerDataMgr.Instance.GetMaxStage(GetBattleType());
            PlayerDataMgr.Instance.SetMaxStage(GetBattleType(), Math.Max(maxStage, Level).ToString());
            QuestMgr.Instance.ReportAction(QuestAction.ClearDungeon);
        }

        public override BattleType GetBattleType() => BattleType.RogueDungeon;
        public override string GetStageBgPath() => Table.GetString(TableType.Stage, MapId, "bg_res");
        public override BGMType GetStageBGMType() => (BGMType)Enum.Parse(typeof(BGMType), Table.GetString(TableType.Stage, MapId, "bgm"));
        public override bool IsWaitTime() => State == RoguelikeState.Fighting;
        public bool IsEnded() => State == RoguelikeState.Ended;
        public bool IsClear() => ClearResult;
        public float MaxTime() => CurrentWave.TimeLimit;
        public float GetNowTime() => Mathf.Max(0, CurrentWave.TimeLimit - Timer);
        public bool IsCountableGoal() => CurrentWave.ClearType == WaveClearType.KillAll;
        public int Goal() => CurrentWave.TotalCount;
        public int Progress() => WaveSpawner.KillCount;
        public float GoalPercent() => IsCountableGoal() ? Progress() / (float)Goal() : Mathf.Clamp01(Timer / CurrentWave.TimeLimit);
    }
}
