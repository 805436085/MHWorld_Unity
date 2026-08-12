namespace MHIdle.Data
{
    /// <summary>2G 全 11 武器种。</summary>
    public enum WeaponType
    {
        GreatSword,      // 大剑
        LongSword,       // 太刀
        SwordAndShield,  // 单手剑
        DualBlades,      // 双剑
        Hammer,          // 大锤
        HuntingHorn,     // 狩猎笛
        Lance,           // 长枪
        Gunlance,        // 铳枪
        Bow,             // 弓
        LightBowgun,     // 轻弩
        HeavyBowgun      // 重弩
    }

    /// <summary>心法层：武器风格组（原内内圈）。</summary>
    public enum WeaponStyleGroup
    {
        Aggressive,   // 太刀 / 双剑 —— 进攻特化
        GuardCapable, // 大剑 / 单手剑 —— 可守可攻
        Polearm,      // 长枪 / 铳枪
        Ranged,       // 弓 / 轻弩 / 重弩
        Blunt         // 大锤 / 狩猎笛
    }

    public enum ArmorSlot
    {
        Head,
        Chest,
        Arms,
        Legs
    }

    public enum MaterialId
    {
        MonsterBone,
        MonsterHide,
        SharpClaw,
        WyvernGem,
        ElderDragonBlood
    }

    public enum MonsterSize
    {
        Small,
        Large
    }

    public enum MapId
    {
        ForestAndHills,   // 森丘
        Jungle,           // 密林
        Desert,           // 沙漠
        Swamp,            // 沼泽
        Volcano,          // 火山
        SnowyMountains    // 雪山
    }

    public enum TechniqueId
    {
        GsCharge2,
        GsCharge3,
        GsDrawSlash,
        LsSpiritBlade,
        LsFadeSlash,
        DbDemonMode,
        SnSGuardSlash
    }
}
