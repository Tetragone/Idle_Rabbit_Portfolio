using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class UI_Loading : MonoBehaviour
    {
        [SerializeField]
        private Image Image;
        public bool IsDone = false;

        public void StartLoading()
        {
            IsDone = false;
            StartCoroutine(FadeOut());
        }

        public void EndLoading()
        {
            IsDone = false;
            StartCoroutine(FadeIn());
        }

        private IEnumerator FadeIn()
        {
            float alpha = 1f;
            while (alpha > 0f)
            {
                alpha -= 0.1f;
                Image.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
            Image.raycastTarget = false;
            IsDone = true;
            yield return null;
        }

        private IEnumerator FadeOut()
        {
            Image.raycastTarget = true;
            float alpha = 0f;
            while (alpha < 1f)
            {
                alpha += 0.1f;
                Image.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
            IsDone = true;
        }
    }
}
