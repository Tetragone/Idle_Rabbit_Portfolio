using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using Game.Common;
using Game.UI;

namespace Game.Pooling
{
    public interface IObjectCount
    {
        public int Count { get; }
        public int Capacity { get; }
    }

    public class ObjectPool<T> : IObjectCount where T : Component, IPoolable
    {
        public int Count
        {
            get
            {
                return Pool.Count;
            }
        }

        public int Capacity
        {
            get
            {
                return PoolCapacity;
            }
        }

        private Queue<T> Pool = new Queue<T>();
        private int PoolCapacity = 10;
        private T Res;
        private Transform Parent;
        private HashSet<T> PooledSet = new HashSet<T>();

        public ObjectPool(T res, Transform parent)
        {
            Res = res;
            Parent = parent;
        }

        public IEnumerator InitCreate(int poolCount, int batchSize = 5, UnityAction endAction = null)
        {
            PoolCapacity = poolCount;

            for (int i = 0; i < PoolCapacity; i++)
            {
                if (i % batchSize == 0)
                {
                    yield return null;
                    yield return null;
                    yield return null;
                }

                Create();
            }

            if (endAction != null)
            {
                endAction.Invoke();
            }
        }

        private void Create()
        {
            T obj = GameObject.Instantiate(Res, Parent);
            obj.gameObject.SetActive(false);
            Pool.Enqueue(obj);
            PooledSet.Add(obj);
        }

        public T Get()
        {
            if (Pool.Count == 0)
            {
                Create();
                PoolCapacity++;
            }

            T obj = Pool.Dequeue();
            obj.gameObject.SetActive(true);
            obj.OnObtain();
            PooledSet.Remove(obj);
            return obj;
        }

        public void Return(T obj)
        {
            if (PooledSet.Contains(obj))
            {
                MyLogger.Log(LogType.Warning, LogSituation.ReturnPool, ProcessType.Failed, "duplicated obj");
                return;
            }

            obj.OnDispose();
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(Parent);
            Pool.Enqueue(obj);
            PooledSet.Add(obj);
        }
    }
}
