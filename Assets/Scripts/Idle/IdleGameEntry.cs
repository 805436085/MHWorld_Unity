using MHIdle.UI;
using UnityEngine;

namespace MHIdle
{
    /// <summary>
    /// 给已发布主工程调用的入口。
    /// 大厅按钮 → IdleGameEntry.Enter()；返回大厅 → IdleGameEntry.Exit()。
    /// </summary>
    public static class IdleGameEntry
    {
        public static bool IsActive => IdleGameManager.Instance != null;

        /// <summary>进入挂机玩法（幂等）。</summary>
        public static void Enter()
        {
            if (IdleGameManager.Instance != null) return;

            var root = new GameObject("IdleGame");
            root.AddComponent<IdleGameManager>();
            root.AddComponent<IdleGameUI>();
        }

        /// <summary>退出挂机：停战斗、存档、销毁 UI/总控。</summary>
        public static void Exit()
        {
            var manager = IdleGameManager.Instance;
            if (manager == null) return;

            manager.PrepareForHostExit();
            Object.Destroy(manager.gameObject);
        }
    }
}
