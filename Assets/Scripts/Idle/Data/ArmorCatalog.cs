using System.Collections.Generic;

namespace MHIdle.Data
{
    public static class ArmorCatalog
    {
        public static List<ArmorDef> Build()
        {
            var list = new List<ArmorDef>();

            AddSet(list, "leather", "皮革", 1, 1, 4f, 12f, 0, null,
                PerSlot(
                    Skill(SkillId.Health, 3),
                    Skill(SkillId.Health, 3),
                    Skill(SkillId.Health, 3),
                    Skill(SkillId.Health, 3)));

            AddSet(list, "bone", "骨制", 2, 2, 8f, 24f, 600,
                CraftCosts.Of((MaterialId.MonsterBone, 4), (MaterialId.MonsterHide, 2)),
                PerSlot(
                    Skill(SkillId.Attack, 3),
                    Skill(SkillId.Attack, 3),
                    Skill(SkillId.Attack, 2),
                    Skill(SkillId.Attack, 3)));

            AddSet(list, "kutku", "怪鸟", 3, 3, 12f, 36f, 1600,
                CraftCosts.Of((MaterialId.MonsterBone, 5), (MaterialId.MonsterHide, 6), (MaterialId.SharpClaw, 2)),
                PerSlot(
                    Skills(Skill(SkillId.ItemUse, 3), Skill(SkillId.RecSpeed, 1)),
                    Skills(Skill(SkillId.ItemUse, 3), Skill(SkillId.RecSpeed, 1)),
                    Skills(Skill(SkillId.ItemUse, 2), Skill(SkillId.RecSpeed, 2)),
                    Skills(Skill(SkillId.ItemUse, 3), Skill(SkillId.RecSpeed, 2))));

            AddSet(list, "congalala", "桃毛兽", 3, 4, 14f, 38f, 2000,
                CraftCosts.Of((MaterialId.MonsterHide, 8), (MaterialId.Fang, 3)),
                PerSlot(
                    Skills(Skill(SkillId.ItemUse, 2), Skill(SkillId.Health, 2)),
                    Skills(Skill(SkillId.ItemUse, 3), Skill(SkillId.Health, 1)),
                    Skills(Skill(SkillId.ItemUse, 2), Skill(SkillId.Health, 2)),
                    Skills(Skill(SkillId.ItemUse, 3), Skill(SkillId.Health, 2))));

            AddSet(list, "gypceros", "毒怪鸟", 3, 4, 16f, 40f, 2400,
                CraftCosts.Of((MaterialId.MonsterHide, 8), (MaterialId.SharpClaw, 4)),
                PerSlot(
                    Skills(Skill(SkillId.Paralysis, 3), Skill(SkillId.TrapMaster, 1)),
                    Skills(Skill(SkillId.Paralysis, 3), Skill(SkillId.TrapMaster, 2)),
                    Skills(Skill(SkillId.Paralysis, 2), Skill(SkillId.TrapMaster, 3)),
                    Skills(Skill(SkillId.Paralysis, 2), Skill(SkillId.TrapMaster, 3))));

            AddSet(list, "hermitaur", "盾蟹", 4, 5, 20f, 46f, 3600,
                CraftCosts.Of((MaterialId.MonsterBone, 8), (MaterialId.MonsterScale, 6)),
                PerSlot(
                    Skills(Skill(SkillId.Guard, 3), Skill(SkillId.Defense, 2)),
                    Skills(Skill(SkillId.Guard, 3), Skill(SkillId.Defense, 2)),
                    Skills(Skill(SkillId.Guard, 2), Skill(SkillId.Defense, 3)),
                    Skills(Skill(SkillId.Guard, 3), Skill(SkillId.Defense, 2))));

            AddSet(list, "khezu", "电龙", 4, 6, 18f, 58f, 5200,
                CraftCosts.Of((MaterialId.MonsterHide, 10), (MaterialId.MonsterFluid, 5)),
                PerSlot(
                    Skills(Skill(SkillId.Health, 3), Skill(SkillId.Paralysis, 2)),
                    Skills(Skill(SkillId.Health, 3), Skill(SkillId.Paralysis, 1)),
                    Skills(Skill(SkillId.Health, 2), Skill(SkillId.Paralysis, 3)),
                    Skills(Skill(SkillId.Health, 3), Skill(SkillId.Paralysis, 2))));

            AddSet(list, "hypnoc", "眠鸟", 4, 6, 18f, 48f, 5000,
                CraftCosts.Of((MaterialId.MonsterHide, 10), (MaterialId.SharpClaw, 4), (MaterialId.MonsterBone, 6)),
                PerSlot(
                    Skills(Skill(SkillId.Sleep, 3), Skill(SkillId.Attack, 1)),
                    Skills(Skill(SkillId.Sleep, 3), Skill(SkillId.Attack, 2)),
                    Skills(Skill(SkillId.Sleep, 2), Skill(SkillId.Attack, 2)),
                    Skills(Skill(SkillId.Sleep, 3), Skill(SkillId.Attack, 1))));

            AddSet(list, "rathian", "雌火龙", 4, 7, 20f, 55f, 6500,
                CraftCosts.Of((MaterialId.SharpClaw, 5), (MaterialId.MonsterHide, 8), (MaterialId.WyvernGem, 1)),
                PerSlot(
                    Skills(Skill(SkillId.Poison, 3), Skill(SkillId.StatusAtk, 2)),
                    Skills(Skill(SkillId.Poison, 3), Skill(SkillId.StatusAtk, 2)),
                    Skills(Skill(SkillId.Poison, 2), Skill(SkillId.StatusAtk, 3)),
                    Skills(Skill(SkillId.Poison, 2), Skill(SkillId.StatusAtk, 3))));

            AddSet(list, "basarios", "岩龙", 4, 7, 22f, 50f, 4200,
                CraftCosts.Of((MaterialId.MonsterBone, 10), (MaterialId.WyvernGem, 1)),
                PerSlot(
                    Skills(Skill(SkillId.Defense, 3), Skill(SkillId.Guard, 2)),
                    Skills(Skill(SkillId.Defense, 3), Skill(SkillId.Guard, 2)),
                    Skills(Skill(SkillId.Defense, 2), Skill(SkillId.Guard, 3)),
                    Skills(Skill(SkillId.Defense, 3), Skill(SkillId.Guard, 2))));

            AddSet(list, "ceanataur", "镰蟹", 5, 8, 24f, 52f, 7800,
                CraftCosts.Of((MaterialId.SharpClaw, 8), (MaterialId.MonsterScale, 8), (MaterialId.Fang, 4)),
                PerSlot(
                    Skills(Skill(SkillId.Expert, 3), Skill(SkillId.Attack, 1)),
                    Skills(Skill(SkillId.Expert, 3), Skill(SkillId.Attack, 2)),
                    Skills(Skill(SkillId.Expert, 2), Skill(SkillId.Attack, 2)),
                    Skills(Skill(SkillId.Expert, 3), Skill(SkillId.Attack, 1))));

            AddSet(list, "rathalos", "火龙", 5, 8, 24f, 70f, 7000,
                CraftCosts.Of((MaterialId.SharpClaw, 6), (MaterialId.WyvernGem, 1), (MaterialId.MonsterHide, 8)),
                PerSlot(
                    Skills(Skill(SkillId.Attack, 3), Skill(SkillId.Expert, 2)),
                    Skills(Skill(SkillId.Attack, 2), Skill(SkillId.Expert, 3)),
                    Skills(Skill(SkillId.Attack, 2), Skill(SkillId.Expert, 3)),
                    Skills(Skill(SkillId.Attack, 3), Skill(SkillId.Expert, 2))));

            AddSet(list, "gravios", "铠龙", 5, 9, 30f, 80f, 9800,
                CraftCosts.Of((MaterialId.Plate, 1), (MaterialId.MonsterBone, 14), (MaterialId.WyvernGem, 2)),
                PerSlot(
                    Skills(Skill(SkillId.Defense, 4), Skill(SkillId.Guard, 2)),
                    Skills(Skill(SkillId.Defense, 3), Skill(SkillId.Guard, 3)),
                    Skills(Skill(SkillId.Defense, 3), Skill(SkillId.Guard, 3)),
                    Skills(Skill(SkillId.Defense, 4), Skill(SkillId.Guard, 2))));

            AddSet(list, "blangonga", "雪狮子", 5, 9, 26f, 72f, 9200,
                CraftCosts.Of((MaterialId.Fang, 8), (MaterialId.MonsterHide, 10), (MaterialId.Horn, 2)),
                PerSlot(
                    Skills(Skill(SkillId.Health, 3), Skill(SkillId.Attack, 2)),
                    Skills(Skill(SkillId.Health, 3), Skill(SkillId.Attack, 2)),
                    Skills(Skill(SkillId.Health, 2), Skill(SkillId.Attack, 3)),
                    Skills(Skill(SkillId.Health, 3), Skill(SkillId.Attack, 2))));

            AddSet(list, "diablos", "角龙", 6, 10, 28f, 64f, 12000,
                CraftCosts.Of((MaterialId.Horn, 5), (MaterialId.Fang, 8), (MaterialId.Plate, 1)),
                PerSlot(
                    Skills(Skill(SkillId.Attack, 4), Skill(SkillId.Expert, 1)),
                    Skills(Skill(SkillId.Attack, 3), Skill(SkillId.Expert, 2)),
                    Skills(Skill(SkillId.Attack, 3), Skill(SkillId.Expert, 2)),
                    Skills(Skill(SkillId.Attack, 4), Skill(SkillId.Expert, 1))));

            AddSet(list, "tigrex", "轰龙", 6, 10, 28f, 62f, 12500,
                CraftCosts.Of((MaterialId.Fang, 10), (MaterialId.SharpClaw, 8), (MaterialId.Plate, 1)),
                PerSlot(
                    Skills(Skill(SkillId.Attack, 4), Skill(SkillId.StatusAtk, 1)),
                    Skills(Skill(SkillId.Attack, 3), Skill(SkillId.StatusAtk, 2)),
                    Skills(Skill(SkillId.Attack, 3), Skill(SkillId.StatusAtk, 2)),
                    Skills(Skill(SkillId.Attack, 4), Skill(SkillId.StatusAtk, 1))));

            AddSet(list, "narga", "迅龙", 6, 11, 26f, 60f, 14000,
                CraftCosts.Of((MaterialId.Fang, 8), (MaterialId.Webbing, 6), (MaterialId.Plate, 1)),
                PerSlot(
                    Skills(Skill(SkillId.Evasion, 3), Skill(SkillId.Expert, 2)),
                    Skills(Skill(SkillId.Evasion, 3), Skill(SkillId.Expert, 2)),
                    Skills(Skill(SkillId.Evasion, 2), Skill(SkillId.Expert, 3)),
                    Skills(Skill(SkillId.Evasion, 3), Skill(SkillId.Expert, 2))));

            AddSet(list, "rajang", "金狮子", 7, 12, 32f, 74f, 18000,
                CraftCosts.Of((MaterialId.Horn, 4), (MaterialId.Fang, 12), (MaterialId.ElderDragonBlood, 1)),
                PerSlot(
                    Skills(Skill(SkillId.Attack, 4), Skill(SkillId.Expert, 2)),
                    Skills(Skill(SkillId.Attack, 3), Skill(SkillId.Expert, 3)),
                    Skills(Skill(SkillId.Attack, 3), Skill(SkillId.Expert, 3)),
                    Skills(Skill(SkillId.Attack, 4), Skill(SkillId.Expert, 2))));

            AddSet(list, "kushala", "钢龙", 7, 13, 34f, 86f, 21000,
                CraftCosts.Of((MaterialId.ElderDragonBlood, 2), (MaterialId.Plate, 2), (MaterialId.Webbing, 8)),
                PerSlot(
                    Skills(Skill(SkillId.Defense, 4), Skill(SkillId.Guard, 2)),
                    Skills(Skill(SkillId.Defense, 3), Skill(SkillId.Guard, 3)),
                    Skills(Skill(SkillId.Defense, 3), Skill(SkillId.Guard, 3)),
                    Skills(Skill(SkillId.Defense, 4), Skill(SkillId.Guard, 2))));

            AddSet(list, "teostra", "炎王", 7, 13, 34f, 78f, 21000,
                CraftCosts.Of((MaterialId.ElderDragonBlood, 2), (MaterialId.Horn, 3), (MaterialId.Plate, 2)),
                PerSlot(
                    Skills(Skill(SkillId.Attack, 4), Skill(SkillId.Expert, 2)),
                    Skills(Skill(SkillId.Attack, 3), Skill(SkillId.Expert, 3)),
                    Skills(Skill(SkillId.Attack, 3), Skill(SkillId.Expert, 3)),
                    Skills(Skill(SkillId.Attack, 4), Skill(SkillId.Expert, 2))));

            AddSet(list, "chameleos", "霞龙", 7, 14, 32f, 80f, 23000,
                CraftCosts.Of((MaterialId.ElderDragonBlood, 2), (MaterialId.MonsterFluid, 6), (MaterialId.Plate, 2)),
                PerSlot(
                    Skills(Skill(SkillId.StatusAtk, 3), Skill(SkillId.ItemUse, 2)),
                    Skills(Skill(SkillId.StatusAtk, 3), Skill(SkillId.ItemUse, 2)),
                    Skills(Skill(SkillId.StatusAtk, 2), Skill(SkillId.ItemUse, 3)),
                    Skills(Skill(SkillId.StatusAtk, 3), Skill(SkillId.ItemUse, 3))));

            AddSet(list, "kirin", "麒麟", 8, 14, 30f, 70f, 24000,
                CraftCosts.Of((MaterialId.ElderDragonBlood, 3), (MaterialId.Horn, 4), (MaterialId.WyvernGem, 3)),
                PerSlot(
                    Skills(Skill(SkillId.Expert, 4), Skill(SkillId.Paralysis, 1)),
                    Skills(Skill(SkillId.Expert, 3), Skill(SkillId.Paralysis, 2)),
                    Skills(Skill(SkillId.Expert, 3), Skill(SkillId.Paralysis, 2)),
                    Skills(Skill(SkillId.Expert, 4), Skill(SkillId.Paralysis, 1))));

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

        static void AddSet(
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
            var slots = new[] { ArmorSlot.Head, ArmorSlot.Chest, ArmorSlot.Arms, ArmorSlot.Legs };
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
    }
}
