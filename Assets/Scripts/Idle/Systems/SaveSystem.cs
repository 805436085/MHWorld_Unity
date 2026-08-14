using System;
using System.Collections.Generic;
using MHIdle.Model;
using UnityEngine;

namespace MHIdle.Systems
{
    [Serializable]
    class SaveData
    {
        public int HunterRank = 1;
        public int HunterRankExp;
        public int Zenny;
        public string EquippedWeaponId;
        public int CurrentMonsterIndex;
        public int HighestMonsterIndexUnlocked;
        public int TotalKills;
        public int TotalLargeKills;
        public int HuntDeaths;
        public long LastSaveUnix;
        public List<MaterialEntry> Materials = new List<MaterialEntry>();
        public List<WeaponEntry> Weapons = new List<WeaponEntry>();
        public List<ArmorEquipEntry> EquippedArmor = new List<ArmorEquipEntry>();
        public List<string> OwnedArmor = new List<string>();
        public List<RingEntry> TypeRings = new List<RingEntry>();
        public List<RingEntry> StyleRings = new List<RingEntry>();
        public List<MapEntry> Maps = new List<MapEntry>();
        public List<string> UnlockedTechniques = new List<string>();
        public List<MaterialEntry> Loadout = new List<MaterialEntry>();
        public List<MaterialEntry> ItemInventory = new List<MaterialEntry>();
        public string SelectedPlaystyleId;
    }

    [Serializable]
    class MaterialEntry
    {
        public string Id;
        public int Amount;
    }

    [Serializable]
    class WeaponEntry
    {
        public string WeaponId;
        public int ProficiencyLevel;
        public int ProficiencyExp;
        public bool Owned;
        public bool BottleneckBroken;
    }

    [Serializable]
    class ArmorEquipEntry
    {
        public string Slot;
        public string ArmorId;
    }

    [Serializable]
    class RingEntry
    {
        public string Id;
        public int Level;
        public int Exp;
    }

    [Serializable]
    class MapEntry
    {
        public string MapId;
        public int Level;
        public int Exp;
        public bool TrapUnlocked;
        public bool AdvantageUnlocked;
    }

    public static class SaveSystem
    {
        const string SaveKey = "mh_idle_save_v2";
        const string LegacySaveKey = "mh_idle_save_v1";

        public static void Save(HunterProgress progress)
        {
            var data = new SaveData
            {
                HunterRank = progress.HunterRank,
                HunterRankExp = progress.HunterRankExp,
                Zenny = progress.Zenny,
                EquippedWeaponId = progress.EquippedWeaponId,
                CurrentMonsterIndex = progress.CurrentMonsterIndex,
                HighestMonsterIndexUnlocked = progress.HighestMonsterIndexUnlocked,
                TotalKills = progress.TotalKills,
                TotalLargeKills = progress.TotalLargeKills,
                HuntDeaths = progress.HuntDeaths,
                LastSaveUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                SelectedPlaystyleId = progress.SelectedPlaystyleId
            };
            progress.LastSaveUnix = data.LastSaveUnix;

            foreach (var pair in progress.Materials)
                data.Materials.Add(new MaterialEntry { Id = pair.Key, Amount = pair.Value });

            foreach (var pair in progress.Weapons)
            {
                data.Weapons.Add(new WeaponEntry
                {
                    WeaponId = pair.Key,
                    ProficiencyLevel = pair.Value.Outer.Level,
                    ProficiencyExp = pair.Value.Outer.Exp,
                    Owned = pair.Value.Owned,
                    BottleneckBroken = pair.Value.BottleneckBroken
                });
            }

            foreach (var pair in progress.EquippedArmor)
                data.EquippedArmor.Add(new ArmorEquipEntry { Slot = pair.Key, ArmorId = pair.Value });

            data.OwnedArmor.AddRange(progress.OwnedArmor);

            foreach (var pair in progress.TypeRings)
                data.TypeRings.Add(new RingEntry { Id = pair.Key, Level = pair.Value.Level, Exp = pair.Value.Exp });

            foreach (var pair in progress.StyleRings)
                data.StyleRings.Add(new RingEntry { Id = pair.Key, Level = pair.Value.Level, Exp = pair.Value.Exp });

            foreach (var pair in progress.Maps)
            {
                data.Maps.Add(new MapEntry
                {
                    MapId = pair.Key,
                    Level = pair.Value.Ring.Level,
                    Exp = pair.Value.Ring.Exp,
                    TrapUnlocked = pair.Value.TrapUnlocked,
                    AdvantageUnlocked = pair.Value.AdvantageUnlocked
                });
            }

            data.UnlockedTechniques.AddRange(progress.UnlockedTechniques);

            foreach (var pair in progress.Loadout)
                data.Loadout.Add(new MaterialEntry { Id = pair.Key, Amount = pair.Value });

            foreach (var pair in progress.ItemInventory)
                data.ItemInventory.Add(new MaterialEntry { Id = pair.Key, Amount = pair.Value });

            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public static HunterProgress LoadOrCreate()
        {
            string raw = null;
            if (PlayerPrefs.HasKey(SaveKey)) raw = PlayerPrefs.GetString(SaveKey);
            else if (PlayerPrefs.HasKey(LegacySaveKey)) raw = PlayerPrefs.GetString(LegacySaveKey);

            if (string.IsNullOrEmpty(raw)) return HunterProgress.CreateNew();

            try
            {
                var data = JsonUtility.FromJson<SaveData>(raw);
                if (data == null) return HunterProgress.CreateNew();

                var progress = new HunterProgress
                {
                    HunterRank = Mathf.Max(1, data.HunterRank),
                    HunterRankExp = Mathf.Max(0, data.HunterRankExp),
                    Zenny = Mathf.Max(0, data.Zenny),
                    EquippedWeaponId = data.EquippedWeaponId,
                    CurrentMonsterIndex = Mathf.Max(0, data.CurrentMonsterIndex),
                    HighestMonsterIndexUnlocked = Mathf.Max(0, data.HighestMonsterIndexUnlocked),
                    TotalKills = Mathf.Max(0, data.TotalKills),
                    TotalLargeKills = Mathf.Max(0, data.TotalLargeKills),
                    HuntDeaths = Mathf.Max(0, data.HuntDeaths),
                    LastSaveUnix = data.LastSaveUnix,
                    SelectedPlaystyleId = data.SelectedPlaystyleId
                };

                foreach (var entry in data.Materials) progress.Materials[entry.Id] = entry.Amount;

                foreach (var entry in data.Weapons)
                {
                    progress.Weapons[entry.WeaponId] = new WeaponProgress
                    {
                        WeaponId = entry.WeaponId,
                        Owned = entry.Owned,
                        BottleneckBroken = entry.BottleneckBroken,
                        Outer = new RingProgress
                        {
                            Level = Mathf.Max(1, entry.ProficiencyLevel),
                            Exp = Mathf.Max(0, entry.ProficiencyExp)
                        }
                    };
                }

                foreach (var entry in data.EquippedArmor) progress.EquippedArmor[entry.Slot] = entry.ArmorId;
                foreach (var armorId in data.OwnedArmor) progress.OwnedArmor.Add(armorId);

                if (data.TypeRings != null)
                {
                    foreach (var entry in data.TypeRings)
                        progress.TypeRings[entry.Id] = new RingProgress { Level = Mathf.Max(1, entry.Level), Exp = Mathf.Max(0, entry.Exp) };
                }

                if (data.StyleRings != null)
                {
                    foreach (var entry in data.StyleRings)
                        progress.StyleRings[entry.Id] = new RingProgress { Level = Mathf.Max(1, entry.Level), Exp = Mathf.Max(0, entry.Exp) };
                }

                if (data.Maps != null)
                {
                    foreach (var entry in data.Maps)
                    {
                        progress.Maps[entry.MapId] = new MapProgress
                        {
                            MapId = entry.MapId,
                            TrapUnlocked = entry.TrapUnlocked,
                            AdvantageUnlocked = entry.AdvantageUnlocked,
                            Ring = new RingProgress { Level = Mathf.Max(1, entry.Level), Exp = Mathf.Max(0, entry.Exp) }
                        };
                    }
                }

                if (data.UnlockedTechniques != null)
                {
                    foreach (var t in data.UnlockedTechniques) progress.UnlockedTechniques.Add(t);
                }

                if (data.Loadout != null)
                {
                    foreach (var entry in data.Loadout) progress.Loadout[entry.Id] = entry.Amount;
                }

                if (data.ItemInventory != null)
                {
                    foreach (var entry in data.ItemInventory) progress.ItemInventory[entry.Id] = entry.Amount;
                }

                progress.EnsureDefaults();
                return progress;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"读取存档失败，将新建进度: {e.Message}");
                return HunterProgress.CreateNew();
            }
        }

        public static void Delete()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.DeleteKey(LegacySaveKey);
            PlayerPrefs.Save();
        }
    }
}
