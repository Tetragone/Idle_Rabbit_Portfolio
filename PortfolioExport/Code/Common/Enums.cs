namespace Game.Common
{
    public enum TableType
    {
        Currency, Dungeon, Enemy, Equip, EquipGacha, Gachas, Part, Shop
            , ShopCategory, Skill, Soul, SoulSubOption, Sound, Stage, Stat, Upgrade
            , Event, EventCurrency, EventHunt, EventShop, Quest
    }

    public enum Currency
    {
        Gold, Gem, EquipUp, MoonBless, SkillUp, Exp, EquipTicket, SkillTicket, DungeonTicket1, 
        SoulUp,
    }

    public enum ProcessType
    {
        Ready, Start, Doing, Done, Failed
    }

    public enum BattleType
    {
        Challenge, Normal, ChapterBoss, GoldDungeon,
    }

    public enum UnitState
    {
        Born, None, Move, Attack, Skill, Dying, Die
    }

    // inspector에서 사용중이기 때문에 순서를 바꾸면 문제가 생긴다. 
    public enum PopupType
    {
        Common, Warning, EquipGradeUp, Message, SkillEquip, StageSelect, ShopConfirm, GetItem, Gacha
            , WipeOut, GachaDetail
    }

    // inspector에서 사용중이기 때문에 순서를 바꾸면 문제가 생긴다. 
    public enum MainUIType
    {
        Equip, Upgrade, Skill, Dungeon, EquipSub, SkillSub, UpgradeSub, Shop
            , SoulSub, Event, Quest
    }

    public enum BuffType
    {
        AtkBuff, HpBuff, AtkSpdBuff
    }

    public enum DebuffType
    {
        Weakness,
    }

    public enum ShopIconSize
    {
        Small, Middle, Big
    }

    public enum ShopLimitType
    {
        None, Daily, Week, Month
    }

    public enum GachaType
    {
        Equip, Skill
    }

    public enum ItemType
    {
        Equip, Currency, Skill, EventCurrency
    }

    public enum GachaGrade
    {
        Normal, Good, Great
    }

    public enum SoundType
    {
        BGM, SFX
    }

    public enum SFXType
    {
        None, PlayerAttack, PlayerHit, SkillFire, SkillAtkBuff, SkillImpact, Equip, Ok, No, UIOpen, UIClose, Upgrade
    }

    public enum BGMType
    {
        Intro, Normal1, Normal1Challenge, Normal1Boss, GoldDungeon
    }

    public enum LogSituation
    {
        Save, Load, AddCurrency, GetCurrency, UseCurreny, ReturnPool, GetPool, RegistPool, Skill
    }

    public enum AtlasType
    {
        Common, Skill, Equip, Upgrade, Enemys
    }
    
    public enum EventPeriod
    {
        Upcoming, Active, ExchangeOnly, Closed
    }

    public enum QuestAction
    {
        KillMonster, EquipUpgrade, StatUpgrade, EquipGradeUp, ClearDungeon
    }

    public enum QuestType
    {
        Daily, Weekly, None, Repeat
    }

    public enum ShopPurchaseResult
    {
        Success, EventClosed, NotEnoughCurrency, LimitReached, RewardLimitReached, Busy, InvalidProduct
    }
}
