using System.Collections.Generic;
using System.Text;
using MHIdle.Data;
using MHIdle.Model;
using UnityEngine;

namespace MHIdle.Systems
{
    public enum ItemActionResult
    {
        Success,
        Failed,
        NotOwned,
        LoadoutFull,
        StackFull,
        Locked,
        NotEnoughZenny,
        NotEnoughMaterials,
        Invalid
    }

    /// <summary>
    /// 道具购买/制造、仓库↔出征背包（最多 10 种）、战斗自动使用。
    /// </summary>
    public static class ItemSystem
    {
        public static string Describe(ItemActionResult result)
        {
            switch (result)
            {
                case ItemActionResult.Success: return "成功";
                case ItemActionResult.NotOwned: return "仓库中没有该道具";
                case ItemActionResult.LoadoutFull: return "出征背包已满（最多 10 种）";
                case ItemActionResult.StackFull: return "该道具堆叠已满";
                case ItemActionResult.Locked: return "猎人等级不足";
                case ItemActionResult.NotEnoughZenny: return "金币不足";
                case ItemActionResult.NotEnoughMaterials: return "素材不足";
                default: return "无法完成";
            }
        }

        public static ItemActionResult Buy(HunterProgress progress, ItemId id, int amount = 1)
        {
            var def = ItemDatabase.Get(id);
            if (def == null || amount <= 0) return ItemActionResult.Invalid;
            if (progress.HunterRank < def.UnlockHunterRank) return ItemActionResult.Locked;

            int cost = def.ShopPrice * amount;
            if (progress.Zenny < cost) return ItemActionResult.NotEnoughZenny;

            int have = progress.GetItem(id);
            if (have + amount > def.MaxStack * 5) // 仓库总上限：5 组
                return ItemActionResult.StackFull;

            progress.Zenny -= cost;
            progress.AddItem(id, amount);
            return ItemActionResult.Success;
        }

        public static ItemActionResult Craft(HunterProgress progress, ItemId id, int amount = 1)
        {
            var def = ItemDatabase.Get(id);
            if (def == null || amount <= 0) return ItemActionResult.Invalid;
            if (def.CraftCost == null || def.CraftCost.Count == 0 && def.CraftZenny <= 0)
                return ItemActionResult.Invalid;
            if (progress.HunterRank < def.UnlockHunterRank) return ItemActionResult.Locked;

            int zenny = def.CraftZenny * amount;
            var cost = new Dictionary<MaterialId, int>();
            foreach (var pair in def.CraftCost)
                cost[pair.Key] = pair.Value * amount;

            if (!progress.CanAfford(zenny, cost)) return ItemActionResult.NotEnoughMaterials;
            if (!progress.Spend(zenny, cost)) return ItemActionResult.NotEnoughMaterials;

            progress.AddItem(id, amount);
            return ItemActionResult.Success;
        }

        public static ItemActionResult PackToLoadout(HunterProgress progress, ItemId id, int amount = 1)
        {
            var def = ItemDatabase.Get(id);
            if (def == null || amount <= 0) return ItemActionResult.Invalid;

            int warehouse = progress.GetItem(id);
            if (warehouse < amount) return ItemActionResult.NotOwned;

            string key = id.ToString();
            int inLoadout = progress.GetLoadoutCount(id);
            if (inLoadout + amount > def.MaxStack) return ItemActionResult.StackFull;

            if (!progress.Loadout.ContainsKey(key) && progress.Loadout.Count >= HunterProgress.MaxLoadoutSlots)
                return ItemActionResult.LoadoutFull;

            progress.AddItem(id, -amount);
            progress.AddLoadout(id, amount);
            return ItemActionResult.Success;
        }

        public static ItemActionResult UnpackFromLoadout(HunterProgress progress, ItemId id, int amount = 1)
        {
            var def = ItemDatabase.Get(id);
            if (def == null || amount <= 0) return ItemActionResult.Invalid;

            int inLoadout = progress.GetLoadoutCount(id);
            if (inLoadout < amount) return ItemActionResult.NotOwned;

            progress.AddLoadout(id, -amount);
            progress.AddItem(id, amount);
            return ItemActionResult.Success;
        }

        public static bool ConsumeFromLoadout(HunterProgress progress, ItemId id, int amount = 1)
        {
            int have = progress.GetLoadoutCount(id);
            if (have < amount) return false;
            progress.AddLoadout(id, -amount);
            return true;
        }
    }

    /// <summary>
    /// 单场战斗的道具运行时状态（buff / 陷阱冷却等）。
    /// </summary>
    public class CombatItemState
    {
        public float AttackBuffMul = 1f;
        public float DefenseBuffMul = 1f;
        public float FlashTimer;
        public float FlashIncomingMul = 1f;
        public float ImmobilizeTimer;
        public float TrapCooldown;
        public float HealCooldown;
        public float BombCooldown;
        public float FlashCooldown;
        public bool UsedOpeningBuffs;
        public bool UsedPaintball;
        public int TranqBonusUses;
        public string LastItemLog = string.Empty;

        public void Reset()
        {
            AttackBuffMul = 1f;
            DefenseBuffMul = 1f;
            FlashTimer = 0f;
            FlashIncomingMul = 1f;
            ImmobilizeTimer = 0f;
            TrapCooldown = 0f;
            HealCooldown = 0f;
            BombCooldown = 0f;
            FlashCooldown = 0f;
            UsedOpeningBuffs = false;
            UsedPaintball = false;
            TranqBonusUses = 0;
            LastItemLog = string.Empty;
        }

        public void Tick(float dt)
        {
            if (FlashTimer > 0f) FlashTimer -= dt;
            if (ImmobilizeTimer > 0f) ImmobilizeTimer -= dt;
            if (TrapCooldown > 0f) TrapCooldown -= dt;
            if (HealCooldown > 0f) HealCooldown -= dt;
            if (BombCooldown > 0f) BombCooldown -= dt;
            if (FlashCooldown > 0f) FlashCooldown -= dt;
        }
    }

    public static class CombatItemController
    {
        public static void OnHuntStart(HunterProgress progress, CombatItemState state, CombatMode mode)
        {
            state.Reset();
            if (mode != CombatMode.ActiveHunt) return;

            var skills = ArmorSkillSystem.Evaluate(progress);

            // 开场 buff
            TryUseBuff(progress, state, ItemId.Demondrug, skills);
            TryUseBuff(progress, state, ItemId.Armorskin, skills);

            if (progress.GetLoadoutCount(ItemId.Paintball) > 0 &&
                ItemSystem.ConsumeFromLoadout(progress, ItemId.Paintball))
            {
                state.UsedPaintball = true;
                state.LastItemLog = "使用追踪玉";
            }

            // 麻醉球：本场可用次数
            state.TranqBonusUses = progress.GetLoadoutCount(ItemId.TranqBomb);
        }

        static void TryUseBuff(HunterProgress progress, CombatItemState state, ItemId id, SkillCombatEffects skills)
        {
            var def = ItemDatabase.Get(id);
            if (def == null) return;
            if (progress.GetLoadoutCount(id) <= 0) return;
            if (!ItemSystem.ConsumeFromLoadout(progress, id)) return;

            // 道具使用技能：小概率不消耗（已消耗则补回）
            if (skills.HealOnKill >= 8f && Random.value < 0.15f)
            {
                progress.AddLoadout(id, 1);
            }

            state.AttackBuffMul *= def.AttackBuffMul;
            state.DefenseBuffMul *= def.DefenseBuffMul;
            state.UsedOpeningBuffs = true;
            state.LastItemLog = $"服用 {def.Name}";
        }

        public static void TickAutoUse(
            HunterProgress progress,
            CombatItemState state,
            CombatMode mode,
            ref float playerHp,
            float playerMaxHp,
            ref float monsterHp,
            MonsterDef monster,
            System.Random rng,
            List<string> logs)
        {
            state.Tick(Time.deltaTime);
            var skills = ArmorSkillSystem.Evaluate(progress);
            float itemPower = 1f + (skills.HealOnKill >= 8f ? 0.15f : 0f) + (skills.HealOnKill >= 16f ? 0.1f : 0f);

            // 回血
            if (state.HealCooldown <= 0f)
            {
                float hpRatio = playerMaxHp <= 0f ? 1f : playerHp / playerMaxHp;
                if (hpRatio < 0.28f && TryHeal(progress, state, ItemId.MegaPotion, ref playerHp, playerMaxHp, itemPower, mode, logs))
                {
                    state.HealCooldown = 2.2f;
                }
                else if (hpRatio < 0.45f && TryHeal(progress, state, ItemId.Potion, ref playerHp, playerMaxHp, itemPower, mode, logs))
                {
                    state.HealCooldown = 1.8f;
                }
                else if (mode == CombatMode.ActiveHunt && hpRatio < 0.55f &&
                         TryHeal(progress, state, ItemId.Lifepowder, ref playerHp, playerMaxHp, itemPower, mode, logs))
                {
                    state.HealCooldown = 2.5f;
                }
                else if (mode == CombatMode.ActiveHunt && hpRatio < 0.7f &&
                         TryHeal(progress, state, ItemId.WellDoneSteak, ref playerHp, playerMaxHp, itemPower, mode, logs))
                {
                    state.HealCooldown = 2.0f;
                }
            }

            if (mode != CombatMode.ActiveHunt) return;

            // 闪光弹：血少或定时
            if (state.FlashCooldown <= 0f && state.FlashTimer <= 0f &&
                playerHp / playerMaxHp < 0.6f)
            {
                if (TryFlash(progress, state, logs))
                {
                    state.FlashCooldown = 8f;
                }
            }

            // 陷阱：CD 到了且怪物还活着
            if (state.TrapCooldown <= 0f && state.ImmobilizeTimer <= 0f && monsterHp > 0f)
            {
                if (TryTrap(progress, state, ItemId.ShockTrap, ref monsterHp, skills, logs) ||
                    TryTrap(progress, state, ItemId.PitfallTrap, ref monsterHp, skills, logs))
                {
                    state.TrapCooldown = 10f - skills.TrapChanceBonus * 8f;
                }
            }

            // 炸弹：陷阱/睡眠窗口中丢
            if (state.BombCooldown <= 0f && state.ImmobilizeTimer > 0.5f && monsterHp > 0f)
            {
                if (TryBomb(progress, state, ref monsterHp, skills, logs))
                {
                    state.BombCooldown = 12f;
                }
            }
        }

        static bool TryHeal(
            HunterProgress progress,
            CombatItemState state,
            ItemId id,
            ref float playerHp,
            float playerMaxHp,
            float itemPower,
            CombatMode mode,
            List<string> logs)
        {
            var def = ItemDatabase.Get(id);
            if (def == null) return false;
            if (def.ActiveHuntOnly && mode != CombatMode.ActiveHunt) return false;
            if (!def.AutoUseInIdle && mode == CombatMode.IdleSmall && id != ItemId.Potion && id != ItemId.MegaPotion)
                return false;
            if (progress.GetLoadoutCount(id) <= 0) return false;
            if (!ItemSystem.ConsumeFromLoadout(progress, id)) return false;

            float heal = (def.HealAmount + playerMaxHp * def.HealPercent) * itemPower;
            playerHp = Mathf.Min(playerMaxHp, playerHp + heal);
            string msg = $"使用 {def.Name}，回复 {heal:0}";
            state.LastItemLog = msg;
            logs.Insert(0, msg);
            return true;
        }

        static bool TryFlash(HunterProgress progress, CombatItemState state, List<string> logs)
        {
            var def = ItemDatabase.Get(ItemId.FlashBomb);
            if (progress.GetLoadoutCount(ItemId.FlashBomb) <= 0) return false;
            if (!ItemSystem.ConsumeFromLoadout(progress, ItemId.FlashBomb)) return false;

            state.FlashTimer = def.FlashWeakenSeconds;
            state.FlashIncomingMul = def.FlashIncomingMul;
            string msg = $"投掷 {def.Name}！怪物陷入眩晕虚弱";
            state.LastItemLog = msg;
            logs.Insert(0, msg);
            return true;
        }

        static bool TryTrap(
            HunterProgress progress,
            CombatItemState state,
            ItemId id,
            ref float monsterHp,
            SkillCombatEffects skills,
            List<string> logs)
        {
            var def = ItemDatabase.Get(id);
            if (def == null) return false;
            if (progress.GetLoadoutCount(id) <= 0) return false;
            if (!ItemSystem.ConsumeFromLoadout(progress, id)) return false;

            float duration = def.TrapImmobilizeSeconds * (1f + skills.TrapChanceBonus);
            float damage = def.TrapDamage * (1f + skills.TrapChanceBonus * 1.5f);
            state.ImmobilizeTimer = duration;
            monsterHp = Mathf.Max(0f, monsterHp - damage);

            string msg = $"设置 {def.Name}！定身 {duration:0.0}s，伤害 {damage:0}";
            state.LastItemLog = msg;
            logs.Insert(0, msg);
            return true;
        }

        static bool TryBomb(
            HunterProgress progress,
            CombatItemState state,
            ref float monsterHp,
            SkillCombatEffects skills,
            List<string> logs)
        {
            var def = ItemDatabase.Get(ItemId.BarrelBomb);
            if (progress.GetLoadoutCount(ItemId.BarrelBomb) <= 0) return false;
            if (!ItemSystem.ConsumeFromLoadout(progress, ItemId.BarrelBomb)) return false;

            float damage = def.BombDamage;
            if (skills.HasSleep) damage *= 1.35f; // 睡眠炸弹窗口
            damage *= state.AttackBuffMul;

            monsterHp = Mathf.Max(0f, monsterHp - damage);
            string msg = $"引爆 {def.Name}！造成 {damage:0} 伤害";
            state.LastItemLog = msg;
            logs.Insert(0, msg);
            return true;
        }

        public static float ApplyDropBonus(CombatItemState state, float chance)
        {
            if (state.TranqBonusUses <= 0) return chance;
            return Mathf.Clamp01(chance + 0.12f);
        }

        public static int MapExpBonus(CombatItemState state) => state.UsedPaintball ? 2 : 0;

        public static string SummarizeLoadout(HunterProgress progress)
        {
            if (progress.Loadout == null || progress.Loadout.Count == 0) return "空";
            var sb = new StringBuilder();
            foreach (var pair in progress.Loadout)
            {
                var def = ItemDatabase.Get(pair.Key);
                string name = def != null ? def.Name : pair.Key;
                if (sb.Length > 0) sb.Append("  ");
                sb.Append(name).Append("×").Append(pair.Value);
            }

            return sb.ToString();
        }
    }
}
