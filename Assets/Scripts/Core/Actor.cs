using UnityEngine;

namespace MHIdle.Core
{
    /// <summary>
    /// 场景中所有可交互实体的基类。
    /// </summary>
    public class Actor : MonoBehaviour
    {
        [SerializeField] private string displayName = "Actor";

        public string DisplayName
        {
            get => displayName;
            set => displayName = value;
        }
    }
}
