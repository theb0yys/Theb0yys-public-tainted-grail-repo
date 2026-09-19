using System;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Items;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace TGExample.EquipmentChangeObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.equipment-change-observer";
    public const string PluginName = "TG Example - Equipment Change Observer";
    public const string PluginVersion = "0.1.0";

    internal static ManualLogSource? LogSource { get; private set; }

    private Harmony? _harmony;

    private void Awake()
    {
        LogSource = Logger;
        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo("${PluginName} loaded. This example observes item-level equip transitions only.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        LogSource = null;
    }
}

[HarmonyPatch(typeof(Item), nameof(Item.EquipInSlot))]
internal static class EquipInSlotPatch
{
    private static void Postfix(Item __instance, EquipmentSlotType __0)
    {
        EquipmentLog.Write("equip", __instance, __0);
    }
}

[HarmonyPatch(typeof(Item), nameof(Item.UnequipInSlot))]
internal static class UnequipInSlotPatch
{
    private static void Postfix(Item __instance, EquipmentSlotType __0)
    {
        EquipmentLog.Write("unequip", __instance, __0);
    }
}

internal static class EquipmentLog
{
    internal static void Write(string transition, Item item, EquipmentSlotType slot)
    {
        try
        {
            Hero? hero = Hero.Current;
            if (hero == null
                || hero.HasBeenDiscarded
                || item == null
                || !ReferenceEquals(item.Character, hero))
            {
                return;
            }

            Plugin.LogSource?.LogInfo(
                "${transition}: item={SafeName(item)}; slot={slot}; isEquipped={SafeEquipped(item)}");
        }
        catch (Exception ex)
        {
            Plugin.LogSource?.LogWarning("${transition} observation failed: {ex.Message}");
        }
    }

    private static string SafeName(Item item)
    {
        try
        {
            return string.IsNullOrWhiteSpace(item.DisplayName)
                ? item.Template?.ItemName ?? item.GetType().Name
                : item.DisplayName;
        }
        catch
        {
            return item.GetType().Name;
        }
    }

    private static bool SafeEquipped(Item item)
    {
        try
        {
            return item.IsEquipped;
        }
        catch
        {
            return false;
        }
    }
}
