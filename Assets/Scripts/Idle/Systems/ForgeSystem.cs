using MHIdle.Data;
using MHIdle.Model;

namespace MHIdle.Systems
{
    public enum ForgeResult
    {
        Success,
        AlreadyOwned,
        LockedByRank,
        NotEnoughResources,
        NotOwned,
        Invalid
    }

    public static class ForgeSystem
    {
        public static ForgeResult CraftWeapon(HunterProgress progress, string weaponId)
        {
            var def = GameDatabase.GetWeapon(weaponId);
            if (def == null) return ForgeResult.Invalid;

            progress.EnsureDefaults();
            var weaponProgress = progress.Weapons[weaponId];
            if (weaponProgress.Owned) return ForgeResult.AlreadyOwned;
            if (progress.HunterRank < def.UnlockHunterRank) return ForgeResult.LockedByRank;
            if (!progress.Spend(def.CraftZenny, def.CraftCost)) return ForgeResult.NotEnoughResources;

            weaponProgress.Owned = true;
            progress.EquippedWeaponId = weaponId;
            return ForgeResult.Success;
        }

        public static ForgeResult EquipWeapon(HunterProgress progress, string weaponId)
        {
            var def = GameDatabase.GetWeapon(weaponId);
            if (def == null) return ForgeResult.Invalid;

            progress.EnsureDefaults();
            if (!progress.Weapons[weaponId].Owned) return ForgeResult.NotOwned;

            progress.EquippedWeaponId = weaponId;
            return ForgeResult.Success;
        }

        public static ForgeResult CraftArmor(HunterProgress progress, string armorId)
        {
            var def = GameDatabase.GetArmor(armorId);
            if (def == null) return ForgeResult.Invalid;

            progress.EnsureDefaults();
            if (progress.OwnsArmor(armorId)) return ForgeResult.AlreadyOwned;
            if (progress.HunterRank < def.UnlockHunterRank) return ForgeResult.LockedByRank;
            if (!progress.Spend(def.CraftZenny, def.CraftCost)) return ForgeResult.NotEnoughResources;

            progress.OwnedArmor.Add(armorId);
            progress.EquippedArmor[def.Slot.ToString()] = armorId;
            return ForgeResult.Success;
        }

        public static ForgeResult EquipArmor(HunterProgress progress, string armorId)
        {
            var def = GameDatabase.GetArmor(armorId);
            if (def == null) return ForgeResult.Invalid;
            if (!progress.OwnsArmor(armorId)) return ForgeResult.NotOwned;

            progress.EquippedArmor[def.Slot.ToString()] = armorId;
            return ForgeResult.Success;
        }

        public static string Describe(ForgeResult result)
        {
            switch (result)
            {
                case ForgeResult.Success: return "锻造/装备成功";
                case ForgeResult.AlreadyOwned: return "已拥有";
                case ForgeResult.LockedByRank: return "猎人等级不足";
                case ForgeResult.NotEnoughResources: return "材料或金币不足";
                case ForgeResult.NotOwned: return "尚未拥有";
                default: return "无效操作";
            }
        }
    }
}
