using TMPro;
using UnityEngine;

using Game.Common;
using Game.Pooling;

namespace Game.UI.Battle
{
    public class UI_Damage : MonoBehaviour, IPoolable
    {
        [SerializeField]
        private TextMeshProUGUI TextDamage;
        [SerializeField]
        private Animator Ani;

        private bool IsStart = false;

        public void SetValue(double damage, bool isCri, Vector3 position)
        {
            TextDamage.color = isCri ? Color.red : Color.white;
            TextDamage.text = NumberFormatter.ToCompact(damage);
            if (isCri)
            {
                Ani.SetTrigger("cri");
            }
            else
            {
                Ani.SetTrigger("normal");
            }

            IsStart = true;
            transform.position = Camera.main.WorldToScreenPoint(position) + new Vector3(0, 80, 0);
        }

        private void Update()
        {
            if (IsStart)
            {
                if (Ani.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f)
                {
                    PoolMgr.Instance.Return(Constant.DamageUIKey, this);
                }
            }
        }

        public void OnDispose()
        {
            IsStart = false;
        }

        public void OnObtain()
        {
            IsStart = false;
            transform.SetParent(UI_HpRoot.Root);
        }
    }
}
