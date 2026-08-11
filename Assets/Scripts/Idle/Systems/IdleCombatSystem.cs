using System;
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

    public class IdleCombatSystem
    {
        public HunterProgress Progress { get; private set; }
        public MonsterDef CurrentMonster { get; private set; }
        public float PlayerHp { get; private set; }
        public float MonsterHp { get; private set; }
        public bool IsRunning { get; private set; } = true;
        public string LastRewardSummary { get; private set; } = "准备狩猎";
        public IReadOnlyList<CombatLogEntry> Logs => _logs;

        readonly List<CombatLogEntry> _logs = new List<CombatLogEntry>();
        float _playerAttackTimer;
        float _monsterAttackTimer;
        float _saveTimer;
        System.Random _rng = new System.Random();

        const float MonsterAttackInterval = 2.2f;
        const int MaxLogs = 8;

        public void Initialize(HunterProgress progress)
        {
            Progress = progress;
            Progress.EnsureDefaults();
            BindMonster(Progress.CurrentMonsterIndex);
            RefreshPlayerVitals(true);
            AddLog($"猎人出发了。目标：{CurrentMonster.Name}");
        }

        public void SetRunning(bool running) => IsRunning = running;

        public void SelectMonster(int index)
        {
            if (index < 0 || index > Progress.HighestMonsterIndexUnlocked) return;
            if (index >= GameDatabase.Monsters.Count) return;

            Progress.CurrentMonsterIndex = index;
            BindMonster(index);
            RefreshPlayerVitals(true);
            AddLog($"转移狩猎目标：{CurrentMonster.Name}");
        }

        public void Tick(float deltaTime)
        {
            if (!IsRunning || Progress == null || CurrentMonster == null) return;

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

        public void RecalculateAfterGearChange()
        {
            RefreshPlayerVitals(false);
        }

        void PlayerAttack()
        {
            if (MonsterHp <= 0f) return;

            float raw = Progress.GetPlayerAttack();
            // 大剑偶尔真蓄力一击
            bool charged = Progress.GetEquippedWeapon().Type == WeaponType.GreatSword && _rng.NextDouble() < 0.18f;
            if (charged) raw *= 1.85f;

            float damage = Mathf.Max(1f, raw - CurrentMonster.Defense * 0.3f);
            MonsterHp = Mathf.Max(0f, MonsterHp - damage);

            string action = charged ? "蓄力斩" : "挥砍";
            AddLog($"你对 {CurrentMonster.Name} {action}，造成 {damage:0} 伤害");

            if (MonsterHp <= 0f)
            {
                OnMonsterDefeated();
            }
        }

        void MonsterAttack()
        {
            if (PlayerHp <= 0f) return;

            float damage = Mathf.Max(1f, CurrentMonster.Attack - Progress.GetTotalDefense() * 0.35f);
            PlayerHp = Mathf.Max(0f, PlayerHp - damage);
            AddLog($"{CurrentMonster.Name} 反击，造成 {damage:0} 伤害");

            if (PlayerHp <= 0f)
            {
                AddLog("你被打倒了，营地疗伤后重新投入狩猎…");
                RefreshPlayerVitals(true);
                MonsterHp = CurrentMonster.MaxHp;
                _playerAttackTimer = 0f;
                _monsterAttackTimer = 0.6f;
            }
        }

        void OnMonsterDefeated()
        {
            Progress.TotalKills++;
            Progress.Zenny += CurrentMonster.ZennyReward;
            Progress.AddHunterRankExp(CurrentMonster.HunterRankExp);

            var weaponProgress = Progress.GetEquippedWeaponProgress();
            bool leveled = weaponProgress.AddExp(CurrentMonster.WeaponProficiencyExp);

            var rewardBuilder = new StringBuilder();
            rewardBuilder.Append($"讨伐 {CurrentMonster.Name}！+{CurrentMonster.ZennyReward}z");

            foreach (var drop in CurrentMonster.Drops)
            {
                if (_rng.NextDouble() > drop.Chance) continue;
                int amount = _rng.Next(drop.MinAmount, drop.MaxAmount + 1);
                Progress.AddMaterial(drop.Material, amount);
                rewardBuilder.Append($"，{ToMaterialName(drop.Material)} x{amount}");
            }

            if (leveled)
            {
                rewardBuilder.Append($"，熟练度升至 Lv.{weaponProgress.ProficiencyLevel}");
            }

            LastRewardSummary = rewardBuilder.ToString();
            AddLog(LastRewardSummary);

            // 解锁下一只怪
            int nextIndex = Progress.CurrentMonsterIndex + 1;
            if (nextIndex < GameDatabase.Monsters.Count &&
                Progress.HunterRank >= GameDatabase.Monsters[nextIndex].Rank - 1)
            {
                Progress.HighestMonsterIndexUnlocked = Mathf.Max(
                    Progress.HighestMonsterIndexUnlocked,
                    nextIndex);
            }

            // 若当前怪已被轻松碾压且已解锁下一只，则自动推进
            if (Progress.CurrentMonsterIndex < Progress.HighestMonsterIndexUnlocked &&
                Progress.GetPlayerAttack() > CurrentMonster.MaxHp * 0.35f)
            {
                Progress.CurrentMonsterIndex++;
                BindMonster(Progress.CurrentMonsterIndex);
                AddLog($"狩猎地推进：{CurrentMonster.Name}");
            }
            else
            {
                MonsterHp = CurrentMonster.MaxHp;
            }

            RefreshPlayerVitals(true);
            SaveSystem.Save(Progress);
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
            if (_logs.Count > MaxLogs)
            {
                _logs.RemoveAt(_logs.Count - 1);
            }
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

        public static string ToWeaponTypeName(WeaponType type)
        {
            switch (type)
            {
                case WeaponType.GreatSword: return "大剑";
                case WeaponType.LongSword: return "太刀";
                case WeaponType.DualBlades: return "双剑";
                default: return type.ToString();
            }
        }
    }
}
