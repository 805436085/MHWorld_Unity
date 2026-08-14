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

        public bool AddExp(int amount)
        {
            if (amount <= 0) return false;
            Exp += amount;
            bool leveled = false;
            while (Exp >= ExpToNext)
            {
                Exp -= ExpToNext;
                Level++;
                leveled = true;
            }

            return leveled;
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

            // 小怪：若专精卡在瓶颈点（每 5 级）且未破，则专精收益大幅衰减
            bool atBottleneck = weaponProgress.Outer.Level > 0 &&
                               weaponProgress.Outer.Level % 5 == 0 &&
                               !weaponProgress.BottleneckBroken;

            if (size == MonsterSize.Small && atBottleneck)
            {
                outerGain = Mathf.Max(1, outerGain / 5);
                notes.Add(ProficiencyNaming.BottleneckIdleNote);
            }

            if (size == MonsterSize.Large && atBottleneck)
            {
                weaponProgress.BottleneckBroken = true;
                outerGain = Mathf.RoundToInt(outerGain * 1.8f);
                notes.Add(ProficiencyNaming.BottleneckBrokenNote);
            }

            // 过了瓶颈后，升到下一级重置瓶颈标记，等待下一个 5 级门槛
            int oldOuter = weaponProgress.Outer.Level;
            if (weaponProgress.Outer.AddExp(outerGain) && weaponProgress.Outer.Level / 5 > oldOuter / 5)
            {
                weaponProgress.BottleneckBroken = false;
            }

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
