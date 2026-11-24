using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using Game.Common;
using Game.Core;

namespace Game.UI.Popup
{
    // 씬 전환에서 사용할 UI와 동시에 사용하기 위해 Alltime으로 
    public class PopupMgr : SingletonAllTime<PopupMgr>
    {
        [Header("Button Lock Obj")]
        [SerializeField]
        private GameObject ObjButtonLock;

        [Header("Popups")]
        [SerializeField]
        private Transform PopupRoot;
        [SerializeField]
        private List<SerialKeyValue<PopupType, string>> TypeAndPath = new List<SerialKeyValue<PopupType, string>>();

        private Dictionary<PopupType, string> PopupPath = new Dictionary<PopupType, string>();
        private Stack<BasePopup> PopupStack = new Stack<BasePopup>();
        private Dictionary<PopupType, BasePopup> PopupCache = new Dictionary<PopupType, BasePopup>();
        private Dictionary<PopupType, AsyncOperationHandle> Handles = new Dictionary<PopupType, AsyncOperationHandle>();

        private void Start()
        {
            foreach (var value in TypeAndPath)
            {
                if (!PopupPath.ContainsKey(value.Key))
                {
                    PopupPath.Add(value.Key, value.Value);
                }
            }
        }

        public async Task<T> Get<T>(PopupType type) where T : BasePopup
        {
            if (PopupCache.ContainsKey(type))
            {
                PopupStack.Push(PopupCache[type]);
                return (T)PopupCache[type];
            }

            if (!PopupPath.ContainsKey(type))
            {
                MyLogger.Log(LogType.Error, LogSituation.GetPool, ProcessType.Failed, "didn't setting popup path");
                return null;
            }

            if (Handles.ContainsKey(type))
            {
                await Handles[type].Task;
                PopupStack.Push(PopupCache[type]);
                return (T)PopupCache[type];
            }

            AsyncOperationHandle handle = Addressables.LoadAssetAsync<GameObject>(PopupPath[type]);
            Handles.Add(type, handle);
            handle.Completed += (result) =>
            {
                if (result.Status == AsyncOperationStatus.Succeeded)
                {

                    T obj = Instantiate(result.Result as GameObject, PopupRoot).GetComponent<T>();
                    obj.gameObject.transform.localPosition = Vector3.zero;
                    obj.gameObject.SetActive(false);
                    obj.SetType(type);
                    PopupCache.Add(type, obj);
                    PopupStack.Push(obj);
                    MyLogger.Log(LogType.Log, LogSituation.GetPool, ProcessType.Done, "Success Open and Caching Popup");
                }
                else
                {
                    MyLogger.Log(LogType.Error, LogSituation.GetPool, ProcessType.Failed, "Load Failed");
                }

                Handles.Remove(type);
            };

            await handle.Task;

            return (T)PopupCache[type];
        }

        public void Close()
        {
            if (PopupStack.Count == 0)
            {
                return;
            }

            BasePopup popup = PopupStack.Pop();
        }

        public void Close(PopupType type)
        {
            if (PopupStack.Count == 0)
            {
                return;
            }

            if (PopupStack.Contains(PopupCache[type]))
            {
                BasePopup popup = PopupCache[type];
                Stack<BasePopup> tempStack = new Stack<BasePopup>();

                while (PopupStack.Count > 0)
                {
                    BasePopup current = PopupStack.Pop();

                    if (current == popup)
                    {
                        break;
                    }

                    tempStack.Push(current);
                }

                while (tempStack.Count > 0)
                {
                    PopupStack.Push(tempStack.Pop());
                }
            }
        }

        public void SetActiveButtonLock(bool active)
        {
            ObjButtonLock.SetActive(active);
        }
    }
}
