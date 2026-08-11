using System;
using MHIdle.Data;
using MHIdle.Model;
using UnityEngine;

namespace MHIdle.Systems
{
    public static class OfflineProgressSystem
    {
        const int MaxOfflineSeconds = 8 * 60 * 60;
        const float Efficiency = 0.55f;

        public static string Apply(HunterProgress progress)
        {
            if (progress.LastSaveUnix <= 0) return string.Empty;

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long elapsed = now - progress.LastSaveUnix;
            if (elapsed < 30) return string.Empty;

            int seconds = (int)Math.Min(elapsed, MaxOfflineSeconds);

            // 离线只结算小怪挂机
            MonsterDef monster = null;
            for (int i = 0; i <= progress.HighestMonsterIndexUnlocked && i < GameDatabase.Monsters.Count; i++)
            {
                if (GameDatabase.Monsters[i].Size == MonsterSize.Small)
                    monster = GameDatabase.Monsters[i];
            }

            if (monster == null) monster = GameDatabase.GetMonsterByIndex(0);
            if (monster == null) return string.Empty;

            float attack = progress.GetPlayerAttack();
            float interval = progress.GetAttackInterval();
            float dps = attack / Math.Max(0.55f, interval);
            float timePerKill = Math.Max(3f, monster.MaxHp / Math.Max(1f, dps));

            int kills = (int)(seconds / timePerKill * Efficiency);
            if (kills <= 0) return string.Empty;

            kills = Math.Min(kills, 120);
            int zenny = monster.ZennyReward * kills;
            int hrExp = Math.Max(1, monster.HunterRankExp * kills / 2);
            int profExp = Math.Max(1, monster.WeaponProficiencyExp * kills / 2);

            progress.Zenny += zenny;
            progress.TotalKills += kills;
            progress.AddHunterRankExp(hrExp);

            // 分批灌熟练度，避免一次性跳过瓶颈逻辑异常
            int batches = Math.Max(1, kills / 5);
            int perBatch = Math.Max(1, profExp / batches);
            for (int i = 0; i < batches; i++)
            {
                ProficiencySystem.GrantCombatExp(
                    progress,
                    progress.GetEquippedWeapon(),
                    perBatch,
                    MonsterSize.Small,
                    monster.MapId);
            }

            progress.AddMaterial(MaterialId.MonsterBone, Math.Max(1, kills / 4));
            progress.AddMaterial(MaterialId.MonsterHide, Math.Max(1, kills / 5));

            int hours = seconds / 3600;
            int minutes = (seconds % 3600) / 60;
            return $"离线 {hours}小时{minutes}分：挂机小怪 {kills} 次，获得 {zenny}z";
        }
    }
}
