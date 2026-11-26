using UnityEngine;
using UnityEngine.UI;

using Game.Pooling;

namespace Game.UI.Battle
{
    public class UI_Hp : MonoBehaviour, IPoolable
    {
        [SerializeField]
        private UI_Gauge Gauge;

        private Transform Target;

        private const float HideHpTime = 5f;
        private float Timer = 0f;

        private void Awake()
        {
            Gauge.SetFillMethod(Image.FillMethod.Horizontal);
        }

        public void SetTarget(Transform target)
        {
            Target = target;
        }

        public void SetGauge(double nowHp, double maxHp)
        {
            Gauge.gameObject.SetActive(true);
            Timer = 0f;
            Gauge.SetGauge(nowHp, maxHp);
        }

        public void OnDispose()
        {
            Target = null;
        }

        public void OnObtain()
        {
            Gauge.SetGauge(1.0f);
            Gauge.gameObject.SetActive(false);
            transform.SetParent(UI_HpRoot.Root);
        }

        private void LateUpdate()
        {
            if (Target != null)
            {
                gameObject.transform.position = Camera.main.WorldToScreenPoint(Target.transform.position) + new Vector3(0, -20f, 0);
            }

            Timer += Time.deltaTime;

            if (Timer > HideHpTime)
            {
                Gauge.gameObject.SetActive(false);
            }
        }
    }
}
