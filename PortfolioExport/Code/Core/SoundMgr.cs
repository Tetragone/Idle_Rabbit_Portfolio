using System;

using Game.Common;
using Game.Data;

namespace Game.Core
{
    // 공개본에서는 요청만 전달한다. 오디오 로딩과 실제 재생은 포함하지 않는다.
    public class SoundMgr : SingletonAllTime<SoundMgr>
    {
        public event Action<SFXType> SfxRequested;
        public event Action<BGMType, bool> BgmRequested;

        // 대기할 오디오 로딩이 없다는 의미이며 실제 소리 재생 완료를 뜻하지 않는다.
        public bool IsReadiedSFXLoad => true;
        public bool IsReadiedBGM => true;

        public void StartInit() { }

        public void PlaySFXSound(SFXType type)
        {
            if (type != SFXType.None)
            {
                SfxRequested?.Invoke(type);
            }
        }

        public void PlayBGMSound(BGMType type, bool replay = false)
        {
            BgmRequested?.Invoke(type, replay);
        }

        public void SetSound(SoundType type, float value)
        {
            OptionDataMgr.Instance.SetSoundValue(type, value);
        }
    }
}
