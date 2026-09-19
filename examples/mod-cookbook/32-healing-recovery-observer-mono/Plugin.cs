using System;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Items;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace TGExample.HealingRecoveryObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.healing-recovery-observer";
    public const string PluginName = "TG Example - Healing and Recovery Observer";
    public const string PluginVersion = "0.1.0";

    internal static ManualLogSource? LogSource { get; private set; }

    private Harmony? _harmony;

    private void Awake()
    {
        LogSource = Logger;
        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{PluginName} loaded. This example observes synchronous health deltas only.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        LogSource = null;
    }
}

[HarmonyPatch(typeof(Item), nameof(Item.Use))]
internal static class HealingRecoveryPatch
{
    private const float DeltaEpsilon = 0.01f;

    private static void Prefix(Item __instance, out UseState __state)
    {
        __state = default;

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

            __state = new UseState(
                true,
                SafeItemName(__instance, template),
                HealthValue(hero),
                SafeQuantity(__instance));
        }
        catch (Exception ex)
        {
            Plugin.LogSource?.LogWarning($"Healing prefix snapshot failed: {ex.Message}");
        }
    }

    private static void Postfix(Item __instance, UseState __state)
    {
        if (!__state.Valid)
        {
            return;
        }

        try
        {
            Hero? hero = Hero.Current;
            if (hero == null || hero.HasBeenDiscarded)
            {
                return;
            }

            float healthAfter = HealthValue(hero);
            float delta = healthAfter - __state.HealthBefore;
            float healed = delta > DeltaEpsilon ? delta : 0f;
            int quantityAfter = SafeQuantity(__instance);

            Plugin.LogSource?.LogInfo(
                $"Healing observation: item={__state.ItemName}; " +
                $"healthBefore={__state.HealthBefore:0.###}; healthAfter={healthAfter:0.###}; " +
                $"healed={healed:0.###}; actualHealing={(healed > 0f ? "true" : "false")}; " +
                $"quantityBefore={__state.QuantityBefore}; quantityAfter={quantityAfter}");
        }
        catch (Exception ex)
        {
            Plugin.LogSource?.LogWarning($"Healing postfix snapshot failed: {ex.Message}");
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

    private static float HealthValue(Hero hero)
    {
        try
        {
            return hero.Health.ModifiedValue;
        }
        catch
        {
            return 0f;
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

    private readonly struct UseState
    {
        internal UseState(bool valid, string itemName, float healthBefore, int quantityBefore)
        {
            Valid = valid;
            ItemName = itemName;
            HealthBefore = healthBefore;
            QuantityBefore = quantityBefore;
        }

        internal bool Valid { get; }
        internal string ItemName { get; }
        internal float HealthBefore { get; }
        internal int QuantityBefore { get; }
    }
}
