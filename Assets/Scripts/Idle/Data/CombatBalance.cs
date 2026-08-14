namespace MHIdle.Data
{
    /// <summary>
    /// 战斗与挂机节奏集中调参。目标：单场小怪约 10～18 秒，大怪约 40～90 秒，
    /// 支撑约一个月日常挂机 + 主动狩猎。
    /// </summary>
    public static class CombatBalance
    {
        /// <summary>怪物攻击间隔（秒）。原 2.2，拉长以降低双方出手密度。</summary>
        public const float MonsterAttackInterval = 3.05f;

        /// <summary>玩家攻速下限，避免熟练度把战斗打成无脑连点。</summary>
        public const float MinPlayerAttackInterval = 0.78f;

        /// <summary>挂机击杀后的搜刮/寻怪间隔。</summary>
        public const float IdlePackDelay = 1.8f;

        /// <summary>挂机被打倒后的休整（怪物下次出手延迟）。</summary>
        public const float IdleDownedMonsterDelay = 1.4f;

        /// <summary>碾压时才把挂机目标推到下一只小怪（攻击 / 血量）。原 0.4，提高门槛。</summary>
        public const float IdleAdvanceAttackToHp = 0.22f;

        public const float OfflineEfficiency = 0.32f;
        public const float OfflineMinSecondsPerKill = 10f;
        public const int OfflineMaxKills = 64;
        public const int OfflineMaxSeconds = 8 * 60 * 60;
    }
}
