using System;
using System.Collections.Generic;
using MHIdle.Data;
using MHIdle.Model;
using MHIdle.Systems;
using UnityEngine;

namespace MHIdle.UI
{
    /// <summary>
    /// 四页主界面 + 战斗弹窗。熟练度页用同心圆展示外圈/内圈/内内圈。
    /// </summary>
    public class IdleGameUI : MonoBehaviour
    {
        enum MainTab
        {
            Hunter,
            Proficiency,
            Warehouse,
            Forge
        }

        const float IconSm = 22f;
        const float IconMd = 36f;
        const float IconLg = 48f;

        MainTab _tab = MainTab.Hunter;
        Vector2 _scroll;
        Vector2 _forgeScroll;
        GUIStyle _titleStyle;
        GUIStyle _boxStyle;
        GUIStyle _labelStyle;
        GUIStyle _smallStyle;
        GUIStyle _headerStyle;
        bool _stylesReady;
        Texture2D _ringTex;

        void OnGUI()
        {
            if (IdleGameManager.Instance == null) return;

            IconLibrary.EnsureLoaded();
            EnsureStyles();
            DrawBackground();

            var manager = IdleGameManager.Instance;
            var combat = manager.Combat;
            var progress = manager.Progress;

            float pad = 12f;
            Rect root = new Rect(pad, pad, Screen.width - pad * 2f, Screen.height - pad * 2f);
            GUILayout.BeginArea(root);

            GUILayout.Label("MONSTER HUNTER  ·  IDLE", _titleStyle);
            GUILayout.Label("挂机养熟 · 出击突破 · 三圈熟练度 · Build 流派", _smallStyle);
            GUILayout.Space(6f);

            DrawMainTabs();
            GUILayout.Space(6f);

            _scroll = GUILayout.BeginScrollView(_scroll);
            switch (_tab)
            {
                case MainTab.Hunter:
                    DrawHunterPage(manager, combat, progress);
                    break;
                case MainTab.Proficiency:
                    DrawProficiencyPage(progress);
                    break;
                case MainTab.Warehouse:
                    DrawWarehousePage(progress);
                    break;
                case MainTab.Forge:
                    DrawForgePage(manager, progress);
                    break;
            }

            GUILayout.EndScrollView();

            if (!string.IsNullOrEmpty(manager.StatusMessage))
            {
                GUILayout.Space(4f);
                GUILayout.Label($"提示：{manager.StatusMessage}", _labelStyle);
            }

            GUILayout.Label("Icons by Lorc / Delapouite · game-icons.net (CC BY 3.0)", _smallStyle);
            GUILayout.EndArea();

            if (combat.IsCombatPopupOpen)
            {
                DrawCombatPopup(manager, combat, progress);
            }
        }

        void DrawMainTabs()
        {
            GUILayout.BeginHorizontal();
            TabButton("角色", MainTab.Hunter);
            TabButton("熟练度", MainTab.Proficiency);
            TabButton("仓库", MainTab.Warehouse);
            TabButton("制造", MainTab.Forge);
            GUILayout.EndHorizontal();
        }

        void TabButton(string label, MainTab tab)
        {
            var old = GUI.backgroundColor;
            if (_tab == tab) GUI.backgroundColor = new Color(0.85f, 0.62f, 0.28f);
            if (GUILayout.Button(label, GUILayout.Height(30f))) _tab = tab;
            GUI.backgroundColor = old;
        }

        // ——— 第 1 页：角色 ———
        void DrawHunterPage(IdleGameManager manager, IdleCombatSystem combat, HunterProgress progress)
        {
            var weapon = progress.GetEquippedWeapon();
            var wp = progress.GetEquippedWeaponProgress();

            GUILayout.BeginVertical(_boxStyle);
            GUILayout.Label($"猎人 HR{progress.HunterRank}  ({progress.HunterRankExp}/{progress.ExpToNextRank})", _headerStyle);
            DrawCurrencyChip(progress.Zenny);
            GUILayout.Label($"讨伐 {progress.TotalKills}（大怪 {progress.TotalLargeKills}）  战败 {progress.HuntDeaths}", _smallStyle);
            GUILayout.EndVertical();

            GUILayout.Space(6f);
            GUILayout.BeginVertical(_boxStyle);
            GUILayout.Label("当前装备", _headerStyle);
            GUILayout.BeginHorizontal();
            IconLibrary.DrawIcon(IconLibrary.GetWeapon(weapon.Type), IconLg, new Color(0.95f, 0.85f, 0.45f));
            GUILayout.BeginVertical();
            GUILayout.Label($"{weapon.Name} · {WeaponTaxonomy.TypeName(weapon.Type)}", _labelStyle);
            GUILayout.Label($"外圈 Lv.{wp.Outer.Level}  攻击 {progress.GetPlayerAttack():0.0}  防御 {progress.GetTotalDefense():0.0}", _smallStyle);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            foreach (ArmorSlot slot in Enum.GetValues(typeof(ArmorSlot)))
            {
                string armorId = progress.GetEquippedArmorId(slot);
                IconLibrary.DrawIcon(IconLibrary.GetArmor(slot), IconSm, new Color(0.7f, 0.82f, 0.95f));
                GUILayout.Label(armorId ?? "-", _smallStyle);
                GUILayout.Space(4f);
            }

            GUILayout.EndHorizontal();

            // Build 技能面板
            GUILayout.Space(6f);
            var skillFx = progress.GetSkillEffects();
            GUILayout.Label(ArmorSkillSystem.DescribeBuildFocus(skillFx), _headerStyle);
            var board = ArmorSkillSystem.GetSkillBoard(progress);
            if (board.Count == 0)
            {
                GUILayout.Label("当前防具无技能点", _smallStyle);
            }
            else
            {
                foreach (var info in board)
                {
                    string active = info.Tier != null
                        ? $"→ {info.Tier.ActiveName}"
                        : (info.NextThreshold > 0 ? $"（差 {info.NextThreshold - info.Points} 点激活）" : string.Empty);
                    GUILayout.Label($"{ArmorSkillDatabase.SkillName(info.Skill)} {info.Points}  {active}", _labelStyle);
                    if (info.Tier != null)
                    {
                        GUILayout.Label(info.Tier.Description, _smallStyle);
                    }
                }
            }

            GUILayout.EndVertical();

            GUILayout.Space(6f);
            GUILayout.BeginVertical(_boxStyle);
            GUILayout.Label($"出征背包（{progress.LoadoutTypeCount}/{HunterProgress.MaxLoadoutSlots}）", _headerStyle);
            if (progress.Loadout.Count == 0)
            {
                GUILayout.Label("空 · 可从仓库装配道具（原型阶段占位）", _smallStyle);
            }
            else
            {
                foreach (var pair in progress.Loadout)
                {
                    GUILayout.Label($"{pair.Key} x{pair.Value}", _smallStyle);
                }
            }

            GUILayout.EndVertical();

            GUILayout.Space(6f);
            GUILayout.BeginVertical(_boxStyle);
            GUILayout.Label("日常挂机（小怪）", _headerStyle);
            if (combat.Mode == CombatMode.IdleSmall && combat.CurrentMonster != null)
            {
                GUILayout.Label($"当前：{combat.CurrentMonster.Name} · {combat.CurrentMonster.Locale}", _labelStyle);
                DrawBar("猎人", combat.PlayerHp, progress.GetPlayerMaxHp(), new Color(0.35f, 0.75f, 0.45f));
                DrawBar("目标", combat.MonsterHp, combat.CurrentMonster.MaxHp, new Color(0.85f, 0.35f, 0.3f));
                GUILayout.Label(combat.LastRewardSummary, _smallStyle);
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(combat.IsRunning && combat.Mode == CombatMode.IdleSmall ? "暂停挂机" : "开始挂机", GUILayout.Height(32f)))
            {
                if (combat.Mode != CombatMode.IdleSmall) manager.StartIdleFarm();
                else manager.ToggleCombat();
            }

            if (GUILayout.Button("重置进度", GUILayout.Height(32f), GUILayout.Width(100f)))
            {
                manager.ResetProgress();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(6f);
            GUILayout.Label("主动出击（大型怪物）", _headerStyle);
            for (int i = 0; i <= progress.HighestMonsterIndexUnlocked && i < GameDatabase.Monsters.Count; i++)
            {
                var m = GameDatabase.Monsters[i];
                if (m.Size != MonsterSize.Large) continue;

                float rate = HuntSystem.EstimateWinRate(progress, m);
                GUILayout.BeginVertical(_boxStyle);
                GUILayout.Label($"{m.Name}  ·  {m.Locale}  ·  危险度 {m.Rank}", _labelStyle);
                GUILayout.Label(HuntSystem.FormatWinRate(rate), _smallStyle);
                var map = progress.GetMapProgress(m.MapId);
                GUILayout.Label(
                    $"地图熟练 Lv.{map.Ring.Level}" +
                    (map.TrapUnlocked ? " · 陷阱已解锁" : string.Empty) +
                    (map.AdvantageUnlocked ? " · 场地优势" : string.Empty),
                    _smallStyle);

                DrawMiniMap(m.MapId, map);

                if (GUILayout.Button("出击", GUILayout.Height(28f)))
                {
                    manager.StartActiveHunt(i);
                }

                GUILayout.EndVertical();
                GUILayout.Space(4f);
            }

            GUILayout.Space(6f);
            GUILayout.Label("挂机小怪列表", _headerStyle);
            for (int i = 0; i <= progress.HighestMonsterIndexUnlocked && i < GameDatabase.Monsters.Count; i++)
            {
                var m = GameDatabase.Monsters[i];
                if (m.Size != MonsterSize.Small) continue;
                if (GUILayout.Button($"{m.Name}  ·  {m.Locale}  ·  +{m.ZennyReward}z", GUILayout.Height(26f)))
                {
                    manager.SelectMonster(i);
                }
            }
        }

        // ——— 第 2 页：熟练度圆形 ———
        void DrawProficiencyPage(HunterProgress progress)
        {
            var weapon = progress.GetEquippedWeapon();
            var wp = progress.GetEquippedWeaponProgress();
            var typeRing = progress.GetTypeRing(weapon.Type);
            var style = WeaponTaxonomy.GetStyleGroup(weapon.Type);
            var styleRing = progress.GetStyleRing(style);

            GUILayout.Label("三圈熟练度（外圈武器 / 内圈武器种 / 内内圈风格）", _headerStyle);

            float size = Mathf.Min(280f, Screen.width - 48f);
            Rect ringRect = GUILayoutUtility.GetRect(size, size, GUILayout.ExpandWidth(false));
            DrawProficiencyRings(ringRect, wp.Outer, typeRing, styleRing);

            GUILayout.Space(6f);
            GUILayout.BeginVertical(_boxStyle);
            GUILayout.Label($"外圈 · {weapon.Name}  Lv.{wp.Outer.Level}  ({wp.Outer.Exp}/{wp.Outer.ExpToNext})", _labelStyle);
            GUILayout.Label($"内圈 · {WeaponTaxonomy.TypeName(weapon.Type)}系  Lv.{typeRing.Level}  ({typeRing.Exp}/{typeRing.ExpToNext})", _labelStyle);
            GUILayout.Label($"内内圈 · {WeaponTaxonomy.StyleName(style)}  Lv.{styleRing.Level}  ({styleRing.Exp}/{styleRing.ExpToNext})", _labelStyle);
            if (wp.Outer.Level % 5 == 0 && !wp.BottleneckBroken)
            {
                GUILayout.Label("⚠ 外圈瓶颈：请主动讨伐大型怪物突破", _labelStyle);
            }

            GUILayout.EndVertical();

            GUILayout.Space(6f);
            GUILayout.Label("已学招式 / 天赋", _headerStyle);
            GUILayout.BeginVertical(_boxStyle);
            bool any = false;
            foreach (var tech in TechniqueDatabase.All)
            {
                bool unlocked = progress.UnlockedTechniques.Contains(tech.Id.ToString());
                if (tech.WeaponType != weapon.Type && !unlocked) continue;
                any = true;
                string mark = unlocked ? "✓" : "锁";
                GUILayout.Label($"[{mark}] {tech.Name}  需外圈{tech.RequiredOuterLevel}/种{tech.RequiredTypeLevel}", _labelStyle);
                GUILayout.Label(tech.Description, _smallStyle);
            }

            if (!any) GUILayout.Label("暂无相关招式", _smallStyle);
            GUILayout.EndVertical();

            GUILayout.Space(6f);
            GUILayout.Label("地图熟练度", _headerStyle);
            foreach (MapId mapId in Enum.GetValues(typeof(MapId)))
            {
                var map = progress.GetMapProgress(mapId);
                GUILayout.BeginHorizontal(_boxStyle);
                GUILayout.Label($"{WeaponTaxonomy.MapName(mapId)}  Lv.{map.Ring.Level}", _labelStyle);
                if (map.TrapUnlocked) GUILayout.Label("陷阱", _smallStyle);
                if (map.AdvantageUnlocked) GUILayout.Label("优势", _smallStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }

        void DrawProficiencyRings(Rect area, RingProgress outer, RingProgress type, RingProgress style)
        {
            Vector2 c = new Vector2(area.x + area.width * 0.5f, area.y + area.height * 0.5f);
            float maxR = area.width * 0.48f;

            DrawRing(c, maxR, maxR * 0.78f, new Color(0.2f, 0.2f, 0.22f, 0.9f), 1f);
            DrawRing(c, maxR, maxR * 0.78f, new Color(0.95f, 0.75f, 0.3f, 0.95f), outer.Fill01);

            DrawRing(c, maxR * 0.72f, maxR * 0.52f, new Color(0.18f, 0.2f, 0.24f, 0.9f), 1f);
            DrawRing(c, maxR * 0.72f, maxR * 0.52f, new Color(0.45f, 0.75f, 0.95f, 0.95f), type.Fill01);

            DrawRing(c, maxR * 0.46f, maxR * 0.22f, new Color(0.16f, 0.18f, 0.2f, 0.9f), 1f);
            DrawRing(c, maxR * 0.46f, maxR * 0.22f, new Color(0.7f, 0.55f, 0.9f, 0.95f), style.Fill01);

            // 中心文字
            var center = new Rect(c.x - 40f, c.y - 18f, 80f, 36f);
            GUI.Label(center, "风格", _smallStyle);
        }

        void DrawRing(Vector2 center, float outerR, float innerR, Color color, float fill01)
        {
            if (_ringTex == null)
            {
                _ringTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                _ringTex.SetPixel(0, 0, Color.white);
                _ringTex.Apply();
            }

            fill01 = Mathf.Clamp01(fill01);
            int segments = 48;
            float start = -90f;
            float sweep = 360f * fill01;
            Color old = GUI.color;
            GUI.color = color;

            for (int i = 0; i < segments; i++)
            {
                float t0 = i / (float)segments;
                float t1 = (i + 1) / (float)segments;
                float a0 = (start + sweep * t0) * Mathf.Deg2Rad;
                float a1 = (start + sweep * t1) * Mathf.Deg2Rad;

                // 近似用小方块沿弧铺
                float mid = (a0 + a1) * 0.5f;
                float r = (outerR + innerR) * 0.5f;
                float thickness = Mathf.Max(2f, outerR - innerR);
                Vector2 p = center + new Vector2(Mathf.Cos(mid), Mathf.Sin(mid)) * r;
                GUI.DrawTexture(new Rect(p.x - thickness * 0.35f, p.y - thickness * 0.35f, thickness * 0.7f, thickness * 0.7f), _ringTex);
            }

            GUI.color = old;
        }

        // ——— 第 3 页：仓库 ———
        void DrawWarehousePage(HunterProgress progress)
        {
            GUILayout.BeginVertical(_boxStyle);
            DrawCurrencyChip(progress.Zenny, large: true);
            GUILayout.Space(8f);
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
                GUILayout.Space(4f);
            }

            GUILayout.EndVertical();

            GUILayout.Space(8f);
            GUILayout.Label("已拥有武器", _headerStyle);
            foreach (var weapon in GameDatabase.Weapons)
            {
                var wp = progress.Weapons[weapon.Id];
                if (!wp.Owned) continue;
                GUILayout.BeginHorizontal(_boxStyle);
                IconLibrary.DrawIcon(IconLibrary.GetWeapon(weapon.Type), IconMd, new Color(0.95f, 0.85f, 0.45f));
                GUILayout.Label($"{weapon.Name}  外圈 Lv.{wp.Outer.Level}" +
                               (progress.EquippedWeaponId == weapon.Id ? "  [装备中]" : string.Empty), _labelStyle);
                GUILayout.EndHorizontal();
            }
        }

        // ——— 第 4 页：制造 ———
        void DrawForgePage(IdleGameManager manager, HunterProgress progress)
        {
            GUILayout.Label("武器工房", _headerStyle);
            foreach (var weapon in GameDatabase.Weapons)
            {
                var wp = progress.Weapons[weapon.Id];
                GUILayout.BeginVertical(_boxStyle);
                GUILayout.BeginHorizontal();
                IconLibrary.DrawIcon(IconLibrary.GetWeapon(weapon.Type), IconLg, new Color(0.95f, 0.85f, 0.45f));
                GUILayout.BeginVertical();
                GUILayout.Label($"{weapon.Name}  T{weapon.Tier}  伤害 {weapon.BaseDamage}", _labelStyle);
                GUILayout.Label($"{WeaponTaxonomy.TypeName(weapon.Type)} · 解锁 HR{weapon.UnlockHunterRank} · {(wp.Owned ? "已拥有" : "未拥有")}", _smallStyle);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                DrawCostRow(weapon.CraftZenny, weapon.CraftCost);
                GUILayout.BeginHorizontal();
                GUI.enabled = !wp.Owned;
                if (GUILayout.Button("锻造", GUILayout.Height(26f))) manager.CraftWeapon(weapon.Id);
                GUI.enabled = wp.Owned && progress.EquippedWeaponId != weapon.Id;
                if (GUILayout.Button(progress.EquippedWeaponId == weapon.Id ? "装备中" : "装备", GUILayout.Height(26f)))
                    manager.EquipWeapon(weapon.Id);
                GUI.enabled = true;
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(3f);
            }

            GUILayout.Space(8f);
            GUILayout.Label("防具工房", _headerStyle);
            foreach (var armor in GameDatabase.Armors)
            {
                bool owned = progress.OwnsArmor(armor.Id);
                bool equipped = progress.GetEquippedArmorId(armor.Slot) == armor.Id;
                GUILayout.BeginVertical(_boxStyle);
                GUILayout.BeginHorizontal();
                IconLibrary.DrawIcon(IconLibrary.GetArmor(armor.Slot), IconMd, new Color(0.7f, 0.82f, 0.95f));
                GUILayout.BeginVertical();
                GUILayout.Label($"{armor.Name}  防+{armor.Defense:0}  血+{armor.HpBonus:0}", _labelStyle);
                if (armor.SkillPoints != null && armor.SkillPoints.Count > 0)
                {
                    var parts = new List<string>();
                    foreach (var g in armor.SkillPoints)
                    {
                        parts.Add($"{ArmorSkillDatabase.SkillName(g.Skill)}+{g.Points}");
                    }

                    GUILayout.Label(string.Join("  ", parts), _smallStyle);
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                DrawCostRow(armor.CraftZenny, armor.CraftCost);
                GUILayout.BeginHorizontal();
                GUI.enabled = !owned;
                if (GUILayout.Button("锻造", GUILayout.Height(26f))) manager.CraftArmor(armor.Id);
                GUI.enabled = owned && !equipped;
                if (GUILayout.Button(equipped ? "装备中" : "装备", GUILayout.Height(26f)))
                    manager.EquipArmor(armor.Id);
                GUI.enabled = true;
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(3f);
            }
        }

        // ——— 战斗弹窗 ———
        void DrawCombatPopup(IdleGameManager manager, IdleCombatSystem combat, HunterProgress progress)
        {
            Color old = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = old;

            float w = Mathf.Min(420f, Screen.width - 40f);
            float h = Mathf.Min(460f, Screen.height - 40f);
            Rect panel = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.Box(panel, GUIContent.none);
            GUILayout.BeginArea(new Rect(panel.x + 12f, panel.y + 12f, panel.width - 24f, panel.height - 24f));

            var m = combat.CurrentMonster;
            GUILayout.Label("战斗", _titleStyle);
            GUILayout.Label($"{m.Name}  ·  {m.Locale}  ·  大型讨伐", _labelStyle);
            float rate = HuntSystem.EstimateWinRate(progress, m);
            GUILayout.Label(HuntSystem.FormatWinRate(rate), _smallStyle);
            GUILayout.Space(6f);
            DrawBar("猎人 HP", combat.PlayerHp, progress.GetPlayerMaxHp(), new Color(0.35f, 0.75f, 0.45f));
            DrawBar("怪物 HP", combat.MonsterHp, m.MaxHp, new Color(0.85f, 0.35f, 0.3f));
            GUILayout.Label(combat.LastRewardSummary, _smallStyle);

            GUILayout.Space(4f);
            GUILayout.BeginVertical(_boxStyle);
            foreach (var log in combat.Logs)
            {
                GUILayout.Label($"• {log.Text}", _smallStyle);
            }

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(combat.IsRunning ? "暂停" : "继续", GUILayout.Height(34f)))
            {
                manager.ToggleCombat();
            }

            if (GUILayout.Button("返回营地", GUILayout.Height(34f)))
            {
                manager.CloseCombatPopup();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        void DrawMiniMap(MapId mapId, MapProgress map)
        {
            Rect rect = GUILayoutUtility.GetRect(120f, 64f, GUILayout.Width(120f));
            EditorLikeBox(rect, new Color(0.12f, 0.16f, 0.14f, 0.95f));
            // 简易分区色块
            var zones = new[]
            {
                new Rect(rect.x + 4, rect.y + 4, 40, 28),
                new Rect(rect.x + 48, rect.y + 8, 36, 24),
                new Rect(rect.x + 88, rect.y + 4, 28, 36),
                new Rect(rect.x + 12, rect.y + 36, 50, 24),
                new Rect(rect.x + 70, rect.y + 40, 40, 20)
            };
            for (int i = 0; i < zones.Length; i++)
            {
                float shade = 0.25f + (i * 0.08f);
                if (map.AdvantageUnlocked) shade += 0.1f;
                EditorLikeBox(zones[i], new Color(shade, shade + 0.1f, shade * 0.7f, 0.9f));
            }

            if (map.TrapUnlocked)
            {
                EditorLikeBox(new Rect(rect.x + 52, rect.y + 28, 10, 10), new Color(0.9f, 0.4f, 0.2f, 1f));
            }

            GUI.Label(new Rect(rect.x + 4, rect.y + rect.height - 16, rect.width, 16), WeaponTaxonomy.MapName(mapId), _smallStyle);
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
                    GUILayout.Space(6f);
                }

                if (cost != null)
                {
                    foreach (var pair in cost)
                    {
                        DrawMaterialChip(pair.Key, $"x{pair.Value}");
                        GUILayout.Space(4f);
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
            Rect rect = GUILayoutUtility.GetRect(16f, 14f);
            float ratio = max <= 0f ? 0f : Mathf.Clamp01(current / max);
            EditorLikeBox(rect, new Color(0.15f, 0.15f, 0.15f, 0.9f));
            EditorLikeBox(new Rect(rect.x, rect.y, rect.width * ratio, rect.height), color);
        }

        static void EditorLikeBox(Rect rect, Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = old;
        }

        void DrawBackground()
        {
            EditorLikeBox(new Rect(0, 0, Screen.width, Screen.height), new Color(0.08f, 0.1f, 0.12f, 0.94f));
        }

        void EnsureStyles()
        {
            if (_stylesReady) return;
            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.86f, 0.55f) }
            };
            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.92f, 0.9f, 0.78f) }
            };
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true,
                normal = { textColor = new Color(0.9f, 0.9f, 0.88f) }
            };
            _smallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                wordWrap = true,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.78f, 0.78f, 0.76f) }
            };
            _boxStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(8, 8, 8, 8) };
            _stylesReady = true;
        }
    }
}
