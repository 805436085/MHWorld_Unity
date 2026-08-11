using System;
using System.Collections.Generic;

namespace MHIdle.Data
{
    public static class WeaponTaxonomy
    {
        public static WeaponStyleGroup GetStyleGroup(WeaponType type)
        {
            switch (type)
            {
                case WeaponType.LongSword:
                case WeaponType.DualBlades:
                    return WeaponStyleGroup.Aggressive;
                case WeaponType.GreatSword:
                case WeaponType.SwordAndShield:
                    return WeaponStyleGroup.GuardCapable;
                case WeaponType.Lance:
                case WeaponType.Gunlance:
                    return WeaponStyleGroup.Polearm;
                case WeaponType.Bow:
                case WeaponType.LightBowgun:
                case WeaponType.HeavyBowgun:
                    return WeaponStyleGroup.Ranged;
                case WeaponType.Hammer:
                case WeaponType.HuntingHorn:
                    return WeaponStyleGroup.Blunt;
                default:
                    return WeaponStyleGroup.GuardCapable;
            }
        }

        public static string TypeName(WeaponType type)
        {
            switch (type)
            {
                case WeaponType.GreatSword: return "大剑";
                case WeaponType.LongSword: return "太刀";
                case WeaponType.SwordAndShield: return "单手剑";
                case WeaponType.DualBlades: return "双剑";
                case WeaponType.Hammer: return "大锤";
                case WeaponType.HuntingHorn: return "狩猎笛";
                case WeaponType.Lance: return "长枪";
                case WeaponType.Gunlance: return "铳枪";
                case WeaponType.Bow: return "弓";
                case WeaponType.LightBowgun: return "轻弩";
                case WeaponType.HeavyBowgun: return "重弩";
                default: return type.ToString();
            }
        }

        public static string StyleName(WeaponStyleGroup group)
        {
            switch (group)
            {
                case WeaponStyleGroup.Aggressive: return "进攻特化";
                case WeaponStyleGroup.GuardCapable: return "可守可攻";
                case WeaponStyleGroup.Polearm: return "枪系";
                case WeaponStyleGroup.Ranged: return "射击系";
                case WeaponStyleGroup.Blunt: return "打击系";
                default: return group.ToString();
            }
        }

        public static string MapName(MapId id)
        {
            switch (id)
            {
                case MapId.ForestAndHills: return "森丘";
                case MapId.Jungle: return "密林";
                case MapId.Desert: return "沙漠";
                case MapId.Swamp: return "沼泽";
                case MapId.Volcano: return "火山";
                case MapId.SnowyMountains: return "雪山";
                default: return id.ToString();
            }
        }
    }

    [Serializable]
    public class TechniqueDef
    {
        public TechniqueId Id;
        public string Name;
        public string Description;
        public WeaponType WeaponType;
        public int RequiredOuterLevel;
        public int RequiredTypeLevel;
        public float DamageBonus;
        public float ChargeBonus;
    }

    public static class TechniqueDatabase
    {
        public static readonly IReadOnlyList<TechniqueDef> All = new List<TechniqueDef>
        {
            new TechniqueDef
            {
                Id = TechniqueId.GsCharge2,
                Name = "二段蓄力",
                Description = "蓄力砍伤害提升，外圈 Lv.3 解锁。",
                WeaponType = WeaponType.GreatSword,
                RequiredOuterLevel = 3,
                RequiredTypeLevel = 1,
                DamageBonus = 0.08f,
                ChargeBonus = 0.05f
            },
            new TechniqueDef
            {
                Id = TechniqueId.GsCharge3,
                Name = "三段蓄力",
                Description = "真蓄力一击。外圈 Lv.8 + 大剑系 Lv.4。",
                WeaponType = WeaponType.GreatSword,
                RequiredOuterLevel = 8,
                RequiredTypeLevel = 4,
                DamageBonus = 0.18f,
                ChargeBonus = 0.12f
            },
            new TechniqueDef
            {
                Id = TechniqueId.GsDrawSlash,
                Name = "拔刀斩",
                Description = "出鞘第一击暴击率上升。大怪突破后易解锁。",
                WeaponType = WeaponType.GreatSword,
                RequiredOuterLevel = 5,
                RequiredTypeLevel = 2,
                DamageBonus = 0.1f,
                ChargeBonus = 0f
            },
            new TechniqueDef
            {
                Id = TechniqueId.LsSpiritBlade,
                Name = "气刃斩",
                Description = "太刀气刃槽收益提升。",
                WeaponType = WeaponType.LongSword,
                RequiredOuterLevel = 4,
                RequiredTypeLevel = 2,
                DamageBonus = 0.1f
            },
            new TechniqueDef
            {
                Id = TechniqueId.LsFadeSlash,
                Name = "登龙剑预备",
                Description = "太刀收招位移与补刀窗口。",
                WeaponType = WeaponType.LongSword,
                RequiredOuterLevel = 7,
                RequiredTypeLevel = 3,
                DamageBonus = 0.12f
            },
            new TechniqueDef
            {
                Id = TechniqueId.DbDemonMode,
                Name = "鬼人化",
                Description = "双剑鬼人状态持续时间延长。",
                WeaponType = WeaponType.DualBlades,
                RequiredOuterLevel = 5,
                RequiredTypeLevel = 2,
                DamageBonus = 0.1f
            },
            new TechniqueDef
            {
                Id = TechniqueId.SnSGuardSlash,
                Name = "防御斩",
                Description = "单手剑守中反击。",
                WeaponType = WeaponType.SwordAndShield,
                RequiredOuterLevel = 4,
                RequiredTypeLevel = 2,
                DamageBonus = 0.06f
            }
        };
    }
}
