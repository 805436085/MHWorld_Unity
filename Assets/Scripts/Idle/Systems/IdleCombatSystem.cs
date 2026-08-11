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
        public CombatItemState ItemState { get; private set; } = new CombatItemState();

        readonly List<CombatLogEntry> _logs = new List<CombatLogEntry>();
        readonly List<string> _itemLogBuffer = new List<string>();
        float _playerAttackTimer;
        float _monsterAttackTimer;
        float _saveTimer;
        float _itemTickTimer;
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
            CombatItemController.OnHuntStart(Progress, ItemState, Mode);
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
            CombatItemController.OnHuntStart(Progress, ItemState, Mode);
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
            CombatItemController.OnHuntStart(Progress, ItemState, Mode);
            float win = HuntSystem.EstimateWinRate(Progress, monster);
            AddLog($"出击 {monster.Name}！{HuntSystem.FormatWinRate(win)}");
            if (!string.IsNullOrEmpty(ItemState.LastItemLog)) AddLog(ItemState.LastItemLog);
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
            _itemTickTimer += deltaTime;

            if (_itemTickTimer >= 0.25f)
            {
                _itemTickTimer = 0f;
                float hp = PlayerHp;
                float mhp = MonsterHp;
                _itemLogBuffer.Clear();
                CombatItemController.TickAutoUse(
                    Progress, ItemState, Mode,
                    ref hp, Progress.GetPlayerMaxHp(),
                    ref mhp, CurrentMonster, _rng, _itemLogBuffer);
                PlayerHp = hp;
                MonsterHp = mhp;
                foreach (var line in _itemLogBuffer) AddLog(line);
                if (MonsterHp <= 0f)
                {
                    OnMonsterDefeated();
                    return;
                }
            }

            float playerInterval = Progress.GetAttackInterval();
            if (_playerAttackTimer >= playerInterval)
            {
                _playerAttackTimer -= playerInterval;
                PlayerAttack();
            }

            bool monsterCanAct = ItemState.ImmobilizeTimer <= 0f;
            if (monsterCanAct && MonsterHp > 0f && _monsterAttackTimer >= MonsterAttackInterval)
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

            var skills = Progress.GetSkillEffects();
            float raw = Progress.GetPlayerAttack() * ItemState.AttackBuffMul;

            // 会心
            bool crit = _rng.NextDouble() < skills.CritChance;
            if (crit) raw *= 1.35f;

            bool charged = Progress.GetEquippedWeapon().Type == WeaponType.GreatSword &&
                           _rng.NextDouble() < Progress.GetChargeChance();
            if (charged)
            {
                float chargeMul = 1.6f;
                if (Progress.UnlockedTechniques.Contains(TechniqueId.GsCharge3.ToString())) chargeMul = 2.1f;
                else if (Progress.UnlockedTechniques.Contains(TechniqueId.GsCharge2.ToString())) chargeMul = 1.85f;
                // 睡眠流派：睡眠窗口中蓄力更痛
                if (skills.HasSleep) chargeMul += 0.25f;
                raw *= chargeMul;
            }

            bool drawSlash = Progress.UnlockedTechniques.Contains(TechniqueId.GsDrawSlash.ToString()) &&
                             _rng.NextDouble() < 0.12f;
            if (drawSlash) raw *= 1.35f;

            float damage = Mathf.Max(1f, raw - CurrentMonster.Defense * 0.3f);

            // 毒：持续补伤
            if (skills.HasPoison && _rng.NextDouble() < 0.2f + skills.StatusChance)
            {
                damage += Mathf.Max(3f, Progress.GetPlayerAttack() * 0.12f);
                AddLog("毒属性生效");
            }

            // 地图陷阱 + 陷阱师技能
            var map = Progress.GetMapProgress(CurrentMonster.MapId);
            float trapChance = (Mode == CombatMode.ActiveHunt && map.TrapUnlocked ? 0.1f : 0.02f)
                               + skills.TrapChanceBonus;
            if (_rng.NextDouble() < trapChance)
            {
                float trapMul = 1.4f + skills.TrapChanceBonus;
                damage *= trapMul;
                AddLog("场地陷阱触发！额外伤害");
            }

            MonsterHp = Mathf.Max(0f, MonsterHp - damage);
            string action = charged ? "蓄力斩" : (drawSlash ? "拔刀斩" : (crit ? "会心一击" : "挥砍"));
            AddLog($"你对 {CurrentMonster.Name} {action}，造成 {damage:0} 伤害");

            // 麻痹：延缓怪物下次攻击
            if (skills.HasParalysis && _rng.NextDouble() < 0.15f + skills.StatusChance * 0.5f)
            {
                _monsterAttackTimer = Mathf.Min(_monsterAttackTimer, -0.8f);
                AddLog("麻痹：怪物动作迟缓");
            }

            // 睡眠：短暂停手，下一次蓄力更容易
            if (skills.HasSleep && _rng.NextDouble() < 0.08f + skills.StatusChance * 0.4f)
            {
                _monsterAttackTimer = Mathf.Min(_monsterAttackTimer, -1.2f);
                AddLog("睡眠：怪物陷入昏睡");
            }

            if (MonsterHp <= 0f) OnMonsterDefeated();
        }

        void MonsterAttack()
        {
            if (PlayerHp <= 0f) return;

            var skills = Progress.GetSkillEffects();
            float damage = Mathf.Max(1f, CurrentMonster.Attack - Progress.GetTotalDefense() * ItemState.DefenseBuffMul * 0.35f);
            damage *= skills.IncomingDamageMul;
            if (ItemState.FlashTimer > 0f) damage *= ItemState.FlashIncomingMul;

            if (Mode == CombatMode.ActiveHunt)
            {
                var map = Progress.GetMapProgress(CurrentMonster.MapId);
                if (map.AdvantageUnlocked) damage *= 0.9f;
            }

            PlayerHp = Mathf.Max(0f, PlayerHp - damage);
            AddLog($"{CurrentMonster.Name} 反击，造成 {damage:0} 伤害");

            // 回复速度：挨打后微量回血
            if (skills.HealOnKill > 0f && PlayerHp > 0f)
            {
                float regen = skills.HealOnKill * 0.15f;
                PlayerHp = Mathf.Min(Progress.GetPlayerMaxHp(), PlayerHp + regen);
            }

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

            var skills = Progress.GetSkillEffects();
            if (skills.HealOnKill > 0f)
            {
                PlayerHp = Mathf.Min(Progress.GetPlayerMaxHp(), PlayerHp + skills.HealOnKill);
            }

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
                float chance = CombatItemController.ApplyDropBonus(ItemState, drop.Chance);
                if (_rng.NextDouble() > chance) continue;
                int amount = _rng.Next(drop.MinAmount, drop.MaxAmount + 1);
                // 麻醉球：本场消耗 1 换掉落加成
                if (ItemState.TranqBonusUses > 0 && Mode == CombatMode.ActiveHunt)
                {
                    if (ItemSystem.ConsumeFromLoadout(Progress, ItemId.TranqBomb))
                    {
                        ItemState.TranqBonusUses--;
                        amount += 1;
                    }
                    else ItemState.TranqBonusUses = 0;
                }

                Progress.AddMaterial(drop.Material, amount);
                rewardBuilder.Append($"，{ToMaterialName(drop.Material)} x{amount}");
            }

            int mapBonus = CombatItemController.MapExpBonus(ItemState);
            if (mapBonus > 0)
            {
                Progress.GetMapProgress(CurrentMonster.MapId).Ring.AddExp(mapBonus);
                rewardBuilder.Append("，地图熟练+");
                rewardBuilder.Append(mapBonus);
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
