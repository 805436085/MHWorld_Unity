using System;
using System.Collections.Generic;
using MHIdle.Data;
using MHIdle.Systems;
using UnityEngine;

namespace MHIdle.Model
{
    [Serializable]
    public class HunterProgress
    {
        public const int MaxLoadoutSlots = 10;

        public int HunterRank = 1;
        public int HunterRankExp;
        public int Zenny = 120;
        public string EquippedWeaponId = "gs_buster";
        public int CurrentMonsterIndex;
        public int HighestMonsterIndexUnlocked;
        public int TotalKills;
        public int TotalLargeKills;
        public int HuntDeaths;
        public long LastSaveUnix;

        public Dictionary<string, int> Materials = new Dictionary<string, int>();
        public Dictionary<string, WeaponProgress> Weapons = new Dictionary<string, WeaponProgress>();
        public Dictionary<string, string> EquippedArmor = new Dictionary<string, string>();
        public HashSet<string> OwnedArmor = new HashSet<string>();

        /// <summary>武种层：武器种熟练度 key = WeaponType.ToString()</summary>
        public Dictionary<string, RingProgress> TypeRings = new Dictionary<string, RingProgress>();

        /// <summary>心法层：风格组 key = WeaponStyleGroup.ToString()</summary>
        public Dictionary<string, RingProgress> StyleRings = new Dictionary<string, RingProgress>();

        public Dictionary<string, MapProgress> Maps = new Dictionary<string, MapProgress>();
        public HashSet<string> UnlockedTechniques = new HashSet<string>();

        /// <summary>出征携带栏（道具 id → 数量），最多 10 种。</summary>
        public Dictionary<string, int> Loadout = new Dictionary<string, int>();

        /// <summary>仓库道具库存（非出征）。</summary>
        public Dictionary<string, int> ItemInventory = new Dictionary<string, int>();

        public int ExpToNextRank => 70 + HunterRank * 40;

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
            if (TypeRings == null) TypeRings = new Dictionary<string, RingProgress>();
            if (StyleRings == null) StyleRings = new Dictionary<string, RingProgress>();
            if (Maps == null) Maps = new Dictionary<string, MapProgress>();
            if (UnlockedTechniques == null) UnlockedTechniques = new HashSet<string>();
            if (Loadout == null) Loadout = new Dictionary<string, int>();
            if (ItemInventory == null) ItemInventory = new Dictionary<string, int>();

            // 新手赠送基础药品
            if (GetItem(ItemId.Potion) == 0 && GetLoadoutCount(ItemId.Potion) == 0 && TotalKills == 0)
            {
                AddItem(ItemId.Potion, 5);
            }

            EquippedArmor.Remove("Waist");
            OwnedArmor.RemoveWhere(id => id.EndsWith("_waist", StringComparison.OrdinalIgnoreCase));
            OwnedArmor.RemoveWhere(id => GameDatabase.GetArmor(id) == null);
            foreach (var key in new List<string>(EquippedArmor.Keys))
            {
                if (!Enum.TryParse<ArmorSlot>(key, out _) || GameDatabase.GetArmor(EquippedArmor[key]) == null)
                {
                    EquippedArmor.Remove(key);
                }
            }

            foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
            {
                string key = type.ToString();
                if (!TypeRings.ContainsKey(key)) TypeRings[key] = new RingProgress();
            }

            foreach (WeaponStyleGroup group in Enum.GetValues(typeof(WeaponStyleGroup)))
            {
                string key = group.ToString();
                if (!StyleRings.ContainsKey(key)) StyleRings[key] = new RingProgress();
            }

            foreach (MapId mapId in Enum.GetValues(typeof(MapId)))
            {
                string key = mapId.ToString();
                if (!Maps.ContainsKey(key))
                {
                    Maps[key] = new MapProgress { MapId = key };
                }
            }

            // 图鉴扩容后按猎人等级重同步解锁，避免旧存档下标错位
            if (TotalKills > 0 || HunterRank > 1 || TotalLargeKills > 0)
            {
                int rankCap = HunterRank + 1;
                for (int i = 0; i < GameDatabase.Monsters.Count; i++)
                {
                    if (GameDatabase.Monsters[i].Rank <= rankCap)
                    {
                        HighestMonsterIndexUnlocked = Mathf.Max(HighestMonsterIndexUnlocked, i);
                    }
                }
            }

            HighestMonsterIndexUnlocked = Mathf.Clamp(
                HighestMonsterIndexUnlocked, 0, Mathf.Max(0, GameDatabase.Monsters.Count - 1));
            CurrentMonsterIndex = Mathf.Clamp(
                CurrentMonsterIndex, 0, HighestMonsterIndexUnlocked);

            foreach (var weapon in GameDatabase.Weapons)
            {
                if (!Weapons.ContainsKey(weapon.Id))
                {
                    Weapons[weapon.Id] = new WeaponProgress
                    {
                        WeaponId = weapon.Id,
                        Owned = weapon.Id == "gs_buster",
                        Outer = new RingProgress()
                    };
                }
                else if (Weapons[weapon.Id].Outer == null)
                {
                    Weapons[weapon.Id].Outer = new RingProgress
                    {
                        Level = Mathf.Max(1, Weapons[weapon.Id].ProficiencyLevel),
                        Exp = Mathf.Max(0, Weapons[weapon.Id].ProficiencyExp)
                    };
                }
            }

            if (string.IsNullOrEmpty(EquippedWeaponId) || GetWeaponDef(EquippedWeaponId) == null)
            {
                EquippedWeaponId = "gs_buster";
            }

            Weapons[EquippedWeaponId].Owned = true;

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

        public RingProgress GetTypeRing(WeaponType type)
        {
            EnsureDefaults();
            return TypeRings[type.ToString()];
        }

        public RingProgress GetStyleRing(WeaponStyleGroup group)
        {
            EnsureDefaults();
            return StyleRings[group.ToString()];
        }

        public MapProgress GetMapProgress(MapId mapId)
        {
            EnsureDefaults();
            return Maps[mapId.ToString()];
        }

        public int GetMaterial(MaterialId id)
        {
            string key = id.ToString();
            return Materials.TryGetValue(key, out int value) ? value : 0;
        }

        public void AddMaterial(MaterialId id, int amount)
        {
            if (amount <= 0) return;
            Materials[id.ToString()] = GetMaterial(id) + amount;
        }

        public bool SpendMaterial(MaterialId id, int amount)
        {
            if (GetMaterial(id) < amount) return false;
            Materials[id.ToString()] = GetMaterial(id) - amount;
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
            foreach (var pair in cost) SpendMaterial(pair.Key, pair.Value);
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

            var style = GetStyleRing(WeaponTaxonomy.GetStyleGroup(GetEquippedWeapon().Type));
            total += (style.Level - 1) * 0.6f;

            var skills = ArmorSkillSystem.Evaluate(this);
            return total * skills.DefenseMul;
        }

        public float GetTotalHpBonus()
        {
            float total = 0f;
            foreach (var pair in EquippedArmor)
            {
                var armor = GameDatabase.GetArmor(pair.Value);
                if (armor != null) total += armor.HpBonus;
            }

            total += ArmorSkillSystem.Evaluate(this).HpFlat;
            return total;
        }

        public float GetPlayerMaxHp() => 100f + GetTotalHpBonus() + (HunterRank - 1) * 8f;

        public float GetPlayerAttack()
        {
            var weapon = GetEquippedWeapon();
            var progress = GetEquippedWeaponProgress();
            if (weapon == null) return 10f;

            float typeBonus = 1f + (GetTypeRing(weapon.Type).Level - 1) * 0.015f;
            float styleBonus = 1f + (GetStyleRing(WeaponTaxonomy.GetStyleGroup(weapon.Type)).Level - 1) * 0.01f;
            float techBonus = 1f + ProficiencySystem.GetTechniqueDamageBonus(this, weapon.Type);
            float skillMul = ArmorSkillSystem.Evaluate(this).AttackMul;

            return weapon.BaseDamage * progress.DamageMultiplier * typeBonus * styleBonus * techBonus * skillMul
                   + (HunterRank - 1) * 1.5f;
        }

        public float GetAttackInterval()
        {
            var weapon = GetEquippedWeapon();
            var progress = GetEquippedWeaponProgress();
            if (weapon == null) return 1.5f;
            float interval = weapon.AttackInterval / progress.SpeedMultiplier;
            interval *= ArmorSkillSystem.Evaluate(this).AttackIntervalMul;
            return Mathf.Max(CombatBalance.MinPlayerAttackInterval, interval);
        }

        public float GetChargeChance()
        {
            var weapon = GetEquippedWeapon();
            if (weapon == null || weapon.Type != WeaponType.GreatSword) return 0f;
            float chance = 0.12f + ProficiencySystem.GetTechniqueChargeBonus(this, weapon.Type);
            if (UnlockedTechniques.Contains(TechniqueId.GsCharge3.ToString())) chance += 0.06f;
            chance += ArmorSkillSystem.Evaluate(this).ChargeChanceBonus;
            return Mathf.Clamp01(chance);
        }

        public SkillCombatEffects GetSkillEffects() => ArmorSkillSystem.Evaluate(this);

        public bool OwnsArmor(string armorId) => OwnedArmor.Contains(armorId);

        public string GetEquippedArmorId(ArmorSlot slot)
        {
            return EquippedArmor.TryGetValue(slot.ToString(), out string id) ? id : null;
        }

        public int LoadoutTypeCount => Loadout.Count;

        public int GetItem(ItemId id)
        {
            string key = id.ToString();
            return ItemInventory.TryGetValue(key, out int v) ? v : 0;
        }

        public void AddItem(ItemId id, int amount)
        {
            if (amount == 0) return;
            string key = id.ToString();
            int next = GetItem(id) + amount;
            if (next <= 0) ItemInventory.Remove(key);
            else ItemInventory[key] = next;
        }

        public int GetLoadoutCount(ItemId id)
        {
            string key = id.ToString();
            return Loadout.TryGetValue(key, out int v) ? v : 0;
        }

        public void AddLoadout(ItemId id, int amount)
        {
            if (amount == 0) return;
            string key = id.ToString();
            int next = GetLoadoutCount(id) + amount;
            if (next <= 0) Loadout.Remove(key);
            else Loadout[key] = next;
        }

        public bool TryAddToLoadout(string itemId, int amount = 1)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0) return false;
            if (!Enum.TryParse(itemId, out ItemId id)) return false;
            if (!Loadout.ContainsKey(itemId) && Loadout.Count >= MaxLoadoutSlots) return false;
            var def = ItemDatabase.Get(id);
            if (def != null && GetLoadoutCount(id) + amount > def.MaxStack) return false;
            AddLoadout(id, amount);
            return true;
        }
    }
}
