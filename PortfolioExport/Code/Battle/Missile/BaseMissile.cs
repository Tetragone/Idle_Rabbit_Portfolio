using System;
using System.Collections.Generic;

using UnityEngine;

using Game.Battle.Unit;
using Game.Common;
using Game.Pooling;

namespace Game.Battle.Missile
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class BaseMissile : MonoBehaviour, IPoolable
    {
        [Header("Object 설정")]
        public ParticleSystem Particle;
        public GameObject ObjRotate;
        [Header("부딪히면 꺼지는 이펙트인지")]
        public bool IsHitOffParicle = false;

        [Header("------------------------------------------------------------------")]
        [Header("미사일 셋팅 값들")]
        public float Spd = 0f;

        [Header("미사일이 지속되는 시간")]
        public float DisappearTime = 99f;
        [Header("미사일의 맞고 난 뒤 몇초동안 남아있을지")]
        public float EndAfterTime = 0.5f;

        [Header("관통 관련 설정")]
        public Penetration Penetrate;

        [Header("디버프 관련 설정")]
        public DebuffSetting Debuff;

        [Header("버프 관련 설정")]
        public BuffSetting MissileBuff;

        [Header("타켓 위치에서 생성될지")]
        public bool IsCreateOnTarget;
        [Header("추가로 사용할 미사일")]
        [SerializeField]
        protected List<BaseMissile> ChildMissile = new List<BaseMissile>();

        public bool IsChild { get; protected set; } = false;
        public bool IsEnded { get; protected set; } = false;

        protected float PowMulti = 1f;
        protected float ChildPowMulti = 1f;
        protected BattleStat AttackerStat;
        protected Buff AttackerBuff;
        protected Vector3 Dir;
        protected HashSet<BaseUnit> CollidedUnit = new HashSet<BaseUnit>();
        protected float ActiveTimer = 0f;
        protected bool IsInited = false;
        protected float EndTimer = 0f;

        private Rigidbody2D Body;
        private CircleCollider2D Collider;
        private string Key;
        private bool IsRegist = false;

        public static string GetKey(string path)
        {
            return string.Format("missile_{0}", path);
        }

        public virtual void InitMissile(BattleStat stat, Buff buff, Vector3 startPosition, Vector3 targetPosition)
        {
            Vector3 dir = (targetPosition - startPosition).normalized;
            transform.position = IsCreateOnTarget ? targetPosition : startPosition;
            AttackerStat = stat;
            AttackerBuff = buff;
            Dir = dir.normalized;
            ActiveTimer = 0f;
            IsInited = true;
            IsChild = false;
            RegistAction(true);

            if (Particle != null)
            {
                Vector3 scale = Particle.transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (dir.x > 0f ? -1f : 1f);
                Particle.transform.localScale = scale;
                Particle.gameObject.SetActive(true);
                Particle.Stop();
                Particle.Play();
            }
        }

        public void SetValue(float basePow, float childPow, float buffTime, float buffValue)
        {
            PowMulti = basePow * 0.01f;
            ChildPowMulti = childPow * 0.01f;

            if (MissileBuff.IsBuff)
            {
                AttackerBuff.Add(MissileBuff.Type, buffTime, buffValue);
            }
        }

        public void SetChildMissile()
        {
            IsChild = true;
        }

        private void RegistAction(bool regist)
        {
            if (regist)
            {
                if (!IsRegist)
                {
                    BattleMgr.PlayerDie += ReturnToPool;
                    BattleMgr.StageChanged += ReturnToPool;
                    BattleMgr.WaveCombatCleared += ReturnToPool;
                    IsRegist = true;
                }
            }
            else
            {
                if (IsRegist)
                {
                    BattleMgr.PlayerDie -= ReturnToPool;
                    BattleMgr.StageChanged -= ReturnToPool;
                    BattleMgr.WaveCombatCleared -= ReturnToPool;
                    IsRegist = false;
                }
            }
        }

        public void SetKey(string key)
        {
            Key = key;
        }

        public void ReturnToPool()
        {
            if (!gameObject.activeSelf || !IsInited)
            {
                return;
            }

            foreach (var child in ChildMissile)
            {
                if (child.gameObject.activeInHierarchy)
                {
                    child.ReturnToPool();
                }
            }

            if (IsChild)
            {
                gameObject.SetActive(false);
                IsInited = false;
                RegistAction(false);
            }
            else
            {
                PoolMgr.Instance.Return(Key, this);
                RegistAction(false);
            }
        }

        private void Awake()
        {
            Body = GetComponent<Rigidbody2D>();
            Collider = GetComponent<CircleCollider2D>();
            // 미사일은 실제 충돌을 통해 물리적 변환이 있으면 안되기때문에 
            Collider.isTrigger = true;
        }

        protected virtual void Update()
        {
            if (BattleMgr.Instance != null && BattleMgr.Instance.IsCombatPaused)
            {
                Body.linearVelocity = Vector2.zero;
                return;
            }

            if (!IsInited)
            {
                return;
            }

            if (IsEnded)
            {
                EndAction();
            }
            else
            {
                Move();
                UpdateTimer();
            }
        }

        protected virtual void Move()
        {
            // Epsilon를 더해서 0일때 움직이기 않아 Collision이 안 일어나는 상황을 대비
            Body.linearVelocity = Dir * (Spd + float.Epsilon);

            if (ObjRotate != null)
            {
                float zAngle = Dir.x == 0 ? Dir.y > 0 ? 0 : 180 : Mathf.Atan2(Dir.y, Dir.x) * Mathf.Rad2Deg;
                ObjRotate.transform.localEulerAngles = new Vector3(0f, 0f, zAngle);
            }
        }

        protected virtual void UpdateTimer()
        {
            ActiveTimer += Time.deltaTime;

            if (ActiveTimer > DisappearTime)
            {
                End();
            }
        }

        protected virtual void EndAction()
        {
            EndTimer += Time.deltaTime;
            Dir = Vector3.zero;
            Body.linearVelocity = Dir * (Spd + float.Epsilon);

            if (EndTimer > EndAfterTime)
            {
                if (IsChild)
                {
                    gameObject.SetActive(false);
                }
                else
                {
                    Collider.enabled = false;
                    bool check = false;

                    foreach (var child in ChildMissile)
                    {
                        check = check || child.gameObject.activeSelf;
                    }

                    if (!check)
                    {
                        PoolMgr.Instance.Return(Key, this);
                        RegistAction(false);
                    }
                }
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (!IsInited || IsEnded || (BattleMgr.Instance != null && BattleMgr.Instance.IsCombatPaused))
            {
                return;
            }

            BaseUnit unit = collision.gameObject.GetComponent<BaseUnit>();

            if (unit == null)
            {
                return;
            }

            if (CollidedUnit.Contains(unit))
            {
                return;
            }

            CollidedUnit.Add(unit);
            // enter라서 이런식으로 해도 상관 X
            unit.TakeDamage(AttackerStat, AttackerBuff, PowMulti);

            if (Debuff.IsDebuff)
            {
                unit.AddBuff(Debuff.Type, Debuff.Time, Debuff.Value);
            }

            if (!Penetrate.IsPenetration)
            {
                End();
            }
            else if (Penetrate.PenetrationCount <= CollidedUnit.Count)
            {
                End();
            }
        }

        protected virtual void End()
        {
            IsEnded = true;

            if (IsHitOffParicle && Particle != null)
            {
                Particle.gameObject.SetActive(false);
            }
        }

        public virtual void OnObtain()
        {
            ResetCondition();
        }

        public virtual void OnDispose()
        {
            ResetCondition();
            RegistAction(false);
        }

        protected virtual void ResetCondition()
        {
            CollidedUnit.Clear();
            ActiveTimer = 0f;
            EndTimer = 0f;
            IsInited = false;
            IsEnded = false;
            Collider.enabled = true;
        }
    }

    [Serializable]
    public struct Penetration
    {
        public bool IsPenetration;
        public int PenetrationCount;
    }

    [Serializable]
    public struct DebuffSetting
    {
        public bool IsDebuff;
        public DebuffType Type;
        public float Value;
        public float Time;
    }

    [Serializable]
    public struct BuffSetting
    {
        public bool IsBuff;
        public BuffType Type;
    }
}
