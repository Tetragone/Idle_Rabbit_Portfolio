using System;
using System.Threading.Tasks;

using UnityEngine;

using Game.Data;

namespace Game.Battle.Unit
{
    // 외부 빌더 구현을 배포하지 않기 위해 공개본에서 정의한 호출 계약이다.
    public interface IPlayerSkinBuilder
    {
        string Armor { set; }
        string Helmet { set; }
        string Weapon { set; }
        Task RebuildAsync();
        Sprite GetIcon();
    }

    public class PlayerUnitSkinAdapter : MonoBehaviour
    {
        // 실제 빌더 코드는 포함하지 않는다. 계약을 구현한 컴포넌트를 연결한다.
        [SerializeField] private MonoBehaviour Builder;
        public bool IsBuildDone = false;

        private IPlayerSkinBuilder SkinBuilder => Builder as IPlayerSkinBuilder
            ?? throw new InvalidOperationException("IPlayerSkinBuilder를 구현한 컴포넌트를 연결해야 합니다.");

        public async void SetArmor(string name)
        {
            SkinBuilder.Armor = name;
            SkinBuilder.Helmet = name;
            IsBuildDone = false;
            await SkinBuilder.RebuildAsync();
            SetIconSprite();
            IsBuildDone = true;
        }

        public async void SetWeapon(string name)
        {
            SkinBuilder.Weapon = name;
            IsBuildDone = false;
            await SkinBuilder.RebuildAsync();
            SetIconSprite();
            IsBuildDone = true;
        }

        public void SetIconSprite()
        {
            PlayerDataMgr.Instance.Portrait = SkinBuilder.GetIcon();
        }
    }
}
