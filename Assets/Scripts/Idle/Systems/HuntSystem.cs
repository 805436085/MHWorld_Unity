using System;
using System.Collections.Generic;
using MHIdle.Data;
using MHIdle.Model;
using UnityEngine;

namespace MHIdle.Systems
{
    /// <summary>
    /// 主动出击：胜率预估 + 死亡惩罚。
    /// </summary>
    public static class HuntSystem
    {
        public static float EstimateWinRate(HunterProgress progress, MonsterDef monster)
        {
            if (monster == null) return 0f;
            float atk = progress.GetPlayerAttack();
            float def = progress.GetTotalDefense();
            float hp = progress.GetPlayerMaxHp();

            float playerDps = atk / Mathf.Max(0.55f, progress.GetAttackInterval());
            float monsterDps = Mathf.Max(1f, monster.Attack - def * 0.35f) / 2.2f;

            float timeToKill = monster.MaxHp / Mathf.Max(1f, playerDps);
            float timeToDie = hp / Mathf.Max(0.5f, monsterDps);

            float raw = timeToDie / Mathf.Max(0.5f, timeToKill + timeToDie);

            // 地图优势 / 陷阱微调
            var map = progress.GetMapProgress(monster.MapId);
            if (map.AdvantageUnlocked) raw += 0.06f;
            if (map.TrapUnlocked) raw += 0.04f;

            // 大怪且外圈瓶颈未破：略微降低胜率提示，鼓励先养
            var weapon = progress.GetEquippedWeaponProgress();
            if (monster.Size == MonsterSize.Large &&
                weapon.Outer.Level % 5 == 0 &&
                !weapon.BottleneckBroken)
            {
                raw -= 0.05f;
            }

            return Mathf.Clamp01(raw);
        }

        public static string FormatWinRate(float rate)
        {
            if (rate >= 0.75f) return $"胜率约 {rate * 100f:0}% · 有利";
            if (rate >= 0.55f) return $"胜率约 {rate * 100f:0}% · 五五开";
            if (rate >= 0.35f) return $"胜率约 {rate * 100f:0}% · 危险";
            return $"胜率约 {rate * 100f:0}% · 极危";
        }

        /// <summary>主动狩猎失败惩罚：丢部分金币与随机素材。</summary>
        public static string ApplyDeathPenalty(HunterProgress progress, System.Random rng)
        {
            progress.HuntDeaths++;
            int lostZenny = Mathf.Min(progress.Zenny, 40 + progress.HunterRank * 8);
            progress.Zenny -= lostZenny;

            string lostMat = string.Empty;
            var mats = new List<MaterialId>((MaterialId[])Enum.GetValues(typeof(MaterialId)));
            mats.Sort((a, b) => progress.GetMaterial(b).CompareTo(progress.GetMaterial(a)));
            foreach (var mat in mats)
            {
                int have = progress.GetMaterial(mat);
                if (have <= 0) continue;
                int lose = Mathf.Clamp(have / 5 + 1, 1, 5);
                progress.SpendMaterial(mat, lose);
                lostMat = $"{IdleCombatSystem.ToMaterialName(mat)} x{lose}";
                break;
            }

            // 低概率卸下一件防具（不销毁，回到仓库未装备状态——这里简化为卸下）
            string gearNote = string.Empty;
            if (rng.NextDouble() < 0.12f)
            {
                foreach (ArmorSlot slot in Enum.GetValues(typeof(ArmorSlot)))
                {
                    string id = progress.GetEquippedArmorId(slot);
                    if (string.IsNullOrEmpty(id) || id.StartsWith("leather_")) continue;
                    progress.EquippedArmor[slot.ToString()] = $"leather_{slot.ToString().ToLowerInvariant()}";
                    gearNote = $"，装备受损卸下 {id}";
                    break;
                }
            }

            return $"讨伐失败惩罚：-{lostZenny}z" +
                   (string.IsNullOrEmpty(lostMat) ? string.Empty : $"，丢失 {lostMat}") +
                   gearNote;
        }
    }
}
