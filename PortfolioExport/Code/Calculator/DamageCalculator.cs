using UnityEngine;

using Game.Battle;
using Game.Common;

namespace Game.Calculate
{
    public static class DamageCalculator
    {
        public static double PlayerToEnemy(float missilePow, BattleStat player, Buff playerBuff, BattleStat enemy, Buff enemyBuff)
        {
            double baseDamage = Calculate(missilePow, player, playerBuff, enemy, enemyBuff);
            return baseDamage;
        }

        public static double EnemyToPlayer(float missilePow, BattleStat player, Buff playerBuff, BattleStat enemy, Buff enemyBuff)
        {
            double baseDamage = Calculate(missilePow, enemy, enemyBuff, player, playerBuff);
            return baseDamage;
        }

        private static double Calculate(float missilePow, BattleStat attackerStat, Buff attackerBuff, BattleStat hitterStat, Buff hitterBuff)
        {
            double result = 0d;
            double atk = attackerStat.Atk;
            if (attackerBuff.IsContain(BuffType.AtkBuff))
            {
                atk = atk * (1 + attackerBuff.GetValue(BuffType.AtkBuff));
            }
            result = atk;
            result = result * 100d / (100d + hitterStat.Def) * missilePow;
            return result;
        }

        public static double ApplyCri(double damage, BattleStat stat)
        {
            if (Random.Range(0, 100f) < stat.Cri)
            {
                damage *= (1 + stat.CriDamage / 100d);
            }

            return damage;
        }
    }
}
