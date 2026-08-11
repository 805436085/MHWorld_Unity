using System;
using System.Collections.Generic;
using MHIdle.Data;
using UnityEngine;

namespace MHIdle.Model
{
    [Serializable]
    public class WeaponProgress
    {
        public string WeaponId;
        public int ProficiencyLevel = 1;
        public int ProficiencyExp;
        public bool Owned;

        public int ExpToNext => 20 + ProficiencyLevel * 15;

        public bool AddExp(int amount)
        {
            if (amount <= 0) return false;

            ProficiencyExp += amount;
            bool leveled = false;
            while (ProficiencyExp >= ExpToNext)
            {
                ProficiencyExp -= ExpToNext;
                ProficiencyLevel++;
                leveled = true;
            }

            return leveled;
        }

        public float DamageMultiplier => 1f + (ProficiencyLevel - 1) * 0.04f;
        public float SpeedMultiplier => 1f + (ProficiencyLevel - 1) * 0.015f;
    }

    [Serializable]
    public class HunterProgress
    {
        public int HunterRank = 1;
        public int HunterRankExp;
        public int Zenny = 120;
        public string EquippedWeaponId = "gs_buster";
        public int CurrentMonsterIndex;
        public int HighestMonsterIndexUnlocked;
        public int TotalKills;
        public long LastSaveUnix;
        public Dictionary<string, int> Materials = new Dictionary<string, int>();
        public Dictionary<string, WeaponProgress> Weapons = new Dictionary<string, WeaponProgress>();
        public Dictionary<string, string> EquippedArmor = new Dictionary<string, string>();
        public HashSet<string> OwnedArmor = new HashSet<string>();

        public int ExpToNextRank => 40 + HunterRank * 25;

        public static HunterProgress CreateNew()
        {
            var progress = new HunterProgress();
            progress.EnsureDefaults();
            return progress;
        }

        public void EnsureDefaults()
        {
            if (Materials == null) Materials = new Dictionary<string, int>();
            if (Weapons == null) Weapons = new Dictionary<string, WeaponProgress>();
            if (EquippedArmor == null) EquippedArmor = new Dictionary<string, string>();
            if (OwnedArmor == null) OwnedArmor = new HashSet<string>();

            foreach (var weapon in GameDatabase.Weapons)
            {
                if (!Weapons.ContainsKey(weapon.Id))
                {
                    Weapons[weapon.Id] = new WeaponProgress
                    {
                        WeaponId = weapon.Id,
                        Owned = weapon.Id == "gs_buster",
                        ProficiencyLevel = 1,
                        ProficiencyExp = 0
                    };
                }
            }

            if (string.IsNullOrEmpty(EquippedWeaponId) || GetWeaponDef(EquippedWeaponId) == null)
            {
                EquippedWeaponId = "gs_buster";
            }

            Weapons[EquippedWeaponId].Owned = true;

            // 默认皮革套
            foreach (ArmorSlot slot in Enum.GetValues(typeof(ArmorSlot)))
            {
                string leatherId = $"leather_{slot.ToString().ToLowerInvariant()}";
                OwnedArmor.Add(leatherId);
                if (!EquippedArmor.ContainsKey(slot.ToString()) || string.IsNullOrEmpty(EquippedArmor[slot.ToString()]))
                {
                    EquippedArmor[slot.ToString()] = leatherId;
                }
            }
        }

        public WeaponDef GetWeaponDef(string id) => GameDatabase.GetWeapon(id);

        public WeaponDef GetEquippedWeapon() => GameDatabase.GetWeapon(EquippedWeaponId);

        public WeaponProgress GetEquippedWeaponProgress()
        {
            EnsureDefaults();
            return Weapons[EquippedWeaponId];
        }

        public int GetMaterial(MaterialId id)
        {
            string key = id.ToString();
            return Materials.TryGetValue(key, out int value) ? value : 0;
        }

        public void AddMaterial(MaterialId id, int amount)
        {
            if (amount <= 0) return;
            string key = id.ToString();
            Materials[key] = GetMaterial(id) + amount;
        }

        public bool SpendMaterial(MaterialId id, int amount)
        {
            if (GetMaterial(id) < amount) return false;
            string key = id.ToString();
            Materials[key] = GetMaterial(id) - amount;
            return true;
        }

        public bool CanAfford(int zenny, Dictionary<MaterialId, int> cost)
        {
            if (Zenny < zenny) return false;
            if (cost == null) return true;

            foreach (var pair in cost)
            {
                if (GetMaterial(pair.Key) < pair.Value) return false;
            }

            return true;
        }

        public bool Spend(int zenny, Dictionary<MaterialId, int> cost)
        {
            if (!CanAfford(zenny, cost)) return false;
            Zenny -= zenny;
            if (cost == null) return true;

            foreach (var pair in cost)
            {
                SpendMaterial(pair.Key, pair.Value);
            }

            return true;
        }

        public void AddHunterRankExp(int amount)
        {
            if (amount <= 0) return;
            HunterRankExp += amount;
            while (HunterRankExp >= ExpToNextRank)
            {
                HunterRankExp -= ExpToNextRank;
                HunterRank++;
            }
        }

        public float GetTotalDefense()
        {
            float total = 0f;
            foreach (var pair in EquippedArmor)
            {
                var armor = GameDatabase.GetArmor(pair.Value);
                if (armor != null) total += armor.Defense;
            }

            return total;
        }

        public float GetTotalHpBonus()
        {
            float total = 0f;
            foreach (var pair in EquippedArmor)
            {
                var armor = GameDatabase.GetArmor(pair.Value);
                if (armor != null) total += armor.HpBonus;
            }

            return total;
        }

        public float GetPlayerMaxHp() => 100f + GetTotalHpBonus() + (HunterRank - 1) * 8f;

        public float GetPlayerAttack()
        {
            var weapon = GetEquippedWeapon();
            var progress = GetEquippedWeaponProgress();
            if (weapon == null) return 10f;
            return weapon.BaseDamage * progress.DamageMultiplier + (HunterRank - 1) * 1.5f;
        }

        public float GetAttackInterval()
        {
            var weapon = GetEquippedWeapon();
            var progress = GetEquippedWeaponProgress();
            if (weapon == null) return 1.5f;
            return Mathf.Max(0.55f, weapon.AttackInterval / progress.SpeedMultiplier);
        }

        public bool OwnsArmor(string armorId) => OwnedArmor.Contains(armorId);

        public string GetEquippedArmorId(ArmorSlot slot)
        {
            return EquippedArmor.TryGetValue(slot.ToString(), out string id) ? id : null;
        }
    }
}
