using System;
using System.Collections.Generic;
using MHIdle.Data;
using MHIdle.Model;
using MHIdle.Systems;
using UnityEngine;

namespace MHIdle.UI
{
    /// <summary>
    /// 微信小游戏竖屏主界面（标准设备 iPhone 12：390×844）。
    /// 底部四 Tab；熟练度页为上下结构：上方圆环展示等级，下方详情。
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

        // iPhone 12 逻辑分辨率（微信小游戏竖屏基准）
        const float DesignW = 390f;
        const float DesignH = 844f;
        const float SafeTopDesign = 44f;
        const float SafeBottomDesign = 28f;
        const float HeaderDesign = 48f;
        const float TabBarDesign = 58f;

        const float IconSm = 20f;
        const float IconMd = 32f;
        const float IconLg = 44f;

        MainTab _tab = MainTab.Hunter;
        Vector2 _scroll;
        GUIStyle _titleStyle;
        GUIStyle _boxStyle;
        GUIStyle _labelStyle;
        GUIStyle _smallStyle;
        GUIStyle _headerStyle;
        GUIStyle _tabStyle;
        GUIStyle _tabActiveStyle;
        GUIStyle _centerStyle;
        GUIStyle _ringLvStyle;
        bool _stylesReady;
        Texture2D _ringTex;
        float _uiScale = 1f;
        Rect _frame;

        void OnGUI()
        {
            if (IdleGameManager.Instance == null) return;

            IconLibrary.EnsureLoaded();
            EnsureStyles();
            _frame = ComputePhoneFrame();
            _uiScale = _frame.width / DesignW;
            ApplyScaledStyles();

            DrawLetterbox();
            DrawPhoneBackground(_frame);

            var manager = IdleGameManager.Instance;
            var combat = manager.Combat;
            var progress = manager.Progress;

            float safeTop = SafeTopDesign * _uiScale;
            float safeBottom = SafeBottomDesign * _uiScale;
            float headerH = HeaderDesign * _uiScale;
            float tabH = TabBarDesign * _uiScale;
            float pad = 10f * _uiScale;

            Rect header = new Rect(_frame.x + pad, _frame.y + safeTop, _frame.width - pad * 2f, headerH);
            Rect tabBar = new Rect(
                _frame.x + pad,
                _frame.yMax - safeBottom - tabH,
                _frame.width - pad * 2f,
                tabH);
            Rect content = new Rect(
                _frame.x + pad,
                header.yMax + 4f * _uiScale,
                _frame.width - pad * 2f,
                tabBar.y - (header.yMax + 8f * _uiScale));

            DrawHeader(header, progress);
            DrawContent(content, manager, combat, progress);
            DrawBottomTabs(tabBar);

            if (!string.IsNullOrEmpty(manager.StatusMessage))
            {
                var tip = new Rect(_frame.x + pad, tabBar.y - 22f * _uiScale, _frame.width - pad * 2f, 20f * _uiScale);
                GUI.Label(tip, manager.StatusMessage, _smallStyle);
            }

            if (combat.IsCombatPopupOpen)
            {
                DrawCombatPopup(manager, combat, progress);
            }
        }

        static Rect ComputePhoneFrame()
        {
            float targetAspect = DesignW / DesignH;
            float screenAspect = (float)Screen.width / Mathf.Max(1, Screen.height);
            float w, h, x, y;
            if (screenAspect > targetAspect)
            {
                h = Screen.height;
                w = h * targetAspect;
                x = (Screen.width - w) * 0.5f;
                y = 0f;
            }
            else
            {
                w = Screen.width;
                h = w / targetAspect;
                x = 0f;
                y = (Screen.height - h) * 0.5f;
            }

            return new Rect(x, y, w, h);
        }

        void DrawLetterbox()
        {
            EditorLikeBox(new Rect(0, 0, Screen.width, Screen.height), new Color(0.04f, 0.05f, 0.06f, 1f));
        }

        void DrawPhoneBackground(Rect frame)
        {
            // 竖屏氛围底：深绿灰渐变感（用上下两块近似）
            EditorLikeBox(frame, new Color(0.09f, 0.11f, 0.12f, 1f));
            EditorLikeBox(
                new Rect(frame.x, frame.y, frame.width, frame.height * 0.28f),
                new Color(0.12f, 0.16f, 0.14f, 0.55f));
        }

        void DrawHeader(Rect area, HunterProgress progress)
        {
            GUILayout.BeginArea(area);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label("MH IDLE", _titleStyle);
            GUILayout.Label("挂机养熟 · 出击突破", _smallStyle);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(GUILayout.Width(110f * _uiScale));
            GUILayout.Label($"HR{progress.HunterRank}", _headerStyle);
            DrawCurrencyChip(progress.Zenny, compact: true);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        void DrawContent(Rect area, IdleGameManager manager, IdleCombatSystem combat, HunterProgress progress)
        {
            GUILayout.BeginArea(area);
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

            GUILayout.Space(8f * _uiScale);
            GUILayout.Label("Icons · game-icons.net (CC BY 3.0)", _smallStyle);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        void DrawBottomTabs(Rect area)
        {
            GUILayout.BeginArea(area);
            GUILayout.BeginHorizontal();
            TabButton("角色", MainTab.Hunter);
            TabButton("熟练", MainTab.Proficiency);
            TabButton("仓库", MainTab.Warehouse);
            TabButton("制造", MainTab.Forge);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        void TabButton(string label, MainTab tab)
        {
            var old = GUI.backgroundColor;
            GUI.backgroundColor = _tab == tab
                ? new Color(0.9f, 0.72f, 0.35f, 1f)
                : new Color(0.22f, 0.24f, 0.26f, 1f);
            var style = _tab == tab ? _tabActiveStyle : _tabStyle;
            if (GUILayout.Button(label, style, GUILayout.Height(46f * _uiScale)))
            {
                _tab = tab;
                _scroll = Vector2.zero;
            }

            GUI.backgroundColor = old;
        }

        void DrawPlaystylePicker(IdleGameManager manager, HunterProgress progress)
        {
            var current = PlaystyleSystem.Current(progress);
            int equipped = PlaystyleSystem.EquippedSetPieces(progress, current);
            int owned = PlaystyleSystem.OwnedSetPieces(progress, current);

            GUILayout.BeginVertical(_boxStyle);
            GUILayout.Label("选择流派", _headerStyle);
            GUILayout.Label("点选切换；立即获得流派特化，防具/道具会进一步加强。", _smallStyle);

            int i = 0;
            foreach (var def in PlaystyleDatabase.All)
            {
                if (i % 3 == 0)
                {
                    if (i > 0) GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }

                bool selected = def.Id == current.Id;
                bool unlocked = PlaystyleSystem.IsUnlocked(progress, def);
                var old = GUI.backgroundColor;
                if (selected) GUI.backgroundColor = new Color(0.9f, 0.72f, 0.35f, 1f);
                else if (!unlocked) GUI.backgroundColor = new Color(0.16f, 0.16f, 0.18f, 1f);
                else GUI.backgroundColor = new Color(0.22f, 0.24f, 0.26f, 1f);

                GUI.enabled = unlocked || selected;
                string label = unlocked ? def.ShortName : $"{def.ShortName}\nHR{def.UnlockHunterRank}";
                if (GUILayout.Button(label, GUILayout.Height(36f * _uiScale)))
                    manager.SelectPlaystyle(def.Id);
                GUI.enabled = true;
                GUI.backgroundColor = old;
                i++;
            }

            if (i > 0) GUILayout.EndHorizontal();

            GUILayout.Space(4f * _uiScale);
            GUILayout.Label($"当前：{current.Name}", _headerStyle);
            GUILayout.Label(current.Description, _smallStyle);
            GUILayout.Label(
                $"推荐 {current.RecommendedGear} · 武器 {current.RecommendedWeapons} · 已有 {owned} 件 / 穿戴 {equipped}/4",
                _smallStyle);

            if (GUILayout.Button("一键装配推荐防具和道具", GUILayout.Height(30f * _uiScale)))
                manager.EquipPlaystyleGear();

            GUILayout.EndVertical();
        }

        // ——— 第 1 页：角色 ———
        void DrawHunterPage(IdleGameManager manager, IdleCombatSystem combat, HunterProgress progress)
        {
            var weapon = progress.GetEquippedWeapon();
            var wp = progress.GetEquippedWeaponProgress();

            GUILayout.BeginVertical(_boxStyle);
            GUILayout.Label($"猎人 HR{progress.HunterRank}  ({progress.HunterRankExp}/{progress.ExpToNextRank})", _headerStyle);
            GUILayout.Label($"讨伐 {progress.TotalKills}（大怪 {progress.TotalLargeKills}）  战败 {progress.HuntDeaths}", _smallStyle);
            GUILayout.EndVertical();

            GUILayout.Space(6f * _uiScale);
            DrawPlaystylePicker(manager, progress);

            GUILayout.Space(6f * _uiScale);
            GUILayout.BeginVertical(_boxStyle);
            GUILayout.Label("当前装备", _headerStyle);
            GUILayout.BeginHorizontal();
            IconLibrary.DrawIcon(IconLibrary.GetWeapon(weapon.Type), IconLg * _uiScale, new Color(0.95f, 0.85f, 0.45f));
            GUILayout.BeginVertical();
            GUILayout.Label($"{weapon.Name} · {WeaponTaxonomy.TypeName(weapon.Type)}", _labelStyle);
            GUILayout.Label(
                $"{ProficiencyNaming.LevelLabel(ProficiencyNaming.Weapon, wp.Outer.Level)}  攻 {progress.GetPlayerAttack():0.0}  防 {progress.GetTotalDefense():0.0}",
                _smallStyle);
            if (wp.IsProficiencyLocked)
                GUILayout.Label(ProficiencyNaming.BottleneckHint, _smallStyle);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(4f * _uiScale);
            GUILayout.BeginHorizontal();
            foreach (ArmorSlot slot in Enum.GetValues(typeof(ArmorSlot)))
            {
                string armorId = progress.GetEquippedArmorId(slot);
                IconLibrary.DrawIcon(IconLibrary.GetArmor(slot), IconSm * _uiScale, new Color(0.7f, 0.82f, 0.95f));
                GUILayout.Label(ShortArmorName(armorId), _smallStyle);
                GUILayout.FlexibleSpace();
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(6f * _uiScale);
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
                        : (info.NextThreshold > 0 ? $"（差 {info.NextThreshold - info.Points} 点）" : string.Empty);
                    GUILayout.Label($"{ArmorSkillDatabase.SkillName(info.Skill)} {info.Points}  {active}", _labelStyle);
                    if (info.Tier != null)
                        GUILayout.Label(info.Tier.Description, _smallStyle);
                }
            }

            GUILayout.EndVertical();

            GUILayout.Space(6f * _uiScale);
            GUILayout.BeginVertical(_boxStyle);
            GUILayout.Label($"出征背包（{progress.LoadoutTypeCount}/{HunterProgress.MaxLoadoutSlots}）", _headerStyle);
            GUILayout.Label("最多 10 种；战斗自动使用回复/陷阱/炸弹。", _smallStyle);
            if (progress.Loadout.Count == 0)
            {
                GUILayout.Label("空 · 去「仓库」装入", _smallStyle);
            }
            else
            {
                foreach (ItemId id in Enum.GetValues(typeof(ItemId)))
                {
                    int count = progress.GetLoadoutCount(id);
                    if (count <= 0) continue;
                    var def = ItemDatabase.Get(id);
                    GUILayout.BeginHorizontal();
                    IconLibrary.DrawIcon(IconLibrary.GetItem(id), IconSm * _uiScale, IconLibrary.ItemTint(id));
                    GUILayout.Label($"{def.Name} ×{count}", _labelStyle);
                    if (GUILayout.Button("卸1", GUILayout.Width(40f * _uiScale), GUILayout.Height(24f * _uiScale)))
                        manager.UnpackItem(id);
                    if (GUILayout.Button("全卸", GUILayout.Width(40f * _uiScale), GUILayout.Height(24f * _uiScale)))
                        manager.UnpackItem(id, count);
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();

            GUILayout.Space(6f * _uiScale);
            GUILayout.BeginVertical(_boxStyle);
            GUILayout.Label("日常挂机", _headerStyle);
            if (combat.Mode == CombatMode.IdleSmall && combat.CurrentMonster != null)
            {
                GUILayout.Label($"{combat.CurrentMonster.Name} · {combat.CurrentMonster.Locale}", _labelStyle);
                DrawBar("猎人", combat.PlayerHp, progress.GetPlayerMaxHp(), new Color(0.35f, 0.75f, 0.45f));
                DrawBar("目标", combat.MonsterHp, combat.CurrentMonster.MaxHp, new Color(0.85f, 0.35f, 0.3f));
                GUILayout.Label(combat.LastRewardSummary, _smallStyle);
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(combat.IsRunning && combat.Mode == CombatMode.IdleSmall ? "暂停挂机" : "开始挂机",
                    GUILayout.Height(34f * _uiScale)))
            {
                if (combat.Mode != CombatMode.IdleSmall) manager.StartIdleFarm();
                else manager.ToggleCombat();
            }

            if (GUILayout.Button("重置", GUILayout.Height(34f * _uiScale), GUILayout.Width(64f * _uiScale)))
                manager.ResetProgress();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(6f * _uiScale);
            GUILayout.Label("主动出击", _headerStyle);
            for (int i = 0; i <= progress.HighestMonsterIndexUnlocked && i < GameDatabase.Monsters.Count; i++)
            {
                var m = GameDatabase.Monsters[i];
                if (m.Size != MonsterSize.Large) continue;

                float rate = HuntSystem.EstimateWinRate(progress, m);
                GUILayout.BeginVertical(_boxStyle);
                GUILayout.Label($"{m.Name} · {m.Locale} · 危{m.Rank}", _labelStyle);
                GUILayout.Label(HuntSystem.FormatWinRate(rate), _smallStyle);
                var map = progress.GetMapProgress(m.MapId);
                GUILayout.Label(
                    $"地图 Lv.{map.Ring.Level}" +
                    (map.TrapUnlocked ? " · 陷阱" : string.Empty) +
                    (map.AdvantageUnlocked ? " · 优势" : string.Empty),
                    _smallStyle);
                DrawMiniMap(m.MapId, map);
                if (GUILayout.Button("出击", GUILayout.Height(30f * _uiScale)))
                    manager.StartActiveHunt(i);
                GUILayout.EndVertical();
                GUILayout.Space(4f * _uiScale);
            }

            GUILayout.Space(6f * _uiScale);
            GUILayout.Label("挂机小怪", _headerStyle);
            for (int i = 0; i <= progress.HighestMonsterIndexUnlocked && i < GameDatabase.Monsters.Count; i++)
            {
                var m = GameDatabase.Monsters[i];
                if (m.Size != MonsterSize.Small) continue;
                if (GUILayout.Button($"{m.Name} · +{m.ZennyReward}z", GUILayout.Height(28f * _uiScale)))
                    manager.SelectMonster(i);
            }
        }

        // ——— 第 2 页：熟练度（上下结构） ———
        void DrawProficiencyPage(HunterProgress progress)
        {
            var weapon = progress.GetEquippedWeapon();
            var wp = progress.GetEquippedWeaponProgress();
            var typeRing = progress.GetTypeRing(weapon.Type);
            var style = WeaponTaxonomy.GetStyleGroup(weapon.Type);
            var styleRing = progress.GetStyleRing(style);

            // 上：圆环可视化（等级直接画在环上）
            GUILayout.BeginVertical(_boxStyle);
            GUILayout.Label("武器熟练", _headerStyle);
            GUILayout.Label("圆环转满升一级 · 等级见环上数字", _smallStyle);

            float size = Mathf.Min(220f * _uiScale, (_frame.width - 48f * _uiScale));
            Rect ringRect = GUILayoutUtility.GetRect(size, size, GUILayout.ExpandWidth(true));
            // 居中圆环
            float ringSide = Mathf.Min(ringRect.width, ringRect.height);
            Rect centered = new Rect(
                ringRect.x + (ringRect.width - ringSide) * 0.5f,
                ringRect.y + (ringRect.height - ringSide) * 0.5f,
                ringSide,
                ringSide);
            DrawProficiencyRings(centered, wp.Outer, typeRing, styleRing);
            GUILayout.EndVertical();

            GUILayout.Space(8f * _uiScale);

            // 下：三层进度详情（纵向堆叠）
            DrawRingDetailCard(
                ProficiencyNaming.WeaponTitle(weapon.Name),
                wp.Outer,
                new Color(0.95f, 0.75f, 0.3f));
            DrawRingDetailCard(
                ProficiencyNaming.TypeTitle(weapon.Type),
                typeRing,
                new Color(0.45f, 0.75f, 0.95f));
            DrawRingDetailCard(
                ProficiencyNaming.StyleTitle(style),
                styleRing,
                new Color(0.78f, 0.62f, 0.42f));

            if (wp.IsProficiencyLocked)
            {
                GUILayout.Space(4f * _uiScale);
                GUILayout.BeginVertical(_boxStyle);
                GUILayout.Label(ProficiencyNaming.BottleneckHint, _labelStyle);
                GUILayout.EndVertical();
            }

            GUILayout.Space(8f * _uiScale);
            GUILayout.Label("已学招式", _headerStyle);
            GUILayout.BeginVertical(_boxStyle);
            bool any = false;
            foreach (var tech in TechniqueDatabase.All)
            {
                bool unlocked = progress.UnlockedTechniques.Contains(tech.Id.ToString());
                if (tech.WeaponType != weapon.Type && !unlocked) continue;
                any = true;
                string mark = unlocked ? "✓" : "锁";
                GUILayout.Label(
                    $"[{mark}] {tech.Name}  需{ProficiencyNaming.Weapon}{tech.RequiredOuterLevel}/{ProficiencyNaming.Type}{tech.RequiredTypeLevel}",
                    _labelStyle);
                GUILayout.Label(tech.Description, _smallStyle);
            }

            if (!any) GUILayout.Label("暂无相关招式", _smallStyle);
            GUILayout.EndVertical();

            GUILayout.Space(6f * _uiScale);
            GUILayout.Label("地图熟练", _headerStyle);
            foreach (MapId mapId in Enum.GetValues(typeof(MapId)))
            {
                var map = progress.GetMapProgress(mapId);
                GUILayout.BeginVertical(_boxStyle);
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{WeaponTaxonomy.MapName(mapId)}  Lv.{map.Ring.Level}", _labelStyle);
                GUILayout.FlexibleSpace();
                if (map.TrapUnlocked) GUILayout.Label("陷阱", _smallStyle);
                if (map.AdvantageUnlocked) GUILayout.Label("优势", _smallStyle);
                GUILayout.EndHorizontal();
                DrawFillBar(map.Ring.Fill01, new Color(0.45f, 0.7f, 0.55f));
                GUILayout.Label($"{map.Ring.Exp}/{map.Ring.ExpToNext}", _smallStyle);
                GUILayout.EndVertical();
                GUILayout.Space(3f * _uiScale);
            }
        }

        void DrawRingDetailCard(string title, RingProgress ring, Color fillColor)
        {
            GUILayout.BeginVertical(_boxStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label(title, _labelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Lv.{ring.Level}", _headerStyle);
            GUILayout.EndHorizontal();
            DrawFillBar(ring.Fill01, fillColor);
            GUILayout.Label($"{ring.Exp}/{ring.ExpToNext} · 转满升级", _smallStyle);
            GUILayout.EndVertical();
            GUILayout.Space(4f * _uiScale);
        }

        void DrawFillBar(float fill01, Color color)
        {
            Rect rect = GUILayoutUtility.GetRect(12f, 10f * _uiScale);
            EditorLikeBox(rect, new Color(0.15f, 0.15f, 0.15f, 0.9f));
            EditorLikeBox(new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(fill01), rect.height), color);
        }

        void DrawProficiencyRings(
            Rect area,
            RingProgress outer,
            RingProgress type,
            RingProgress style)
        {
            Vector2 c = new Vector2(area.x + area.width * 0.5f, area.y + area.height * 0.5f);
            float maxR = area.width * 0.48f;

            // 专精（外）
            DrawRing(c, maxR, maxR * 0.78f, new Color(0.2f, 0.2f, 0.22f, 0.9f), 1f);
            DrawRing(c, maxR, maxR * 0.78f, new Color(0.95f, 0.75f, 0.3f, 0.95f), outer.Fill01);
            DrawRingLevelBadge(c, (maxR + maxR * 0.78f) * 0.5f, -70f, outer.Level, new Color(0.95f, 0.75f, 0.3f));

            // 武种（中）
            DrawRing(c, maxR * 0.72f, maxR * 0.52f, new Color(0.18f, 0.2f, 0.24f, 0.9f), 1f);
            DrawRing(c, maxR * 0.72f, maxR * 0.52f, new Color(0.45f, 0.75f, 0.95f, 0.95f), type.Fill01);
            DrawRingLevelBadge(c, (maxR * 0.72f + maxR * 0.52f) * 0.5f, 20f, type.Level, new Color(0.45f, 0.75f, 0.95f));

            // 心法（内）
            DrawRing(c, maxR * 0.46f, maxR * 0.22f, new Color(0.16f, 0.18f, 0.2f, 0.9f), 1f);
            DrawRing(c, maxR * 0.46f, maxR * 0.22f, new Color(0.78f, 0.62f, 0.42f, 0.95f), style.Fill01);
            DrawRingLevelBadge(c, (maxR * 0.46f + maxR * 0.22f) * 0.5f, 140f, style.Level, new Color(0.78f, 0.62f, 0.42f));

            // 中心：心法名 + 等级
            float cw = 72f * _uiScale;
            float ch = 40f * _uiScale;
            var center = new Rect(c.x - cw * 0.5f, c.y - ch * 0.5f, cw, ch);
            GUI.Label(center, $"{ProficiencyNaming.Style}\nLv.{style.Level}", _centerStyle);

            DrawLegendChip(
                new Rect(area.x + 4f * _uiScale, area.y + 4f * _uiScale, 78f * _uiScale, 18f * _uiScale),
                ProficiencyNaming.LevelLabel(ProficiencyNaming.Weapon, outer.Level),
                new Color(0.95f, 0.75f, 0.3f));
            DrawLegendChip(
                new Rect(area.xMax - 82f * _uiScale, area.y + 4f * _uiScale, 78f * _uiScale, 18f * _uiScale),
                ProficiencyNaming.LevelLabel(ProficiencyNaming.Type, type.Level),
                new Color(0.45f, 0.75f, 0.95f));
        }

        void DrawLegendChip(Rect rect, string text, Color accent)
        {
            EditorLikeBox(rect, new Color(0.1f, 0.12f, 0.14f, 0.85f));
            EditorLikeBox(new Rect(rect.x, rect.y, 3f * _uiScale, rect.height), accent);
            GUI.Label(new Rect(rect.x + 6f * _uiScale, rect.y, rect.width - 6f * _uiScale, rect.height), text, _smallStyle);
        }

        void DrawRingLevelBadge(Vector2 center, float radius, float angleDeg, int level, Color color)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            Vector2 p = center + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            float s = 22f * _uiScale;
            var r = new Rect(p.x - s * 0.5f, p.y - s * 0.5f, s, s);
            EditorLikeBox(r, new Color(0.08f, 0.09f, 0.1f, 0.92f));
            Color old = GUI.color;
            GUI.color = color;
            GUI.Label(r, level.ToString(), _ringLvStyle);
            GUI.color = old;
        }

        void DrawRing(Vector2 center, float outerR, float innerR, Color color, float fill01)
        {
            EnsurePixel();
            fill01 = Mathf.Clamp01(fill01);
            int segments = 56;
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
                float mid = (a0 + a1) * 0.5f;
                float r = (outerR + innerR) * 0.5f;
                float thickness = Mathf.Max(2f, outerR - innerR);
                Vector2 p = center + new Vector2(Mathf.Cos(mid), Mathf.Sin(mid)) * r;
                GUI.DrawTexture(
                    new Rect(p.x - thickness * 0.35f, p.y - thickness * 0.35f, thickness * 0.7f, thickness * 0.7f),
                    _ringTex);
            }

            GUI.color = old;
        }

        // ——— 第 3 页：仓库 ———
        void DrawWarehousePage(HunterProgress progress)
        {
            var manager = IdleGameManager.Instance;

            GUILayout.BeginVertical(_boxStyle);
            DrawCurrencyChip(progress.Zenny, large: true);
            GUILayout.Space(6f * _uiScale);
            GUILayout.Label("素材", _headerStyle);
            foreach (MaterialId id in Enum.GetValues(typeof(MaterialId)))
            {
                GUILayout.BeginHorizontal();
                IconLibrary.DrawIcon(IconLibrary.GetMaterial(id), IconMd * _uiScale, IconLibrary.MaterialTint(id));
                GUILayout.BeginVertical();
                GUILayout.Label(IdleCombatSystem.ToMaterialName(id), _labelStyle);
                GUILayout.Label($"持有 x{progress.GetMaterial(id)}", _smallStyle);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(3f * _uiScale);
            }

            GUILayout.EndVertical();

            GUILayout.Space(8f * _uiScale);
            GUILayout.Label("道具 → 出征背包", _headerStyle);
            GUILayout.Label($"占用 {progress.LoadoutTypeCount}/{HunterProgress.MaxLoadoutSlots}", _smallStyle);
            foreach (var def in ItemDatabase.All)
            {
                int stock = progress.GetItem(def.Id);
                int packed = progress.GetLoadoutCount(def.Id);
                GUILayout.BeginVertical(_boxStyle);
                GUILayout.BeginHorizontal();
                IconLibrary.DrawIcon(IconLibrary.GetItem(def.Id), IconMd * _uiScale, IconLibrary.ItemTint(def.Id));
                GUILayout.BeginVertical();
                GUILayout.Label($"{def.Name}  仓{stock}  包{packed}/{def.MaxStack}", _labelStyle);
                GUILayout.Label(def.Description, _smallStyle);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUI.enabled = stock > 0;
                if (GUILayout.Button("+1", GUILayout.Height(28f * _uiScale))) manager.PackItem(def.Id);
                if (GUILayout.Button("+5", GUILayout.Height(28f * _uiScale))) manager.PackItem(def.Id, 5);
                GUI.enabled = packed > 0;
                if (GUILayout.Button("-1", GUILayout.Height(28f * _uiScale))) manager.UnpackItem(def.Id);
                GUI.enabled = true;
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(3f * _uiScale);
            }

            GUILayout.Space(8f * _uiScale);
            GUILayout.Label("已拥有武器", _headerStyle);
            foreach (var weapon in GameDatabase.Weapons)
            {
                var wp = progress.Weapons[weapon.Id];
                if (!wp.Owned) continue;
                GUILayout.BeginHorizontal(_boxStyle);
                IconLibrary.DrawIcon(IconLibrary.GetWeapon(weapon.Type), IconMd * _uiScale, new Color(0.95f, 0.85f, 0.45f));
                GUILayout.Label(
                    $"{weapon.Name}  {ProficiencyNaming.LevelLabel(ProficiencyNaming.Weapon, wp.Outer.Level)}" +
                    (progress.EquippedWeaponId == weapon.Id ? "  [装备]" : string.Empty),
                    _labelStyle);
                GUILayout.EndHorizontal();
            }
        }

        // ——— 第 4 页：制造 ———
        void DrawForgePage(IdleGameManager manager, HunterProgress progress)
        {
            GUILayout.Label("道具店", _headerStyle);
            foreach (var def in ItemDatabase.All)
            {
                GUILayout.BeginVertical(_boxStyle);
                GUILayout.BeginHorizontal();
                IconLibrary.DrawIcon(IconLibrary.GetItem(def.Id), IconMd * _uiScale, IconLibrary.ItemTint(def.Id));
                GUILayout.BeginVertical();
                GUILayout.Label($"{def.Name}  库存 {progress.GetItem(def.Id)}", _labelStyle);
                GUILayout.Label($"{def.Description}", _smallStyle);
                GUILayout.Label($"价 {def.ShopPrice}z · HR{def.UnlockHunterRank}", _smallStyle);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                if (def.CraftCost != null && def.CraftCost.Count > 0)
                    DrawCostRow(def.CraftZenny, def.CraftCost);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("买1", GUILayout.Height(28f * _uiScale))) manager.BuyItem(def.Id);
                if (GUILayout.Button("买5", GUILayout.Height(28f * _uiScale))) manager.BuyItem(def.Id, 5);
                GUI.enabled = def.CraftCost != null && def.CraftCost.Count > 0;
                if (GUILayout.Button("造", GUILayout.Height(28f * _uiScale))) manager.CraftItem(def.Id);
                GUI.enabled = true;
                if (GUILayout.Button("装入", GUILayout.Height(28f * _uiScale))) manager.PackItem(def.Id);
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(3f * _uiScale);
            }

            GUILayout.Space(8f * _uiScale);
            GUILayout.Label("武器工房", _headerStyle);
            foreach (var weapon in GameDatabase.Weapons)
            {
                var wp = progress.Weapons[weapon.Id];
                GUILayout.BeginVertical(_boxStyle);
                GUILayout.BeginHorizontal();
                IconLibrary.DrawIcon(IconLibrary.GetWeapon(weapon.Type), IconLg * _uiScale, new Color(0.95f, 0.85f, 0.45f));
                GUILayout.BeginVertical();
                GUILayout.Label($"{weapon.Name}  T{weapon.Tier}", _labelStyle);
                GUILayout.Label($"伤{weapon.BaseDamage} · {WeaponTaxonomy.TypeName(weapon.Type)} · {(wp.Owned ? "已有" : "未有")}", _smallStyle);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                DrawCostRow(weapon.CraftZenny, weapon.CraftCost);
                GUILayout.BeginHorizontal();
                GUI.enabled = !wp.Owned;
                if (GUILayout.Button("锻造", GUILayout.Height(28f * _uiScale))) manager.CraftWeapon(weapon.Id);
                GUI.enabled = wp.Owned && progress.EquippedWeaponId != weapon.Id;
                if (GUILayout.Button(progress.EquippedWeaponId == weapon.Id ? "装备中" : "装备", GUILayout.Height(28f * _uiScale)))
                    manager.EquipWeapon(weapon.Id);
                GUI.enabled = true;
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(3f * _uiScale);
            }

            GUILayout.Space(8f * _uiScale);
            GUILayout.Label("防具工房", _headerStyle);
            foreach (var armor in GameDatabase.Armors)
            {
                bool owned = progress.OwnsArmor(armor.Id);
                bool equipped = progress.GetEquippedArmorId(armor.Slot) == armor.Id;
                GUILayout.BeginVertical(_boxStyle);
                GUILayout.BeginHorizontal();
                IconLibrary.DrawIcon(IconLibrary.GetArmor(armor.Slot), IconMd * _uiScale, new Color(0.7f, 0.82f, 0.95f));
                GUILayout.BeginVertical();
                GUILayout.Label($"{armor.Name}  防+{armor.Defense:0}  血+{armor.HpBonus:0}", _labelStyle);
                if (armor.SkillPoints != null && armor.SkillPoints.Count > 0)
                {
                    var parts = new List<string>();
                    foreach (var g in armor.SkillPoints)
                        parts.Add($"{ArmorSkillDatabase.SkillName(g.Skill)}+{g.Points}");
                    GUILayout.Label(string.Join("  ", parts), _smallStyle);
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                DrawCostRow(armor.CraftZenny, armor.CraftCost);
                GUILayout.BeginHorizontal();
                GUI.enabled = !owned;
                if (GUILayout.Button("锻造", GUILayout.Height(28f * _uiScale))) manager.CraftArmor(armor.Id);
                GUI.enabled = owned && !equipped;
                if (GUILayout.Button(equipped ? "装备中" : "装备", GUILayout.Height(28f * _uiScale)))
                    manager.EquipArmor(armor.Id);
                GUI.enabled = true;
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(3f * _uiScale);
            }
        }

        // ——— 战斗弹窗 ———
        void DrawCombatPopup(IdleGameManager manager, IdleCombatSystem combat, HunterProgress progress)
        {
            Color old = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = old;

            float w = Mathf.Min(_frame.width - 24f * _uiScale, 360f * _uiScale);
            float h = Mathf.Min(_frame.height - 80f * _uiScale, 520f * _uiScale);
            Rect panel = new Rect(_frame.center.x - w * 0.5f, _frame.center.y - h * 0.5f, w, h);
            EditorLikeBox(panel, new Color(0.12f, 0.14f, 0.15f, 0.98f));
            GUILayout.BeginArea(new Rect(panel.x + 12f * _uiScale, panel.y + 12f * _uiScale, panel.width - 24f * _uiScale, panel.height - 24f * _uiScale));

            var m = combat.CurrentMonster;
            float rate = HuntSystem.EstimateWinRate(progress, m);
            GUILayout.Label("讨伐", _titleStyle);
            GUILayout.Label($"{m.Name} · {m.Locale}", _labelStyle);
            GUILayout.Label($"流派 {PlaystyleSystem.Current(progress).Name}  ·  {HuntSystem.FormatWinRate(rate)}", _smallStyle);
            GUILayout.Space(6f * _uiScale);
            DrawBar("猎人", combat.PlayerHp, progress.GetPlayerMaxHp(), new Color(0.35f, 0.75f, 0.45f));
            DrawBar("怪物", combat.MonsterHp, m.MaxHp, new Color(0.85f, 0.35f, 0.3f));
            GUILayout.Label(combat.LastRewardSummary, _smallStyle);
            if (combat.ItemState != null)
            {
                string buff = string.Empty;
                if (combat.ItemState.AttackBuffMul > 1.01f) buff += " 鬼人";
                if (combat.ItemState.DefenseBuffMul > 1.01f) buff += " 硬化";
                if (combat.ItemState.AttackIntervalMul < 0.99f) buff += " 强走";
                if (combat.ItemState.ImmobilizeTimer > 0f) buff += $" 定身{combat.ItemState.ImmobilizeTimer:0.0}s";
                if (combat.ItemState.FlashTimer > 0f) buff += " 闪光";
                if (!string.IsNullOrEmpty(buff))
                    GUILayout.Label("状态：" + buff.Trim(), _smallStyle);
                if (!string.IsNullOrEmpty(combat.ItemState.LastItemLog))
                    GUILayout.Label(combat.ItemState.LastItemLog, _smallStyle);
            }

            GUILayout.Label($"携带：{CombatItemController.SummarizeLoadout(progress)}", _smallStyle);

            GUILayout.Space(4f * _uiScale);
            GUILayout.BeginVertical(_boxStyle);
            foreach (var log in combat.Logs)
                GUILayout.Label($"• {log.Text}", _smallStyle);
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(combat.IsRunning ? "暂停" : "继续", GUILayout.Height(36f * _uiScale)))
                manager.ToggleCombat();
            if (GUILayout.Button("返回", GUILayout.Height(36f * _uiScale)))
                manager.CloseCombatPopup();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        void DrawMiniMap(MapId mapId, MapProgress map)
        {
            float mw = 120f * _uiScale;
            float mh = 56f * _uiScale;
            Rect rect = GUILayoutUtility.GetRect(mw, mh, GUILayout.Width(mw));
            EditorLikeBox(rect, new Color(0.12f, 0.16f, 0.14f, 0.95f));
            var zones = new[]
            {
                new Rect(rect.x + 4, rect.y + 4, 40 * _uiScale, 24 * _uiScale),
                new Rect(rect.x + 48 * _uiScale, rect.y + 6 * _uiScale, 32 * _uiScale, 20 * _uiScale),
                new Rect(rect.x + 86 * _uiScale, rect.y + 4 * _uiScale, 26 * _uiScale, 30 * _uiScale),
                new Rect(rect.x + 10 * _uiScale, rect.y + 30 * _uiScale, 46 * _uiScale, 20 * _uiScale),
                new Rect(rect.x + 64 * _uiScale, rect.y + 34 * _uiScale, 36 * _uiScale, 16 * _uiScale)
            };
            for (int i = 0; i < zones.Length; i++)
            {
                float shade = 0.25f + (i * 0.08f);
                if (map.AdvantageUnlocked) shade += 0.1f;
                EditorLikeBox(zones[i], new Color(shade, shade + 0.1f, shade * 0.7f, 0.9f));
            }

            if (map.TrapUnlocked)
                EditorLikeBox(new Rect(rect.x + 50f * _uiScale, rect.y + 24f * _uiScale, 8f * _uiScale, 8f * _uiScale),
                    new Color(0.9f, 0.4f, 0.2f, 1f));

            GUI.Label(new Rect(rect.x + 4, rect.y + rect.height - 14f * _uiScale, rect.width, 14f * _uiScale),
                WeaponTaxonomy.MapName(mapId), _smallStyle);
        }

        static string ShortArmorName(string armorId)
        {
            if (string.IsNullOrEmpty(armorId)) return "-";
            int us = armorId.IndexOf('_');
            return us > 0 ? armorId.Substring(0, Mathf.Min(us, 4)) : armorId;
        }

        void DrawCostRow(int zenny, Dictionary<MaterialId, int> cost)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("消耗：", _smallStyle);
            if (zenny <= 0 && (cost == null || cost.Count == 0))
            {
                GUILayout.Label("免费", _smallStyle);
            }
            else
            {
                if (zenny > 0)
                {
                    DrawCurrencyChip(zenny, compact: true);
                    GUILayout.Space(4f * _uiScale);
                }

                if (cost != null)
                {
                    foreach (var pair in cost)
                    {
                        DrawMaterialChip(pair.Key, $"x{pair.Value}");
                        GUILayout.Space(2f * _uiScale);
                    }
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        void DrawCurrencyChip(int amount, bool large = false, bool compact = false)
        {
            float size = (large ? IconMd : IconSm) * _uiScale;
            GUILayout.BeginHorizontal(GUILayout.Height(size));
            IconLibrary.DrawIcon(IconLibrary.GetCurrency(), size, new Color(1f, 0.85f, 0.35f));
            GUILayout.Label(compact ? $"{amount}z" : $"金币 {amount}z", large ? _labelStyle : _smallStyle);
            GUILayout.EndHorizontal();
        }

        void DrawMaterialChip(MaterialId id, string amountText)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(IconSm * _uiScale));
            IconLibrary.DrawIcon(IconLibrary.GetMaterial(id), IconSm * _uiScale, IconLibrary.MaterialTint(id));
            GUILayout.Label($"{IdleCombatSystem.ToMaterialName(id)} {amountText}", _smallStyle);
            GUILayout.EndHorizontal();
        }

        void DrawBar(string label, float current, float max, Color color)
        {
            GUILayout.Label($"{label}  {current:0}/{max:0}", _labelStyle);
            Rect rect = GUILayoutUtility.GetRect(16f, 12f * _uiScale);
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

        void EnsurePixel()
        {
            if (_ringTex == null)
            {
                _ringTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                _ringTex.SetPixel(0, 0, Color.white);
                _ringTex.Apply();
            }
        }

        void EnsureStyles()
        {
            if (_stylesReady) return;
            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.86f, 0.55f) }
            };
            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.92f, 0.9f, 0.78f) }
            };
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true,
                normal = { textColor = new Color(0.9f, 0.9f, 0.88f) }
            };
            _smallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                wordWrap = true,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.78f, 0.78f, 0.76f) }
            };
            _centerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal = { textColor = new Color(0.92f, 0.88f, 0.78f) }
            };
            _ringLvStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            _tabStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.75f, 0.75f, 0.72f) }
            };
            _tabActiveStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.15f, 0.12f, 0.08f) }
            };
            _boxStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(8, 8, 8, 8) };
            _stylesReady = true;
        }

        void ApplyScaledStyles()
        {
            int ScaleFont(int baseSize) => Mathf.Max(9, Mathf.RoundToInt(baseSize * _uiScale));

            _titleStyle.fontSize = ScaleFont(20);
            _headerStyle.fontSize = ScaleFont(14);
            _labelStyle.fontSize = ScaleFont(12);
            _smallStyle.fontSize = ScaleFont(10);
            _centerStyle.fontSize = ScaleFont(11);
            _ringLvStyle.fontSize = ScaleFont(11);
            _tabStyle.fontSize = ScaleFont(13);
            _tabActiveStyle.fontSize = ScaleFont(13);
            _tabActiveStyle.normal.textColor = new Color(0.12f, 0.1f, 0.06f);
            _tabStyle.normal.textColor = new Color(0.78f, 0.78f, 0.74f);
        }

        void OnEnable()
        {
            // 微信小游戏 / 移动端强制竖屏
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
        }
    }
}
