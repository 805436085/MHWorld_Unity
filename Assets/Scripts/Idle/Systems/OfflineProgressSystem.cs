using System;
using MHIdle.Data;
using MHIdle.Model;

namespace MHIdle.Systems
{
    public static class OfflineProgressSystem
    {
        const int MaxOfflineSeconds = 8 * 60 * 60; // 最多结算 8 小时
        const float Efficiency = 0.55f; // 离线效率低于在线

        public static string Apply(HunterProgress progress)
        {
            if (progress.LastSaveUnix <= 0) return string.Empty;

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long elapsed = now - progress.LastSaveUnix;
            if (elapsed < 30) return string.Empty;

            int seconds = (int)Math.Min(elapsed, MaxOfflineSeconds);
            var monster = GameDatabase.GetMonsterByIndex(progress.CurrentMonsterIndex);
            if (monster == null) return string.Empty;

            float attack = progress.GetPlayerAttack();
            float interval = progress.GetAttackInterval();
            float dps = attack / Math.Max(0.55f, interval);
            float timePerKill = Math.Max(3f, monster.MaxHp / Math.Max(1f, dps));

            int kills = (int)(seconds / timePerKill * Efficiency);
            if (kills <= 0) return string.Empty;

            // 离线奖励略收敛，避免一夜暴富
            kills = Math.Min(kills, 120);
            int zenny = monster.ZennyReward * kills;
            int hrExp = Math.Max(1, monster.HunterRankExp * kills / 2);
            int profExp = Math.Max(1, monster.WeaponProficiencyExp * kills / 2);

            progress.Zenny += zenny;
            progress.TotalKills += kills;
            progress.AddHunterRankExp(hrExp);
            progress.GetEquippedWeaponProgress().AddExp(profExp);

            // 给一点保底素材
            progress.AddMaterial(MaterialId.MonsterBone, Math.Max(1, kills / 4));
            progress.AddMaterial(MaterialId.MonsterHide, Math.Max(1, kills / 5));

            int hours = seconds / 3600;
            int minutes = (seconds % 3600) / 60;
            return $"离线 {hours}小时{minutes}分：模拟讨伐 {kills} 次，获得 {zenny}z";
        }
    }
}
