using System;
using System.Collections.Generic;
using MHIdle.Data;
using UnityEngine;

namespace MHIdle.Model
{
    [Serializable]
    public class RingProgress
    {
        public int Level = 1;
        public int Exp;

        public int ExpToNext => 42 + Level * 26;

        /// <summary>
        /// 增加经验。maxLevel &gt; 0 时最多升到该级（满环停住，用于专精瓶颈锁定）。
        /// </summary>
        public bool AddExp(int amount, int maxLevel = 0)
        {
            if (amount <= 0) return false;
            Exp += amount;
            bool leveled = false;
            while (Exp >= ExpToNext)
            {
                if (maxLevel > 0 && Level >= maxLevel)
                {
                    Exp = ExpToNext;
                    break;
                }

                Exp -= ExpToNext;
                Level++;
                leveled = true;

                if (maxLevel > 0 && Level >= maxLevel)
                {
                    if (Exp > ExpToNext) Exp = ExpToNext;
                    break;
                }
            }

            return leveled;
        }

        public void FillRing()
        {
            Exp = ExpToNext;
        }

        public float Fill01 => ExpToNext <= 0 ? 0f : Mathf.Clamp01((float)Exp / ExpToNext);
    }

    [Serializable]
    public class WeaponProgress
    {
        public string WeaponId;
        public bool Owned;
        public RingProgress Outer = new RingProgress(); // 专精：具体武器
        public bool BottleneckBroken; // 是否已被大怪突破过瓶颈

        // 兼容旧字段读取
        public int ProficiencyLevel
        {
            get => Outer.Level;
            set => Outer.Level = Mathf.Max(1, value);
        }

        public int ProficiencyExp
        {
            get => Outer.Exp;
            set => Outer.Exp = Mathf.Max(0, value);
        }

        public int ExpToNext => Outer.ExpToNext;

        public bool AddExp(int amount) => Outer.AddExp(amount);

        public float DamageMultiplier => 1f + (Outer.Level - 1) * 0.028f;
        public float SpeedMultiplier => 1f + (Outer.Level - 1) * 0.006f;

        public bool IsProficiencyLocked =>
            ProficiencySystem.IsBottleneckLevel(Outer.Level) && !BottleneckBroken;
    }

    [Serializable]
    public class MapProgress
    {
        public string MapId;
        public RingProgress Ring = new RingProgress();
        public bool TrapUnlocked;
        public bool AdvantageUnlocked;

        public void SyncUnlocks()
        {
            if (Ring.Level >= 3) TrapUnlocked = true;
            if (Ring.Level >= 6) AdvantageUnlocked = true;
        }
    }

    /// <summary>
    /// 三层熟练度结算：专精（武器）/ 武种 / 心法（风格组）。
    /// </summary>
    public static class ProficiencySystem
    {
        // 经验分配比例：专精为主，武种/心法较慢
        const float OuterRatio = 1f;
        const float TypeRatio = 0.35f;
        const float StyleRatio = 0.12f;

        public static bool IsBottleneckLevel(int level) => level > 0 && level % 5 == 0;

        /// <summary>专精最多升到这一级：未讨伐大怪时不能越过 5/10/15… 关口。</summary>
        public static int ProficiencyCapLevel(WeaponProgress weaponProgress)
        {
            int level = weaponProgress.Outer.Level;
            if (IsBottleneckLevel(level) && !weaponProgress.BottleneckBroken)
                return level;
            if (IsBottleneckLevel(level))
                return level + 5;
            return (level / 5 + 1) * 5;
        }

        public static List<string> GrantCombatExp(
            HunterProgress progress,
            WeaponDef weapon,
            int baseExp,
            MonsterSize size,
            MapId mapId)
        {
            var notes = new List<string>();
            if (weapon == null || baseExp <= 0) return notes;

            progress.EnsureDefaults();
            var weaponProgress = progress.Weapons[weapon.Id];

            int outerGain = Mathf.Max(1, Mathf.RoundToInt(baseExp * OuterRatio));
            int typeGain = Mathf.Max(1, Mathf.RoundToInt(baseExp * TypeRatio));
            int styleGain = Mathf.Max(1, Mathf.RoundToInt(baseExp * StyleRatio));

            bool locked = weaponProgress.IsProficiencyLocked;

            if (size == MonsterSize.Large && locked)
            {
                weaponProgress.BottleneckBroken = true;
                outerGain = Mathf.RoundToInt(outerGain * 1.8f);
                notes.Add(ProficiencyNaming.BottleneckBrokenNote);
            }
            else if (size != MonsterSize.Large && locked)
            {
                outerGain = 0;
                weaponProgress.Outer.FillRing();
                notes.Add(ProficiencyNaming.BottleneckIdleNote);
            }

            int oldOuter = weaponProgress.Outer.Level;
            int cap = ProficiencyCapLevel(weaponProgress);
            if (weaponProgress.Outer.AddExp(outerGain, cap) && weaponProgress.Outer.Level > oldOuter)
            {
                // 离开本段瓶颈后，下一关必须再打大怪
                if (IsBottleneckLevel(oldOuter) || IsBottleneckLevel(weaponProgress.Outer.Level))
                    weaponProgress.BottleneckBroken = false;
            }

            if (weaponProgress.IsProficiencyLocked)
                weaponProgress.Outer.FillRing();

            var typeKey = weapon.Type.ToString();
            var styleKey = WeaponTaxonomy.GetStyleGroup(weapon.Type).ToString();
            progress.TypeRings[typeKey].AddExp(typeGain);
            progress.StyleRings[styleKey].AddExp(styleGain);

            // 地图熟练度：大怪给更多
            int mapGain = size == MonsterSize.Large ? Mathf.Max(2, baseExp / 3) : Mathf.Max(1, baseExp / 6);
            var map = progress.GetMapProgress(mapId);
            map.Ring.AddExp(mapGain);
            map.SyncUnlocks();

            // 招式解锁检查
            foreach (var tech in TechniqueDatabase.All)
            {
                if (tech.WeaponType != weapon.Type) continue;
                if (progress.UnlockedTechniques.Contains(tech.Id.ToString())) continue;
                if (weaponProgress.Outer.Level >= tech.RequiredOuterLevel &&
                    progress.TypeRings[typeKey].Level >= tech.RequiredTypeLevel)
                {
                    progress.UnlockedTechniques.Add(tech.Id.ToString());
                    notes.Add($"学会招式：{tech.Name}");
                }
            }

            return notes;
        }

        public static float GetTechniqueDamageBonus(HunterProgress progress, WeaponType type)
        {
            float bonus = 0f;
            foreach (var tech in TechniqueDatabase.All)
            {
                if (tech.WeaponType != type) continue;
                if (!progress.UnlockedTechniques.Contains(tech.Id.ToString())) continue;
                bonus += tech.DamageBonus;
            }

            return bonus;
        }

        public static float GetTechniqueChargeBonus(HunterProgress progress, WeaponType type)
        {
            float bonus = 0f;
            foreach (var tech in TechniqueDatabase.All)
            {
                if (tech.WeaponType != type) continue;
                if (!progress.UnlockedTechniques.Contains(tech.Id.ToString())) continue;
                bonus += tech.ChargeBonus;
            }

            return bonus;
        }
    }
}
