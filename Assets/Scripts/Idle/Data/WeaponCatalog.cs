using System.Collections.Generic;

namespace MHIdle.Data
{
    public static class CraftCosts
    {
        public static Dictionary<MaterialId, int> Of(params (MaterialId id, int n)[] xs)
        {
            var d = new Dictionary<MaterialId, int>();
            if (xs == null) return d;
            foreach (var x in xs) d[x.id] = x.n;
            return d;
        }
    }

    public static class WeaponCatalog
    {
        public static List<WeaponDef> Build()
        {
            return new List<WeaponDef>
            {
                // —— 大剑（保留既有 id）——
                W("gs_buster", "爆破大剑", WeaponType.GreatSword, 1, 28, 2.45f, 1, 0),
                W("gs_jagras", "大贼龙大剑", WeaponType.GreatSword, 2, 42, 2.38f, 2, 800,
                    CraftCosts.Of((MaterialId.MonsterBone, 8), (MaterialId.MonsterHide, 6))),
                W("gs_anjanath", "蛮颚龙大剑", WeaponType.GreatSword, 3, 68, 2.30f, 5, 3200,
                    CraftCosts.Of((MaterialId.MonsterBone, 12), (MaterialId.SharpClaw, 8), (MaterialId.MonsterHide, 10))),
                W("gs_rathalos", "火龙大剑", WeaponType.GreatSword, 4, 96, 2.22f, 8, 9000,
                    CraftCosts.Of((MaterialId.SharpClaw, 14), (MaterialId.WyvernGem, 3), (MaterialId.MonsterHide, 16))),
                W("gs_nergigante", "灭尽龙大剑", WeaponType.GreatSword, 5, 135, 2.10f, 12, 22000,
                    CraftCosts.Of((MaterialId.WyvernGem, 6), (MaterialId.ElderDragonBlood, 4), (MaterialId.SharpClaw, 20))),

                // —— 太刀 ——
                W("ls_iron", "铁刀", WeaponType.LongSword, 1, 22, 1.55f, 1, 200),
                W("ls_kutku", "怪鸟刀", WeaponType.LongSword, 2, 34, 1.50f, 3, 900,
                    CraftCosts.Of((MaterialId.MonsterBone, 6), (MaterialId.MonsterScale, 4))),
                W("ls_rathian", "雌火龙太刀", WeaponType.LongSword, 3, 72, 1.45f, 6, 7800,
                    CraftCosts.Of((MaterialId.SharpClaw, 10), (MaterialId.WyvernGem, 2), (MaterialId.MonsterHide, 12))),
                W("ls_rathalos", "火龙太刀", WeaponType.LongSword, 4, 78, 1.42f, 8, 8500,
                    CraftCosts.Of((MaterialId.SharpClaw, 12), (MaterialId.WyvernGem, 2), (MaterialId.MonsterHide, 14))),
                W("ls_kushala", "钢龙刀", WeaponType.LongSword, 5, 110, 1.35f, 13, 24000,
                    CraftCosts.Of((MaterialId.ElderDragonBlood, 3), (MaterialId.Plate, 2), (MaterialId.Webbing, 8))),

                // —— 单手剑 ——
                W("sns_hunter", "猎人单手剑", WeaponType.SwordAndShield, 1, 18, 1.35f, 1, 180),
                W("sns_bone", "骨制单手剑", WeaponType.SwordAndShield, 2, 28, 1.30f, 3, 700,
                    CraftCosts.Of((MaterialId.MonsterBone, 6), (MaterialId.MonsterHide, 3))),
                W("sns_kutku", "怪鸟单手剑", WeaponType.SwordAndShield, 3, 44, 1.25f, 5, 2400,
                    CraftCosts.Of((MaterialId.MonsterScale, 6), (MaterialId.MonsterHide, 5))),
                W("sns_rathian", "雌火龙单手剑", WeaponType.SwordAndShield, 4, 64, 1.20f, 8, 8200,
                    CraftCosts.Of((MaterialId.MonsterScale, 10), (MaterialId.Webbing, 4), (MaterialId.WyvernGem, 1))),
                W("sns_narga", "迅龙单手剑", WeaponType.SwordAndShield, 5, 90, 1.15f, 11, 18000,
                    CraftCosts.Of((MaterialId.Fang, 8), (MaterialId.Plate, 2), (MaterialId.SharpClaw, 10))),

                // —— 双剑 ——
                W("db_hunter", "猎人双剑", WeaponType.DualBlades, 1, 16, 0.95f, 1, 180),
                W("db_bone", "骨制双剑", WeaponType.DualBlades, 2, 26, 0.92f, 3, 720,
                    CraftCosts.Of((MaterialId.MonsterBone, 8), (MaterialId.Fang, 2))),
                W("db_gypceros", "毒怪鸟双剑", WeaponType.DualBlades, 3, 40, 0.88f, 5, 2600,
                    CraftCosts.Of((MaterialId.MonsterHide, 8), (MaterialId.MonsterFluid, 3))),
                W("db_narga", "迅龙双剑", WeaponType.DualBlades, 4, 58, 0.85f, 11, 16000,
                    CraftCosts.Of((MaterialId.Fang, 10), (MaterialId.SharpClaw, 8), (MaterialId.Plate, 1))),
                W("db_rajang", "金狮子双剑", WeaponType.DualBlades, 5, 82, 0.80f, 12, 21000,
                    CraftCosts.Of((MaterialId.Horn, 4), (MaterialId.Fang, 12), (MaterialId.ElderDragonBlood, 2))),

                // —— 大锤 ——
                W("hm_iron", "铁锤", WeaponType.Hammer, 1, 32, 2.60f, 1, 220),
                W("hm_bone", "骨锤", WeaponType.Hammer, 2, 48, 2.52f, 3, 850,
                    CraftCosts.Of((MaterialId.MonsterBone, 10), (MaterialId.Horn, 1))),
                W("hm_basarios", "岩龙锤", WeaponType.Hammer, 3, 74, 2.45f, 7, 5200,
                    CraftCosts.Of((MaterialId.MonsterBone, 14), (MaterialId.WyvernGem, 1))),
                W("hm_diablos", "角龙锤", WeaponType.Hammer, 4, 105, 2.38f, 10, 14000,
                    CraftCosts.Of((MaterialId.Horn, 6), (MaterialId.Fang, 8), (MaterialId.Plate, 1))),
                W("hm_gravios", "铠龙锤", WeaponType.Hammer, 5, 148, 2.28f, 12, 20000,
                    CraftCosts.Of((MaterialId.Plate, 3), (MaterialId.MonsterBone, 18), (MaterialId.WyvernGem, 3))),

                // —— 狩猎笛 ——
                W("hh_metal", "金属笛", WeaponType.HuntingHorn, 1, 24, 2.10f, 1, 240),
                W("hh_bone", "骨笛", WeaponType.HuntingHorn, 2, 38, 2.04f, 3, 880,
                    CraftCosts.Of((MaterialId.MonsterBone, 8), (MaterialId.MonsterHide, 4))),
                W("hh_congalala", "桃毛兽笛", WeaponType.HuntingHorn, 3, 58, 1.98f, 5, 2800,
                    CraftCosts.Of((MaterialId.MonsterHide, 10), (MaterialId.Fang, 4))),
                W("hh_khezu", "电龙笛", WeaponType.HuntingHorn, 4, 84, 1.92f, 7, 9000,
                    CraftCosts.Of((MaterialId.MonsterFluid, 6), (MaterialId.MonsterHide, 10), (MaterialId.WyvernGem, 1))),
                W("hh_teostra", "炎王笛", WeaponType.HuntingHorn, 5, 118, 1.85f, 13, 25000,
                    CraftCosts.Of((MaterialId.ElderDragonBlood, 3), (MaterialId.Horn, 4), (MaterialId.Plate, 2))),

                // —— 长枪 ——
                W("ln_iron", "铁枪", WeaponType.Lance, 1, 20, 1.70f, 1, 200),
                W("ln_bone", "骨枪", WeaponType.Lance, 2, 32, 1.64f, 3, 760,
                    CraftCosts.Of((MaterialId.MonsterBone, 8), (MaterialId.MonsterHide, 3))),
                W("ln_basarios", "岩龙枪", WeaponType.Lance, 3, 50, 1.58f, 7, 4800,
                    CraftCosts.Of((MaterialId.MonsterBone, 12), (MaterialId.MonsterScale, 6))),
                W("ln_rathalos", "火龙枪", WeaponType.Lance, 4, 72, 1.52f, 8, 9200,
                    CraftCosts.Of((MaterialId.Webbing, 6), (MaterialId.SharpClaw, 8), (MaterialId.WyvernGem, 2))),
                W("ln_kushala", "钢龙枪", WeaponType.Lance, 5, 102, 1.45f, 13, 23000,
                    CraftCosts.Of((MaterialId.ElderDragonBlood, 3), (MaterialId.Plate, 2), (MaterialId.Webbing, 8))),

                // —— 铳枪 ——
                W("gl_iron", "铁铳枪", WeaponType.Gunlance, 1, 22, 1.90f, 1, 240),
                W("gl_bone", "骨铳枪", WeaponType.Gunlance, 2, 34, 1.84f, 3, 820,
                    CraftCosts.Of((MaterialId.MonsterBone, 8), (MaterialId.Fang, 2))),
                W("gl_rathian", "雌火龙铳枪", WeaponType.Gunlance, 3, 54, 1.78f, 7, 5400,
                    CraftCosts.Of((MaterialId.MonsterScale, 8), (MaterialId.Webbing, 4), (MaterialId.MonsterFluid, 2))),
                W("gl_gravios", "铠龙铳枪", WeaponType.Gunlance, 4, 78, 1.72f, 9, 12000,
                    CraftCosts.Of((MaterialId.Plate, 2), (MaterialId.MonsterBone, 14), (MaterialId.WyvernGem, 2))),
                W("gl_teostra", "炎王铳枪", WeaponType.Gunlance, 5, 110, 1.65f, 13, 24000,
                    CraftCosts.Of((MaterialId.ElderDragonBlood, 3), (MaterialId.Horn, 3), (MaterialId.Plate, 2))),

                // —— 弓 ——
                W("bow_hunter", "猎人弓", WeaponType.Bow, 1, 19, 1.40f, 1, 200),
                W("bow_kutku", "怪鸟弓", WeaponType.Bow, 2, 30, 1.35f, 3, 800,
                    CraftCosts.Of((MaterialId.MonsterHide, 6), (MaterialId.Webbing, 2))),
                W("bow_rathian", "雌火龙弓", WeaponType.Bow, 3, 46, 1.30f, 7, 5600,
                    CraftCosts.Of((MaterialId.Webbing, 6), (MaterialId.MonsterScale, 8))),
                W("bow_rathalos", "火龙弓", WeaponType.Bow, 4, 68, 1.25f, 8, 9800,
                    CraftCosts.Of((MaterialId.Webbing, 8), (MaterialId.SharpClaw, 8), (MaterialId.WyvernGem, 2))),
                W("bow_narga", "迅龙弓", WeaponType.Bow, 5, 96, 1.18f, 11, 19000,
                    CraftCosts.Of((MaterialId.Fang, 10), (MaterialId.Webbing, 8), (MaterialId.Plate, 2))),

                // —— 轻弩 ——
                W("lbg_hunter", "猎人轻弩", WeaponType.LightBowgun, 1, 17, 1.25f, 1, 220),
                W("lbg_bone", "骨轻弩", WeaponType.LightBowgun, 2, 28, 1.20f, 3, 780,
                    CraftCosts.Of((MaterialId.MonsterBone, 6), (MaterialId.MonsterHide, 4))),
                W("lbg_khezu", "电龙轻弩", WeaponType.LightBowgun, 3, 44, 1.15f, 7, 5800,
                    CraftCosts.Of((MaterialId.MonsterFluid, 6), (MaterialId.MonsterHide, 8))),
                W("lbg_rathalos", "火龙轻弩", WeaponType.LightBowgun, 4, 64, 1.10f, 8, 9600,
                    CraftCosts.Of((MaterialId.Webbing, 6), (MaterialId.SharpClaw, 8), (MaterialId.WyvernGem, 2))),
                W("lbg_chameleos", "霞龙轻弩", WeaponType.LightBowgun, 5, 90, 1.05f, 14, 26000,
                    CraftCosts.Of((MaterialId.ElderDragonBlood, 3), (MaterialId.Plate, 2), (MaterialId.MonsterFluid, 6))),

                // —— 重弩 ——
                W("hbg_hunter", "猎人重弩", WeaponType.HeavyBowgun, 1, 30, 2.20f, 1, 260),
                W("hbg_bone", "骨重弩", WeaponType.HeavyBowgun, 2, 46, 2.12f, 3, 900,
                    CraftCosts.Of((MaterialId.MonsterBone, 10), (MaterialId.MonsterHide, 4))),
                W("hbg_gravios", "铠龙重弩", WeaponType.HeavyBowgun, 3, 70, 2.05f, 9, 11000,
                    CraftCosts.Of((MaterialId.Plate, 2), (MaterialId.MonsterBone, 16))),
                W("hbg_diablos", "角龙重弩", WeaponType.HeavyBowgun, 4, 100, 1.98f, 10, 15000,
                    CraftCosts.Of((MaterialId.Horn, 6), (MaterialId.Fang, 8), (MaterialId.WyvernGem, 2))),
                W("hbg_kushala", "钢龙重弩", WeaponType.HeavyBowgun, 5, 140, 1.90f, 13, 27000,
                    CraftCosts.Of((MaterialId.ElderDragonBlood, 4), (MaterialId.Plate, 3), (MaterialId.Webbing, 8)))
            };
        }

        static WeaponDef W(
            string id,
            string name,
            WeaponType type,
            int tier,
            float damage,
            float interval,
            int hr,
            int zenny,
            Dictionary<MaterialId, int> cost = null)
        {
            return new WeaponDef
            {
                Id = id,
                Name = name,
                Type = type,
                Tier = tier,
                BaseDamage = damage,
                AttackInterval = interval,
                UnlockHunterRank = hr,
                CraftZenny = zenny,
                CraftCost = cost ?? new Dictionary<MaterialId, int>()
            };
        }
    }
}
