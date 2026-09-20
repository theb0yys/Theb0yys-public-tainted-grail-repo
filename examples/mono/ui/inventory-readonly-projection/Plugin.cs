using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Awaken.TG.Main.Heroes;
using BepInEx;
using UnityEngine;

namespace TGCommunity.Example.InventoryReadonlyProjection;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.inventory-readonly-projection";
    public const string PluginName = "TG Example - Inventory Read-Only Projection";
    public const string PluginVersion = "0.1.0";

    private readonly List<Row> _rows = new();
    private bool _open;
    private string _query = string.Empty;
    private Vector2 _scroll;
    private float _nextRefresh;
    private Rect _window = new Rect(30f, 80f, 560f, 600f);

    private void Awake()
    {
        Logger.LogInfo($"{PluginName} loaded. F8 toggles the read-only projection.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
            _open = !_open;

        if (!_open || Time.unscaledTime < _nextRefresh)
            return;

        _nextRefresh = Time.unscaledTime + 0.5f;
        Refresh();
    }

    private void OnGUI()
    {
        if (!_open) return;
        _window = GUI.Window(GetInstanceID(), _window, DrawWindow, "FoA Inventory Projection");
    }

    private void DrawWindow(int id)
    {
        GUILayout.Label("Read-only projection of HeroItems.Inventory");
        _query = GUILayout.TextField(_query ?? string.Empty);

        _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(500f));
        string q = (_query ?? string.Empty).Trim();

        foreach (Row row in _rows)
        {
            if (q.Length > 0 &&
                row.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) < 0 &&
                row.Category.IndexOf(q, StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            GUILayout.Label($"{row.Name}  x{row.Quantity}  [{row.Category}]");
        }

        GUILayout.EndScrollView();
        GUI.DragWindow(new Rect(0, 0, 10000, 24));
    }

    private void Refresh()
    {
        _rows.Clear();

        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded || hero.HeroItems == null)
            return;

        object heroItems = hero.HeroItems;
        object? inventory = Read(heroItems, "Inventory");
        if (inventory is not IEnumerable items)
            return;

        foreach (object? item in items)
        {
            if (item == null) continue;

            object? template = Read(item, "Template");
            if (template == null)
                template = Read(item, "ItemTemplate");

            if (template != null && ReadBool(template, "HiddenOnUI"))
                continue;

            string name = Read(item, "DisplayName")?.ToString()
                ?? Read(template, "name")?.ToString()
                ?? item.GetType().Name;

            int quantity = ReadInt(item, "Quantity", 1);
            string category = Category(template);

            _rows.Add(new Row(name, quantity, category));
        }

        _rows.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
    }

    private static string Category(object? template)
    {
        if (template == null) return "Unknown";
        if (ReadBool(template, "IsWeapon")) return "Weapon";
        if (ReadBool(template, "IsArmor")) return "Armour";
        if (ReadBool(template, "IsConsumable")) return "Consumable";
        if (ReadBool(template, "IsCrafting")) return "Material";
        if (ReadBool(template, "IsQuestItem")) return "Quest";
        return "Misc";
    }

    private static object? Read(object? owner, string name)
    {
        if (owner == null) return null;
        Type type = owner.GetType();

        PropertyInfo? property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property != null && property.GetIndexParameters().Length == 0)
        {
            try { return property.GetValue(owner); } catch { }
        }

        FieldInfo? field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            try { return field.GetValue(owner); } catch { }
        }

        return null;
    }

    private static bool ReadBool(object? owner, string name) => Read(owner, name) is bool value && value;

    private static int ReadInt(object owner, string name, int fallback)
    {
        object? value = Read(owner, name);
        return value is int i ? i : fallback;
    }

    private readonly struct Row
    {
        internal Row(string name, int quantity, string category)
        {
            Name = name;
            Quantity = quantity;
            Category = category;
        }

        internal string Name { get; }
        internal int Quantity { get; }
        internal string Category { get; }
    }
}
