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
        public List<SkillPointGrant> SkillPoints = new List<SkillPointGrant>();
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
        public MapId MapId;
        public MonsterSize Size;
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
                },
                new WeaponDef
                {
                    Id = "ls_iron",
                    Name = "铁刀",
                    Type = WeaponType.LongSword,
                    Tier = 1,
                    BaseDamage = 22f,
                    AttackInterval = 1.15f,
                    UnlockHunterRank = 1,
                    CraftZenny = 200
                },
                new WeaponDef
                {
                    Id = "ls_rathalos",
                    Name = "火龙太刀",
                    Type = WeaponType.LongSword,
                    Tier = 4,
                    BaseDamage = 78f,
                    AttackInterval = 1.05f,
                    UnlockHunterRank = 6,
                    CraftZenny = 8500,
                    CraftCost = new Dictionary<MaterialId, int>
                    {
                        { MaterialId.SharpClaw, 12 },
                        { MaterialId.WyvernGem, 2 },
                        { MaterialId.MonsterHide, 14 }
                    }
                },
                new WeaponDef
                {
                    Id = "ls_rathian",
                    Name = "雌火龙太刀",
                    Type = WeaponType.LongSword,
                    Tier = 4,
                    BaseDamage = 72f,
                    AttackInterval = 1.08f,
                    UnlockHunterRank = 6,
                    CraftZenny = 7800,
                    CraftCost = new Dictionary<MaterialId, int>
                    {
                        { MaterialId.SharpClaw, 10 },
                        { MaterialId.WyvernGem, 2 },
                        { MaterialId.MonsterHide, 12 }
                    }
                }
            };
        }

        static List<ArmorDef> BuildArmors()
        {
            var list = new List<ArmorDef>();

            // 皮革：体力流入门（4×3=12 → 体力UP小）
            AddArmorSet(list, "leather", "皮革", 1, 1, 4f, 12f, 0, null,
                PerSlot(
                    Skill(SkillId.Health, 3),
                    Skill(SkillId.Health, 3),
                    Skill(SkillId.Health, 3),
                    Skill(SkillId.Health, 3)));

            // 骨制：攻击入门（4×3=12 → 攻击UP小）
            AddArmorSet(list, "bone", "骨制", 2, 2, 8f, 24f, 600,
                new Dictionary<MaterialId, int>
                {
                    { MaterialId.MonsterBone, 4 },
                    { MaterialId.MonsterHide, 2 }
                },
                PerSlot(
                    Skill(SkillId.Attack, 3),
                    Skill(SkillId.Attack, 3),
                    Skill(SkillId.Attack, 2),
                    Skill(SkillId.Attack, 3)));

            // 怪鸟：道具/回复流
            AddArmorSet(list, "kutku", "怪鸟", 3, 3, 12f, 36f, 1600,
                new Dictionary<MaterialId, int>
                {
                    { MaterialId.MonsterBone, 5 },
                    { MaterialId.MonsterHide, 6 },
                    { MaterialId.SharpClaw, 2 }
                },
                PerSlot(
                    Skills(Skill(SkillId.ItemUse, 3), Skill(SkillId.RecSpeed, 1)),
                    Skills(Skill(SkillId.ItemUse, 3), Skill(SkillId.RecSpeed, 1)),
                    Skills(Skill(SkillId.ItemUse, 2), Skill(SkillId.RecSpeed, 2)),
                    Skills(Skill(SkillId.ItemUse, 3), Skill(SkillId.RecSpeed, 2))));

            // 毒怪鸟：麻痹 + 陷阱
            AddArmorSet(list, "gypceros", "毒怪鸟", 3, 4, 16f, 40f, 2400,
                new Dictionary<MaterialId, int>
                {
                    { MaterialId.MonsterHide, 8 },
                    { MaterialId.SharpClaw, 4 }
                },
                PerSlot(
                    Skills(Skill(SkillId.Paralysis, 3), Skill(SkillId.TrapMaster, 1)),
                    Skills(Skill(SkillId.Paralysis, 3), Skill(SkillId.TrapMaster, 2)),
                    Skills(Skill(SkillId.Paralysis, 2), Skill(SkillId.TrapMaster, 3)),
                    Skills(Skill(SkillId.Paralysis, 2), Skill(SkillId.TrapMaster, 3))));

            // 岩龙：防守
            AddArmorSet(list, "basarios", "岩龙", 4, 5, 22f, 50f, 4200,
                new Dictionary<MaterialId, int>
                {
                    { MaterialId.MonsterBone, 10 },
                    { MaterialId.WyvernGem, 1 }
                },
                PerSlot(
                    Skills(Skill(SkillId.Defense, 3), Skill(SkillId.Guard, 2)),
                    Skills(Skill(SkillId.Defense, 3), Skill(SkillId.Guard, 2)),
                    Skills(Skill(SkillId.Defense, 2), Skill(SkillId.Guard, 3)),
                    Skills(Skill(SkillId.Defense, 3), Skill(SkillId.Guard, 2))));

            // 雌火龙：毒 + 状态（异常压制）
            AddArmorSet(list, "rathian", "雌火龙", 4, 6, 20f, 55f, 6500,
                new Dictionary<MaterialId, int>
                {
                    { MaterialId.SharpClaw, 5 },
                    { MaterialId.MonsterHide, 8 },
                    { MaterialId.WyvernGem, 1 }
                },
                PerSlot(
                    Skills(Skill(SkillId.Poison, 3), Skill(SkillId.StatusAtk, 2)),
                    Skills(Skill(SkillId.Poison, 3), Skill(SkillId.StatusAtk, 2)),
                    Skills(Skill(SkillId.Poison, 2), Skill(SkillId.StatusAtk, 3)),
                    Skills(Skill(SkillId.Poison, 2), Skill(SkillId.StatusAtk, 3))));

            // 火龙：攻击 + 达人（会心输出）
            AddArmorSet(list, "rathalos", "火龙", 4, 6, 24f, 70f, 7000,
                new Dictionary<MaterialId, int>
                {
                    { MaterialId.SharpClaw, 6 },
                    { MaterialId.WyvernGem, 1 },
                    { MaterialId.MonsterHide, 8 }
                },
                PerSlot(
                    Skills(Skill(SkillId.Attack, 3), Skill(SkillId.Expert, 2)),
                    Skills(Skill(SkillId.Attack, 2), Skill(SkillId.Expert, 3)),
                    Skills(Skill(SkillId.Attack, 2), Skill(SkillId.Expert, 3)),
                    Skills(Skill(SkillId.Attack, 3), Skill(SkillId.Expert, 2))));

            // 眠鸟气质：睡眠暴力一刀（睡眠 + 攻击）
            AddArmorSet(list, "hypnoc", "眠鸟", 4, 5, 18f, 48f, 5000,
                new Dictionary<MaterialId, int>
                {
                    { MaterialId.MonsterHide, 10 },
                    { MaterialId.SharpClaw, 4 },
                    { MaterialId.MonsterBone, 6 }
                },
                PerSlot(
                    Skills(Skill(SkillId.Sleep, 3), Skill(SkillId.Attack, 1)),
                    Skills(Skill(SkillId.Sleep, 3), Skill(SkillId.Attack, 2)),
                    Skills(Skill(SkillId.Sleep, 2), Skill(SkillId.Attack, 2)),
                    Skills(Skill(SkillId.Sleep, 3), Skill(SkillId.Attack, 1))));

            return list;
        }

        static SkillPointGrant Skill(SkillId id, int points) => new SkillPointGrant(id, points);

        static List<SkillPointGrant> Skills(params SkillPointGrant[] grants) =>
            new List<SkillPointGrant>(grants);

        static List<SkillPointGrant>[] PerSlot(
            List<SkillPointGrant> head,
            List<SkillPointGrant> chest,
            List<SkillPointGrant> arms,
            List<SkillPointGrant> legs) =>
            new[] { head, chest, arms, legs };

        static List<SkillPointGrant>[] PerSlot(
            SkillPointGrant head,
            SkillPointGrant chest,
            SkillPointGrant arms,
            SkillPointGrant legs) =>
            new[]
            {
                new List<SkillPointGrant> { head },
                new List<SkillPointGrant> { chest },
                new List<SkillPointGrant> { arms },
                new List<SkillPointGrant> { legs }
            };

        static void AddArmorSet(
            List<ArmorDef> list,
            string idPrefix,
            string namePrefix,
            int tier,
            int unlockRank,
            float defense,
            float hpBonus,
            int zenny,
            Dictionary<MaterialId, int> cost,
            List<SkillPointGrant>[] slotSkills = null)
        {
            var slots = new[]
            {
                ArmorSlot.Head, ArmorSlot.Chest, ArmorSlot.Arms, ArmorSlot.Legs
            };
            var slotNames = new[] { "头盔", "铠甲", "腕甲", "护腿" };

            for (int i = 0; i < slots.Length; i++)
            {
                var copiedCost = cost == null
                    ? new Dictionary<MaterialId, int>()
                    : new Dictionary<MaterialId, int>(cost);

                var skills = slotSkills != null && i < slotSkills.Length
                    ? new List<SkillPointGrant>(slotSkills[i])
                    : new List<SkillPointGrant>();

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
                    CraftCost = copiedCost,
                    SkillPoints = skills
                });
            }
        }

        static List<MonsterDef> BuildMonsters()
        {
            // 前半：日常挂机小怪；后半：主动出击大型怪（2G 气质）
            return new List<MonsterDef>
            {
                MakeMonster("kelbi", "凯欧比", MapId.ForestAndHills, MonsterSize.Small, 1, 55, 4, 0, 12, 3, 6,
                    (MaterialId.MonsterHide, 1, 2, 0.9f)),
                MakeMonster("bullfango", "野猪", MapId.ForestAndHills, MonsterSize.Small, 1, 70, 6, 1, 18, 4, 8,
                    (MaterialId.MonsterBone, 1, 2, 1f),
                    (MaterialId.MonsterHide, 1, 1, 0.7f)),
                MakeMonster("velociprey", "蓝速龙", MapId.Jungle, MonsterSize.Small, 2, 90, 8, 2, 25, 5, 10,
                    (MaterialId.MonsterBone, 1, 3, 1f),
                    (MaterialId.SharpClaw, 1, 1, 0.35f)),
                MakeMonster("genprey", "黄速龙", MapId.Desert, MonsterSize.Small, 2, 95, 9, 2, 28, 5, 10,
                    (MaterialId.MonsterHide, 1, 2, 1f),
                    (MaterialId.SharpClaw, 1, 1, 0.4f)),
                MakeMonster("yian_kut_ku", "怪鸟", MapId.ForestAndHills, MonsterSize.Large, 3, 320, 18, 6, 160, 22, 28,
                    (MaterialId.MonsterHide, 2, 4, 1f),
                    (MaterialId.SharpClaw, 1, 2, 0.6f)),
                MakeMonster("gypceros", "毒怪鸟", MapId.Swamp, MonsterSize.Large, 4, 420, 22, 8, 210, 28, 34,
                    (MaterialId.MonsterHide, 2, 5, 1f),
                    (MaterialId.SharpClaw, 1, 3, 0.55f)),
                MakeMonster("basarios", "岩龙", MapId.Volcano, MonsterSize.Large, 5, 560, 26, 14, 280, 34, 40,
                    (MaterialId.MonsterBone, 3, 6, 1f),
                    (MaterialId.WyvernGem, 1, 1, 0.15f)),
                MakeMonster("rathian", "雌火龙", MapId.ForestAndHills, MonsterSize.Large, 6, 720, 36, 16, 360, 45, 48,
                    (MaterialId.SharpClaw, 2, 5, 1f),
                    (MaterialId.WyvernGem, 1, 1, 0.25f),
                    (MaterialId.MonsterHide, 3, 6, 1f)),
                MakeMonster("rathalos", "火龙", MapId.ForestAndHills, MonsterSize.Large, 7, 950, 46, 20, 500, 58, 58,
                    (MaterialId.SharpClaw, 3, 6, 1f),
                    (MaterialId.WyvernGem, 1, 2, 0.35f),
                    (MaterialId.ElderDragonBlood, 1, 1, 0.08f)),
                MakeMonster("kushala", "钢龙", MapId.SnowyMountains, MonsterSize.Large, 8, 1300, 58, 26, 760, 80, 72,
                    (MaterialId.WyvernGem, 1, 2, 0.7f),
                    (MaterialId.ElderDragonBlood, 1, 2, 0.5f),
                    (MaterialId.SharpClaw, 4, 8, 1f))
            };
        }

        static MonsterDef MakeMonster(
            string id,
            string name,
            MapId mapId,
            MonsterSize size,
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
                Locale = WeaponTaxonomy.MapName(mapId),
                MapId = mapId,
                Size = size,
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
