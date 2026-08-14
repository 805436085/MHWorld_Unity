using System;
using System.Collections.Generic;

namespace MHIdle.Data
{
    /// <summary>防具技能（2G 气质：点数累计，达标激活）。</summary>
    public enum SkillId
    {
        Attack,      // 攻击
        Defense,     // 防御
        Health,      // 体力
        Expert,      // 达人（会心）
        Guard,       // 防御性能
        StatusAtk,   // 状态异常攻击
        Poison,      // 毒属性强化
        Sleep,       // 睡眠属性强化
        Paralysis,   // 麻痹属性强化
        TrapMaster,  // 陷阱师
        ItemUse,     // 道具使用强化
        RecSpeed,    // 回复速度
        Evasion      // 回避（迅龙气质）
    }

    [Serializable]
    public class SkillPointGrant
    {
        public SkillId Skill;
        public int Points;

        public SkillPointGrant() { }

        public SkillPointGrant(SkillId skill, int points)
        {
            Skill = skill;
            Points = points;
        }
    }

    [Serializable]
    public class SkillTierDef
    {
        public SkillId Skill;
        public string SkillName;
        public int PointsRequired;
        public string ActiveName;
        public string Description;
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
    }

    public static class ArmorSkillDatabase
    {
        public static string SkillName(SkillId id)
        {
            switch (id)
            {
                case SkillId.Attack: return "攻击";
                case SkillId.Defense: return "防御";
                case SkillId.Health: return "体力";
                case SkillId.Expert: return "达人";
                case SkillId.Guard: return "防御性能";
                case SkillId.StatusAtk: return "状态异常攻击";
                case SkillId.Poison: return "毒";
                case SkillId.Sleep: return "睡眠";
                case SkillId.Paralysis: return "麻痹";
                case SkillId.TrapMaster: return "陷阱师";
                case SkillId.ItemUse: return "道具使用";
                case SkillId.RecSpeed: return "回复速度";
                case SkillId.Evasion: return "回避";
                default: return id.ToString();
            }
        }

        public static readonly IReadOnlyList<SkillTierDef> Tiers = new List<SkillTierDef>
        {
            // 攻击
            Tier(SkillId.Attack, 10, "攻击力UP【小】", "攻击 +8%", attackMul: 1.08f),
            Tier(SkillId.Attack, 15, "攻击力UP【大】", "攻击 +16%", attackMul: 1.16f),
            // 防御
            Tier(SkillId.Defense, 10, "防御力UP【小】", "防御 +12%", defenseMul: 1.12f),
            Tier(SkillId.Defense, 15, "防御力UP【大】", "防御 +22%", defenseMul: 1.22f),
            // 体力
            Tier(SkillId.Health, 10, "体力UP【小】", "最大HP +35", hpFlat: 35f),
            Tier(SkillId.Health, 15, "体力UP【大】", "最大HP +60", hpFlat: 60f),
            // 达人
            Tier(SkillId.Expert, 10, "见切【小】", "会心 +10%", crit: 0.10f),
            Tier(SkillId.Expert, 15, "见切【大】", "会心 +18%", crit: 0.18f),
            // 防御性能
            Tier(SkillId.Guard, 10, "防御性能+1", "受伤 -10%", incoming: 0.90f),
            Tier(SkillId.Guard, 15, "防御性能+2", "受伤 -18%", incoming: 0.82f),
            // 状态异常攻击（通用异常）
            Tier(SkillId.StatusAtk, 10, "状态异常攻击+1", "异常积累 +12%", status: 0.12f),
            Tier(SkillId.StatusAtk, 15, "状态异常攻击+2", "异常积累 +22%", status: 0.22f),
            // 毒
            Tier(SkillId.Poison, 10, "毒属性强化+1", "攻击附带毒伤", status: 0.08f, attackMul: 1.03f),
            Tier(SkillId.Poison, 15, "毒属性强化+2", "毒伤增强", status: 0.14f, attackMul: 1.06f),
            // 睡眠 — 睡眠暴力一刀
            Tier(SkillId.Sleep, 10, "睡眠属性强化+1", "睡眠窗口蓄力暴击", status: 0.10f, charge: 0.06f),
            Tier(SkillId.Sleep, 15, "睡眠属性强化+2", "睡奸一击强化", status: 0.16f, charge: 0.12f, attackMul: 1.08f),
            // 麻痹 — 麻痹持续输出
            Tier(SkillId.Paralysis, 10, "麻痹属性强化+1", "麻痹减速怪物", status: 0.10f, intervalMul: 0.96f),
            Tier(SkillId.Paralysis, 15, "麻痹属性强化+2", "麻痹延长，攻速微升", status: 0.16f, intervalMul: 0.90f),
            // 陷阱师
            Tier(SkillId.TrapMaster, 10, "陷阱师", "场地陷阱触发率大幅提升", trap: 0.18f),
            Tier(SkillId.TrapMaster, 15, "陷阱大师", "陷阱伤害提升", trap: 0.30f, attackMul: 1.04f),
            // 道具使用
            Tier(SkillId.ItemUse, 10, "道具使用强化", "击杀回复与掉落微增", heal: 8f),
            Tier(SkillId.ItemUse, 15, "高速收集", "击杀回复增强", heal: 16f, attackMul: 1.03f),
            // 回复速度
            Tier(SkillId.RecSpeed, 10, "回复速度+1", "战斗中缓慢回血", heal: 4f),
            Tier(SkillId.RecSpeed, 15, "回复速度+2", "回血加快", heal: 9f),
            // 回避
            Tier(SkillId.Evasion, 10, "回避性能+1", "受伤 -8%", incoming: 0.92f),
            Tier(SkillId.Evasion, 15, "回避性能+2", "受伤 -16%", incoming: 0.84f),
        };

        static SkillTierDef Tier(
            SkillId skill,
            int points,
            string activeName,
            string desc,
            float attackMul = 1f,
            float defenseMul = 1f,
            float hpFlat = 0f,
            float crit = 0f,
            float incoming = 1f,
            float status = 0f,
            float trap = 0f,
            float heal = 0f,
            float intervalMul = 1f,
            float charge = 0f)
        {
            return new SkillTierDef
            {
                Skill = skill,
                SkillName = SkillName(skill),
                PointsRequired = points,
                ActiveName = activeName,
                Description = desc,
                AttackMul = attackMul,
                DefenseMul = defenseMul,
                HpFlat = hpFlat,
                CritChance = crit,
                IncomingDamageMul = incoming,
                StatusChance = status,
                TrapChanceBonus = trap,
                HealOnKill = heal,
                AttackIntervalMul = intervalMul,
                ChargeChanceBonus = charge
            };
        }
    }
}
