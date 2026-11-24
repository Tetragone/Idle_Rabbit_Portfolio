using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using Game.Core;

namespace Game.Battle
{
    public class Map : MonoBehaviour
    {
        public static bool IsReadied => IsDone && SoundMgr.Instance.IsReadiedBGM;
        private static bool IsDone = false;

        public SpriteRenderer Bg;

        private string BeforePath = "";

        private void Awake()
        {
            BattleMgr.StageChanged += ChangeBg;
        }

        private void OnDestroy()
        {
            BattleMgr.StageChanged -= ChangeBg;
        }

        private void ChangeBg()
        {
            string path = BattleMgr.Instance.StageBgPath;

            if (path == BeforePath)
            {
                return;
            }

            IsDone = false;
            SoundMgr.Instance.PlayBGMSound(BattleMgr.Instance.StageBGMType);
            var handle = Addressables.LoadAssetAsync<Sprite>(BattleMgr.Instance.StageBgPath);
            handle.Completed += (result) =>
            {
                if (result.Status == AsyncOperationStatus.Succeeded)
                {
                    Bg.sprite = result.Result;
                    Addressables.Release(handle);
                    IsDone = true;
                }
            };
        }
    }
}
