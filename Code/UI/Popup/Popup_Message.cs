using TMPro;
using UnityEngine;

namespace Game.UI.Popup
{
    public class Popup_Message : BasePopup
    {
        [SerializeField]
        private TextMeshProUGUI TextMessage;

        private float CloseTime = 1.5f;
        private float Timer = 0f;
        public void Init(string text, float time = 5f)
        {
            TextMessage.text = text;
            Timer = 0f;
            CloseTime = time;
        }

        protected override void Update()
        {
            base.Update();

            Timer += Time.deltaTime;

            if (Timer > CloseTime && !IsEnd)
            {
                Close();
            }
        }
    }
}
