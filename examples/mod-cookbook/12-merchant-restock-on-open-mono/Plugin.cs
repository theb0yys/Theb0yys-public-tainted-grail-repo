using System;
using Awaken.TG.Main.Locations.Shops;
using Awaken.TG.Main.Locations.Shops.Stocks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGExample.MerchantRestock;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.merchant-restock";
    public const string PluginName = "TG Example - Merchant Restock";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<int> _passes = null!;
    private Harmony? _harmony;

    internal static bool Enabled => s_instance?._enabled.Value ?? false;
    internal static int Passes => Math.Max(1, Math.Min(3, s_instance?._passes.Value ?? 1));
    internal static BepInEx.Logging.ManualLogSource? Log => s_instance?.Logger;

    private void Awake()
    {
        s_instance = this;
        _enabled = Config.Bind("General", "Enabled", true, "Enable restock-on-open.");
        _passes = Config.Bind("Merchant", "RestockPasses", 1,
            new ConfigDescription("Restock passes per shop open.", new AcceptableValueRange<int>(1, 3)));

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{PluginName} loaded. Passes={Passes}");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        s_instance = null;
    }
}

[HarmonyPatch(typeof(Shop), nameof(Shop.OpenShop))]
internal static class ShopOpenPatch
{
    private static void Prefix(Shop __instance)
    {
        if (!Plugin.Enabled)
        {
            return;
        }

        try
        {
            int touched = 0;
            for (int pass = 0; pass < Plugin.Passes; pass++)
            {
                foreach (RestockableStock stock in __instance.Elements<RestockableStock>())
                {
                    stock.Restock();
                    touched++;
                }
            }

            Plugin.Log?.LogInfo($"Merchant restock example touched {touched} RestockableStock instance(s).");
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogWarning($"Merchant restock example failed open: {ex.GetType().Name}: {ex.Message}");
        }
    }
}
