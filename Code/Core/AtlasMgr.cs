using System.Collections.Generic;

using UnityEngine;
using UnityEngine.U2D;

using Game.Common;

namespace Game.Core
{
    public class AtlasMgr : SingletonAllTime<AtlasMgr>
    {
        [SerializeField] private SpriteAtlas Common;
        [SerializeField] private SpriteAtlas Skill;
        [SerializeField] private SpriteAtlas Equip;
        [SerializeField] private SpriteAtlas Upgrade;
        [SerializeField] private SpriteAtlas Enemys;

        private Dictionary<string, Sprite> DicCommon = new Dictionary<string, Sprite>();
        private Dictionary<string, Sprite> DicSkill = new Dictionary<string, Sprite>();
        private Dictionary<string, Sprite> DicEquip = new Dictionary<string, Sprite>();
        private Dictionary<string, Sprite> DicUpgrade = new Dictionary<string, Sprite>();
        private Dictionary<string, Sprite> DicEnemys = new Dictionary<string, Sprite>();

        public Sprite GetSprite(AtlasType type, string path)
        {
            switch (type)
            {
                case AtlasType.Skill: return GetSpriteInDic(Skill, DicSkill, path);
                case AtlasType.Equip: return GetSpriteInDic(Equip, DicEquip, path);
                case AtlasType.Upgrade: return GetSpriteInDic(Upgrade, DicUpgrade, path);
                case AtlasType.Enemys: return GetSpriteInDic(Enemys, DicEnemys, path);
                case AtlasType.Common: 
                default: return GetSpriteInDic(Common, DicCommon, path);
            }
        }

        private Sprite GetSpriteInDic(SpriteAtlas atlas, Dictionary<string, Sprite> dic, string path)
        {
            if (!dic.ContainsKey(path))
            {
                // 공개본에는 원본 Atlas가 없으므로 Image의 기본 사각형 표시를 사용한다.
                if (atlas == null)
                {
                    return null;
                }

                Sprite sprite = atlas.GetSprite(path);

                if (sprite == null)
                {
                    return null;
                }

                dic.Add(path, sprite);
            }

            return dic[path];
        }
    }
}
