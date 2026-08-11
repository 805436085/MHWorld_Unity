namespace MHIdle.Core
{
    /// <summary>
    /// 可参与战斗的实体（猎人 / 怪物）。
    /// </summary>
    public class Pawn : Actor
    {
        public float MaxHp { get; protected set; } = 100f;
        public float CurrentHp { get; protected set; } = 100f;
        public float Attack { get; protected set; } = 10f;
        public float Defense { get; protected set; } = 0f;
        public bool IsAlive => CurrentHp > 0f;

        public virtual void InitStats(float maxHp, float attack, float defense)
        {
            MaxHp = maxHp;
            CurrentHp = maxHp;
            Attack = attack;
            Defense = defense;
        }

        public virtual float TakeDamage(float rawDamage)
        {
            float mitigated = UnityEngine.Mathf.Max(1f, rawDamage - Defense * 0.35f);
            CurrentHp = UnityEngine.Mathf.Max(0f, CurrentHp - mitigated);
            return mitigated;
        }

        public virtual void HealFull()
        {
            CurrentHp = MaxHp;
        }
    }
}
