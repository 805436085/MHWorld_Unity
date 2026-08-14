using System;
using System.Collections.Generic;

namespace MHIdle.Data
{
    public enum PlaystyleId
    {
        Balanced,      // 均衡续航
        RawAttack,     // 攻击强化
        ItemSustain,   // 道具续航
        ParaTrap,      // 麻痹陷阱
        GuardTank,     // 防守生存
        SleepSlam,     // 睡眠暴力一刀
        Poison,        // 毒异常压制
        CritOutput,    // 会心输出
        EvasionCrit    // 回避会心
    }

    [Serializable]
    public class PlaystyleDef
    {
        public PlaystyleId Id;
        public string Name;
        public string ShortName;
        public string Description;
        public int UnlockHunterRank = 1;
        public string[] ArmorPrefixes = Array.Empty<string>();
        public string RecommendedGear;
        public string RecommendedWeapons;
        public ItemId[] RecommendedItems = Array.Empty<ItemId>();

        public float AttackMul = 1f;
        public float DefenseMul = 1f;
        public float HpFlat;
        public float CritChance;
        public float IncomingDamageMul = 1f;
        public float StatusChance;
        public float TrapChanceBonus;
        public float HealOnKill;
        public float AttackIntervalMul = 1f;
        public float ChargeChanceBonus;
        public bool HasPoison;
        public bool HasSleep;
        public bool HasParalysis;
    }

    public static class PlaystyleDatabase
    {
        public static readonly IReadOnlyList<PlaystyleDef> All = Build();

        public static PlaystyleDef Get(PlaystyleId id)
        {
            foreach (var def in All)
            {
                if (def.Id == id) return def;
            }

            return All[0];
        }

        public static PlaystyleDef Get(string id)
        {
            if (string.IsNullOrEmpty(id) || !Enum.TryParse(id, out PlaystyleId parsed))
                return Get(PlaystyleId.Balanced);
            return Get(parsed);
        }

        public static bool MatchesArmor(PlaystyleDef def, string armorId)
        {
            if (def == null || string.IsNullOrEmpty(armorId) || def.ArmorPrefixes == null) return false;
            foreach (var prefix in def.ArmorPrefixes)
            {
                if (!string.IsNullOrEmpty(prefix) &&
                    armorId.StartsWith(prefix + "_", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        static List<PlaystyleDef> Build()
        {
            return new List<PlaystyleDef>
            {
                new PlaystyleDef
                {
                    Id = PlaystyleId.Balanced,
                    Name = "均衡续航",
                    ShortName = "均衡",
                    Description = "稳妥开荒：体力更好，适合挂机。推荐皮革套 + 回复药。",
                    UnlockHunterRank = 1,
                    ArmorPrefixes = new[] { "leather" },
                    RecommendedGear = "皮革套",
                    RecommendedWeapons = "任意",
                    RecommendedItems = new[] { ItemId.Potion, ItemId.Paintball },
                    HpFlat = 18f,
                    HealOnKill = 3f
                },
                new PlaystyleDef
                {
                    Id = PlaystyleId.RawAttack,
                    Name = "攻击强化",
                    ShortName = "攻击",
                    Description = "直球输出。推荐骨制 / 角龙 / 轰龙套。",
                    UnlockHunterRank = 2,
                    ArmorPrefixes = new[] { "bone", "diablos", "tigrex" },
                    RecommendedGear = "骨制 → 角龙 / 轰龙",
                    RecommendedWeapons = "大剑、大锤",
                    RecommendedItems = new[] { ItemId.MightSeed, ItemId.Demondrug, ItemId.Potion },
                    AttackMul = 1.06f
                },
                new PlaystyleDef
                {
                    Id = PlaystyleId.ItemSustain,
                    Name = "道具续航",
                    ShortName = "续航",
                    Description = "靠药和粉撑过长战。推荐怪鸟 / 桃毛兽套。",
                    UnlockHunterRank = 3,
                    ArmorPrefixes = new[] { "kutku", "congalala", "chameleos" },
                    RecommendedGear = "怪鸟 → 桃毛兽 / 霞龙",
                    RecommendedWeapons = "单手剑",
                    RecommendedItems = new[]
                    {
                        ItemId.Potion, ItemId.MegaPotion, ItemId.Lifepowder, ItemId.HerbalMedicine
                    },
                    HealOnKill = 8f,
                    HpFlat = 10f
                },
                new PlaystyleDef
                {
                    Id = PlaystyleId.ParaTrap,
                    Name = "麻痹陷阱",
                    ShortName = "陷阱",
                    Description = "麻痹减速 + 陷阱开窗接炸弹。推荐毒怪鸟套。",
                    UnlockHunterRank = 4,
                    ArmorPrefixes = new[] { "gypceros", "khezu" },
                    RecommendedGear = "毒怪鸟 → 电龙",
                    RecommendedWeapons = "太刀、双剑",
                    RecommendedItems = new[]
                    {
                        ItemId.ShockTrap, ItemId.PitfallTrap, ItemId.BarrelBomb, ItemId.FlashBomb
                    },
                    HasParalysis = true,
                    TrapChanceBonus = 0.10f,
                    AttackIntervalMul = 0.97f,
                    StatusChance = 0.06f
                },
                new PlaystyleDef
                {
                    Id = PlaystyleId.GuardTank,
                    Name = "防守生存",
                    ShortName = "防守",
                    Description = "减伤硬扛，适合危险大怪。推荐盾蟹 / 岩龙 / 铠龙套。",
                    UnlockHunterRank = 5,
                    ArmorPrefixes = new[] { "hermitaur", "basarios", "gravios", "kushala" },
                    RecommendedGear = "盾蟹 → 岩龙 / 铠龙",
                    RecommendedWeapons = "长枪、大剑",
                    RecommendedItems = new[] { ItemId.Armorskin, ItemId.AdamantSeed, ItemId.MegaPotion },
                    DefenseMul = 1.08f,
                    IncomingDamageMul = 0.94f,
                    HpFlat = 12f
                },
                new PlaystyleDef
                {
                    Id = PlaystyleId.SleepSlam,
                    Name = "睡眠暴力一刀",
                    ShortName = "睡眠",
                    Description = "睡死窗口打蓄力 / 丢大桶。推荐眠鸟套 + 大剑。",
                    UnlockHunterRank = 5,
                    ArmorPrefixes = new[] { "hypnoc" },
                    RecommendedGear = "眠鸟套",
                    RecommendedWeapons = "大剑",
                    RecommendedItems = new[]
                    {
                        ItemId.BarrelBomb, ItemId.MegaBarrelBomb, ItemId.ShockTrap, ItemId.Demondrug
                    },
                    HasSleep = true,
                    ChargeChanceBonus = 0.07f,
                    AttackMul = 1.04f,
                    StatusChance = 0.08f
                },
                new PlaystyleDef
                {
                    Id = PlaystyleId.Poison,
                    Name = "毒异常压制",
                    ShortName = "毒",
                    Description = "攻击附带毒伤，磨血稳定。推荐雌火龙套。",
                    UnlockHunterRank = 6,
                    ArmorPrefixes = new[] { "rathian" },
                    RecommendedGear = "雌火龙套",
                    RecommendedWeapons = "太刀、双剑",
                    RecommendedItems = new[] { ItemId.Demondrug, ItemId.Potion, ItemId.TranqBomb },
                    HasPoison = true,
                    StatusChance = 0.08f,
                    AttackMul = 1.03f
                },
                new PlaystyleDef
                {
                    Id = PlaystyleId.CritOutput,
                    Name = "会心输出",
                    ShortName = "会心",
                    Description = "攻击 + 达人，追求会心。推荐火龙 / 炎王 / 金狮子套。",
                    UnlockHunterRank = 8,
                    ArmorPrefixes = new[] { "rathalos", "teostra", "rajang" },
                    RecommendedGear = "火龙 → 炎王 / 金狮子",
                    RecommendedWeapons = "太刀、双剑、弓",
                    RecommendedItems = new[] { ItemId.MegaDemondrug, ItemId.Demondrug, ItemId.MightSeed },
                    AttackMul = 1.05f,
                    CritChance = 0.06f
                },
                new PlaystyleDef
                {
                    Id = PlaystyleId.EvasionCrit,
                    Name = "回避会心",
                    ShortName = "回避",
                    Description = "少挨打、会心补伤。推荐迅龙 / 镰蟹 / 麒麟套。",
                    UnlockHunterRank = 8,
                    ArmorPrefixes = new[] { "narga", "ceanataur", "kirin" },
                    RecommendedGear = "镰蟹 → 迅龙 / 麒麟",
                    RecommendedWeapons = "太刀、双剑、单手剑",
                    RecommendedItems = new[] { ItemId.DashJuice, ItemId.FlashBomb, ItemId.MightSeed },
                    IncomingDamageMul = 0.94f,
                    CritChance = 0.05f,
                    AttackIntervalMul = 0.97f
                }
            };
        }
    }
}
