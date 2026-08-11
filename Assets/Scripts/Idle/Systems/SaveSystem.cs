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
        public long LastSaveUnix;
        public List<MaterialEntry> Materials = new List<MaterialEntry>();
        public List<WeaponEntry> Weapons = new List<WeaponEntry>();
        public List<ArmorEquipEntry> EquippedArmor = new List<ArmorEquipEntry>();
        public List<string> OwnedArmor = new List<string>();
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
    }

    [Serializable]
    class ArmorEquipEntry
    {
        public string Slot;
        public string ArmorId;
    }

    public static class SaveSystem
    {
        const string SaveKey = "mh_idle_save_v1";

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
                LastSaveUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            progress.LastSaveUnix = data.LastSaveUnix;

            foreach (var pair in progress.Materials)
            {
                data.Materials.Add(new MaterialEntry { Id = pair.Key, Amount = pair.Value });
            }

            foreach (var pair in progress.Weapons)
            {
                data.Weapons.Add(new WeaponEntry
                {
                    WeaponId = pair.Key,
                    ProficiencyLevel = pair.Value.ProficiencyLevel,
                    ProficiencyExp = pair.Value.ProficiencyExp,
                    Owned = pair.Value.Owned
                });
            }

            foreach (var pair in progress.EquippedArmor)
            {
                data.EquippedArmor.Add(new ArmorEquipEntry { Slot = pair.Key, ArmorId = pair.Value });
            }

            data.OwnedArmor.AddRange(progress.OwnedArmor);

            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public static HunterProgress LoadOrCreate()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                return HunterProgress.CreateNew();
            }

            try
            {
                var data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SaveKey));
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
                    LastSaveUnix = data.LastSaveUnix
                };

                foreach (var entry in data.Materials)
                {
                    progress.Materials[entry.Id] = entry.Amount;
                }

                foreach (var entry in data.Weapons)
                {
                    progress.Weapons[entry.WeaponId] = new WeaponProgress
                    {
                        WeaponId = entry.WeaponId,
                        ProficiencyLevel = Mathf.Max(1, entry.ProficiencyLevel),
                        ProficiencyExp = Mathf.Max(0, entry.ProficiencyExp),
                        Owned = entry.Owned
                    };
                }

                foreach (var entry in data.EquippedArmor)
                {
                    progress.EquippedArmor[entry.Slot] = entry.ArmorId;
                }

                foreach (var armorId in data.OwnedArmor)
                {
                    progress.OwnedArmor.Add(armorId);
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
            PlayerPrefs.Save();
        }
    }
}
