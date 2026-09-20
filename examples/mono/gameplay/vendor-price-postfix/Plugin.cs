using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGCommunity.Example.VendorPricePostfix;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.vendor-price-postfix";
    public const string PluginName = "TG Example - Vendor Price Postfix";
    public const string PluginVersion = "0.1.0";

    private const string TradeUtilsTypeName = "Awaken.TG.Main.Locations.Shops.TradeUtils";
    private const string HeroTypeName = "Awaken.TG.Main.Heroes.Hero";

    private static ConfigEntry<float>? _buyMultiplier;
    private static ConfigEntry<float>? _sellMultiplier;
    private static BepInEx.Logging.ManualLogSource? _log;
    private Harmony? _harmony;

    private void Awake()
    {
        _log = Logger;
        _buyMultiplier = Config.Bind("Price", "BuyMultiplier", 1.0f, "Final hero-buy price multiplier.");
        _sellMultiplier = Config.Bind("Price", "SellMultiplier", 1.0f, "Final hero-sell price multiplier.");

        Type? tradeUtils = AccessTools.TypeByName(TradeUtilsTypeName);
        MethodInfo? target = tradeUtils == null ? null : AccessTools.Method(tradeUtils, "Price");
        MethodInfo? postfix = AccessTools.Method(typeof(Plugin), nameof(PricePostfix));

        if (target == null || postfix == null)
        {
            Logger.LogError("TradeUtils.Price was not found. No price patch was installed.");
            return;
        }

        _harmony = new Harmony(PluginGuid);
        _harmony.Patch(target, postfix: new HarmonyMethod(postfix));
        Logger.LogInfo($"{PluginName} loaded. Buy={BuyMultiplier:0.###} Sell={SellMultiplier:0.###}.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        _log = null;
    }

    private static float BuyMultiplier => ClampMultiplier(_buyMultiplier?.Value ?? 1f);
    private static float SellMultiplier => ClampMultiplier(_sellMultiplier?.Value ?? 1f);

    private static void PricePostfix(object[] __args, ref int __result)
    {
        if (__result <= 0 || __args.Length < 2)
        {
            return;
        }

        object? seller = __args[0];
        object? buyer = __args[1];
        bool sellerIsHero = IsHero(seller);
        bool buyerIsHero = IsHero(buyer);

        float multiplier;
        string direction;

        if (buyerIsHero && !sellerIsHero)
        {
            multiplier = BuyMultiplier;
            direction = "hero-buy";
        }
        else if (sellerIsHero && !buyerIsHero)
        {
            multiplier = SellMultiplier;
            direction = "hero-sell";
        }
        else
        {
            return;
        }

        if (Math.Abs(multiplier - 1f) < 0.001f)
        {
            return;
        }

        double raw = __result * (double)multiplier;
        if (double.IsNaN(raw) || double.IsInfinity(raw) || raw > int.MaxValue)
        {
            return;
        }

        int adjusted = (int)Math.Round(raw, MidpointRounding.AwayFromZero);
        if (adjusted < 1)
        {
            adjusted = 1;
        }

        int vanilla = __result;
        __result = adjusted;
        _log?.LogInfo($"TradeUtils.Price adjusted. direction={direction}; vanilla={vanilla}; final={adjusted}; multiplier={multiplier:0.###}.");
    }

    private static bool IsHero(object? value)
    {
        return string.Equals(value?.GetType().FullName, HeroTypeName, StringComparison.Ordinal);
    }

    private static float ClampMultiplier(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
        {
            return 1f;
        }

        return Math.Max(0.1f, Math.Min(10f, value));
    }
}
