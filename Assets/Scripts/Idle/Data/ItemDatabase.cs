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
                    Description = "HP 约四成以下自动服用，回复 35 点。",
                    MaxStack = 10,
                    ShopPrice = 30,
                    HealAmount = 35f,
                    AutoUseInIdle = true
                },
                new ItemDef
                {
                    Id = ItemId.MegaPotion,
                    Name = "回复药·大",
                    Category = ItemCategory.Heal,
                    Description = "危急（约三成 HP）服用，回复 70 点。",
                    MaxStack = 5,
                    ShopPrice = 90,
                    UnlockHunterRank = 2,
                    HealAmount = 70f,
                    CraftZenny = 40,
                    CraftCost = new Dictionary<MaterialId, int> { { MaterialId.MonsterHide, 1 } },
                    AutoUseInIdle = true
                },
                new ItemDef
                {
                    Id = ItemId.WellDoneSteak,
                    Name = "熟肉",
                    Category = ItemCategory.Heal,
                    Description = "出击用：回复 45，偏续航而非急救。",
                    MaxStack = 5,
                    ShopPrice = 50,
                    HealAmount = 45f,
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
                    Description = "道具流核心：一次性回复 60。",
                    MaxStack = 3,
                    ShopPrice = 150,
                    UnlockHunterRank = 4,
                    HealAmount = 60f,
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
                    Description = "定身 5.0s 并造成少量伤害；偏开窗，不靠陷阱本身清血。CD≈14s。",
                    MaxStack = 2,
                    ShopPrice = 200,
                    UnlockHunterRank = 3,
                    TrapImmobilizeSeconds = 5.0f,
                    TrapDamage = 32f,
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
                    Description = "定身更久（6.5s），伤害更低；适合接炸弹/输出。CD≈14s。",
                    MaxStack = 2,
                    ShopPrice = 220,
                    UnlockHunterRank = 4,
                    TrapImmobilizeSeconds = 6.5f,
                    TrapDamage = 18f,
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
                    Description = "怪物虚弱 4.5s，受到伤害降至 70%。CD≈10s。",
                    MaxStack = 5,
                    ShopPrice = 80,
                    UnlockHunterRank = 2,
                    FlashWeakenSeconds = 4.5f,
                    FlashIncomingMul = 0.70f,
                    ActiveHuntOnly = true
                },
                new ItemDef
                {
                    Id = ItemId.BarrelBomb,
                    Name = "大桶炸弹",
                    Category = ItemCategory.Bomb,
                    Description = "仅在陷阱定身窗口引爆，基础伤害 72；睡眠 Build ×1.25。CD≈16s。",
                    MaxStack = 2,
                    ShopPrice = 180,
                    UnlockHunterRank = 4,
                    BombDamage = 72f,
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
