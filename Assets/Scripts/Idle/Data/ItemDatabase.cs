using System;
using System.Collections.Generic;

namespace MHIdle.Data
{
    public enum ItemCategory
    {
        Heal,
        Buff,
        Trap,
        Tool,
        Bomb
    }

    public enum ItemId
    {
        Potion,          // 回复药
        MegaPotion,      // 回复药大
        WellDoneSteak,   // 熟肉（回血+临时HP）
        Demondrug,       // 鬼人药
        Armorskin,       // 硬化药
        Lifepowder,      // 生命之粉
        PitfallTrap,     // 落穴陷阱
        ShockTrap,       // 麻痹陷阱
        FlashBomb,       // 闪光弹
        BarrelBomb,      // 大桶炸弹
        TranqBomb,       // 麻醉球（出击掉落加成占位）
        Paintball        // 追踪玉（地图经验微增占位）
    }

    [Serializable]
    public class ItemDef
    {
        public ItemId Id;
        public string Name;
        public ItemCategory Category;
        public string Description;
        public int MaxStack = 10;
        public int ShopPrice;
        public int UnlockHunterRank = 1;
        public Dictionary<MaterialId, int> CraftCost = new Dictionary<MaterialId, int>();
        public int CraftZenny;

        // 效果参数
        public float HealAmount;
        public float HealPercent;
        public float AttackBuffMul = 1f;
        public float DefenseBuffMul = 1f;
        public float BuffDuration;
        public float TrapImmobilizeSeconds;
        public float TrapDamage;
        public float BombDamage;
        public float FlashWeakenSeconds;
        public float FlashIncomingMul = 1f;
        public bool ActiveHuntOnly; // 仅主动出击可用
        public bool AutoUseInIdle;  // 挂机是否自动用
    }

    public static class ItemDatabase
    {
        public static readonly IReadOnlyList<ItemDef> All = Build();

        public static ItemDef Get(ItemId id)
        {
            foreach (var item in All)
            {
                if (item.Id == id) return item;
            }

            return null;
        }

        public static ItemDef Get(string id)
        {
            if (!Enum.TryParse(id, out ItemId parsed)) return null;
            return Get(parsed);
        }

        static List<ItemDef> Build()
        {
            return new List<ItemDef>
            {
                new ItemDef
                {
                    Id = ItemId.Potion,
                    Name = "回复药",
                    Category = ItemCategory.Heal,
                    Description = "HP 较低时自动服用，回复 40 点。",
                    MaxStack = 10,
                    ShopPrice = 30,
                    HealAmount = 40f,
                    AutoUseInIdle = true
                },
                new ItemDef
                {
                    Id = ItemId.MegaPotion,
                    Name = "回复药·大",
                    Category = ItemCategory.Heal,
                    Description = "危急时服用，回复 80 点。",
                    MaxStack = 5,
                    ShopPrice = 90,
                    UnlockHunterRank = 2,
                    HealAmount = 80f,
                    CraftZenny = 40,
                    CraftCost = new Dictionary<MaterialId, int> { { MaterialId.MonsterHide, 1 } },
                    AutoUseInIdle = true
                },
                new ItemDef
                {
                    Id = ItemId.WellDoneSteak,
                    Name = "熟肉",
                    Category = ItemCategory.Heal,
                    Description = "回复 25 并短时提升最大体力感（回复额外 15）。",
                    MaxStack = 5,
                    ShopPrice = 50,
                    HealAmount = 40f,
                    AutoUseInIdle = false,
                    ActiveHuntOnly = true
                },
                new ItemDef
                {
                    Id = ItemId.Demondrug,
                    Name = "鬼人药",
                    Category = ItemCategory.Buff,
                    Description = "出击开场提升攻击 12%，持续整场。",
                    MaxStack = 3,
                    ShopPrice = 120,
                    UnlockHunterRank = 3,
                    AttackBuffMul = 1.12f,
                    BuffDuration = 999f,
                    ActiveHuntOnly = true
                },
                new ItemDef
                {
                    Id = ItemId.Armorskin,
                    Name = "硬化药",
                    Category = ItemCategory.Buff,
                    Description = "出击开场提升防御 15%，持续整场。",
                    MaxStack = 3,
                    ShopPrice = 120,
                    UnlockHunterRank = 3,
                    DefenseBuffMul = 1.15f,
                    BuffDuration = 999f,
                    ActiveHuntOnly = true
                },
                new ItemDef
                {
                    Id = ItemId.Lifepowder,
                    Name = "生命之粉",
                    Category = ItemCategory.Heal,
                    Description = "一次性回复 55（道具流核心）。",
                    MaxStack = 3,
                    ShopPrice = 150,
                    UnlockHunterRank = 4,
                    HealAmount = 55f,
                    CraftZenny = 80,
                    CraftCost = new Dictionary<MaterialId, int>
                    {
                        { MaterialId.MonsterBone, 2 },
                        { MaterialId.MonsterHide, 1 }
                    },
                    ActiveHuntOnly = true
                },
                new ItemDef
                {
                    Id = ItemId.PitfallTrap,
                    Name = "落穴陷阱",
                    Category = ItemCategory.Trap,
                    Description = "陷阱流：使大型怪物定身并造成伤害。",
                    MaxStack = 2,
                    ShopPrice = 200,
                    UnlockHunterRank = 3,
                    TrapImmobilizeSeconds = 4.5f,
                    TrapDamage = 45f,
                    ActiveHuntOnly = true,
                    CraftZenny = 100,
                    CraftCost = new Dictionary<MaterialId, int>
                    {
                        { MaterialId.MonsterBone, 3 },
                        { MaterialId.MonsterHide, 2 }
                    }
                },
                new ItemDef
                {
                    Id = ItemId.ShockTrap,
                    Name = "麻痹陷阱",
                    Category = ItemCategory.Trap,
                    Description = "麻痹流：定身更久，伤害略低。",
                    MaxStack = 2,
                    ShopPrice = 220,
                    UnlockHunterRank = 4,
                    TrapImmobilizeSeconds = 5.5f,
                    TrapDamage = 35f,
                    ActiveHuntOnly = true,
                    CraftZenny = 120,
                    CraftCost = new Dictionary<MaterialId, int>
                    {
                        { MaterialId.SharpClaw, 2 },
                        { MaterialId.MonsterBone, 2 }
                    }
                },
                new ItemDef
                {
                    Id = ItemId.FlashBomb,
                    Name = "闪光弹",
                    Category = ItemCategory.Tool,
                    Description = "使怪物虚弱，短时间降低其伤害。",
                    MaxStack = 5,
                    ShopPrice = 80,
                    UnlockHunterRank = 2,
                    FlashWeakenSeconds = 5f,
                    FlashIncomingMul = 0.65f,
                    ActiveHuntOnly = true
                },
                new ItemDef
                {
                    Id = ItemId.BarrelBomb,
                    Name = "大桶炸弹",
                    Category = ItemCategory.Bomb,
                    Description = "睡眠/陷阱窗口爆发伤害。",
                    MaxStack = 2,
                    ShopPrice = 180,
                    UnlockHunterRank = 4,
                    BombDamage = 90f,
                    ActiveHuntOnly = true,
                    CraftZenny = 90,
                    CraftCost = new Dictionary<MaterialId, int>
                    {
                        { MaterialId.MonsterBone, 2 },
                        { MaterialId.SharpClaw, 1 }
                    }
                },
                new ItemDef
                {
                    Id = ItemId.TranqBomb,
                    Name = "麻醉球",
                    Category = ItemCategory.Tool,
                    Description = "讨伐成功时提高素材掉落概率。",
                    MaxStack = 8,
                    ShopPrice = 40,
                    UnlockHunterRank = 2,
                    ActiveHuntOnly = true
                },
                new ItemDef
                {
                    Id = ItemId.Paintball,
                    Name = "追踪玉",
                    Category = ItemCategory.Tool,
                    Description = "出击时增加地图熟练度获取。",
                    MaxStack = 10,
                    ShopPrice = 25,
                    ActiveHuntOnly = true
                }
            };
        }
    }
}
