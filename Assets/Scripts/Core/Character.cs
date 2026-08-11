namespace MHIdle.Core
{
    /// <summary>
    /// 玩家控制的猎人角色。
    /// </summary>
    public class Character : Pawn
    {
        public int HunterRank { get; private set; } = 1;

        public void SetHunterRank(int rank)
        {
            HunterRank = UnityEngine.Mathf.Max(1, rank);
        }
    }
}
