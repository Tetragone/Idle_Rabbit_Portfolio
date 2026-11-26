using System.Collections.Generic;

namespace Game.Common
{
    public static class Constant
    {
        public const string BaseMissilePath = "EquipMissile";
        public const string BaseEquip = "HunterKnife";
        public const SFXType BaseAttackSound = SFXType.PlayerAttack;
        public const string PlayerKey = "Player1";
        public const string PlayerPath = "PlayerUnit";
        public const string MiniUnitKey = "Mini";
        public const string MiniUnitPath = "MiniUnit";
        public const string EnemyHpKey = "EnemyHp";
        public const string EnemyHpPath = "EnemyHp";
        public const string DamageUIKey = "UI_Damage";
        public const string DamageUIPath = "UI_Damage";

        public static readonly List<string> GradeBg = new List<string> { "common", "rare", "eqic", "unique", "lengenary" };
        public static readonly List<string> GradeName = new List<string> { "일반", "희귀", "영웅", "유일", "전설" };
        public static readonly List<string> GradeColor = new List<string> { "656565", "2F637B", "752121", "806C1C", "317431" };

        public const float NormalMapMaxX = 37.5f;
        public const float NormalMapMaxY = 12f;

        public static readonly List<Currency> TopUICurrency = new List<Currency> { Currency.Gold, Currency.Gem };
        public static readonly List<Currency> EquipUICurrency = new List<Currency> { Currency.EquipUp };
        public static readonly List<Currency> SkillUICurrency = new List<Currency> { Currency.EquipUp };
        public static readonly List<Currency> UpgradeUICurrency = new List<Currency> { Currency.Gold, Currency.MoonBless };
        public static readonly List<Currency> UpgradeSoulUICurrency = new List<Currency> { Currency.Gold, Currency.MoonBless };

        public const string UpgradedTextColor = "55FF00";

        public const int MaxGoldDunegonStage = 50;
        public const int MaxRoguelikeStage = 50;

        public const int SkillEquipCount = 6;
        public const int SkillPresetCount = 6;
        public static readonly List<int> UnlockSkillSlotLvs = new List<int> { 0, 0, 25, 50, 100, 200 };
        public static readonly List<int> SkillStarUpNeed = new List<int> { 5, 5, 5, 5, 5, 10, 10, 10, 10, 10, 15, 15, 15, 15, 15 };
        public const int SkillMaxStar = 15;
        public const int SkillMaxLv = 25;

        public const int EquipAdvanceCount = 5;
        public const int EquipStarUpCount = 5;
        public const int EquipMaxLevel = 100;

        public const int SoulSubOptionAddedLvInterval = 20;
        public const int SoulSubOptionMaxCount = 4;
        public const int SoulMaxLevel = 100;

        public const float AtkRange = 5f;

        public const int EnemySpawStartCount = 15;
        public const int MaxEnemySpawCount = 60;
        public const int RegionSpawStartCount = 1;
        public const int MaxRegionSpawCount = 5;

        public const float AutoRecoveryTime = 1f;

        public const float BaseAtk = 100f;
        public const float BaseHp = 1000f;
        public const float BaseCri = 0f;
        public const float BaseCriDam = 100f;
        public const float BaseMp = 1000f;
        public const float BaseAtkSpd = 1f;
        public const float BaseSpd = 1f;
        public const float BaseCooltimeReduce = 1f;

        public const float BaseHpRecovery = 10f;
        public const float BaseMpRecovery = 10f;

        public const float FrictionValue = 0.95f;
        public const float MissMoveDistance = 0.2f;
        public const float MissMovePower = 0.7f;

        public const int IdleRewardMinSecond = 600;
        // 5시간
        public const int IdleReawedMaxSecond = 18000;

        public static readonly Dictionary<Currency, int> DailyAttendReward = new Dictionary<Currency, int>()
        {
            { Currency.DungeonTicket1, 3 }, { Currency.DungeonTicket2, 3 }, 
        };

        public const int EventShopExtraOpenDays = 3;
    }
}
