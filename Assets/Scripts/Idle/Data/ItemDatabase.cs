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
        Potion,
        MegaPotion,
        WellDoneSteak,
        Antidote,
        HerbalMedicine,
        Nutrients,
        MaxPotion,
        AncientPotion,
        Demondrug,
        MegaDemondrug,
        Armorskin,
        MegaArmorskin,
        MightSeed,
        AdamantSeed,
        DashJuice,
        Lifepowder,
        PitfallTrap,
        ShockTrap,
        FlashBomb,
        SonicBomb,
        BarrelBomb,
        SmallBarrelBomb,
        MegaBarrelBomb,
        TranqBomb,
        Paintball,
        Farcaster
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

        public float HealAmount;
        public float HealPercent;
        public float AttackBuffMul = 1f;
        public float DefenseBuffMul = 1f;
        public float AttackIntervalMul = 1f;
        public float BuffDuration;
        public float TrapImmobilizeSeconds;
        public float TrapDamage;
        public float BombDamage;
        public float FlashWeakenSeconds;
        public float FlashIncomingMul = 1f;
        public bool BombNeedsImmobilize = true;
        public bool PreventDeathPenalty;
        public bool ActiveHuntOnly;
        public bool AutoUseInIdle;
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
                    Id = ItemId.Antidote,
                    Name = "解毒药",
                    Category = ItemCategory.Heal,
                    Description = "挂机也可自动服用，回复 22 并压住毒伤。",
                    MaxStack = 8,
                    ShopPrice = 25,
                    HealAmount = 22f,
                    AutoUseInIdle = true
                },
                new ItemDef
                {
                    Id = ItemId.HerbalMedicine,
                    Name = "药草",
                    Category = ItemCategory.Heal,
                    Description = "便宜续航，回复 48。",
                    MaxStack = 10,
                    ShopPrice = 45,
                    UnlockHunterRank = 2,
                    HealAmount = 48f,
                    CraftZenny = 20,
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
                    Id = ItemId.Nutrients,
                    Name = "营养剂",
                    Category = ItemCategory.Heal,
                    Description = "出击中段回复 55，偏续航。",
                    MaxStack = 5,
                    ShopPrice = 80,
                    UnlockHunterRank = 3,
                    HealAmount = 55f,
                    ActiveHuntOnly = true
                },
                new ItemDef
                {
                    Id = ItemId.MaxPotion,
                    Name = "回复药·特大",
                    Category = ItemCategory.Heal,
                    Description = "危急时回复全部 HP。仅出击。",
                    MaxStack = 2,
                    ShopPrice = 280,
                    UnlockHunterRank = 6,
                    HealPercent = 1f,
                    CraftZenny = 140,
                    CraftCost = new Dictionary<MaterialId, int>
                    {
                        { MaterialId.MonsterFluid, 2 },
                        { MaterialId.WyvernGem, 1 }
                    },
                    ActiveHuntOnly = true
                },
                new ItemDef
                {
                    Id = ItemId.AncientPotion,
                    Name = "古代秘药",
                    Category = ItemCategory.Buff,
                    Description = "开场回满并小幅提升攻防，持续整场。",
                    MaxStack = 1,
                    ShopPrice = 480,
                    UnlockHunterRank = 10,
                    HealPercent = 1f,
                    AttackBuffMul = 1.08f,
                    DefenseBuffMul = 1.08f,
                    BuffDuration = 999f,
                    CraftZenny = 220,
                    CraftCost = new Dictionary<MaterialId, int>
                    {
                        { MaterialId.ElderDragonBlood, 1 },
                        { MaterialId.Plate, 1 }
                    },
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
                    Id = ItemId.MegaDemondrug,
                    Name = "鬼人药·G",
                    Category = ItemCategory.Buff,
                    Description = "出击开场提升攻击 18%，持续整场。",
                    MaxStack = 2,
                    ShopPrice = 260,
                    UnlockHunterRank = 7,
                    AttackBuffMul = 1.18f,
                    BuffDuration = 999f,
                    CraftZenny = 120,
                    CraftCost = new Dictionary<MaterialId, int>
                    {
                        { MaterialId.Fang, 2 },
                        { MaterialId.MonsterFluid, 1 }
                    },
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
                    Id = ItemId.MegaArmorskin,
                    Name = "硬化药·G",
                    Category = ItemCategory.Buff,
                    Description = "出击开场提升防御 22%，持续整场。",
                    MaxStack = 2,
                    ShopPrice = 260,
                    UnlockHunterRank = 7,
                    DefenseBuffMul = 1.22f,
                    BuffDuration = 999f,
                    CraftZenny = 120,
                    CraftCost = new Dictionary<MaterialId, int>
                    {
                        { MaterialId.MonsterScale, 3 },
                        { MaterialId.MonsterBone, 2 }
                    },
                    ActiveHuntOnly = true
                },
                new ItemDef
                {
                    Id = ItemId.MightSeed,
                    Name = "怪力之种",
                    Category = ItemCategory.Buff,
                    Description = "便宜开场攻击 +8%。",
                    MaxStack = 5,
                    ShopPrice = 60,
                    UnlockHunterRank = 2,
                    AttackBuffMul = 1.08f,
                    BuffDuration = 999f,
                    ActiveHuntOnly = true
                },
                new ItemDef
                {
                    Id = ItemId.AdamantSeed,
                    Name = "忍耐之种",
                    Category = ItemCategory.Buff,
                    Description = "便宜开场防御 +10%。",
                    MaxStack = 5,
                    ShopPrice = 60,
                    UnlockHunterRank = 2,
                    DefenseBuffMul = 1.10f,
                    BuffDuration = 999f,
                    ActiveHuntOnly = true
                },
                new ItemDef
                {
                    Id = ItemId.DashJuice,
                    Name = "强走药",
                    Category = ItemCategory.Buff,
                    Description = "出击时攻速 +12%（缩短挥刀间隔）。",
                    MaxStack = 3,
                    ShopPrice = 180,
                    UnlockHunterRank = 5,
                    AttackIntervalMul = 0.88f,
                    BuffDuration = 999f,
                    CraftZenny = 80,
                    CraftCost = new Dictionary<MaterialId, int>
                    {
                        { MaterialId.MonsterFluid, 2 }
                    },
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
                    Id = ItemId.SonicBomb,
                    Name = "音爆弹",
                    Category = ItemCategory.Tool,
                    Description = "短眩晕 3.2s，受伤降至 78%。CD 略长。",
                    MaxStack = 5,
                    ShopPrice = 70,
                    UnlockHunterRank = 3,
                    FlashWeakenSeconds = 3.2f,
                    FlashIncomingMul = 0.78f,
                    ActiveHuntOnly = true,
                    CraftZenny = 40,
                    CraftCost = new Dictionary<MaterialId, int> { { MaterialId.MonsterScale, 1 } }
                },
                new ItemDef
                {
                    Id = ItemId.SmallBarrelBomb,
                    Name = "小桶炸弹",
                    Category = ItemCategory.Bomb,
                    Description = "无需定身即可引爆，伤害 36。CD≈18s。",
                    MaxStack = 4,
                    ShopPrice = 70,
                    UnlockHunterRank = 2,
                    BombDamage = 36f,
                    BombNeedsImmobilize = false,
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
                    Id = ItemId.MegaBarrelBomb,
                    Name = "大桶炸弹·G",
                    Category = ItemCategory.Bomb,
                    Description = "定身窗口内高爆 128；睡眠 Build 加成。CD≈20s。",
                    MaxStack = 1,
                    ShopPrice = 320,
                    UnlockHunterRank = 8,
                    BombDamage = 128f,
                    ActiveHuntOnly = true,
                    CraftZenny = 160,
                    CraftCost = new Dictionary<MaterialId, int>
                    {
                        { MaterialId.Fang, 2 },
                        { MaterialId.MonsterFluid, 2 },
                        { MaterialId.SharpClaw, 2 }
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
                },
                new ItemDef
                {
                    Id = ItemId.Farcaster,
                    Name = "返还烟",
                    Category = ItemCategory.Tool,
                    Description = "讨伐濒死时自动使用，免一次死亡惩罚并回营。",
                    MaxStack = 2,
                    ShopPrice = 220,
                    UnlockHunterRank = 4,
                    PreventDeathPenalty = true,
                    ActiveHuntOnly = true,
                    CraftZenny = 100,
                    CraftCost = new Dictionary<MaterialId, int>
                    {
                        { MaterialId.Webbing, 2 },
                        { MaterialId.MonsterHide, 2 }
                    }
                }
            };
        }
    }
}
