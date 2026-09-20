using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGCommunity.Example.ContainerPostRollRules;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.container-post-roll-rules";
    public const string PluginName = "TG Example - Container Post-Roll Rules";
    public const string PluginVersion = "0.1.0";

    private static readonly ConditionalWeakTable<object, Marker> Processed = new();
    private static ConfigEntry<float>? _quantityMultiplier;
    private Harmony? _harmony;

    private void Awake()
    {
        _quantityMultiplier = Config.Bind("Rules", "ConsumableAndCraftingQuantityMultiplier", 2f, "One-shot multiplier for matching generated rows.");

        Type? searchAction = AccessTools.TypeByName("Awaken.TG.Main.Locations.Actions.SearchAction");
        MethodInfo? onInitialize = searchAction == null ? null : AccessTools.Method(searchAction, "OnInitialize");
        if (onInitialize == null)
        {
            Logger.LogError("SearchAction.OnInitialize was not found.");
            return;
        }

        _harmony = new Harmony(PluginGuid);
        _harmony.Patch(onInitialize, postfix: new HarmonyMethod(typeof(Plugin), nameof(Postfix)));
        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy() => _harmony?.UnpatchSelf();

    private static void Postfix(object __instance)
    {
        if (Processed.TryGetValue(__instance, out _))
            return;

        Processed.Add(__instance, new Marker());

        FieldInfo? rowsField = AccessTools.Field(__instance.GetType(), "_itemsInsideContainer");
        if (rowsField?.GetValue(__instance) is not IEnumerable rows)
            return;

        float multiplier = Math.Max(0f, Math.Min(10f, _quantityMultiplier?.Value ?? 1f));
        foreach (object? row in rows)
        {
            if (row == null) continue;

            object? template = Read(row, "ItemTemplate");
            if (template == null) continue;

            bool matches = ReadBool(template, "IsConsumable") || ReadBool(template, "IsCrafting");
            if (!matches) continue;

            FieldInfo? quantityField = AccessTools.Field(row.GetType(), "quantity");
            if (quantityField?.GetValue(row) is not int quantity) continue;

            int adjusted = Math.Max(0, (int)Math.Round(quantity * multiplier, MidpointRounding.AwayFromZero));
            quantityField.SetValue(row, adjusted);
        }
    }

    private static object? Read(object owner, string name)
    {
        PropertyInfo? property = AccessTools.Property(owner.GetType(), name);
        if (property != null) return property.GetValue(owner);
        FieldInfo? field = AccessTools.Field(owner.GetType(), name);
        return field?.GetValue(owner);
    }

    private static bool ReadBool(object owner, string name) => Read(owner, name) is bool value && value;

    private sealed class Marker { }
}
