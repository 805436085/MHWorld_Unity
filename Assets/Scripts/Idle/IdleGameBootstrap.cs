using UnityEngine;

namespace MHIdle
{
    /// <summary>
    /// 独立工程试玩时自动拉起；并入已发布主工程后请把 AutoLaunchInAnyScene 设为 false。
    /// </summary>
    public static class IdleGameBootstrap
    {
        /// <summary>
        /// true：任意场景加载后自动 Enter（本仓库单独 Play 用）。
        /// false：不自动启动，由主工程大厅调用 IdleGameEntry.Enter()。
        /// </summary>
        public const bool AutoLaunchInAnyScene = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (!AutoLaunchInAnyScene) return;
            IdleGameEntry.Enter();
        }
    }
}
