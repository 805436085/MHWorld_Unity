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

        public void ResetProgress()
        {
            SaveSystem.Delete();
            Progress = HunterProgress.CreateNew();
            Combat = new IdleCombatSystem();
            Combat.Initialize(Progress);
            StatusMessage = "进度已重置";
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
