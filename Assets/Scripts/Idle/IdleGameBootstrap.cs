using MHIdle.UI;
using UnityEngine;

namespace MHIdle
{
    /// <summary>
    /// 无需改场景：进入任意场景 Play 后自动拉起挂机系统与 UI。
    /// </summary>
    public static class IdleGameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (Object.FindObjectOfType<IdleGameManager>() != null) return;

            var root = new GameObject("IdleGame");
            root.AddComponent<IdleGameManager>();
            root.AddComponent<IdleGameUI>();
        }
    }
}
