using System;
using System.Collections.Generic;

using Game.Common;
using Game.Data;

namespace Game.Battle.Stage
{
    public enum WaveClearType { KillAll, Survive }
    public enum WaveEnemyType { Normal, Boss }
    public enum RoguelikeState { Preparing, Fighting, Selecting, RewardPending, Ended }

    public class RoguelikeWave
    {
        private static Dictionary<int, List<RoguelikeWave>> PreparedWaves;

        public string Id;
        public int Stage;
        public int Order;
        public WaveClearType ClearType;
        public WaveEnemyType EnemyType;
        public string[] EnemyIds;
        public int[] EnemyCounts;
        public float SpawnInterval;
        public bool RepeatSpawn;
        public int MaxAlive;
        public float TimeLimit;
        public double AtkMulti;
        public double HpMulti;
        public int TotalCount;

        public static IReadOnlyList<RoguelikeWave> Load(int stage)
        {
            if (stage < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(stage));
            }

            if (PreparedWaves == null)
            {
                PreparedWaves = Prepare();
            }

            return PreparedWaves[(stage - 1) % PreparedWaves.Count + 1];
        }

        private static Dictionary<int, List<RoguelikeWave>> Prepare()
        {
            var patterns = new Dictionary<int, List<RoguelikeWave>>();

            foreach (string id in Table.GetTable(TableType.RoguelikeWave).Keys)
            {
                var wave = Read(id);

                if (!patterns.ContainsKey(wave.Stage))
                {
                    patterns.Add(wave.Stage, new List<RoguelikeWave>());
                }

                patterns[wave.Stage].Add(wave);
            }

            if (patterns.Count == 0)
            {
                throw new InvalidOperationException("로그라이크 웨이브 설정이 없습니다.");
            }

            // 단계와 웨이브 순서는 1부터 연속되어야 반복할 구성을 확정할 수 있다.
            for (int i = 1; i <= patterns.Count; i++)
            {
                if (!patterns.TryGetValue(i, out var waves))
                {
                    throw new InvalidOperationException($"로그라이크 {i}단계 설정이 없습니다.");
                }

                waves.Sort((a, b) => a.Order.CompareTo(b.Order));

                for (int j = 0; j < waves.Count; j++)
                {
                    if (waves[j].Order != j + 1)
                    {
                        throw new InvalidOperationException($"로그라이크 {i}단계 웨이브 순서가 잘못되었습니다.");
                    }
                }
            }

            // 전체 설정의 검증이 끝난 뒤에만 공유하여 실패한 준비 결과가 남지 않게 한다.
            return patterns;
        }

        private static RoguelikeWave Read(string id)
        {
            TableType table = TableType.RoguelikeWave;
            var wave = new RoguelikeWave
            {
                Id = id,
                Stage = Table.GetInt(table, id, "stage"),
                Order = Table.GetInt(table, id, "wave"),
                EnemyIds = Table.GetString(table, id, "enemy_ids").Split(';'),
                SpawnInterval = Table.GetFloat(table, id, "spawn_interval"),
                MaxAlive = Table.GetInt(table, id, "max_alive"),
                TimeLimit = Table.GetFloat(table, id, "time_limit"),
                AtkMulti = Table.GetFloat(table, id, "atk_multi"),
                HpMulti = Table.GetFloat(table, id, "hp_multi")
            };

            string repeat = Table.GetString(table, id, "repeat_spawn");
            string[] counts = Table.GetString(table, id, "enemy_counts").Split(';');

            if (!Enum.TryParse(Table.GetString(table, id, "clear_type"), out wave.ClearType) ||
                !Enum.IsDefined(typeof(WaveClearType), wave.ClearType) ||
                !Enum.TryParse(Table.GetString(table, id, "enemy_type"), out wave.EnemyType) ||
                !Enum.IsDefined(typeof(WaveEnemyType), wave.EnemyType) ||
                (repeat != "o" && repeat != "x") ||
                counts.Length != wave.EnemyIds.Length ||
                wave.Stage < 1 || wave.Order < 1 || wave.MaxAlive < 1 ||
                !IsPositive(wave.SpawnInterval) || !IsPositive(wave.AtkMulti) || !IsPositive(wave.HpMulti) ||
                double.IsNaN(wave.TimeLimit) || double.IsInfinity(wave.TimeLimit) || wave.TimeLimit < 0)
            {
                throw new InvalidOperationException($"로그라이크 웨이브 {id}의 설정이 잘못되었습니다.");
            }

            wave.RepeatSpawn = repeat == "o";

            if ((wave.ClearType == WaveClearType.KillAll && wave.RepeatSpawn) ||
                (wave.ClearType == WaveClearType.Survive && wave.TimeLimit <= 0))
            {
                throw new InvalidOperationException($"로그라이크 웨이브 {id}의 종료 조건이 잘못되었습니다.");
            }

            wave.EnemyCounts = new int[counts.Length];

            for (int i = 0; i < counts.Length; i++)
            {
                if (!Table.IsValidKey(TableType.Enemy, wave.EnemyIds[i]) ||
                    !int.TryParse(counts[i], out int count) || count < 1)
                {
                    throw new InvalidOperationException($"로그라이크 웨이브 {id}의 적 설정이 잘못되었습니다.");
                }

                wave.EnemyCounts[i] = count;
                wave.TotalCount = checked(wave.TotalCount + count);
            }

            return wave;
        }

        private static bool IsPositive(double value)
        {
            return value > 0 && !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
