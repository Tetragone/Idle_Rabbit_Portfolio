using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Game.Common
{
    public class ContinueButton : Selectable
    {
        public class ButtonClickedEvent : UnityEvent { }

        // 누르고 있는 동안 호출할 이벤트다.
        [FormerlySerializedAs("onClick")]
        [SerializeField]
        private ButtonClickedEvent m_OnHold = new ButtonClickedEvent();

        public ButtonClickedEvent onHold
        {
            get { return m_OnHold; }
            set { m_OnHold = value; }
        }

        [FormerlySerializedAs("onClickEnd")]
        [SerializeField]
        private ButtonClickedEvent m_OnClickEnd = new ButtonClickedEvent();
        public ButtonClickedEvent onClickEnd
        {
            get { return m_OnClickEnd; }
            set { m_OnClickEnd = value; }
        }

        private int InvokeCount = 0;
        private bool IsPressing = false;
        private float IntervalTimer = 0f;

        private const int ChangeToFastCount = 10;
        private const float SlowInterval = 0.2f;
        private const float FastInterval = 0.05f;

        public void RemoveAllClicks()
        {
            onHold.RemoveAllListeners();
            onClickEnd.RemoveAllListeners();
        }

        public void ForcedClickEnd()
        {
            ClickEnd();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            IsPressing = true;
            InvokeCount = 0;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            ClickEnd();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            ClickEnd();
        }

        private void Update()
        {
            if (IsPressing)
            {
                if (IntervalTimer < 0f)
                {
                    onHold?.Invoke();

                    if (InvokeCount > ChangeToFastCount)
                    {
                        IntervalTimer += FastInterval;
                    }
                    else
                    {
                        IntervalTimer += SlowInterval;
                    }

                    InvokeCount++;
                }

                IntervalTimer -= Time.deltaTime;
            }
        }

        private void ClickEnd()
        {
            // 강제로 손을 땐것과 비슷한 효과를 내는 방식이 들어갈수 있기 때문
            if (IsPressing)
            {
                IsPressing = false;
                InvokeCount = 0;
                onClickEnd?.Invoke();
            }
        }
    }
}
