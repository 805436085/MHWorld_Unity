using System;

namespace MHIdle.Data
{
    /// <summary>
    /// 道具战斗数值集中调参：陷阱 CD、炸弹伤害窗口、回血节奏等。
    /// ItemDef 内是单道具效果；这里管「多久能再用」与技能修正上下限。
    /// </summary>
    public static class ItemBalance
    {
        // —— 冷却（秒）——
        /// <summary>落穴/麻痹陷阱基础 CD；陷阱师技能可缩短。</summary>
        public const float TrapCooldownBase = 14f;

        /// <summary>陷阱 CD 下限（即使满陷阱师也不能无限刷）。</summary>
        public const float TrapCooldownMin = 9f;

        /// <summary>陷阱师每点加成对 CD 的缩短系数（乘在 TrapChanceBonus 上）。</summary>
        public const float TrapCooldownSkillFactor = 6f;

        public const float BombCooldown = 16f;
        public const float FlashCooldown = 10f;

        public const float HealCooldownMega = 2.4f;
        public const float HealCooldownPotion = 2.0f;
        public const float HealCooldownPowder = 2.8f;
        public const float HealCooldownSteak = 2.2f;

        // —— 自动使用阈值 ——
        public const float HealHpMega = 0.28f;
        public const float HealHpPotion = 0.42f;
        public const float HealHpPowder = 0.52f;
        public const float HealHpSteak = 0.68f;
        public const float FlashHpTrigger = 0.55f;

        // —— 炸弹窗口 ——
        /// <summary>定身剩余超过该值才允许丢炸弹，避免尾段浪费。</summary>
        public const float BombImmobilizeWindow = 0.8f;

        /// <summary>睡眠 Build 对炸弹的倍率（原 1.35，略收，避免一轮清半血）。</summary>
        public const float SleepBombMul = 1.25f;

        public static float TrapCooldown(float trapChanceBonus)
        {
            float cd = TrapCooldownBase - trapChanceBonus * TrapCooldownSkillFactor;
            return Math.Max(TrapCooldownMin, cd);
        }
    }
}
