using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Awaken.TG.Main.Heroes.Items.LootTables;
using Awaken.TG.Main.Locations.Actions;
using Awaken.TG.Main.Locations.Shops;
using Awaken.TG.Main.Locations.Shops.Stocks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGCommunity.Example.EconomyRuntimeRules;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.economy-runtime-rules";
    public const string PluginName = "TG Example - Economy Runtime Rules";
    public const string PluginVersion = "0.1.0";

    private static ConfigEntry<bool>? _restockOnOpen;
    private static ConfigEntry<int>? _restockPasses;
    private static ConfigEntry<float>? _containerQuantityMultiplier;
    private static readonly HashSet<int> ProcessedSearchActions = new();
    private static readonly FieldInfo? ItemsInsideContainer =
        AccessTools.Field(typeof(SearchAction), "_itemsInsideContainer");

    private Harmony? _harmony;

    private void Awake()
    {
        _restockOnOpen = Config.Bind("Merchant", "RestockOnOpen", false, "Call Restock() on RestockableStock before Shop.OpenShop continues.");
        _restockPasses = Config.Bind("Merchant", "RestockPasses", 1, "Number of native Restock() passes, clamped to 1-5.");
        _containerQuantityMultiplier = Config.Bind("Container", "QuantityMultiplier", 1f, "Scale generated ItemSpawningDataRuntime quantities once per SearchAction.");

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        ProcessedSearchActions.Clear();
    }

    [HarmonyPatch(typeof(Shop), nameof(Shop.OpenShop))]
    private static class ShopOpenPatch
    {
        private static void Prefix(Shop __instance)
        {
            if (_restockOnOpen?.Value != true)
            {
                return;
            }

            int passes = Math.Max(1, Math.Min(5, _restockPasses?.Value ?? 1));
            for (int pass = 0; pass < passes; pass++)
            {
                foreach (RestockableStock stock in __instance.Elements<RestockableStock>())
                {
                    stock.Restock();
                }
            }
        }
    }

    [HarmonyPatch(typeof(SearchAction), "OnInitialize")]
    private static class SearchActionInitializePatch
    {
        private static void Postfix(SearchAction __instance)
        {
            float multiplier = _containerQuantityMultiplier?.Value ?? 1f;
            if (Math.Abs(multiplier - 1f) <= 0.0001f)
            {
                return;
            }

            int key = RuntimeHelpers.GetHashCode(__instance);
            if (!ProcessedSearchActions.Add(key))
            {
                return;
            }

            if (ItemsInsideContainer?.GetValue(__instance) is not IEnumerable rows)
            {
                return;
            }

            foreach (object? row in rows)
            {
                if (row is not ItemSpawningDataRuntime item)
                {
                    continue;
                }

                int current = Math.Max(0, item.quantity);
                int scaled = (int)Math.Round(current * multiplier, MidpointRounding.AwayFromZero);
                item.quantity = Math.Max(0, Math.Min(9999, scaled));
            }
        }
    }
}
