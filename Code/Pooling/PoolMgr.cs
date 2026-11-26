using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using Game.Common;
using Game.Core;
using Game.UI;

namespace Game.Pooling
{
    public class PoolMgr : SingletonAllTime<PoolMgr>
    {
        private Dictionary<string, Type> PoolType = new Dictionary<string, Type>();
        private Dictionary<string, object> Pools = new Dictionary<string, object>();
        private Dictionary<string, GameObject> PoolParents = new Dictionary<string, GameObject>();
        private Dictionary<string, ProcessType> PoolProgress = new Dictionary<string, ProcessType>();
        private Dictionary<string, int> PoolRegistCounts = new Dictionary<string, int>();

        // 등록후 바로 몇프레임 뒤에 사용 권장
        public void Register<T>(string key, string path, int poolCount) where T : Component, IPoolable
        {
            if (Pools.ContainsKey(key))
            {
                MyLogger.Log(LogType.Error, LogSituation.RegistPool, ProcessType.Failed, "Duplicated Key");
                return;
            }

            if (PoolProgress.ContainsKey(key))
            {
                if (PoolProgress[key] != ProcessType.Failed)
                {
                    MyLogger.Log(LogType.Error, LogSituation.RegistPool, ProcessType.Failed, "Registering Key");
                }

                return;
            }

            PoolType.Add(key, typeof(T));
            PoolProgress.Add(key, ProcessType.Doing);
            PoolRegistCounts.Add(key, poolCount);
            AsyncOperationHandle handle = Addressables.LoadAssetAsync<GameObject>(path);
            handle.Completed += (result) =>
            {
                if (result.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject parent = new GameObject(string.Format("object_pools : {0}", key));
                    parent.transform.parent = transform;
                    // 어찌됐던 안보이는 곳으로
                    // on/off시에 잠시 보이는 현상이 있기 때문
                    parent.transform.localPosition = new Vector3(9999f, 9999f);
                    T res = Instantiate(result.Result as GameObject, parent.transform).GetComponent<T>();
                    ObjectPool<T> newPool = new ObjectPool<T>(res, parent.transform);
                    StartCoroutine(newPool.InitCreate(poolCount, 5, () =>
                    {
                        PoolProgress[key] = ProcessType.Done;
                    }));
                    Pools.Add(key, newPool);
                    PoolParents.Add(key, parent);
                    MyLogger.Log(LogType.Log, LogSituation.RegistPool, ProcessType.Done, "Success Regist Pool");
                    res.gameObject.SetActive(false);
                }
                else
                {
                    PoolProgress[key] = ProcessType.Failed;
                    MyLogger.Log(LogType.Error, LogSituation.RegistPool, ProcessType.Failed, "Load Failed");
                }
            };
        }

        public ProcessType GetProcess(string key)
        {
            if (!PoolProgress.ContainsKey(key))
            {
                return ProcessType.Ready;
            }
            else
            {
                return PoolProgress[key];
            }
        }

        public float PoolPercent(string key)
        {
            if (!PoolRegistCounts.ContainsKey(key))
            {
                return 100f;
            }
            else if (!Pools.ContainsKey(key))
            {
                return 0f;
            }
            else
            {
                return ((IObjectCount)Pools[key]).Count / (float)PoolRegistCounts[key] * 100f;
            }
        }

        public T Get<T>(string key) where T : Component, IPoolable
        {
            if (Pools.ContainsKey(key))
            {
                if (PoolType[key] != typeof(T))
                {
                    MyLogger.Log(LogType.Error, LogSituation.GetPool, ProcessType.Failed, string.Format("Wrong Type {0}", typeof(T)));
                    return null;
                }

                return ((ObjectPool<T>)Pools[key]).Get();
            }

            MyLogger.Log(LogType.Error, LogSituation.GetPool, ProcessType.Failed, string.Format("Wrong Key {0}", key));
            return null;
        }

        public void Return<T>(string key, T obj) where T : Component, IPoolable
        {
            if (Pools.ContainsKey(key))
            {
                ((ObjectPool<T>)Pools[key]).Return(obj);
                return;
            }
            else
            {
                MyLogger.Log(LogType.Error, LogSituation.ReturnPool, ProcessType.Failed, "Wrong Key");
            }
        }

        public bool IsContain(string key)
        {
            return Pools.ContainsKey(key);
        }

    }
}
