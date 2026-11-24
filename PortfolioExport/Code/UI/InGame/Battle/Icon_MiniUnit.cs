using UnityEngine;
using UnityEngine.UI;

using Game.Pooling;

namespace Game.UI.Battle
{
    public class Icon_MiniUnit : MonoBehaviour, IPoolable
    {
        public RectTransform Rect;
        [SerializeField]
        private Image Unit;

        public void ToPlayerUnit(bool isPlayer)
        {
            Unit.color = isPlayer ? Color.green : Color.red;
        }

        public void OnObtain() { }

        public void OnDispose() { }
    }
}
