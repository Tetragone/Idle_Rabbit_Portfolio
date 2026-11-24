using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Common;
using Game.Core;

namespace Game.UI.Popup
{
    public abstract class BasePopup : MonoBehaviour
    {
        public TextMeshProUGUI TextTitle;
        public TextMeshProUGUI TextContent;

        public Button ButtonBlock;
        public Button ButtonYes;
        public TextMeshProUGUI TextYes;
        public Button ButtonNo;
        public TextMeshProUGUI TextNo;
        public Button ButtonOk;
        public TextMeshProUGUI TextOk;
        public Button ButtonClose;

        public Animator Ani;

        protected bool IsEnd = false;
        protected int NowAniHash = 0;

        public PopupType Type => NowType;
        private PopupType NowType;

        public void SetType(PopupType type)
        {
            NowType = type;
        }

        public virtual void OpenAction() { }
        public virtual void CloseAction() { }

        public void Open()
        {
            OpenAction();
            gameObject.SetActive(true);
            SoundMgr.Instance.PlaySFXSound(SFXType.UIOpen);
            if (Ani != null)
            {
                Ani.SetTrigger("Open");
            }
            IsEnd = false;
        }

        public void Close()
        {
            IsEnd = true;
            CloseAction();
            SoundMgr.Instance.PlaySFXSound(SFXType.UIClose);
            if (Ani != null)
            {
                NowAniHash = Ani.GetCurrentAnimatorStateInfo(0).shortNameHash;
                Ani.SetTrigger("Close");
            }
            else
            {
                gameObject.SetActive(false);
            }

            PopupMgr.Instance.Close(NowType);
        }

        protected virtual void Awake()
        {
            // 중간에 연결이 해제되거나 추가될 일이 없으니 ?로 대체
            ButtonClose?.onClick.AddListener(Close);
            ButtonBlock?.onClick.AddListener(Close);
        }

        protected virtual void Update()
        {
            if (IsEnd && Ani != null)
            {
                int hash = Ani.GetCurrentAnimatorStateInfo(0).shortNameHash;

                if (hash != NowAniHash && Ani.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.99f)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
