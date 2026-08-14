namespace MHIdle.Data
{
    /// <summary>
    /// 三层熟练度的玩家可见命名（不暴露「内圈/外圈」技术叫法）。
    /// 圆环转满一圈升一级，UI 直接展示当前等级。
    /// </summary>
    public static class ProficiencyNaming
    {
        /// <summary>具体武器实例熟练度（原外圈）。</summary>
        public const string Weapon = "专精";

        /// <summary>武器种共享熟练度（原内圈）。</summary>
        public const string Type = "武种";

        /// <summary>风格组共享熟练度（原内内圈）。</summary>
        public const string Style = "心法";

        public static string WeaponTitle(string weaponName) => $"{Weapon} · {weaponName}";

        public static string TypeTitle(WeaponType type) =>
            $"{Type} · {WeaponTaxonomy.TypeName(type)}系";

        public static string StyleTitle(WeaponStyleGroup group) =>
            $"{Style} · {WeaponTaxonomy.StyleName(group)}";

        public static string LevelLabel(string name, int level) => $"{name} Lv.{level}";

        public static string BottleneckHint =>
            $"⚠ {Weapon}瓶颈：请主动讨伐大型怪物突破";

        public static string BottleneckIdleNote =>
            $"{Weapon}已锁定：挂机小怪无法突破，请主动讨伐大型怪物";

        public static string BottleneckBrokenNote =>
            $"大型讨伐突破{Weapon}瓶颈！";
    }
}
