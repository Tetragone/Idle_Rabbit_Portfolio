using UnityEngine;

namespace Game.Core
{
    public class SingletonAllTime<T> : MonoBehaviour where T : class
    {
        public static T Instance;

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                DontDestroyOnLoad(this);
                Instance = GetComponent<T>();
            }
            else
            {
                Debug.LogWarningFormat("duplicated singleton obeject name is : {0}", gameObject.name.ToString());
                Destroy(this);
            }
        }
    }
}
