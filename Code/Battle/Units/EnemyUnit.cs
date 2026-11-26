using System;

using UnityEngine;

using Game.Battle.Missile;
using Game.Calculate;
using Game.Common;
using Game.Data;
using Game.Pooling;
using Game.UI.Battle;

namespace Game.Battle.Unit
{
    public class EnemyUnit : BaseUnit
    {
        public string Eid = "1001";
        public string ID => Id;
        private string Id = "";
        public bool CanTakeDamage => IsAlived && IsInit && State.GetState() != UnitState.Born;

        private const float InitInvincibilityTime = 0.5f;

        private string MissileKey = "";
        private float AtkRange = 0f;

        private UI_Hp HpUI;
        private float AttackInterval = 0f;

        public static string GetKey(string eid)
        {
            return string.Format("enemy_{0}", eid);
        }

        // Eid를 설정하고 Init해줘야한다.
        public override void InitUnit()
        {
            Stat = StatCalculator.CalculateEnemy(BattleMgr.Instance.NowBattleType, BattleMgr.Instance.StageId, Eid);
            NowHp = Stat.MaxHp;
            AtkRange = Table.GetFloat(TableType.Enemy, Eid, "range");
            MissileKey = BaseMissile.GetKey(Table.GetString(TableType.Enemy, Eid, "missile"));
            RigidBody.linearVelocity = Vector2.zero;
            AttackInterval = 0f;
            State.SetStateForced(UnitState.Born);
            IsInit = true;
        }

        protected override void UpdateTimer()
        {
            AttackInterval += Time.deltaTime;
        }

        public override void TakeDamage(BattleStat stat, Buff buff, float multi)
        {
            if (!CanTakeDamage)
            {
                return;
            }

            double damage = DamageCalculator.PlayerToEnemy(multi, stat, buff, Stat, NowBuff);
            double before = damage;
            damage = DamageCalculator.ApplyCri(damage, stat);
            NowHp -= damage;
            HpUI.SetGauge(NowHp, Stat.MaxHp);
            PoolMgr.Instance.Get<UI_Damage>(Constant.DamageUIKey).SetValue(damage, damage != before, transform.position);
            Ani.Hit();

            if (!IsAlived)
            {
                State.SetStateNextFrame(UnitState.Dying);
            }
        }

        public void SetForcedDie()
        {
            PoolMgr.Instance.Return(GetKey(Eid), this);
        }

        public override void OnObtain()
        {
            Id = Guid.NewGuid().ToString();
            gameObject.SetActive(true);
            Collider.enabled = true;
            HpUI = PoolMgr.Instance.Get<UI_Hp>(Constant.EnemyHpKey);
            HpUI.SetTarget(transform);
        }

        public override void OnDispose()
        {
            IsInit = false;
            RigidBody.linearVelocity = Vector2.zero;
            gameObject.SetActive(false);
            Collider.enabled = false;
            PoolMgr.Instance.Return(Constant.EnemyHpKey, HpUI);
        }

        protected override void Born()
        {
            if (State.IsFirstFrame)
            {
                Ani.SetState(State.GetState());
            }

            if (State.IsOverTimeOnce(InitInvincibilityTime))
            {
                State.SetStateNextFrame(UnitState.None);
            }
        }

        protected override void None()
        {
            if (State.IsFirstFrame)
            {
                Ani.SetState(State.GetState());
            }

            if (AtkRange < Vector2.Distance(BattleMgr.Instance.PlayerPosition, transform.position))
            {
                State.SetStateNextFrame(UnitState.Move);
            }
            else
            {
                if (AttackInterval > 1 / Stat.AtkSpd)
                {
                    State.SetStateNextFrame(UnitState.Attack);
                }
            }
        }

        protected override void Move()
        {
            if (State.IsFirstFrame)
            {
                Ani.SetState(State.GetState());
            }

            if (AtkRange < Vector2.Distance(BattleMgr.Instance.PlayerPosition, transform.position))
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position
                    , Collider.radius + Constant.MissMoveDistance
                    , 1 << gameObject.layer);

                Vector2 dir = Vector2.zero;

                if (hits.Length > 1)
                {
                    foreach (var hit in hits)
                    {
                        if (hit != Collider)
                        {
                            dir += (Vector2)(transform.position - hit.transform.position).normalized;
                        }
                    }

                    dir = dir.normalized * Constant.MissMovePower;
                    dir += ((Vector2)(BattleMgr.Instance.PlayerPosition - transform.position)).normalized;
                    dir = dir.normalized;
                }
                else
                {
                    dir = ((Vector2)(BattleMgr.Instance.PlayerPosition - transform.position)).normalized;
                }

                RigidBody.linearVelocity = dir * Stat.Spd;
                BodyRenderer.flipX = dir.x < 0f;
            }
            else
            {
                RigidBody.linearVelocity = Vector2.zero;
                State.SetStateNextFrame(UnitState.Attack);
            }
        }

        protected override void Attack()
        {
            if (State.IsFirstFrame)
            {
                Ani.SetState(State.GetState());
                RigidBody.linearVelocity = Vector2.zero;
                AttackInterval = 0f;
                Vector2 dir = ((Vector2)(BattleMgr.Instance.PlayerPosition - transform.position)).normalized;
                BodyRenderer.flipX = dir.x < 0f;
            }

            if (State.IsOverTimeOnce(Ani.GetPercentTime(50f)))
            {
                BaseMissile missile = PoolMgr.Instance.Get<BaseMissile>(MissileKey);
                missile.InitMissile(Stat, NowBuff, transform.position, BattleMgr.Instance.PlayerPosition);
                missile.SetKey(MissileKey);
            }

            if (Ani.IsEndState())
            {
                State.SetStateNextFrame(UnitState.None);
            }
        }

        protected override void Dying()
        {
            if (State.IsFirstFrame)
            {
                Ani.SetState(State.GetState());
                Collider.enabled = false;
                RigidBody.linearVelocity = Vector2.zero;
                BattleMgr.Instance.DieEnemy(this);
            }

            if (Ani.IsEndState())
            {
                State.SetStateNextFrame(UnitState.Die);
            }
        }

        protected override void Die()
        {
            if (State.IsFirstFrame)
            {
                PoolMgr.Instance.Return(GetKey(Eid), this);
            }
        }
    }
}
