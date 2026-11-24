using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Common;

namespace Game.UI
{
    public class UI_Gauge : MonoBehaviour
    {
        public Image GaugeBg;
        public Image GaugeMain;
        public TextMeshProUGUI TextGague;
        [SerializeField]
        private bool IsUseText = true;
        [SerializeField]
        private bool IsUseMaxText = false;

        // 처음에 안들어 올것 같은 값 아무거나
        private double DNowValue = -99d;
        private int INowValue = -99;
        private float PNowValue = -99f;

        public void SetFillMethod(Image.FillMethod method)
        {
            GaugeMain.type = Image.Type.Filled;
            GaugeMain.fillMethod = method;
        }

        public void SetGauge(double nowValue, double maxValue)
        {
            GaugeMain.fillAmount = (float)(nowValue / maxValue);

            if (DNowValue != nowValue && IsUseText)
            {
                if (IsUseMaxText)
                {
                    TextGague.text = string.Format("{0} / {1}", NumberFormatter.ToCompact(nowValue), NumberFormatter.ToCompact(maxValue));
                }
                else
                {
                    TextGague.text = NumberFormatter.ToCompact(nowValue);
                }

                DNowValue = nowValue;
            }
        }

        public void SetGauge(int nowValue, int maxValue)
        {
            GaugeMain.fillAmount = nowValue / (float)maxValue;

            if (INowValue != nowValue && IsUseText)
            {
                if (IsUseMaxText)
                {
                    TextGague.text = string.Format("{0} / {1}", nowValue, maxValue);
                }
                else
                {
                    TextGague.text = nowValue.ToString();
                }

                INowValue = nowValue;
            }
        }

        public void SetGauge(float percent)
        {
            percent = percent < 0f ? 0f : percent;
            GaugeMain.fillAmount = percent;

            if (PNowValue != percent && IsUseText)
            {
                TextGague.text = (percent * 100f).ToString("F0");
                PNowValue = percent;
            }
        }
    }
}
