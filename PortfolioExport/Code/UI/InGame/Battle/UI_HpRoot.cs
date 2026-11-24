using UnityEngine;

namespace Game.UI.Battle
{
    public class UI_HpRoot : MonoBehaviour
    {
        [SerializeField]
        private Transform Parent;
        public static Transform Root;

        private void Awake()
        {
            Root = Parent;
        }
    }
}
