using System.Collections.Generic;

using UnityEngine;

using Game.Common;
using Game.Core;

namespace Game.UI
{
    public class UIMgr : Singleton<UIMgr>
    {
        [SerializeField]
        private Transform Main;
        [SerializeField]
        private List<SerialKeyValue<MainUIType, SubUI>> MainUIs = new List<SerialKeyValue<MainUIType, SubUI>>();

        private Dictionary<MainUIType, SubUI> UIs = new Dictionary<MainUIType, SubUI>();

        private void Start()
        {
            foreach (var value in MainUIs)
            {
                if (!UIs.ContainsKey(value.Key))
                {
                    UIs.Add(value.Key, value.Value);
                }
            }
        }

        public T Get<T>(MainUIType type) where T : SubUI
        {
            if (UIs.ContainsKey(type))
            {
                return (T)UIs[type];
            }
            else
            {
                Debug.LogError("Wrong type");
                return default(T);
            }
        }

        public void CloseAllUis()
        {
            foreach (var ui in UIs.Values)
            {
                ui.Close(false);
            }
        }
    }
}
