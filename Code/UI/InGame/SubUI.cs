using System;

using UnityEngine;
using UnityEngine.UI;

using Game.Common;
using Game.Core;

namespace Game.UI
{
    public class SubUI : MonoBehaviour
    {
        [SerializeField]
        protected Button ButtonClose;

        public event Action<bool> CloseAction;

        protected virtual void Awake()
        {
            ButtonClose.onClick.AddListener(() =>
            {
                Close();
            });
        }

        public virtual void Open(bool useSound = true)
        {
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
                if (useSound)
                {
                    SoundMgr.Instance.PlaySFXSound(SFXType.UIOpen);
                }
            }
        }

        public virtual void Close(bool useSound = true)
        {
            if (gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
                CloseAction?.Invoke(true);
                CloseAction = null;

                if (useSound)
                {
                    SoundMgr.Instance.PlaySFXSound(SFXType.UIClose);
                }
            }
        }
    }
}
