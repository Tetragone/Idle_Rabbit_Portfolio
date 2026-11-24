using UnityEngine;

using Game.Common;

namespace Game.Battle.Unit
{
    public class BaseUnitAnimation : MonoBehaviour
    {
        [SerializeField]
        private Animator Ani;

        public void SetState(UnitState state, float speed = 1f)
        {
            foreach (var variable in new[] { "Idle", "Ready", "Run", "Die" })
            {
                Ani.SetBool(variable, false);
            }

            switch (state)
            {
                case UnitState.Born:
                    Ani.SetBool("Idle", true);
                    break;
                case UnitState.None:
                    Ani.SetBool("Ready", true);
                    break;
                case UnitState.Move:
                    Ani.SetBool("Run", true);
                    break;
                case UnitState.Attack:
                    Ani.SetTrigger("Attack");
                    break;
                case UnitState.Dying:
                    Ani.SetBool("Die", true);
                    break;
                case UnitState.Skill:
                    Ani.SetTrigger("Attack2");
                    break;
            }

            Ani.speed = speed;
        }

        public float GetPercentTime(float percent)
        {
            AnimatorStateInfo stateInfo = Ani.GetCurrentAnimatorStateInfo(0);
            return stateInfo.length * percent * 0.01f;
        }

        public bool IsEndState()
        {
            AnimatorStateInfo stateInfo = Ani.GetCurrentAnimatorStateInfo(0);
            return stateInfo.loop ? false : stateInfo.normalizedTime > 0.99f;
        }

        public void Hit()
        {
            Ani.SetTrigger("Hit");
        }

        public void Heal()
        {
            Ani.SetTrigger("Heal");
        }
    }
}
