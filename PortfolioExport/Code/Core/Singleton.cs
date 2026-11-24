using UnityEngine;

namespace Game.Core
{
    public class Singleton<T> : MonoBehaviour where T : class
    {
        public static T Instance;

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = GetComponent<T>();
            }
            else
            {
                Debug.LogWarningFormat("duplicated singleton obeject name is : {0}", gameObject.name.ToString());
            }
        }
    }
}
