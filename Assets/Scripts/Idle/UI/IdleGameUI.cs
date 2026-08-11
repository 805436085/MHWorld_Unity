using System;
using System.Collections.Generic;
using MHIdle.Data;
using MHIdle.Systems;
using UnityEngine;

namespace MHIdle.UI
{
    /// <summary>
    /// 轻量 IMGUI 界面，不依赖额外 UI 包，打开场景 Play 即可操作。
    /// </summary>
    public class IdleGameUI : MonoBehaviour
    {
        enum Tab
        {
            Hunt,
            ForgeWeapon,
            ForgeArmor,
            Bag
        }

        const float IconSm = 22f;
        const float IconMd = 36f;
        const float IconLg = 48f;

        Tab _tab = Tab.Hunt;
        Vector2 _scroll;
        GUIStyle _titleStyle;
        GUIStyle _boxStyle;
        GUIStyle _labelStyle;
        GUIStyle _smallStyle;
        bool _stylesReady;

        void OnGUI()
        {
            if (IdleGameManager.Instance == null) return;

            IconLibrary.EnsureLoaded();
            EnsureStyles();
            DrawBackground();

            var manager = IdleGameManager.Instance;
            var combat = manager.Combat;
            var progress = manager.Progress;

            float pad = 16f;
            Rect root = new Rect(pad, pad, Screen.width - pad * 2f, Screen.height - pad * 2f);
            GUILayout.BeginArea(root);

            GUILayout.Label("MONSTER HUNTER  ·  IDLE", _titleStyle);
            GUILayout.Label("挂机狩猎 · 大剑开荒 · 素材锻造 · 支持离线结算", _labelStyle);
            GUILayout.Space(8f);

            DrawTabs();
            GUILayout.Space(8f);

            _scroll = GUILayout.BeginScrollView(_scroll);
            switch (_tab)
            {
                case Tab.Hunt:
                    DrawHunt(manager, combat, progress);
                    break;
                case Tab.ForgeWeapon:
                    DrawWeapons(manager, progress);
                    break;
                case Tab.ForgeArmor:
                    DrawArmors(manager, progress);
                    break;
                case Tab.Bag:
                    DrawBag(progress);
                    break;
            }

            GUILayout.EndScrollView();

            if (!string.IsNullOrEmpty(manager.StatusMessage))
            {
                GUILayout.Space(6f);
                GUILayout.Label($"提示：{manager.StatusMessage}", _labelStyle);
            }

            GUILayout.Space(4f);
            GUILayout.Label("Icons by Lorc / Delapouite · game-icons.net (CC BY 3.0)", _smallStyle);

            GUILayout.EndArea();
        }

        void DrawTabs()
        {
            GUILayout.BeginHorizontal();
            TabButton("狩猎", Tab.Hunt);
            TabButton("武器工房", Tab.ForgeWeapon);
            TabButton("防具工房", Tab.ForgeArmor);
            TabButton("背包", Tab.Bag);
            GUILayout.EndHorizontal();
        }

        void TabButton(string label, Tab tab)
        {
            var old = GUI.backgroundColor;
            if (_tab == tab) GUI.backgroundColor = new Color(0.85f, 0.62f, 0.28f);
            if (GUILayout.Button(label, GUILayout.Height(32f))) _tab = tab;
            GUI.backgroundColor = old;
        }

        void DrawHunt(IdleGameManager manager, IdleCombatSystem combat, Model.HunterProgress progress)
        {
            var monster = combat.CurrentMonster;
            var weapon = progress.GetEquippedWeapon();
            var weaponProgress = progress.GetEquippedWeaponProgress();

            GUILayout.BeginVertical(_boxStyle);

            GUILayout.BeginHorizontal();
            DrawCurrencyChip(progress.Zenny);
            GUILayout.Space(12f);
            GUILayout.Label($"猎人等级 HR{progress.HunterRank}   经验 {progress.HunterRankExp}/{progress.ExpToNextRank}", _labelStyle);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            IconLibrary.DrawIcon(IconLibrary.GetWeapon(weapon.Type), IconMd, new Color(0.95f, 0.85f, 0.45f));
            GUILayout.BeginVertical();
            GUILayout.Label(
                $"{weapon.Name}（{IdleCombatSystem.ToWeaponTypeName(weapon.Type)}）  熟练度 Lv.{weaponProgress.ProficiencyLevel}  ({weaponProgress.ProficiencyExp}/{weaponProgress.ExpToNext})",
                _labelStyle);
            GUILayout.Label(
                $"攻击 {progress.GetPlayerAttack():0.0}   防御 {progress.GetTotalDefense():0.0}   攻速间隔 {progress.GetAttackInterval():0.00}s   讨伐 {progress.TotalKills}",
                _labelStyle);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.Space(8f);

            GUILayout.BeginVertical(_boxStyle);
            GUILayout.Label($"当前目标：{monster.Name}  ·  {monster.Locale}  ·  危险度 {monster.Rank}", _labelStyle);
            DrawBar("猎人 HP", combat.PlayerHp, progress.GetPlayerMaxHp(), new Color(0.35f, 0.75f, 0.45f));
            DrawBar("怪物 HP", combat.MonsterHp, monster.MaxHp, new Color(0.85f, 0.35f, 0.3f));
            GUILayout.Label(combat.LastRewardSummary, _labelStyle);

            if (monster.Drops != null && monster.Drops.Count > 0)
            {
                GUILayout.Space(4f);
                GUILayout.Label("可能掉落", _smallStyle);
                GUILayout.BeginHorizontal();
                foreach (var drop in monster.Drops)
                {
                    DrawMaterialChip(drop.Material, $"{drop.MinAmount}-{drop.MaxAmount}");
                    GUILayout.Space(6f);
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(combat.IsRunning ? "暂停挂机" : "继续挂机", GUILayout.Height(34f)))
            {
                manager.ToggleCombat();
            }

            if (GUILayout.Button("重置进度", GUILayout.Height(34f), GUILayout.Width(120f)))
            {
                manager.ResetProgress();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(8f);
            GUILayout.Label("狩猎目标（已解锁）", _labelStyle);
            for (int i = 0; i <= progress.HighestMonsterIndexUnlocked && i < GameDatabase.Monsters.Count; i++)
            {
                var m = GameDatabase.Monsters[i];
                string mark = i == progress.CurrentMonsterIndex ? "▶ " : "   ";
                GUILayout.BeginHorizontal(_boxStyle);
                if (m.Drops.Count > 0)
                {
                    IconLibrary.DrawIcon(
                        IconLibrary.GetMaterial(m.Drops[0].Material),
                        IconSm,
                        IconLibrary.MaterialTint(m.Drops[0].Material));
                }

                if (GUILayout.Button($"{mark}{m.Name}  HR≈{m.Rank}  HP {m.MaxHp:0}  奖励 {m.ZennyReward}z", GUILayout.Height(28f)))
                {
                    manager.SelectMonster(i);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.Space(8f);
            GUILayout.Label("战斗日志", _labelStyle);
            GUILayout.BeginVertical(_boxStyle);
            foreach (var log in combat.Logs)
            {
                GUILayout.Label($"• {log.Text}", _labelStyle);
            }

            GUILayout.EndVertical();
        }

        void DrawWeapons(IdleGameManager manager, Model.HunterProgress progress)
        {
            GUILayout.Label("大剑工房：用素材锻造更强武器，战斗提升熟练度。", _labelStyle);
            foreach (var weapon in GameDatabase.Weapons)
            {
                var wp = progress.Weapons[weapon.Id];
                GUILayout.BeginVertical(_boxStyle);

                GUILayout.BeginHorizontal();
                IconLibrary.DrawIcon(IconLibrary.GetWeapon(weapon.Type), IconLg, new Color(0.95f, 0.85f, 0.45f));
                GUILayout.BeginVertical();
                GUILayout.Label($"{weapon.Name}  T{weapon.Tier}  伤害 {weapon.BaseDamage}  间隔 {weapon.AttackInterval:0.00}s", _labelStyle);
                GUILayout.Label($"解锁 HR{weapon.UnlockHunterRank}  |  {(wp.Owned ? $"已拥有 · 熟练度 Lv.{wp.ProficiencyLevel}" : "未拥有")}", _labelStyle);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                DrawCostRow(weapon.CraftZenny, weapon.CraftCost);

                GUILayout.BeginHorizontal();
                GUI.enabled = !wp.Owned;
                if (GUILayout.Button("锻造", GUILayout.Height(28f))) manager.CraftWeapon(weapon.Id);
                GUI.enabled = wp.Owned && progress.EquippedWeaponId != weapon.Id;
                if (GUILayout.Button(progress.EquippedWeaponId == weapon.Id ? "装备中" : "装备", GUILayout.Height(28f)))
                {
                    manager.EquipWeapon(weapon.Id);
                }

                GUI.enabled = true;
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(4f);
            }
        }

        void DrawArmors(IdleGameManager manager, Model.HunterProgress progress)
        {
            GUILayout.Label("防具工房：提升生存能力，站得更久才能挂更强的怪。", _labelStyle);
            foreach (var armor in GameDatabase.Armors)
            {
                bool owned = progress.OwnsArmor(armor.Id);
                bool equipped = progress.GetEquippedArmorId(armor.Slot) == armor.Id;

                GUILayout.BeginVertical(_boxStyle);

                GUILayout.BeginHorizontal();
                IconLibrary.DrawIcon(IconLibrary.GetArmor(armor.Slot), IconMd, new Color(0.7f, 0.82f, 0.95f));
                GUILayout.BeginVertical();
                GUILayout.Label($"{armor.Name}  ({ToSlotName(armor.Slot)})  防御 +{armor.Defense:0}  生命 +{armor.HpBonus:0}", _labelStyle);
                GUILayout.Label($"解锁 HR{armor.UnlockHunterRank}  |  {(owned ? (equipped ? "装备中" : "已拥有") : "未拥有")}", _labelStyle);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                DrawCostRow(armor.CraftZenny, armor.CraftCost);

                GUILayout.BeginHorizontal();
                GUI.enabled = !owned;
                if (GUILayout.Button("锻造", GUILayout.Height(28f))) manager.CraftArmor(armor.Id);
                GUI.enabled = owned && !equipped;
                if (GUILayout.Button(equipped ? "装备中" : "装备", GUILayout.Height(28f)))
                {
                    manager.EquipArmor(armor.Id);
                }

                GUI.enabled = true;
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(4f);
            }
        }

        void DrawBag(Model.HunterProgress progress)
        {
            GUILayout.BeginVertical(_boxStyle);
            GUILayout.BeginHorizontal();
            DrawCurrencyChip(progress.Zenny, large: true);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);

            foreach (MaterialId id in Enum.GetValues(typeof(MaterialId)))
            {
                GUILayout.BeginHorizontal();
                IconLibrary.DrawIcon(IconLibrary.GetMaterial(id), IconLg, IconLibrary.MaterialTint(id));
                GUILayout.BeginVertical();
                GUILayout.Label(IdleCombatSystem.ToMaterialName(id), _labelStyle);
                GUILayout.Label($"持有 x{progress.GetMaterial(id)}", _smallStyle);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(6f);
            }

            GUILayout.EndVertical();
        }

        void DrawCostRow(int zenny, Dictionary<MaterialId, int> cost)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("锻造：", _smallStyle);
            if (zenny <= 0 && (cost == null || cost.Count == 0))
            {
                GUILayout.Label("免费", _smallStyle);
            }
            else
            {
                if (zenny > 0)
                {
                    DrawCurrencyChip(zenny, compact: true);
                    GUILayout.Space(8f);
                }

                if (cost != null)
                {
                    foreach (var pair in cost)
                    {
                        DrawMaterialChip(pair.Key, $"x{pair.Value}");
                        GUILayout.Space(6f);
                    }
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        void DrawCurrencyChip(int amount, bool large = false, bool compact = false)
        {
            float size = large ? IconMd : IconSm;
            GUILayout.BeginHorizontal(GUILayout.Height(size));
            IconLibrary.DrawIcon(IconLibrary.GetCurrency(), size, new Color(1f, 0.85f, 0.35f));
            GUILayout.Label(compact ? $"{amount}z" : $"金币 {amount}z", large ? _labelStyle : _smallStyle);
            GUILayout.EndHorizontal();
        }

        void DrawMaterialChip(MaterialId id, string amountText)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(IconSm));
            IconLibrary.DrawIcon(IconLibrary.GetMaterial(id), IconSm, IconLibrary.MaterialTint(id));
            GUILayout.Label($"{IdleCombatSystem.ToMaterialName(id)} {amountText}", _smallStyle);
            GUILayout.EndHorizontal();
        }

        void DrawBar(string label, float current, float max, Color color)
        {
            GUILayout.Label($"{label}  {current:0}/{max:0}", _labelStyle);
            Rect rect = GUILayoutUtility.GetRect(18f, 18f);
            float ratio = max <= 0f ? 0f : Mathf.Clamp01(current / max);
            EditorLikeBox(rect, new Color(0.15f, 0.15f, 0.15f, 0.9f));
            Rect fill = new Rect(rect.x, rect.y, rect.width * ratio, rect.height);
            EditorLikeBox(fill, color);
        }

        static void EditorLikeBox(Rect rect, Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = old;
        }

        static string ToSlotName(ArmorSlot slot)
        {
            switch (slot)
            {
                case ArmorSlot.Head: return "头";
                case ArmorSlot.Chest: return "胸";
                case ArmorSlot.Arms: return "腕";
                case ArmorSlot.Waist: return "腰";
                case ArmorSlot.Legs: return "腿";
                default: return slot.ToString();
            }
        }

        void DrawBackground()
        {
            EditorLikeBox(new Rect(0, 0, Screen.width, Screen.height), new Color(0.08f, 0.1f, 0.12f, 0.92f));
        }

        void EnsureStyles()
        {
            if (_stylesReady) return;

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.86f, 0.55f) }
            };
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                wordWrap = true,
                normal = { textColor = new Color(0.9f, 0.9f, 0.88f) }
            };
            _smallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = false,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.82f, 0.82f, 0.8f) }
            };
            _boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };
            _stylesReady = true;
        }
    }
}
