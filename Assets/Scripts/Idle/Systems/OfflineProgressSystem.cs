using System;
using MHIdle.Data;
using MHIdle.Model;
using UnityEngine;

namespace MHIdle.Systems
{
    public static class OfflineProgressSystem
    {
        public static string Apply(HunterProgress progress)
        {
            if (progress.LastSaveUnix <= 0) return string.Empty;

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long elapsed = now - progress.LastSaveUnix;
            if (elapsed < 30) return string.Empty;

            int seconds = (int)Math.Min(elapsed, CombatBalance.OfflineMaxSeconds);

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
            float dps = attack / Math.Max(CombatBalance.MinPlayerAttackInterval, interval);
            float timePerKill = Math.Max(
                CombatBalance.OfflineMinSecondsPerKill,
                monster.MaxHp / Math.Max(1f, dps) + CombatBalance.IdlePackDelay);

            int kills = (int)(seconds / timePerKill * CombatBalance.OfflineEfficiency);
            if (kills <= 0) return string.Empty;

            kills = Math.Min(kills, CombatBalance.OfflineMaxKills);
            int zenny = monster.ZennyReward * kills;
            int hrExp = Math.Max(1, monster.HunterRankExp * kills / 2);
            int profExp = Math.Max(1, monster.WeaponProficiencyExp * kills / 2);

            progress.Zenny += zenny;
            progress.TotalKills += kills;
            progress.AddHunterRankExp(hrExp);

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

            progress.AddMaterial(MaterialId.MonsterBone, Math.Max(1, kills / 5));
            progress.AddMaterial(MaterialId.MonsterHide, Math.Max(1, kills / 6));
            if (kills >= 12) progress.AddMaterial(MaterialId.MonsterScale, Math.Max(1, kills / 10));

            int hours = seconds / 3600;
            int minutes = (seconds % 3600) / 60;
            return $"离线 {hours}小时{minutes}分：挂机小怪 {kills} 次，获得 {zenny}z";
        }
    }
}
