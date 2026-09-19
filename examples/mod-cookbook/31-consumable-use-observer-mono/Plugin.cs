using System;
using System.Collections.Generic;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Items;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace TGExample.ConsumableUseObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.consumable-use-observer";
    public const string PluginName = "TG Example - Consumable Use Observer";
    public const string PluginVersion = "0.1.0";

    internal static ManualLogSource? LogSource { get; private set; }

    private Harmony? _harmony;

    private void Awake()
    {
        LogSource = Logger;
        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{PluginName} loaded. This example observes hero-owned consumable use only.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        LogSource = null;
    }
}

[HarmonyPatch(typeof(Item), nameof(Item.Use))]
internal static class ConsumableUsePatch
{
    private static void Prefix(Item __instance)
    {
        try
        {
            Hero? hero = Hero.Current;
            if (hero == null
                || hero.HasBeenDiscarded
                || __instance == null
                || !ReferenceEquals(__instance.Character, hero))
            {
                return;
            }

            ItemTemplate? template = SafeTemplate(__instance);
            if (template == null || !IsConsumableCandidate(template))
            {
                return;
            }

            Plugin.LogSource?.LogInfo(
                $"Consumable use: item={SafeItemName(__instance, template)}; " +
                $"guid={SafeGuid(template)}; quantity={SafeQuantity(__instance)}; " +
                $"flags={DescribeFlags(template)}");
        }
        catch (Exception ex)
        {
            Plugin.LogSource?.LogWarning($"Consumable use observation failed: {ex.Message}");
        }
    }

    private static bool IsConsumableCandidate(ItemTemplate template)
    {
        return SafeBool(() => template.IsConsumable)
            || SafeBool(() => template.IsPotion)
            || SafeBool(() => template.IsPlainFood)
            || SafeBool(() => template.IsDish)
            || SafeBool(() => template.IsFish)
            || SafeBool(() => template.IsAlcohol)
            || SafeBool(() => template.ConsumablePotionOther);
    }

    private static string DescribeFlags(ItemTemplate template)
    {
        List<string> flags = new();

        AddFlag(flags, SafeBool(() => template.IsConsumable), "consumable");
        AddFlag(flags, SafeBool(() => template.IsPotion), "potion");
        AddFlag(flags, SafeBool(() => template.IsPlainFood), "plain-food");
        AddFlag(flags, SafeBool(() => template.IsDish), "dish");
        AddFlag(flags, SafeBool(() => template.IsFish), "fish");
        AddFlag(flags, SafeBool(() => template.IsAlcohol), "alcohol");
        AddFlag(flags, SafeBool(() => template.ConsumablePotionOther), "potion-other");
        AddFlag(flags, SafeBool(() => template.ConsumableModifiesHealth), "health");
        AddFlag(flags, SafeBool(() => template.ConsumableModifiesMana), "mana");
        AddFlag(flags, SafeBool(() => template.ConsumableStamina), "stamina");

        return flags.Count == 0 ? "none" : string.Join("|", flags);
    }

    private static void AddFlag(List<string> flags, bool enabled, string value)
    {
        if (enabled)
        {
            flags.Add(value);
        }
    }

    private static ItemTemplate? SafeTemplate(Item item)
    {
        try
        {
            return item.Template;
        }
        catch
        {
            return null;
        }
    }

    private static string SafeItemName(Item item, ItemTemplate template)
    {
        try
        {
            return string.IsNullOrWhiteSpace(item.DisplayName)
                ? template.ItemName
                : item.DisplayName;
        }
        catch
        {
            return template.ItemName ?? "unknown";
        }
    }

    private static string SafeGuid(ItemTemplate template)
    {
        try
        {
            return template.GUID ?? "none";
        }
        catch
        {
            return "unavailable";
        }
    }

    private static int SafeQuantity(Item item)
    {
        try
        {
            return item.Quantity;
        }
        catch
        {
            return 0;
        }
    }

    private static bool SafeBool(Func<bool> read)
    {
        try
        {
            return read();
        }
        catch
        {
            return false;
        }
    }
}
