using System.Collections.Generic;
using MHIdle.Data;
using MHIdle.Model;
using UnityEngine;

namespace MHIdle.Systems
{
    public class ActiveSkillInfo
    {
        public SkillId Skill;
        public int Points;
        public SkillTierDef Tier; // 当前激活的最高档，可能为 null
        public int NextThreshold; // 下一档所需点数，0 表示已满
    }

    public class SkillCombatEffects
    {
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
        public readonly List<SkillTierDef> ActiveTiers = new List<SkillTierDef>();
    }

    /// <summary>
    /// 汇总已装备防具技能点，并换算为战斗效果。
    /// </summary>
    public static class ArmorSkillSystem
    {
        public static Dictionary<SkillId, int> AggregatePoints(HunterProgress progress)
        {
            var points = new Dictionary<SkillId, int>();
            if (progress?.EquippedArmor == null) return points;

            foreach (var pair in progress.EquippedArmor)
            {
                var armor = GameDatabase.GetArmor(pair.Value);
                if (armor?.SkillPoints == null) continue;
                foreach (var grant in armor.SkillPoints)
                {
                    if (grant.Points == 0) continue;
                    if (!points.ContainsKey(grant.Skill)) points[grant.Skill] = 0;
                    points[grant.Skill] += grant.Points;
                }
            }

            return points;
        }

        public static List<ActiveSkillInfo> GetSkillBoard(HunterProgress progress)
        {
            var points = AggregatePoints(progress);
            var board = new List<ActiveSkillInfo>();

            foreach (SkillId skill in System.Enum.GetValues(typeof(SkillId)))
            {
                int p = points.TryGetValue(skill, out int v) ? v : 0;
                if (p <= 0) continue;

                SkillTierDef best = null;
                int next = 0;
                foreach (var tier in ArmorSkillDatabase.Tiers)
                {
                    if (tier.Skill != skill) continue;
                    if (p >= tier.PointsRequired)
                    {
                        if (best == null || tier.PointsRequired > best.PointsRequired)
                            best = tier;
                    }
                    else if (next == 0 || tier.PointsRequired < next)
                    {
                        next = tier.PointsRequired;
                    }
                }

                board.Add(new ActiveSkillInfo
                {
                    Skill = skill,
                    Points = p,
                    Tier = best,
                    NextThreshold = next
                });
            }

            board.Sort((a, b) => b.Points.CompareTo(a.Points));
            return board;
        }

        public static SkillCombatEffects Evaluate(HunterProgress progress)
        {
            var fx = new SkillCombatEffects();
            var board = GetSkillBoard(progress);

            foreach (var info in board)
            {
                if (info.Tier == null) continue;
                var t = info.Tier;
                fx.ActiveTiers.Add(t);
                fx.AttackMul *= t.AttackMul;
                fx.DefenseMul *= t.DefenseMul;
                fx.HpFlat += t.HpFlat;
                fx.CritChance += t.CritChance;
                fx.IncomingDamageMul *= t.IncomingDamageMul;
                fx.StatusChance += t.StatusChance;
                fx.TrapChanceBonus += t.TrapChanceBonus;
                fx.HealOnKill += t.HealOnKill;
                fx.AttackIntervalMul *= t.AttackIntervalMul;
                fx.ChargeChanceBonus += t.ChargeChanceBonus;

                if (info.Skill == SkillId.Poison) fx.HasPoison = true;
                if (info.Skill == SkillId.Sleep) fx.HasSleep = true;
                if (info.Skill == SkillId.Paralysis) fx.HasParalysis = true;
            }

            PlaystyleSystem.ApplyTo(fx, progress);

            fx.CritChance = Mathf.Clamp01(fx.CritChance);
            fx.StatusChance = Mathf.Clamp01(fx.StatusChance);
            fx.TrapChanceBonus = Mathf.Clamp01(fx.TrapChanceBonus);
            return fx;
        }

        public static string DescribeBuildFocus(HunterProgress progress)
        {
            var def = PlaystyleSystem.Current(progress);
            int pieces = PlaystyleSystem.EquippedSetPieces(progress, def);
            if (pieces >= 3) return $"流派：{def.Name}（套装成型 {pieces}/4）";
            if (pieces > 0) return $"流派：{def.Name}（防具 {pieces}/4）";
            return $"流派：{def.Name}（点选切换）";
        }

        public static string DescribeBuildFocus(SkillCombatEffects fx)
        {
            if (fx.ActiveTiers.Count == 0) return "未激活技能 · 凑齐防具点数可形成 Build";

            // 粗略流派标签
            if (fx.IncomingDamageMul <= 0.88f && fx.CritChance >= 0.08f) return "流派：回避会心";
            if (fx.HasSleep && fx.ChargeChanceBonus > 0f) return "流派：睡眠暴力一刀";
            if (fx.HasParalysis) return "流派：麻痹持续输出";
            if (fx.HasPoison && fx.StatusChance > 0.1f) return "流派：毒异常压制";
            if (fx.TrapChanceBonus >= 0.15f) return "流派：陷阱流";
            if (fx.HealOnKill >= 10f) return "流派：道具/续航流";
            if (fx.AttackMul >= 1.12f && fx.CritChance >= 0.08f) return "流派：会心输出";
            if (fx.AttackMul >= 1.1f) return "流派：攻击强化";
            if (fx.IncomingDamageMul <= 0.9f || fx.DefenseMul >= 1.1f) return "流派：防守生存";
            return "流派：混合配置";
        }
    }
}
