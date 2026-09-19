using System;
using System.Globalization;
using System.Reflection;
using Awaken.TG.Main.Locations.Shops;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGExample.MerchantGoldFloor;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.merchant-gold-floor";
    public const string PluginName = "TG Example - Merchant Gold Floor";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<int> _goldFloor = null!;
    private Harmony? _harmony;

    internal static int GoldFloor => Math.Max(0, Math.Min(1000000, s_instance?._goldFloor.Value ?? 0));
    internal static BepInEx.Logging.ManualLogSource? Log => s_instance?.Logger;

    private void Awake()
    {
        s_instance = this;
        _goldFloor = Config.Bind("Merchant", "GoldFloor", 5000,
            new ConfigDescription("Minimum merchant gold after native restock.",
                new AcceptableValueRange<int>(0, 1000000)));

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{PluginName} loaded. GoldFloor={GoldFloor}");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        s_instance = null;
    }
}

[HarmonyPatch]
internal static class MerchantGoldPatch
{
    private static Type? s_wealthType;
    private static MethodInfo? s_valueGetter;

    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(Shop), "Restock", new[] { typeof(bool) })
            ?? throw new MissingMethodException(typeof(Shop).FullName, "Restock(bool)");
    }

    private static void Postfix(Shop __instance)
    {
        try
        {
            var wealth = __instance.MerchantStats.Wealth;
            int current = ReadGold(wealth);
            int floor = Plugin.GoldFloor;
            if (current < floor)
            {
                wealth.IncreaseBy(floor - current);
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogWarning($"Merchant gold example failed open: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static int ReadGold(object wealth)
    {
        Type type = wealth.GetType();
        if (s_valueGetter == null || s_wealthType != type)
        {
            s_wealthType = type;
            s_valueGetter = AccessTools.PropertyGetter(type, "Value")
                ?? throw new MissingMemberException(type.FullName, "Value");
        }

        object? raw = s_valueGetter.Invoke(wealth, null);
        return Convert.ToInt32(raw, CultureInfo.InvariantCulture);
    }
}
