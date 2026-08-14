using System.Collections.Generic;

namespace MHIdle.Data
{
    public static class MonsterCatalog
    {
        public static List<MonsterDef> Build()
        {
            return new List<MonsterDef>
            {
                // —— 第 1 周：村任务气质小怪 + 首批大怪 ——
                M("kelbi", "凯欧比", MapId.ForestAndHills, MonsterSize.Small, 1, 150, 5, 0, 10, 2, 5,
                    (MaterialId.MonsterHide, 1, 2, 0.9f)),
                M("aptonoth", "草食龙", MapId.ForestAndHills, MonsterSize.Small, 1, 170, 6, 1, 12, 2, 5,
                    (MaterialId.MonsterHide, 1, 2, 1f),
                    (MaterialId.MonsterBone, 1, 1, 0.45f)),
                M("mosswine", "蘑菇猪", MapId.Swamp, MonsterSize.Small, 1, 165, 6, 1, 14, 2, 6,
                    (MaterialId.MonsterHide, 1, 2, 0.85f),
                    (MaterialId.MonsterFluid, 1, 1, 0.25f)),
                M("bullfango", "野猪", MapId.ForestAndHills, MonsterSize.Small, 1, 190, 8, 2, 16, 3, 6,
                    (MaterialId.MonsterBone, 1, 2, 1f),
                    (MaterialId.MonsterHide, 1, 1, 0.7f)),

                M("velociprey", "蓝速龙", MapId.Jungle, MonsterSize.Small, 2, 250, 10, 3, 20, 3, 7,
                    (MaterialId.MonsterBone, 1, 3, 1f),
                    (MaterialId.SharpClaw, 1, 1, 0.35f),
                    (MaterialId.MonsterScale, 1, 1, 0.4f)),
                M("genprey", "黄速龙", MapId.Desert, MonsterSize.Small, 2, 260, 11, 3, 22, 3, 7,
                    (MaterialId.MonsterHide, 1, 2, 1f),
                    (MaterialId.SharpClaw, 1, 1, 0.4f),
                    (MaterialId.Fang, 1, 1, 0.3f)),
                M("vespoid", "黄蜂", MapId.ForestAndHills, MonsterSize.Small, 2, 230, 12, 2, 18, 3, 6,
                    (MaterialId.MonsterFluid, 1, 2, 0.8f),
                    (MaterialId.MonsterScale, 1, 1, 0.35f)),
                M("hornetaur", "甲虫", MapId.Jungle, MonsterSize.Small, 2, 240, 11, 3, 19, 3, 6,
                    (MaterialId.MonsterBone, 1, 2, 0.9f),
                    (MaterialId.MonsterScale, 1, 2, 0.7f)),

                M("yian_kut_ku", "怪鸟", MapId.ForestAndHills, MonsterSize.Large, 3, 780, 20, 8, 180, 20, 24,
                    (MaterialId.MonsterHide, 2, 4, 1f),
                    (MaterialId.SharpClaw, 1, 2, 0.6f),
                    (MaterialId.Webbing, 1, 2, 0.45f)),

                M("ioprey", "红速龙", MapId.Volcano, MonsterSize.Small, 3, 340, 14, 4, 26, 4, 8,
                    (MaterialId.Fang, 1, 2, 0.7f),
                    (MaterialId.MonsterScale, 1, 2, 0.8f),
                    (MaterialId.MonsterFluid, 1, 1, 0.3f)),
                M("remobra", "翼蛇龙", MapId.Tower, MonsterSize.Small, 3, 360, 15, 4, 28, 4, 8,
                    (MaterialId.Webbing, 1, 2, 0.75f),
                    (MaterialId.MonsterScale, 1, 2, 0.6f)),

                M("gypceros", "毒怪鸟", MapId.Swamp, MonsterSize.Large, 4, 980, 24, 10, 230, 26, 30,
                    (MaterialId.MonsterHide, 2, 5, 1f),
                    (MaterialId.SharpClaw, 1, 3, 0.55f),
                    (MaterialId.MonsterFluid, 1, 2, 0.4f)),
                M("congalala", "桃毛兽王", MapId.Jungle, MonsterSize.Large, 4, 1020, 25, 9, 240, 26, 30,
                    (MaterialId.MonsterHide, 3, 6, 1f),
                    (MaterialId.Fang, 1, 3, 0.7f),
                    (MaterialId.MonsterBone, 2, 4, 0.8f)),

                M("cephalos", "沙鱼", MapId.Desert, MonsterSize.Small, 4, 400, 16, 5, 30, 4, 9,
                    (MaterialId.MonsterScale, 1, 3, 1f),
                    (MaterialId.Fang, 1, 1, 0.4f)),
                M("conga", "桃毛兽", MapId.Jungle, MonsterSize.Small, 4, 380, 15, 4, 28, 4, 8,
                    (MaterialId.MonsterHide, 1, 3, 1f),
                    (MaterialId.Fang, 1, 1, 0.5f)),

                M("daimyo", "大名蟹", MapId.Desert, MonsterSize.Large, 5, 1280, 28, 14, 300, 32, 34,
                    (MaterialId.MonsterBone, 3, 6, 1f),
                    (MaterialId.MonsterScale, 2, 4, 0.8f),
                    (MaterialId.SharpClaw, 1, 2, 0.5f)),
                M("cephadrome", "沙龙王", MapId.Desert, MonsterSize.Large, 5, 1320, 30, 12, 310, 32, 34,
                    (MaterialId.MonsterScale, 3, 6, 1f),
                    (MaterialId.Fang, 2, 4, 0.7f),
                    (MaterialId.MonsterFluid, 1, 2, 0.4f)),
                M("blue_kutku", "青怪鸟", MapId.ForestAndHills, MonsterSize.Large, 5, 1180, 29, 11, 290, 30, 32,
                    (MaterialId.MonsterHide, 3, 5, 1f),
                    (MaterialId.Webbing, 1, 3, 0.55f),
                    (MaterialId.SharpClaw, 1, 3, 0.5f)),

                M("hermitaur", "盾蟹", MapId.Desert, MonsterSize.Small, 5, 520, 18, 8, 34, 5, 10,
                    (MaterialId.MonsterBone, 2, 3, 1f),
                    (MaterialId.MonsterScale, 1, 2, 0.7f)),
                M("giaprey", "白速龙", MapId.SnowyMountains, MonsterSize.Small, 5, 500, 17, 6, 32, 5, 10,
                    (MaterialId.MonsterHide, 1, 3, 1f),
                    (MaterialId.Fang, 1, 2, 0.5f)),

                // —— 第 2 周：第二武器线 + 火龙线 ——
                M("khezu", "电龙", MapId.Swamp, MonsterSize.Large, 6, 1680, 34, 14, 380, 40, 40,
                    (MaterialId.MonsterHide, 3, 6, 1f),
                    (MaterialId.MonsterFluid, 2, 4, 0.8f),
                    (MaterialId.WyvernGem, 1, 1, 0.12f)),
                M("hypnocatrice", "眠鸟", MapId.GreatForest, MonsterSize.Large, 6, 1720, 33, 13, 390, 40, 40,
                    (MaterialId.MonsterHide, 3, 6, 1f),
                    (MaterialId.Webbing, 2, 4, 0.55f),
                    (MaterialId.MonsterBone, 2, 4, 0.7f)),

                M("blango", "雪狮子", MapId.SnowyMountains, MonsterSize.Small, 6, 620, 20, 8, 38, 5, 11,
                    (MaterialId.MonsterHide, 2, 3, 1f),
                    (MaterialId.Fang, 1, 2, 0.55f)),

                M("rathian", "雌火龙", MapId.ForestAndHills, MonsterSize.Large, 7, 2200, 40, 16, 460, 48, 46,
                    (MaterialId.SharpClaw, 2, 5, 1f),
                    (MaterialId.WyvernGem, 1, 1, 0.22f),
                    (MaterialId.MonsterHide, 3, 6, 1f),
                    (MaterialId.Webbing, 1, 3, 0.5f)),
                M("basarios", "岩龙", MapId.Volcano, MonsterSize.Large, 7, 2400, 38, 20, 440, 46, 44,
                    (MaterialId.MonsterBone, 3, 7, 1f),
                    (MaterialId.WyvernGem, 1, 1, 0.18f),
                    (MaterialId.Plate, 1, 1, 0.08f)),

                M("ioprey_volcano", "红速龙（火山）", MapId.Volcano, MonsterSize.Small, 7, 720, 24, 10, 44, 6, 12,
                    (MaterialId.Fang, 1, 3, 0.8f),
                    (MaterialId.MonsterScale, 2, 3, 1f),
                    (MaterialId.MonsterFluid, 1, 2, 0.4f)),

                M("shogun", "将军镰蟹", MapId.Swamp, MonsterSize.Large, 8, 2800, 46, 22, 560, 54, 50,
                    (MaterialId.SharpClaw, 3, 6, 1f),
                    (MaterialId.MonsterScale, 3, 5, 0.85f),
                    (MaterialId.Plate, 1, 1, 0.1f)),
                M("plesioth", "水龙", MapId.Jungle, MonsterSize.Large, 8, 3000, 44, 18, 580, 54, 50,
                    (MaterialId.MonsterScale, 4, 7, 1f),
                    (MaterialId.Webbing, 2, 4, 0.6f),
                    (MaterialId.MonsterFluid, 2, 4, 0.7f)),
                M("rathalos", "火龙", MapId.ForestAndHills, MonsterSize.Large, 8, 3200, 50, 20, 640, 58, 54,
                    (MaterialId.SharpClaw, 3, 6, 1f),
                    (MaterialId.WyvernGem, 1, 2, 0.32f),
                    (MaterialId.Webbing, 2, 4, 0.7f),
                    (MaterialId.ElderDragonBlood, 1, 1, 0.06f)),

                M("remobra_high", "翼蛇龙（高地）", MapId.Tower, MonsterSize.Small, 8, 860, 26, 12, 50, 7, 13,
                    (MaterialId.Webbing, 2, 3, 0.9f),
                    (MaterialId.MonsterScale, 2, 3, 0.8f),
                    (MaterialId.Fang, 1, 2, 0.4f)),

                // —— 第 3 周：多流派 / 高难飞龙 ——
                M("gravios", "铠龙", MapId.Volcano, MonsterSize.Large, 9, 4000, 52, 28, 760, 66, 58,
                    (MaterialId.MonsterBone, 4, 8, 1f),
                    (MaterialId.Plate, 1, 1, 0.2f),
                    (MaterialId.WyvernGem, 1, 2, 0.3f)),
                M("blangonga", "雪狮子王", MapId.SnowyMountains, MonsterSize.Large, 9, 3600, 56, 22, 740, 64, 56,
                    (MaterialId.Fang, 3, 6, 1f),
                    (MaterialId.MonsterHide, 4, 7, 1f),
                    (MaterialId.Horn, 1, 2, 0.45f)),

                M("blango_high", "雪狮子（冻土）", MapId.SnowyMountains, MonsterSize.Small, 9, 980, 28, 14, 56, 7, 14,
                    (MaterialId.MonsterHide, 2, 4, 1f),
                    (MaterialId.Fang, 1, 3, 0.65f)),

                M("diablos", "角龙", MapId.Desert, MonsterSize.Large, 10, 4800, 62, 26, 900, 74, 64,
                    (MaterialId.Horn, 2, 4, 0.8f),
                    (MaterialId.Fang, 3, 6, 1f),
                    (MaterialId.Plate, 1, 1, 0.18f)),
                M("tigrex", "轰龙", MapId.Gorge, MonsterSize.Large, 10, 5000, 68, 24, 940, 76, 66,
                    (MaterialId.Fang, 4, 7, 1f),
                    (MaterialId.SharpClaw, 3, 6, 1f),
                    (MaterialId.Plate, 1, 1, 0.2f)),

                M("nargacuga", "迅龙", MapId.GreatForest, MonsterSize.Large, 11, 5400, 66, 22, 1020, 82, 70,
                    (MaterialId.Fang, 3, 6, 1f),
                    (MaterialId.Webbing, 3, 5, 0.75f),
                    (MaterialId.Plate, 1, 1, 0.22f),
                    (MaterialId.WyvernGem, 1, 1, 0.28f)),

                M("giaprey_high", "白速龙（冻土）", MapId.SnowyMountains, MonsterSize.Small, 11, 1120, 32, 16, 64, 8, 15,
                    (MaterialId.MonsterHide, 2, 4, 1f),
                    (MaterialId.Fang, 2, 3, 0.7f),
                    (MaterialId.MonsterScale, 1, 2, 0.5f)),

                // —— 第 4 周：古龙 / 高难 ——
                M("rajang", "金狮子", MapId.Tower, MonsterSize.Large, 12, 6800, 78, 28, 1280, 96, 80,
                    (MaterialId.Horn, 2, 4, 0.7f),
                    (MaterialId.Fang, 4, 8, 1f),
                    (MaterialId.ElderDragonBlood, 1, 1, 0.2f),
                    (MaterialId.Plate, 1, 1, 0.25f)),

                M("kushala", "钢龙", MapId.SnowyMountains, MonsterSize.Large, 13, 8200, 74, 32, 1500, 108, 88,
                    (MaterialId.WyvernGem, 1, 2, 0.7f),
                    (MaterialId.ElderDragonBlood, 1, 2, 0.5f),
                    (MaterialId.Webbing, 4, 8, 1f),
                    (MaterialId.Plate, 1, 2, 0.35f)),
                M("teostra", "炎王龙", MapId.Volcano, MonsterSize.Large, 13, 8400, 80, 30, 1540, 110, 90,
                    (MaterialId.Horn, 2, 4, 0.65f),
                    (MaterialId.ElderDragonBlood, 1, 2, 0.5f),
                    (MaterialId.Plate, 1, 2, 0.35f),
                    (MaterialId.WyvernGem, 1, 2, 0.6f)),

                M("chameleos", "霞龙", MapId.GreatForest, MonsterSize.Large, 14, 9000, 72, 28, 1680, 118, 94,
                    (MaterialId.MonsterFluid, 3, 6, 0.9f),
                    (MaterialId.ElderDragonBlood, 1, 2, 0.55f),
                    (MaterialId.Plate, 1, 2, 0.4f),
                    (MaterialId.Webbing, 3, 6, 0.7f)),
                M("kirin", "麒麟", MapId.Tower, MonsterSize.Large, 14, 7600, 84, 26, 1720, 120, 96,
                    (MaterialId.Horn, 2, 4, 0.8f),
                    (MaterialId.ElderDragonBlood, 1, 2, 0.6f),
                    (MaterialId.WyvernGem, 2, 3, 0.75f)),

                M("lao_shan", "老山龙", MapId.Tower, MonsterSize.Large, 15, 12000, 70, 36, 2100, 140, 110,
                    (MaterialId.Plate, 1, 3, 0.7f),
                    (MaterialId.ElderDragonBlood, 2, 3, 0.65f),
                    (MaterialId.MonsterBone, 6, 12, 1f),
                    (MaterialId.WyvernGem, 2, 4, 0.8f)),
                M("fatalis", "黑龙", MapId.Tower, MonsterSize.Large, 16, 16000, 92, 40, 2800, 180, 140,
                    (MaterialId.ElderDragonBlood, 2, 4, 0.85f),
                    (MaterialId.Plate, 2, 3, 0.7f),
                    (MaterialId.WyvernGem, 2, 4, 0.9f),
                    (MaterialId.Horn, 2, 4, 0.6f))
            };
        }

        static MonsterDef M(
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
