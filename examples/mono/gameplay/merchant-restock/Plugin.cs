using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Awaken.TG.Main.Locations.Shops;
using Awaken.TG.Main.Locations.Shops.Stocks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TGCommunity.Example.MerchantRestock;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.merchant-restock";
    public const string PluginName = "TG Example - Merchant Restock";
    public const string PluginVersion = "0.1.0";

    private static ConfigEntry<float>? _cooldown;
    private static readonly Dictionary<int, float> LastRestock = new();
    private Harmony? _harmony;

    private void Awake()
    {
        _cooldown = Config.Bind("Restock", "MinimumSecondsBetweenRestocks", 60f, "Per-shop real-time cooldown.");
        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy() => _harmony?.UnpatchSelf();

    [HarmonyPatch(typeof(Shop), nameof(Shop.OpenShop))]
    private static class ShopOpenPatch
    {
        private static void Prefix(Shop __instance)
        {
            int key = RuntimeHelpers.GetHashCode(__instance);
            float now = Time.realtimeSinceStartup;
            float cooldown = Mathf.Clamp(_cooldown?.Value ?? 60f, 0f, 86400f);

            if (cooldown > 0f &&
                LastRestock.TryGetValue(key, out float last) &&
                now - last < cooldown)
                return;

            foreach (RestockableStock stock in __instance.Elements<RestockableStock>())
                stock.Restock();

            LastRestock[key] = now;
        }
    }
}
