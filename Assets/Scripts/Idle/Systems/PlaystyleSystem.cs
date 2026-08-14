using MHIdle.Data;
using MHIdle.Model;
using UnityEngine;

namespace MHIdle.Systems
{
    /// <summary>
    /// 角色流派选择：点选即生效（基础加成），并可一键装上已有的推荐防具/道具。
    /// </summary>
    public static class PlaystyleSystem
    {
        public static PlaystyleDef Current(HunterProgress progress)
        {
            progress?.EnsureDefaults();
            return PlaystyleDatabase.Get(progress?.SelectedPlaystyleId);
        }

        public static bool IsUnlocked(HunterProgress progress, PlaystyleDef def)
        {
            if (progress == null || def == null) return false;
            return progress.HunterRank >= def.UnlockHunterRank;
        }

        public static int EquippedSetPieces(HunterProgress progress, PlaystyleDef def)
        {
            if (progress?.EquippedArmor == null || def == null) return 0;
            int n = 0;
            foreach (var pair in progress.EquippedArmor)
            {
                if (PlaystyleDatabase.MatchesArmor(def, pair.Value)) n++;
            }

            return n;
        }

        public static int OwnedSetPieces(HunterProgress progress, PlaystyleDef def)
        {
            if (progress?.OwnedArmor == null || def == null) return 0;
            int n = 0;
            foreach (var id in progress.OwnedArmor)
            {
                if (PlaystyleDatabase.MatchesArmor(def, id)) n++;
            }

            return n;
        }

        public static string Select(HunterProgress progress, PlaystyleId id, bool autoEquip)
        {
            progress.EnsureDefaults();
            var def = PlaystyleDatabase.Get(id);
            if (!IsUnlocked(progress, def))
                return $"猎人等级不足（需 HR{def.UnlockHunterRank}）";

            progress.SelectedPlaystyleId = def.Id.ToString();
            string extra = string.Empty;
            if (autoEquip)
                extra = " · " + EquipRecommended(progress, def);

            return $"已选择流派：{def.Name}{extra}";
        }

        public static string EquipRecommended(HunterProgress progress, PlaystyleDef def)
        {
            progress.EnsureDefaults();
            int equipped = 0;
            foreach (ArmorSlot slot in System.Enum.GetValues(typeof(ArmorSlot)))
            {
                var best = BestOwnedPiece(progress, def, slot);
                if (best == null) continue;
                progress.EquippedArmor[slot.ToString()] = best.Id;
                equipped++;
            }

            int packed = PackRecommendedItems(progress, def);
            if (equipped == 0 && packed == 0)
                return "仓库还没有对应防具/道具，可先去制造";
            if (equipped == 0)
                return $"已装入推荐道具 {packed} 种（防具尚未打造）";
            return $"已装备推荐防具 {equipped} 件，道具 {packed} 种";
        }

        static ArmorDef BestOwnedPiece(HunterProgress progress, PlaystyleDef def, ArmorSlot slot)
        {
            ArmorDef best = null;
            foreach (var armor in GameDatabase.Armors)
            {
                if (armor.Slot != slot) continue;
                if (!progress.OwnsArmor(armor.Id)) continue;
                if (!PlaystyleDatabase.MatchesArmor(def, armor.Id)) continue;
                if (best == null || armor.Tier > best.Tier ||
                    (armor.Tier == best.Tier && armor.Defense > best.Defense))
                    best = armor;
            }

            return best;
        }

        static int PackRecommendedItems(HunterProgress progress, PlaystyleDef def)
        {
            if (def.RecommendedItems == null) return 0;
            int packedTypes = 0;
            foreach (var itemId in def.RecommendedItems)
            {
                int warehouse = progress.GetItem(itemId);
                if (warehouse <= 0) continue;
                var item = ItemDatabase.Get(itemId);
                if (item == null) continue;

                int already = progress.GetLoadoutCount(itemId);
                int room = item.MaxStack - already;
                if (room <= 0) continue;

                int take = Mathf.Min(warehouse, room);
                if (ItemSystem.PackToLoadout(progress, itemId, take) == ItemActionResult.Success)
                    packedTypes++;
            }

            return packedTypes;
        }

        public static void ApplyTo(SkillCombatEffects fx, HunterProgress progress)
        {
            var def = Current(progress);
            if (def == null) return;

            fx.AttackMul *= def.AttackMul;
            fx.DefenseMul *= def.DefenseMul;
            fx.HpFlat += def.HpFlat;
            fx.CritChance += def.CritChance;
            fx.IncomingDamageMul *= def.IncomingDamageMul;
            fx.StatusChance += def.StatusChance;
            fx.TrapChanceBonus += def.TrapChanceBonus;
            fx.HealOnKill += def.HealOnKill;
            fx.AttackIntervalMul *= def.AttackIntervalMul;
            fx.ChargeChanceBonus += def.ChargeChanceBonus;
            if (def.HasPoison) fx.HasPoison = true;
            if (def.HasSleep) fx.HasSleep = true;
            if (def.HasParalysis) fx.HasParalysis = true;

            int pieces = EquippedSetPieces(progress, def);
            if (pieces >= 3)
            {
                fx.AttackMul *= 1.03f;
                fx.DefenseMul *= 1.03f;
                fx.HealOnKill += 2f;
            }

            fx.CritChance = Mathf.Clamp01(fx.CritChance);
            fx.StatusChance = Mathf.Clamp01(fx.StatusChance);
            fx.TrapChanceBonus = Mathf.Clamp01(fx.TrapChanceBonus);
        }
    }
}
