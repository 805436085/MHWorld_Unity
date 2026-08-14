using MHIdle.Model;
using MHIdle.Systems;
using UnityEngine;

namespace MHIdle
{
    /// <summary>
    /// 挂机玩法总控：战斗循环、锻造入口、存档。
    /// </summary>
    public class IdleGameManager : MonoBehaviour
    {
        public static IdleGameManager Instance { get; private set; }

        public HunterProgress Progress { get; private set; }
        public IdleCombatSystem Combat { get; private set; }
        public string StatusMessage { get; private set; } = string.Empty;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Progress = SaveSystem.LoadOrCreate();
            string offline = OfflineProgressSystem.Apply(Progress);
            Combat = new IdleCombatSystem();
            Combat.Initialize(Progress);
            if (!string.IsNullOrEmpty(offline))
            {
                StatusMessage = offline;
                SaveSystem.Save(Progress);
            }
        }

        void Update()
        {
            Combat?.Tick(Time.deltaTime);
        }

        public void ToggleCombat()
        {
            Combat.SetRunning(!Combat.IsRunning);
            StatusMessage = Combat.IsRunning ? "继续狩猎" : "已暂停";
        }

        public void SelectMonster(int index)
        {
            Combat.SelectMonster(index);
            StatusMessage = $"目标切换为 {Combat.CurrentMonster.Name}";
        }

        public void CraftWeapon(string weaponId)
        {
            var result = ForgeSystem.CraftWeapon(Progress, weaponId);
            StatusMessage = ForgeSystem.Describe(result);
            if (result == ForgeResult.Success)
            {
                Combat.RecalculateAfterGearChange();
                SaveSystem.Save(Progress);
            }
        }

        public void EquipWeapon(string weaponId)
        {
            var result = ForgeSystem.EquipWeapon(Progress, weaponId);
            StatusMessage = ForgeSystem.Describe(result);
            if (result == ForgeResult.Success)
            {
                Combat.RecalculateAfterGearChange();
                SaveSystem.Save(Progress);
            }
        }

        public void CraftArmor(string armorId)
        {
            var result = ForgeSystem.CraftArmor(Progress, armorId);
            StatusMessage = ForgeSystem.Describe(result);
            if (result == ForgeResult.Success)
            {
                Combat.RecalculateAfterGearChange();
                SaveSystem.Save(Progress);
            }
        }

        public void EquipArmor(string armorId)
        {
            var result = ForgeSystem.EquipArmor(Progress, armorId);
            StatusMessage = ForgeSystem.Describe(result);
            if (result == ForgeResult.Success)
            {
                Combat.RecalculateAfterGearChange();
                SaveSystem.Save(Progress);
            }
        }

        public void SelectPlaystyle(Data.PlaystyleId id, bool autoEquip = false)
        {
            StatusMessage = PlaystyleSystem.Select(Progress, id, autoEquip);
            Combat.RecalculateAfterGearChange();
            SaveSystem.Save(Progress);
        }

        public void EquipPlaystyleGear()
        {
            var def = PlaystyleSystem.Current(Progress);
            StatusMessage = $"装配{def.Name}：{PlaystyleSystem.EquipRecommended(Progress, def)}";
            Combat.RecalculateAfterGearChange();
            SaveSystem.Save(Progress);
        }

        public void ResetProgress()
        {
            SaveSystem.Delete();
            Progress = HunterProgress.CreateNew();
            Combat = new IdleCombatSystem();
            Combat.Initialize(Progress);
            StatusMessage = "进度已重置";
        }

        public void StartIdleFarm()
        {
            Combat.StartIdle();
            StatusMessage = "开始日常挂机";
        }

        public void StartActiveHunt(int monsterIndex)
        {
            var monster = Data.GameDatabase.GetMonsterByIndex(monsterIndex);
            if (monster == null) return;
            float rate = HuntSystem.EstimateWinRate(Progress, monster);
            Combat.StartActiveHunt(monsterIndex);
            StatusMessage = $"出击 {monster.Name} · {HuntSystem.FormatWinRate(rate)}";
        }

        public void CloseCombatPopup()
        {
            Combat.CloseCombatPopup();
            StatusMessage = "已返回营地";
        }

        public void BuyItem(Data.ItemId id, int amount = 1)
        {
            var result = ItemSystem.Buy(Progress, id, amount);
            StatusMessage = result == ItemActionResult.Success
                ? $"购入 {Data.ItemDatabase.Get(id).Name} x{amount}"
                : ItemSystem.Describe(result);
            if (result == ItemActionResult.Success) SaveSystem.Save(Progress);
        }

        public void CraftItem(Data.ItemId id, int amount = 1)
        {
            var result = ItemSystem.Craft(Progress, id, amount);
            StatusMessage = result == ItemActionResult.Success
                ? $"制造 {Data.ItemDatabase.Get(id).Name} x{amount}"
                : ItemSystem.Describe(result);
            if (result == ItemActionResult.Success) SaveSystem.Save(Progress);
        }

        public void PackItem(Data.ItemId id, int amount = 1)
        {
            var result = ItemSystem.PackToLoadout(Progress, id, amount);
            StatusMessage = result == ItemActionResult.Success
                ? $"装入出征背包：{Data.ItemDatabase.Get(id).Name} x{amount}"
                : ItemSystem.Describe(result);
            if (result == ItemActionResult.Success) SaveSystem.Save(Progress);
        }

        public void UnpackItem(Data.ItemId id, int amount = 1)
        {
            var result = ItemSystem.UnpackFromLoadout(Progress, id, amount);
            StatusMessage = result == ItemActionResult.Success
                ? $"卸下到仓库：{Data.ItemDatabase.Get(id).Name} x{amount}"
                : ItemSystem.Describe(result);
            if (result == ItemActionResult.Success) SaveSystem.Save(Progress);
        }

        void OnApplicationQuit()
        {
            if (Progress != null) SaveSystem.Save(Progress);
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && Progress != null) SaveSystem.Save(Progress);
        }
    }
}
