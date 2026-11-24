using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

using Game.Battle;
using Game.UI;

namespace Game.Core
{
    public class SceneMgr : SingletonAllTime<SceneMgr>
    {
        public UI_Loading Loading;

        public void LoadScene(string name)
        {
            Loading.StartLoading();

            StartCoroutine(LoadSceneCoroutine(name));
        }

        private IEnumerator LoadSceneCoroutine(string name)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(name);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f || !Loading.IsDone)
            {
                yield return null;
            }

            op.allowSceneActivation = true;

            while (BattleMgr.Instance == null)
            {
                yield return null;
            }

            while (!(BattleMgr.Instance.IsReadied && Map.IsReadied))
            {
                yield return null;
            }

            Loading.EndLoading();
        }

        public void OpenLoading()
        {
            Loading.StartLoading();
        }

        public void CloseLoading()
        {
            Loading.EndLoading();
        }
    }
}
