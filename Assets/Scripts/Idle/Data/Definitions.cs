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
            Weapons = WeaponCatalog.Build();
            Armors = ArmorCatalog.Build();
            Monsters = MonsterCatalog.Build();
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
    }
}
