using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using UnityEngine;

namespace TGCommunity.Example.InventoryProjection;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.inventory-projection";
    public const string PluginName = "TG Example - Inventory Projection";
    public const string PluginVersion = "0.1.0";

    private const BindingFlags InstanceFlags =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private const BindingFlags StaticFlags =
        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    private bool _open;
    private string _query = string.Empty;
    private Vector2 _scroll;
    private CursorLockMode _oldLock;
    private bool _oldVisible;
    private bool _cursorCaptured;

    private void Awake()
    {
        Logger.LogInfo($"{PluginName} loaded. F8 toggles the read-only projection.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            SetOpen(!_open);
        }

        if (_open && Input.GetKeyDown(KeyCode.Escape))
        {
            SetOpen(false);
        }
    }

    private void OnDisable()
    {
        SetOpen(false);
    }

    private void OnDestroy()
    {
        SetOpen(false);
    }

    private void OnGUI()
    {
        if (!_open)
        {
            return;
        }

        Rect panel = new Rect(40f, 70f, Math.Min(720f, Screen.width - 80f), Math.Min(620f, Screen.height - 120f));
        GUI.Box(panel, "Read-only FoA Inventory Projection");

        GUI.Label(new Rect(panel.x + 18f, panel.y + 34f, 58f, 24f), "Search");
        _query = GUI.TextField(new Rect(panel.x + 80f, panel.y + 32f, panel.width - 100f, 26f), _query);

        IReadOnlyList<Row> rows = ReadInventory(out string diagnostic);
        string q = (_query ?? string.Empty).Trim();

        IEnumerable<Row> visible = rows;
        if (q.Length > 0)
        {
            visible = rows.Where(row =>
                row.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                row.Category.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                row.Template.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        Row[] materialized = visible.OrderBy(row => row.Name, StringComparer.CurrentCultureIgnoreCase).ToArray();
        GUI.Label(new Rect(panel.x + 18f, panel.y + 64f, panel.width - 36f, 22f), $"{diagnostic}; visible={materialized.Length}");

        Rect view = new Rect(panel.x + 16f, panel.y + 92f, panel.width - 32f, panel.height - 110f);
        Rect content = new Rect(0f, 0f, view.width - 20f, Math.Max(view.height, materialized.Length * 28f));
        _scroll = GUI.BeginScrollView(view, _scroll, content);

        float y = 2f;
        foreach (Row row in materialized)
        {
            GUI.Label(new Rect(4f, y, content.width - 8f, 24f),
                $"{row.Name}  x{row.Quantity}  [{row.Category}]  {row.Template}");
            y += 28f;
        }

        GUI.EndScrollView();
    }

    private void SetOpen(bool open)
    {
        if (_open == open)
        {
            return;
        }

        _open = open;

        if (open)
        {
            _oldLock = Cursor.lockState;
            _oldVisible = Cursor.visible;
            _cursorCaptured = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (_cursorCaptured)
        {
            Cursor.lockState = _oldLock;
            Cursor.visible = _oldVisible;
            _cursorCaptured = false;
        }
    }

    private static IReadOnlyList<Row> ReadInventory(out string diagnostic)
    {
        var result = new List<Row>();
        diagnostic = "native-items=0";

        try
        {
            Assembly? tgMain = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(assembly =>
                    string.Equals(assembly.GetName().Name, "TG.Main", StringComparison.Ordinal));

            Type? heroType = tgMain?.GetType("Awaken.TG.Main.Heroes.Hero", false);
            object? hero = heroType == null ? null : ReadStatic(heroType, "Current");
            object? heroItems = hero == null ? null : Read(hero, "HeroItems");
            object? inventory = heroItems == null ? null : Read(heroItems, "Inventory");

            if (inventory is not IEnumerable enumerable)
            {
                diagnostic = "HeroItems.Inventory unavailable";
                return result;
            }

            foreach (object? item in enumerable)
            {
                if (item == null ||
                    !string.Equals(
                        item.GetType().FullName,
                        "Awaken.TG.Main.Heroes.Items.Item",
                        StringComparison.Ordinal))
                {
                    continue;
                }

                if (ReadBool(item, "HiddenOnUI"))
                {
                    continue;
                }

                object? template = Read(item, "Template");

                result.Add(new Row(
                    "item@" + RuntimeHelpers.GetHashCode(item).ToString("x8"),
                    ReadText(item, "DisplayName", "(unnamed)"),
                    ReadText(template, "GUID", ReadText(template, "name", string.Empty)),
                    ReadInt(item, "Quantity", 1),
                    Category(item, template)));
            }

            diagnostic = $"native-items={result.Count}; source=HeroItems.Inventory";
        }
        catch (Exception ex)
        {
            diagnostic = $"projection failed: {ex.GetType().Name}";
        }

        return result;
    }

    private static string Category(object item, object? template)
    {
        if (ReadBool(item, "IsWeapon") || ReadBool(template, "IsWeapon")) return "Weapon";
        if (ReadBool(item, "IsArmor") || ReadBool(template, "IsArmor")) return "Armour";
        if (ReadBool(item, "IsConsumable") || ReadBool(template, "IsConsumable")) return "Consumable";
        if (ReadBool(item, "IsCrafting") || ReadBool(template, "IsCrafting")) return "Material";
        if (ReadBool(item, "IsQuestItem") || ReadBool(template, "IsQuestItem")) return "Quest";
        if (ReadBool(item, "IsKey") || ReadBool(template, "IsKey")) return "Key";
        return "Misc";
    }

    private static object? ReadStatic(Type type, string name)
    {
        PropertyInfo? property = type.GetProperty(name, StaticFlags);
        if (property != null && property.GetIndexParameters().Length == 0)
        {
            return property.GetValue(null);
        }

        return type.GetField(name, StaticFlags)?.GetValue(null);
    }

    private static object? Read(object? owner, string name)
    {
        if (owner == null) return null;

        Type type = owner.GetType();
        PropertyInfo? property = type.GetProperty(name, InstanceFlags);
        if (property != null && property.GetIndexParameters().Length == 0)
        {
            try { return property.GetValue(owner); } catch { return null; }
        }

        FieldInfo? field = type.GetField(name, InstanceFlags);
        if (field != null)
        {
            try { return field.GetValue(owner); } catch { return null; }
        }

        return null;
    }

    private static bool ReadBool(object? owner, string name)
    {
        return Read(owner, name) is bool value && value;
    }

    private static int ReadInt(object? owner, string name, int fallback)
    {
        object? value = Read(owner, name);
        try { return value == null ? fallback : Convert.ToInt32(value); }
        catch { return fallback; }
    }

    private static string ReadText(object? owner, string name, string fallback)
    {
        object? value = Read(owner, name);
        if (value == null) return fallback;
        if (value is string text) return text;

        object? translated = Read(value, "Text") ?? Read(value, "Value");
        return translated?.ToString() ?? value.ToString() ?? fallback;
    }

    private sealed class Row
    {
        internal Row(string key, string name, string template, int quantity, string category)
        {
            Key = key;
            Name = name;
            Template = template;
            Quantity = quantity;
            Category = category;
        }

        internal string Key { get; }
        internal string Name { get; }
        internal string Template { get; }
        internal int Quantity { get; }
        internal string Category { get; }
    }
}
