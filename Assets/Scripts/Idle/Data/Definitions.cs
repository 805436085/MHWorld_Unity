using System;
using System.Collections.Generic;

namespace MHIdle.Data
{
    [Serializable]
    public class WeaponDef
    {
        public string Id;
        public string Name;
        public WeaponType Type;
        public int Tier;
        public float BaseDamage;
        public float AttackInterval;
        public int UnlockHunterRank;
        public int CraftZenny;
        public Dictionary<MaterialId, int> CraftCost = new Dictionary<MaterialId, int>();
    }

    [Serializable]
    public class ArmorDef
    {
        public string Id;
        public string Name;
        public ArmorSlot Slot;
        public int Tier;
        public float Defense;
        public float HpBonus;
        public int UnlockHunterRank;
        public int CraftZenny;
        public Dictionary<MaterialId, int> CraftCost = new Dictionary<MaterialId, int>();
    }

    [Serializable]
    public class MonsterDrop
    {
        public MaterialId Material;
        public int MinAmount;
        public int MaxAmount;
        public float Chance;
    }

    [Serializable]
    public class MonsterDef
    {
        public string Id;
        public string Name;
        public string Locale;
        public int Rank;
        public float MaxHp;
        public float Attack;
        public float Defense;
        public int ZennyReward;
        public int HunterRankExp;
        public int WeaponProficiencyExp;
        public List<MonsterDrop> Drops = new List<MonsterDrop>();
    }

    /// <summary>
    /// 运行时内置图鉴：不依赖 ScriptableObject 资源，进 Play 即可用。
    /// </summary>
    public static class GameDatabase
    {
        public static IReadOnlyList<WeaponDef> Weapons { get; private set; }
        public static IReadOnlyList<ArmorDef> Armors { get; private set; }
        public static IReadOnlyList<MonsterDef> Monsters { get; private set; }

        static GameDatabase()
        {
            Weapons = BuildWeapons();
            Armors = BuildArmors();
            Monsters = BuildMonsters();
        }

        public static WeaponDef GetWeapon(string id)
        {
            foreach (var weapon in Weapons)
            {
                if (weapon.Id == id) return weapon;
            }

            return null;
        }

        public static ArmorDef GetArmor(string id)
        {
            foreach (var armor in Armors)
            {
                if (armor.Id == id) return armor;
            }

            return null;
        }

        public static MonsterDef GetMonster(string id)
        {
            foreach (var monster in Monsters)
            {
                if (monster.Id == id) return monster;
            }

            return null;
        }

        public static MonsterDef GetMonsterByIndex(int index)
        {
            if (Monsters.Count == 0) return null;
            int clamped = Math.Max(0, Math.Min(index, Monsters.Count - 1));
            return Monsters[clamped];
        }

        static List<WeaponDef> BuildWeapons()
        {
            return new List<WeaponDef>
            {
                new WeaponDef
                {
                    Id = "gs_buster",
                    Name = "爆破大剑",
                    Type = WeaponType.GreatSword,
                    Tier = 1,
                    BaseDamage = 28f,
                    AttackInterval = 1.8f,
                    UnlockHunterRank = 1,
                    CraftZenny = 0
                },
                new WeaponDef
                {
                    Id = "gs_jagras",
                    Name = "大贼龙大剑",
                    Type = WeaponType.GreatSword,
                    Tier = 2,
                    BaseDamage = 42f,
                    AttackInterval = 1.75f,
                    UnlockHunterRank = 2,
                    CraftZenny = 800,
                    CraftCost = new Dictionary<MaterialId, int>
                    {
                        { MaterialId.MonsterBone, 8 },
                        { MaterialId.MonsterHide, 6 }
                    }
                },
                new WeaponDef
                {
                    Id = "gs_anjanath",
                    Name = "蛮颚龙大剑",
                    Type = WeaponType.GreatSword,
                    Tier = 3,
                    BaseDamage = 68f,
                    AttackInterval = 1.7f,
                    UnlockHunterRank = 4,
                    CraftZenny = 3200,
                    CraftCost = new Dictionary<MaterialId, int>
                    {
                        { MaterialId.MonsterBone, 12 },
                        { MaterialId.SharpClaw, 8 },
                        { MaterialId.MonsterHide, 10 }
                    }
                },
                new WeaponDef
                {
                    Id = "gs_rathalos",
                    Name = "火龙大剑",
                    Type = WeaponType.GreatSword,
                    Tier = 4,
                    BaseDamage = 96f,
                    AttackInterval = 1.65f,
                    UnlockHunterRank = 6,
                    CraftZenny = 9000,
                    CraftCost = new Dictionary<MaterialId, int>
                    {
                        { MaterialId.SharpClaw, 14 },
                        { MaterialId.WyvernGem, 3 },
                        { MaterialId.MonsterHide, 16 }
                    }
                },
                new WeaponDef
                {
                    Id = "gs_nergigante",
                    Name = "灭尽龙大剑",
                    Type = WeaponType.GreatSword,
                    Tier = 5,
                    BaseDamage = 135f,
                    AttackInterval = 1.55f,
                    UnlockHunterRank = 8,
                    CraftZenny = 22000,
                    CraftCost = new Dictionary<MaterialId, int>
                    {
                        { MaterialId.WyvernGem, 6 },
                        { MaterialId.ElderDragonBlood, 4 },
                        { MaterialId.SharpClaw, 20 }
                    }
                }
            };
        }

        static List<ArmorDef> BuildArmors()
        {
            var list = new List<ArmorDef>();
            AddArmorSet(list, "leather", "皮革", 1, 1, 4f, 12f, 0, null);
            AddArmorSet(list, "bone", "骨制", 2, 2, 8f, 24f, 600,
                new Dictionary<MaterialId, int>
                {
                    { MaterialId.MonsterBone, 4 },
                    { MaterialId.MonsterHide, 2 }
                });
            AddArmorSet(list, "jagras", "大贼龙", 3, 3, 14f, 40f, 1800,
                new Dictionary<MaterialId, int>
                {
                    { MaterialId.MonsterBone, 6 },
                    { MaterialId.MonsterHide, 5 },
                    { MaterialId.SharpClaw, 2 }
                });
            AddArmorSet(list, "rathalos", "火龙", 4, 6, 24f, 70f, 7000,
                new Dictionary<MaterialId, int>
                {
                    { MaterialId.SharpClaw, 6 },
                    { MaterialId.WyvernGem, 1 },
                    { MaterialId.MonsterHide, 8 }
                });
            return list;
        }

        static void AddArmorSet(
            List<ArmorDef> list,
            string idPrefix,
            string namePrefix,
            int tier,
            int unlockRank,
            float defense,
            float hpBonus,
            int zenny,
            Dictionary<MaterialId, int> cost)
        {
            var slots = new[]
            {
                ArmorSlot.Head, ArmorSlot.Chest, ArmorSlot.Arms, ArmorSlot.Waist, ArmorSlot.Legs
            };
            var slotNames = new[] { "头盔", "铠甲", "腕甲", "腰甲", "护腿" };

            for (int i = 0; i < slots.Length; i++)
            {
                var copiedCost = cost == null
                    ? new Dictionary<MaterialId, int>()
                    : new Dictionary<MaterialId, int>(cost);

                list.Add(new ArmorDef
                {
                    Id = $"{idPrefix}_{slots[i].ToString().ToLowerInvariant()}",
                    Name = $"{namePrefix}{slotNames[i]}",
                    Slot = slots[i],
                    Tier = tier,
                    Defense = defense,
                    HpBonus = hpBonus,
                    UnlockHunterRank = unlockRank,
                    CraftZenny = zenny,
                    CraftCost = copiedCost
                });
            }
        }

        static List<MonsterDef> BuildMonsters()
        {
            return new List<MonsterDef>
            {
                MakeMonster("jagras", "大贼龙", "古代树森林", 1, 120, 8, 2, 40, 8, 12,
                    (MaterialId.MonsterBone, 1, 3, 1f),
                    (MaterialId.MonsterHide, 1, 2, 0.8f)),
                MakeMonster("kulu", "眩鸟龙", "古代树森林", 2, 180, 12, 4, 70, 12, 16,
                    (MaterialId.MonsterBone, 2, 4, 1f),
                    (MaterialId.MonsterHide, 1, 3, 0.9f)),
                MakeMonster("pukei", "毒妖鸟", "古代树森林", 3, 260, 16, 6, 110, 18, 22,
                    (MaterialId.MonsterHide, 2, 4, 1f),
                    (MaterialId.SharpClaw, 1, 2, 0.55f)),
                MakeMonster("barroth", "土砂龙", "大蚁冢荒地", 4, 360, 22, 10, 160, 24, 28,
                    (MaterialId.MonsterBone, 3, 5, 1f),
                    (MaterialId.SharpClaw, 1, 3, 0.7f)),
                MakeMonster("anjanath", "蛮颚龙", "古代树森林", 5, 520, 30, 12, 240, 32, 36,
                    (MaterialId.SharpClaw, 2, 4, 0.9f),
                    (MaterialId.MonsterHide, 3, 5, 1f),
                    (MaterialId.WyvernGem, 1, 1, 0.12f)),
                MakeMonster("rathian", "雌火龙", "古代树森林", 6, 700, 38, 16, 340, 42, 44,
                    (MaterialId.SharpClaw, 2, 5, 1f),
                    (MaterialId.WyvernGem, 1, 1, 0.22f),
                    (MaterialId.MonsterHide, 3, 6, 1f)),
                MakeMonster("rathalos", "火龙", "古代树森林", 7, 920, 48, 20, 480, 55, 55,
                    (MaterialId.SharpClaw, 3, 6, 1f),
                    (MaterialId.WyvernGem, 1, 2, 0.35f),
                    (MaterialId.ElderDragonBlood, 1, 1, 0.08f)),
                MakeMonster("nergigante", "灭尽龙", "龙结晶之地", 8, 1300, 62, 26, 720, 80, 70,
                    (MaterialId.WyvernGem, 1, 2, 0.7f),
                    (MaterialId.ElderDragonBlood, 1, 2, 0.45f),
                    (MaterialId.SharpClaw, 4, 8, 1f))
            };
        }

        static MonsterDef MakeMonster(
            string id,
            string name,
            string locale,
            int rank,
            float hp,
            float atk,
            float defense,
            int zenny,
            int hrExp,
            int profExp,
            params (MaterialId mat, int min, int max, float chance)[] drops)
        {
            var def = new MonsterDef
            {
                Id = id,
                Name = name,
                Locale = locale,
                Rank = rank,
                MaxHp = hp,
                Attack = atk,
                Defense = defense,
                ZennyReward = zenny,
                HunterRankExp = hrExp,
                WeaponProficiencyExp = profExp
            };

            foreach (var drop in drops)
            {
                def.Drops.Add(new MonsterDrop
                {
                    Material = drop.mat,
                    MinAmount = drop.min,
                    MaxAmount = drop.max,
                    Chance = drop.chance
                });
            }

            return def;
        }
    }
}
