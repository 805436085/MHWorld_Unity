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
                case MapId.GreatForest: return "树海";
                case MapId.Gorge: return "溪谷";
                case MapId.Tower: return "塔";
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
            T(TechniqueId.GsCharge2, "二段蓄力", $"蓄力砍伤害提升，{ProficiencyNaming.Weapon} Lv.3 解锁。",
                WeaponType.GreatSword, 3, 1, 0.08f, 0.05f),
            T(TechniqueId.GsCharge3, "三段蓄力", $"真蓄力一击。{ProficiencyNaming.Weapon} Lv.8 + 大剑系 Lv.4。",
                WeaponType.GreatSword, 8, 4, 0.18f, 0.12f),
            T(TechniqueId.GsDrawSlash, "拔刀斩", "出鞘第一击暴击率上升。大怪突破后易解锁。",
                WeaponType.GreatSword, 5, 2, 0.1f, 0f),
            T(TechniqueId.LsSpiritBlade, "气刃斩", "太刀气刃槽收益提升。",
                WeaponType.LongSword, 4, 2, 0.1f, 0f),
            T(TechniqueId.LsFadeSlash, "登龙剑预备", "太刀收招位移与补刀窗口。",
                WeaponType.LongSword, 7, 3, 0.12f, 0f),
            T(TechniqueId.DbDemonMode, "鬼人化", "双剑鬼人状态持续时间延长。",
                WeaponType.DualBlades, 5, 2, 0.1f, 0f),
            T(TechniqueId.DbDemonDance, "乱舞", "鬼人乱舞收招，对定身目标额外伤害。",
                WeaponType.DualBlades, 8, 4, 0.14f, 0f),
            T(TechniqueId.SnSGuardSlash, "防御斩", "单手剑守中反击。",
                WeaponType.SwordAndShield, 4, 2, 0.06f, 0f),
            T(TechniqueId.SnSRoundSlash, "回旋斩", "道具后立刻接斩，提升一轮输出。",
                WeaponType.SwordAndShield, 7, 3, 0.1f, 0f),
            T(TechniqueId.HmCharge, "蓄力回转", "大锤蓄力等级提升。",
                WeaponType.Hammer, 4, 2, 0.1f, 0.08f),
            T(TechniqueId.HmUpswing, "上捞敲", "击昏窗口扩大。",
                WeaponType.Hammer, 8, 4, 0.12f, 0f),
            T(TechniqueId.HhRecital, "演奏", "狩猎笛旋律生效，全队攻击微增。",
                WeaponType.HuntingHorn, 4, 2, 0.08f, 0f),
            T(TechniqueId.HhEncore, "重奏", "旋律二次强化。",
                WeaponType.HuntingHorn, 8, 3, 0.12f, 0f),
            T(TechniqueId.LnCounter, "防御反击", "长枪反击刺。",
                WeaponType.Lance, 4, 2, 0.08f, 0f),
            T(TechniqueId.LnCharge, "突进", "长枪突进刺命中强化。",
                WeaponType.Lance, 7, 3, 0.1f, 0f),
            T(TechniqueId.GlBurst, "全弹发射", "铳枪炮击爆发。",
                WeaponType.Gunlance, 5, 2, 0.1f, 0f),
            T(TechniqueId.GlWyrmstake, "龙击炮", "龙击炮贯穿伤害。",
                WeaponType.Gunlance, 8, 4, 0.14f, 0f),
            T(TechniqueId.BowPowerShot, "刚射", "弓刚射追加一箭。",
                WeaponType.Bow, 4, 2, 0.1f, 0f),
            T(TechniqueId.BowDragonPiercer, "龙之箭", "贯穿蓄力箭。",
                WeaponType.Bow, 8, 4, 0.14f, 0f),
            T(TechniqueId.LbgRapidFire, "速射", "轻弩对应弹药速射。",
                WeaponType.LightBowgun, 5, 2, 0.1f, 0f),
            T(TechniqueId.HbgSiege, "固定射击", "重弩架枪提高火力。",
                WeaponType.HeavyBowgun, 5, 2, 0.12f, 0f)
        };

        static TechniqueDef T(
            TechniqueId id,
            string name,
            string desc,
            WeaponType type,
            int outer,
            int typeLv,
            float dmg,
            float charge)
        {
            return new TechniqueDef
            {
                Id = id,
                Name = name,
                Description = desc,
                WeaponType = type,
                RequiredOuterLevel = outer,
                RequiredTypeLevel = typeLv,
                DamageBonus = dmg,
                ChargeBonus = charge
            };
        }
    }
}
