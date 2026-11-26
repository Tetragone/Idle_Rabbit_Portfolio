using System;
using System.Collections.Generic;

using UnityEngine;


using Game.Battle.Missile;
using Game.Calculate;
using Game.Common;
using Game.Core;
using Game.Data;
using Game.Pooling;
using Game.UI.Battle;

namespace Game.Battle.Unit
{
    public class PlayerUnit : BaseUnit
    {
        [SerializeField]
        private PlayerUnitSkinAdapter Skin;
        // 무적인지 || 으로 여러개의 무적 요소 중에 하나만 true여도 무적이도록 하자
        public bool IsInvincibility => IsDamagedTime;

        public IReadOnlyList<float> BaseSkillCoolTimeValue => BaseSkillCoolTime;
        public IReadOnlyList<float> NowSkillCoolTimeValue => NowSkillCoolTimes;
        public int NowUseSkillIdx => SkillIdx;
        public float NowMp => Mp;
        public ProcessType IsReadied
        {
            get
            {
                if (!Skin.IsBuildDone)
                {
                    return ProcessType.Doing;
                }
                else if (!IsInit)
                {
                    return ProcessType.Ready;
                }
                else
                {
                    return ProcessType.Done;
                }
            }
        }

        #region private field
        private List<float> BaseSkillCoolTime = new List<float>();
        private List<float> NowSkillCoolTimes = new List<float>();

        private bool IsDamagedTime = true;
        private float DamagedTime = 0f;
        private readonly float DamageInterval = 0.5f;

        private float RecoveryTimer = 0f;

        private EnemyUnit TargetUnit = null;
        private bool IsRegistAction = false;

        private string AtkMissileKey = "";
        private List<string> SkillMissileKeys = new List<string>();
        private int SkillIdx = 0;
        private float Mp = 0f;
        private float AttackInterval = 0f;
        private SFXType AttackSound;
        private List<SFXType> SkillSounds = new List<SFXType>();
        #endregion

        public override void InitUnit()
        {
            CalculateStat();
            NowHp = Stat.MaxHp;
            Mp = Stat.MaxMp;
            NowBuff.Reset();
            IsInit = false;
            State.SetStateNextFrame(UnitState.None);

            if (!IsRegistAction)
            {
                EquipChanged();
                SkillChanged();
                EquipDataMgr.ChangedEquip += EquipChanged;
                SkillDataMgr.ChangedSkillEquip += SkillChanged;
                PowerMgr.ChangedPower += CalculateStat;
                IsRegistAction = true;
            }
        }

        private void OnDestroy()
        {
            EquipDataMgr.ChangedEquip -= EquipChanged;
            SkillDataMgr.ChangedSkillEquip -= SkillChanged;
            PowerMgr.ChangedPower -= CalculateStat;
        }

        public void UnitStart()
        {
            IsInit = true;
        }

        public override void OnDispose()
        {
            IsInit = false;
        }

        public override void OnObtain()
        {

        }

        private void CalculateStat()
        {
            Stat = PowerMgr.PlayerStat;

            if (BattleMgr.Instance.TryGetRoguelike(out var stage))
            {
                Stat = StatCalculator.ApplyRoguelikeEffects(Stat, stage.EffectIds);
            }
        }

        public void RefreshRoguelikeStat(bool addHp)
        {
            double beforeMaxHp = Stat.MaxHp;
            CalculateStat();

            if (addHp)
            {
                NowHp += Math.Max(0, Stat.MaxHp - beforeMaxHp);
            }

            NowHp = Math.Min(NowHp, Stat.MaxHp);
        }

        public void ResetBattleTarget()
        {
            TargetUnit = null;
            RigidBody.linearVelocity = Vector2.zero;

            if (IsAlived)
            {
                State.SetStateForced(UnitState.None);
                Ani.SetState(UnitState.None);
            }
        }

        private void EquipChanged()
        {
            string eid = EquipDataMgr.Instance.EquipId;

            if (Table.IsValidKey(TableType.Equip, eid))
            {
                AtkMissileKey = BaseMissile.GetKey(Table.GetString(TableType.Equip, EquipDataMgr.Instance.EquipId, "missile_res"));
                AttackSound = (SFXType)Enum.Parse(typeof(SFXType), Table.GetString(TableType.Equip, EquipDataMgr.Instance.EquipId, "sound_type"));
                Skin.SetWeapon(Table.GetString(TableType.Equip, eid, "res"));
            }
            else
            {
                AtkMissileKey = BaseMissile.GetKey(Constant.BaseMissilePath);
                AttackSound = Constant.BaseAttackSound;
                Skin.SetWeapon(Constant.BaseEquip);
            }
        }

        private void SkillChanged()
        {
            for (int i = 0; i < SkillDataMgr.Instance.Equips.Count; i++)
            {
                if (i >= SkillMissileKeys.Count)
                {
                    SkillMissileKeys.Add("");
                    BaseSkillCoolTime.Add(1f);
                    NowSkillCoolTimes.Add(1f);
                    SkillSounds.Add(SFXType.None);
                }

                string key = BaseMissile.GetKey(Table.GetString(TableType.Skill, SkillDataMgr.Instance.Equips[i], "missile_res"));

                if (SkillMissileKeys[i] != key)
                {
                    SkillMissileKeys[i] = key;
                    BaseSkillCoolTime[i] = Table.GetFloat(TableType.Skill, SkillDataMgr.Instance.Equips[i], "cooltime");
                    NowSkillCoolTimes[i] = Table.GetFloat(TableType.Skill, SkillDataMgr.Instance.Equips[i], "cooltime") * Stat.CoolTimeReduce;
                    SkillSounds[i] = (SFXType)Enum.Parse(typeof(SFXType), Table.GetString(TableType.Skill, SkillDataMgr.Instance.Equips[i], "sound_type"));
                }
            }
        }

        public override void TakeDamage(BattleStat stat, Buff buff, float multi)
        {
            if (BattleMgr.Instance.IsCombatPaused || IsInvincibility || !IsAlived)
            {
                return;
            }

            SoundMgr.Instance.PlaySFXSound(SFXType.PlayerHit);
            DamagedTime = 0f;
            IsDamagedTime = true;
            double damage = DamageCalculator.EnemyToPlayer(multi, Stat, NowBuff, stat, buff);
            double before = damage;
            damage = DamageCalculator.ApplyCri(damage, stat);
            PoolMgr.Instance.Get<UI_Damage>(Constant.DamageUIKey).SetValue(damage, damage != before, transform.position);
            Ani.Hit();

            if (damage > 0d)
            {
                NowHp -= damage;

                if (!IsAlived)
                {
                    State.SetStateNextFrame(UnitState.Dying);
                }
            }
        }

        protected override void UpdateTimer()
        {
            DamagedTime += Time.deltaTime;

            if (DamagedTime > DamageInterval)
            {
                IsDamagedTime = false;
            }

            for (int i = 0; i < NowSkillCoolTimes.Count; i++)
            {
                NowSkillCoolTimes[i] -= Time.deltaTime;
            }

            AttackInterval += Time.deltaTime;

            RecoveryTimer += Time.deltaTime;

            if (RecoveryTimer > Constant.AutoRecoveryTime)
            {
                RecoveryTimer -= Constant.AutoRecoveryTime;
                AutoRecovery();
            }
        }

        private void AutoRecovery()
        {
            NowHp += Constant.BaseHpRecovery + PowerMgr.GetSingleStat("hp_recovery_add") * PowerMgr.GetSingleStat("hp_recovery_multi");
            Mp += Constant.BaseMpRecovery + (float)PowerMgr.GetSingleStat("mp_recovery");

            if (Mp > Stat.MaxMp)
            {
                Mp = Stat.MaxMp;
            }

            if (NowHp > Stat.MaxHp)
            {
                NowHp = Stat.MaxHp;
            }
        }

        #region State Actions 
        private float TempAtkSpd = 0f;

        protected override void Born()
        {
            InitUnit();
            State.SetStateNextFrame(UnitState.None);
        }

        protected override void None()
        {
            if (State.IsFirstFrame)
            {
                Ani.SetState(State.GetState());
            }

            if (PlayerController.IsControlling)
            {
                Vector2 dir = PlayerController.Diff.normalized;
                RigidBody.linearVelocity = dir * Stat.Spd;
            }

            UpdateTarget();

            if (TargetUnit != null)
            {
                float dis = Vector2.Distance(transform.position, TargetUnit.transform.position);

                if (dis > Constant.AtkRange)
                {
                    if (PlayerController.IsControlling)
                    {
                        if (PlayerController.Diff != Vector3.zero)
                        {
                            State.SetStateNextFrame(UnitState.Move);
                        }
                    }
                    else
                    {
                        State.SetStateNextFrame(UnitState.Move);
                    }
                }
                else
                {
                    TargetAtk();
                }
            }
            else if (PlayerController.IsControlling)
            {
                if (PlayerController.Diff != Vector3.zero)
                {
                    State.SetStateNextFrame(UnitState.Move);
                }
            }
        }

        private void TargetAtk()
        {
            TempAtkSpd = Stat.AtkSpd;

            if (NowBuff.IsContain(BuffType.AtkSpdBuff))
            {
                TempAtkSpd = TempAtkSpd * (1 + NowBuff.GetValue(BuffType.AtkSpdBuff));
            }

            for (int i = 0; i < NowSkillCoolTimes.Count; i++)
            {
                if (NowSkillCoolTimes[i] <= 0f)
                {
                    string sid = SkillDataMgr.Instance.Equips[i];
                    float needMana = Formula.SkillManaNeed(sid, SkillDataMgr.Instance.GetSkillLv(sid));

                    if (Mp >= needMana)
                    {
                        State.SetStateNextFrame(UnitState.Skill);
                        SkillIdx = i;
                        break;
                    }
                }
            }

            if (AttackInterval > 1 / TempAtkSpd)
            {
                State.SetStateNextFrame(UnitState.Attack);
            }
        }

        protected override void Move()
        {
            if (State.IsFirstFrame)
            {
                Ani.SetState(State.GetState());
            }

            UpdateTarget();

            if (TargetUnit == null)
            {
                RigidBody.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 dir = PlayerController.IsControlling
                ? PlayerController.Diff.normalized
                : (TargetUnit.transform.position - transform.position).normalized;

            RigidBody.linearVelocity = dir * Stat.Spd;
            float dis = Vector2.Distance(transform.position, TargetUnit.transform.position);
            BodyRenderer.flipX = dir.x < 0f;

            if (PlayerController.IsControlling && PlayerController.Diff == Vector3.zero)
            {
                State.SetStateNextFrame(UnitState.None);
            }

            if (dis <= Constant.AtkRange)
            {
                TargetAtk();
                if (!PlayerController.IsControlling)
                {
                    RigidBody.linearVelocity = Vector2.zero;
                }
            }
        }

        protected override void Attack()
        {
            if (PlayerController.IsControlling)
            {
                Vector2 dir = PlayerController.Diff.normalized;
                RigidBody.linearVelocity = dir * Stat.Spd;
            }

            if (State.IsFirstFrame)
            {
                if (1 / TempAtkSpd < 0.5f)
                {
                    Ani.SetState(State.GetState(), 1 / TempAtkSpd);
                }
                else
                {
                    Ani.SetState(State.GetState());
                }

                AttackInterval = 0f;
                Vector2 dir = (TargetUnit.transform.position - transform.position).normalized;
                BodyRenderer.flipX = dir.x < 0f;
            }

            if (State.IsOverTimeOnce(Ani.GetPercentTime(50f)))
            {
                SoundMgr.Instance.PlaySFXSound(AttackSound);
                BaseMissile missile = PoolMgr.Instance.Get<BaseMissile>(AtkMissileKey);
                missile.InitMissile(Stat, NowBuff, transform.position, TargetUnit.transform.position);
                missile.SetKey(AtkMissileKey);
            }

            if (Ani.IsEndState())
            {
                if (PlayerController.IsControlling && PlayerController.Diff != Vector3.zero)
                {
                    State.SetStateNextFrame(UnitState.Move);
                }
                else
                {
                    State.SetStateNextFrame(UnitState.None);
                }
            }
        }

        protected override void Skill()
        {
            if (PlayerController.IsControlling)
            {
                Vector2 dir = PlayerController.Diff.normalized;
                RigidBody.linearVelocity = dir * Stat.Spd;
            }

            if (State.IsFirstFrame)
            {
                Ani.SetState(State.GetState(), 1 / TempAtkSpd);
                Vector2 dir = (TargetUnit.transform.position - transform.position).normalized;
                BodyRenderer.flipX = dir.x < 0f;
            }

            if (State.IsOverTimeOnce(Ani.GetPercentTime(50f)))
            {
                SoundMgr.Instance.PlaySFXSound(SkillSounds[SkillIdx]);
                string sid = SkillDataMgr.Instance.Equips[SkillIdx];
                NowSkillCoolTimes[SkillIdx] = BaseSkillCoolTime[SkillIdx] * Stat.CoolTimeReduce;
                BaseMissile missile = PoolMgr.Instance.Get<BaseMissile>(SkillMissileKeys[SkillIdx]);
                missile.InitMissile(Stat, NowBuff, transform.position, TargetUnit.transform.position);

                int lv = SkillDataMgr.Instance.GetSkillLv(sid);
                float baseValue = Formula.SkillValueOnLv(sid, "main_value", lv);
                float childValue = Formula.SkillValueOnLv(sid, "sub_value", lv);
                float buffTime = Formula.SkillValueOnLv(sid, "buff_time", lv);
                float buffValue = Formula.SkillValueOnLv(sid, "buff_value", lv);

                missile.SetValue(baseValue, childValue, buffTime, buffValue);
                missile.SetKey(SkillMissileKeys[SkillIdx]);
                Mp -= Formula.SkillManaNeed(sid, lv);
            }

            if (Ani.IsEndState())
            {
                if (PlayerController.IsControlling)
                {
                    State.SetStateNextFrame(UnitState.Move);
                }
                else
                {
                    State.SetStateNextFrame(UnitState.None);
                }
            }
        }

        protected override void Dying()
        {
            if (State.IsFirstFrame)
            {
                Ani.SetState(State.GetState());
                BattleMgr.Instance.DiePlayer();
            }

            if (Ani.IsEndState())
            {
                State.SetStateNextFrame(UnitState.Die);
            }
        }

        protected override void Die()
        {

        }
        #endregion

        private void UpdateTarget()
        {
            if (TargetUnit == null || !TargetUnit.IsAlived
                || Constant.AtkRange < Vector2.Distance(TargetUnit.transform.position, transform.position))
            {
                TargetUnit = BattleMgr.Instance.GetCloestEnemy(gameObject.transform.position);
            }
        }
    }
}
