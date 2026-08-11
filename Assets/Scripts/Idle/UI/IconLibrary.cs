using System.Collections.Generic;
using System.IO;
using MHIdle.Data;
using UnityEngine;

namespace MHIdle.UI
{
    /// <summary>
    /// 运行时加载道具 / 装备 icon（PNG），并提供着色绘制。
    /// 优先 Resources/Icons，其次 Assets/Art/Icons 原始文件。
    /// </summary>
    public static class IconLibrary
    {
        static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();
        static bool _initialized;

        public static void EnsureLoaded()
        {
            if (_initialized) return;
            _initialized = true;

            Load("material_bone");
            Load("material_hide");
            Load("material_claw");
            Load("material_gem");
            Load("material_blood");
            Load("currency_zenny");
            Load("weapon_greatsword");
            Load("armor_head");
            Load("armor_chest");
            Load("armor_arms");
            Load("armor_legs");
        }

        public static Texture2D GetMaterial(MaterialId id)
        {
            EnsureLoaded();
            switch (id)
            {
                case MaterialId.MonsterBone: return Get("material_bone");
                case MaterialId.MonsterHide: return Get("material_hide");
                case MaterialId.SharpClaw: return Get("material_claw");
                case MaterialId.WyvernGem: return Get("material_gem");
                case MaterialId.ElderDragonBlood: return Get("material_blood");
                default: return null;
            }
        }

        public static Texture2D GetCurrency()
        {
            EnsureLoaded();
            return Get("currency_zenny");
        }

        public static Texture2D GetWeapon(WeaponType type)
        {
            EnsureLoaded();
            // 目前只有大剑线
            return Get("weapon_greatsword");
        }

        public static Texture2D GetArmor(ArmorSlot slot)
        {
            EnsureLoaded();
            switch (slot)
            {
                case ArmorSlot.Head: return Get("armor_head");
                case ArmorSlot.Chest: return Get("armor_chest");
                case ArmorSlot.Arms: return Get("armor_arms");
                case ArmorSlot.Legs: return Get("armor_legs");
                default: return null;
            }
        }

        public static Color MaterialTint(MaterialId id)
        {
            switch (id)
            {
                case MaterialId.MonsterBone: return new Color(0.92f, 0.88f, 0.72f);
                case MaterialId.MonsterHide: return new Color(0.72f, 0.52f, 0.32f);
                case MaterialId.SharpClaw: return new Color(0.85f, 0.78f, 0.7f);
                case MaterialId.WyvernGem: return new Color(0.45f, 0.85f, 0.95f);
                case MaterialId.ElderDragonBlood: return new Color(0.85f, 0.25f, 0.3f);
                default: return Color.white;
            }
        }

        public static void DrawIcon(Texture2D tex, float size, Color tint)
        {
            Rect rect = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size), GUILayout.Height(size));
            DrawIcon(rect, tex, tint);
        }

        public static void DrawIcon(Rect rect, Texture2D tex, Color tint)
        {
            if (tex == null)
            {
                Color oldBg = GUI.color;
                GUI.color = new Color(0.25f, 0.25f, 0.25f, 0.8f);
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = oldBg;
                return;
            }

            Color old = GUI.color;
            GUI.color = tint;
            GUI.DrawTexture(rect, tex, ScaleMode.ScaleToFit, true);
            GUI.color = old;
        }

        static Texture2D Get(string key)
        {
            return Cache.TryGetValue(key, out var tex) ? tex : null;
        }

        static void Load(string key)
        {
            if (Cache.ContainsKey(key)) return;

            Texture2D fromResources = Resources.Load<Texture2D>("Icons/" + key);
            if (fromResources != null)
            {
                Cache[key] = EnsureReadableTintable(fromResources, key);
                return;
            }

            string path = Path.Combine(Application.dataPath, "Art", "Icons", key + ".png");
            if (!File.Exists(path))
            {
                Debug.LogWarning($"Icon 未找到: {key}");
                return;
            }

            byte[] bytes = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(bytes))
            {
                Debug.LogWarning($"Icon 解码失败: {key}");
                return;
            }

            tex.name = key;
            tex.filterMode = FilterMode.Bilinear;
            NormalizeSilhouette(tex);
            Cache[key] = tex;
        }

        static Texture2D EnsureReadableTintable(Texture2D source, string key)
        {
            // Resources 导入的贴图可能不可读；运行时再走文件路径更稳妥
            string path = Path.Combine(Application.dataPath, "Art", "Icons", key + ".png");
            if (File.Exists(path))
            {
                byte[] bytes = File.ReadAllBytes(path);
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (tex.LoadImage(bytes))
                {
                    tex.name = key;
                    tex.filterMode = FilterMode.Bilinear;
                    NormalizeSilhouette(tex);
                    return tex;
                }
            }

            return source;
        }

        /// <summary>
        /// 将剪影统一成白色（保留 alpha），便于 GUI.color 着色。
        /// </summary>
        static void NormalizeSilhouette(Texture2D tex)
        {
            Color32[] pixels = tex.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                byte a = pixels[i].a;
                if (a == 0) continue;
                pixels[i] = new Color32(255, 255, 255, a);
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, false);
        }
    }
}
