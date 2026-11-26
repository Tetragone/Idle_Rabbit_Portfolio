using System.Collections.Generic;

using UnityEngine;

using Game.Common;
using Game.Pooling;

namespace Game.Battle.Unit
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(BaseUnitAnimation))]
    public abstract class BaseUnit : MonoBehaviour, IPoolable
    {
        public SpriteRenderer BodyRenderer;

        public BattleStat Stat;
        public Buff NowBuff = new Buff();
        // hp 의 경우는 stat에 now hp가 있는것이 이상하기 때문에 따로 둠
        public double NowHp = 0d;
        public bool IsAlived => NowHp > 0d;
        public UnitState NowState => State.GetState();

        protected Rigidbody2D RigidBody;
        protected CircleCollider2D Collider;
        protected BaseUnitAnimation Ani;
        protected MyState<UnitState> State = new MyState<UnitState>(UnitState.None);
        protected bool IsInit = false;


        protected virtual void Awake()
        {
            RigidBody = GetComponent<Rigidbody2D>();
            Collider = GetComponent<CircleCollider2D>();
            Ani = GetComponent<BaseUnitAnimation>();
            SetPriority();
        }

        private void SetPriority()
        {
            Dictionary<UnitState, int> priority = new Dictionary<UnitState, int>();
            priority.Add(UnitState.Born, 10);
            priority.Add(UnitState.None, 0);
            priority.Add(UnitState.Move, 0);
            priority.Add(UnitState.Attack, 0);
            priority.Add(UnitState.Skill, 0);
            priority.Add(UnitState.Dying, 10);
            priority.Add(UnitState.Die, 10);
            State.SetPriority(priority);
        }

        public virtual void InitUnit() { }
        public virtual void TakeDamage(BattleStat stat, Buff buff, float multi) { }
        public virtual void AddBuff(BuffType type, float time, float value) { }
        public virtual void AddBuff(DebuffType type, float time, float value) { }
        public virtual void OnDispose()
        {
            throw new System.NotImplementedException();
        }

        public virtual void OnObtain()
        {
            throw new System.NotImplementedException();
        }

        protected virtual void Update()
        {
            if (BattleMgr.Instance != null && BattleMgr.Instance.IsCombatPaused)
            {
                RigidBody.linearVelocity = Vector2.zero;
                return;
            }

            if (IsInit)
            {
                UpdateTimer();
                StateAction();
                BuffTimer();
            }
        }

        protected virtual void UpdateTimer() { }
        protected virtual void StateAction()
        {
            switch (State.GetState())
            {
                case UnitState.Born:
                    Born();
                    break;
                case UnitState.None:
                    None();
                    break;
                case UnitState.Move:
                    Move();
                    break;
                case UnitState.Attack:
                    Attack();
                    break;
                case UnitState.Skill:
                    Skill();
                    break;
                case UnitState.Dying:
                    Dying();
                    break;
                case UnitState.Die:
                    Die();
                    break;
            }

            State.Update();
        }

        protected virtual void Born() { }
        protected virtual void None() { }
        protected virtual void Move() { }
        protected virtual void Attack() { }
        protected virtual void Skill() { }
        protected virtual void Dying() { }
        protected virtual void Die() { }

        protected virtual void BuffTimer()
        {
            NowBuff.Update(Time.deltaTime);
        }

        protected virtual void FixedUpdate()
        {
            if (IsInit)
            {
                Friction();
            }
        }

        protected virtual void Friction()
        {
            RigidBody.linearVelocity *= Constant.FrictionValue;
        }
    }
}

