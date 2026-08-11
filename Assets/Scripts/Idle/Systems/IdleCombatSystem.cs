using System.Collections.Generic;
using System.Text;
using MHIdle.Data;
using MHIdle.Model;
using UnityEngine;

namespace MHIdle.Systems
{
    public class CombatLogEntry
    {
        public string Text;
        public float Time;
    }

    public enum CombatMode
    {
        IdleSmall,   // 日常挂机小怪
        ActiveHunt   // 主动出击大型（弹窗战斗）
    }

    public class IdleCombatSystem
    {
        public HunterProgress Progress { get; private set; }
        public MonsterDef CurrentMonster { get; private set; }
        public CombatMode Mode { get; private set; } = CombatMode.IdleSmall;
        public float PlayerHp { get; private set; }
        public float MonsterHp { get; private set; }
        public bool IsRunning { get; private set; } = true;
        public bool IsCombatPopupOpen { get; private set; }
        public string LastRewardSummary { get; private set; } = "准备狩猎";
        public IReadOnlyList<CombatLogEntry> Logs => _logs;

        readonly List<CombatLogEntry> _logs = new List<CombatLogEntry>();
        float _playerAttackTimer;
        float _monsterAttackTimer;
        float _saveTimer;
        System.Random _rng = new System.Random();

        const float MonsterAttackInterval = 2.2f;
        const int MaxLogs = 10;

        public void Initialize(HunterProgress progress)
        {
            Progress = progress;
            Progress.EnsureDefaults();
            // 默认挂机第一只已解锁小怪
            int idleIndex = FindFirstUnlockedOfSize(MonsterSize.Small);
            Progress.CurrentMonsterIndex = idleIndex;
            Mode = CombatMode.IdleSmall;
            IsCombatPopupOpen = false;
            BindMonster(Progress.CurrentMonsterIndex);
            RefreshPlayerVitals(true);
            AddLog($"日常挂机开始：{CurrentMonster.Name}");
        }

        public void SetRunning(bool running) => IsRunning = running;

        public void CloseCombatPopup()
        {
            IsCombatPopupOpen = false;
            // 关掉弹窗后回到挂机小怪
            StartIdle();
        }

        public void StartIdle()
        {
            Mode = CombatMode.IdleSmall;
            IsCombatPopupOpen = false;
            int index = FindBestIdleTarget();
            Progress.CurrentMonsterIndex = index;
            BindMonster(index);
            RefreshPlayerVitals(true);
            IsRunning = true;
            AddLog($"挂机中：{CurrentMonster.Name}");
        }

        public void StartActiveHunt(int monsterIndex)
        {
            if (monsterIndex < 0 || monsterIndex > Progress.HighestMonsterIndexUnlocked) return;
            if (monsterIndex >= GameDatabase.Monsters.Count) return;
            var monster = GameDatabase.Monsters[monsterIndex];
            if (monster.Size != MonsterSize.Large) return;

            Mode = CombatMode.ActiveHunt;
            IsCombatPopupOpen = true;
            Progress.CurrentMonsterIndex = monsterIndex;
            BindMonster(monsterIndex);
            RefreshPlayerVitals(true);
            IsRunning = true;
            float win = HuntSystem.EstimateWinRate(Progress, monster);
            AddLog($"出击 {monster.Name}！{HuntSystem.FormatWinRate(win)}");
        }

        public void SelectMonster(int index)
        {
            if (index < 0 || index > Progress.HighestMonsterIndexUnlocked) return;
            if (index >= GameDatabase.Monsters.Count) return;

            var monster = GameDatabase.Monsters[index];
            if (monster.Size == MonsterSize.Large)
            {
                StartActiveHunt(index);
            }
            else
            {
                Mode = CombatMode.IdleSmall;
                IsCombatPopupOpen = false;
                Progress.CurrentMonsterIndex = index;
                BindMonster(index);
                RefreshPlayerVitals(true);
                AddLog($"挂机目标：{monster.Name}");
            }
        }

        public void Tick(float deltaTime)
        {
            if (!IsRunning || Progress == null || CurrentMonster == null) return;
            // 主动狩猎只在弹窗打开时推进
            if (Mode == CombatMode.ActiveHunt && !IsCombatPopupOpen) return;

            _playerAttackTimer += deltaTime;
            _monsterAttackTimer += deltaTime;
            _saveTimer += deltaTime;

            float playerInterval = Progress.GetAttackInterval();
            if (_playerAttackTimer >= playerInterval)
            {
                _playerAttackTimer -= playerInterval;
                PlayerAttack();
            }

            if (MonsterHp > 0f && _monsterAttackTimer >= MonsterAttackInterval)
            {
                _monsterAttackTimer -= MonsterAttackInterval;
                MonsterAttack();
            }

            if (_saveTimer >= 5f)
            {
                _saveTimer = 0f;
                SaveSystem.Save(Progress);
            }
        }

        public void RecalculateAfterGearChange() => RefreshPlayerVitals(false);

        void PlayerAttack()
        {
            if (MonsterHp <= 0f) return;

            float raw = Progress.GetPlayerAttack();
            bool charged = Progress.GetEquippedWeapon().Type == WeaponType.GreatSword &&
                           _rng.NextDouble() < Progress.GetChargeChance();
            if (charged)
            {
                float chargeMul = 1.6f;
                if (Progress.UnlockedTechniques.Contains(TechniqueId.GsCharge3.ToString())) chargeMul = 2.1f;
                else if (Progress.UnlockedTechniques.Contains(TechniqueId.GsCharge2.ToString())) chargeMul = 1.85f;
                raw *= chargeMul;
            }

            bool drawSlash = Progress.UnlockedTechniques.Contains(TechniqueId.GsDrawSlash.ToString()) &&
                             _rng.NextDouble() < 0.12f;
            if (drawSlash) raw *= 1.35f;

            float damage = Mathf.Max(1f, raw - CurrentMonster.Defense * 0.3f);

            // 地图陷阱：大怪额外伤害
            var map = Progress.GetMapProgress(CurrentMonster.MapId);
            if (Mode == CombatMode.ActiveHunt && map.TrapUnlocked && _rng.NextDouble() < 0.1f)
            {
                damage *= 1.4f;
                AddLog($"场地陷阱触发！额外伤害");
            }

            MonsterHp = Mathf.Max(0f, MonsterHp - damage);
            string action = charged ? "蓄力斩" : (drawSlash ? "拔刀斩" : "挥砍");
            AddLog($"你对 {CurrentMonster.Name} {action}，造成 {damage:0} 伤害");

            if (MonsterHp <= 0f) OnMonsterDefeated();
        }

        void MonsterAttack()
        {
            if (PlayerHp <= 0f) return;

            float damage = Mathf.Max(1f, CurrentMonster.Attack - Progress.GetTotalDefense() * 0.35f);
            if (Mode == CombatMode.ActiveHunt)
            {
                var map = Progress.GetMapProgress(CurrentMonster.MapId);
                if (map.AdvantageUnlocked) damage *= 0.9f;
            }

            PlayerHp = Mathf.Max(0f, PlayerHp - damage);
            AddLog($"{CurrentMonster.Name} 反击，造成 {damage:0} 伤害");

            if (PlayerHp <= 0f)
            {
                if (Mode == CombatMode.ActiveHunt)
                {
                    string penalty = HuntSystem.ApplyDeathPenalty(Progress, _rng);
                    LastRewardSummary = penalty;
                    AddLog(penalty);
                    AddLog("讨伐失败，返回营地…");
                    IsRunning = false;
                    SaveSystem.Save(Progress);
                }
                else
                {
                    AddLog("挂机被打倒，短暂休整后继续…");
                    RefreshPlayerVitals(true);
                    MonsterHp = CurrentMonster.MaxHp;
                    _playerAttackTimer = 0f;
                    _monsterAttackTimer = 0.6f;
                }
            }
        }

        void OnMonsterDefeated()
        {
            Progress.TotalKills++;
            if (CurrentMonster.Size == MonsterSize.Large) Progress.TotalLargeKills++;

            Progress.Zenny += CurrentMonster.ZennyReward;
            Progress.AddHunterRankExp(CurrentMonster.HunterRankExp);

            var notes = ProficiencySystem.GrantCombatExp(
                Progress,
                Progress.GetEquippedWeapon(),
                CurrentMonster.WeaponProficiencyExp,
                CurrentMonster.Size,
                CurrentMonster.MapId);

            var rewardBuilder = new StringBuilder();
            rewardBuilder.Append($"讨伐 {CurrentMonster.Name}！+{CurrentMonster.ZennyReward}z");

            foreach (var drop in CurrentMonster.Drops)
            {
                if (_rng.NextDouble() > drop.Chance) continue;
                int amount = _rng.Next(drop.MinAmount, drop.MaxAmount + 1);
                Progress.AddMaterial(drop.Material, amount);
                rewardBuilder.Append($"，{ToMaterialName(drop.Material)} x{amount}");
            }

            var wp = Progress.GetEquippedWeaponProgress();
            rewardBuilder.Append($"，外圈 Lv.{wp.Outer.Level}");

            foreach (var note in notes)
            {
                rewardBuilder.Append($"，{note}");
                AddLog(note);
            }

            LastRewardSummary = rewardBuilder.ToString();
            AddLog(LastRewardSummary);

            int nextIndex = Progress.CurrentMonsterIndex + 1;
            if (nextIndex < GameDatabase.Monsters.Count &&
                Progress.HunterRank >= GameDatabase.Monsters[nextIndex].Rank - 1)
            {
                Progress.HighestMonsterIndexUnlocked = Mathf.Max(
                    Progress.HighestMonsterIndexUnlocked,
                    nextIndex);
            }

            if (Mode == CombatMode.ActiveHunt)
            {
                IsRunning = false;
                AddLog("讨伐成功！可关闭战斗窗口。");
            }
            else
            {
                // 挂机：若碾压且下一只仍是小怪则推进
                int next = Progress.CurrentMonsterIndex + 1;
                if (next <= Progress.HighestMonsterIndexUnlocked &&
                    next < GameDatabase.Monsters.Count &&
                    GameDatabase.Monsters[next].Size == MonsterSize.Small &&
                    Progress.GetPlayerAttack() > CurrentMonster.MaxHp * 0.4f)
                {
                    Progress.CurrentMonsterIndex = next;
                    BindMonster(next);
                    AddLog($"挂机推进：{CurrentMonster.Name}");
                }
                else
                {
                    MonsterHp = CurrentMonster.MaxHp;
                }

                RefreshPlayerVitals(true);
            }

            SaveSystem.Save(Progress);
        }

        int FindFirstUnlockedOfSize(MonsterSize size)
        {
            for (int i = 0; i <= Progress.HighestMonsterIndexUnlocked && i < GameDatabase.Monsters.Count; i++)
            {
                if (GameDatabase.Monsters[i].Size == size) return i;
            }

            return 0;
        }

        int FindBestIdleTarget()
        {
            int best = FindFirstUnlockedOfSize(MonsterSize.Small);
            for (int i = 0; i <= Progress.HighestMonsterIndexUnlocked && i < GameDatabase.Monsters.Count; i++)
            {
                var m = GameDatabase.Monsters[i];
                if (m.Size != MonsterSize.Small) continue;
                best = i;
            }

            return best;
        }

        void BindMonster(int index)
        {
            CurrentMonster = GameDatabase.GetMonsterByIndex(index);
            MonsterHp = CurrentMonster.MaxHp;
            _playerAttackTimer = 0f;
            _monsterAttackTimer = 0.8f;
        }

        void RefreshPlayerVitals(bool healFull)
        {
            float maxHp = Progress.GetPlayerMaxHp();
            if (healFull || PlayerHp <= 0f || PlayerHp > maxHp)
            {
                PlayerHp = maxHp;
            }
        }

        void AddLog(string text)
        {
            _logs.Insert(0, new CombatLogEntry { Text = text, Time = Time.time });
            if (_logs.Count > MaxLogs) _logs.RemoveAt(_logs.Count - 1);
        }

        public static string ToMaterialName(MaterialId id)
        {
            switch (id)
            {
                case MaterialId.MonsterBone: return "怪兽骨";
                case MaterialId.MonsterHide: return "兽皮";
                case MaterialId.SharpClaw: return "锐利爪";
                case MaterialId.WyvernGem: return "龙玉";
                case MaterialId.ElderDragonBlood: return "古龙之血";
                default: return id.ToString();
            }
        }

        public static string ToWeaponTypeName(WeaponType type) => WeaponTaxonomy.TypeName(type);
    }
}
