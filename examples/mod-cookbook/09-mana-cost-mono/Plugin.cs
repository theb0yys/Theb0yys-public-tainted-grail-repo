using System;
using System.Reflection;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Combat;
using Awaken.TG.Main.Heroes.Items;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGExample.ManaCost;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.mana-cost";
    public const string PluginName = "TG Example - Mana Cost";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<float> _multiplier = null!;
    private Harmony? _harmony;

    internal static bool Enabled => s_instance?._enabled.Value ?? false;
    internal static float Multiplier => Clamp(s_instance?._multiplier.Value ?? 1f, 0f, 3f);

    private void Awake()
    {
        s_instance = this;
        _enabled = Config.Bind("General", "Enabled", true, "Enable the mana-cost example.");
        _multiplier = Config.Bind("Magic", "ManaCostMultiplier", 1f,
            new ConfigDescription("Player magic mana-cost multiplier. 1 is vanilla.",
                new AcceptableValueRange<float>(0f, 3f)));

        _harmony = new Harmony(PluginGuid);
        ManaCostPatch.Apply(_harmony, Logger);
        Logger.LogInfo($"{PluginName} loaded. Multiplier={Multiplier:0.##}");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        s_instance = null;
    }

    private static float Clamp(float value, float min, float max)
    {
        if (float.IsNaN(value) || float.IsInfinity(value)) return 1f;
        return Math.Max(min, Math.Min(max, value));
    }
}

internal static class ManaCostPatch
{
    internal static void Apply(Harmony harmony, BepInEx.Logging.ManualLogSource logger)
    {
        MethodInfo? target = AccessTools.Method(typeof(MagicUtils), nameof(MagicUtils.GetManaCostMultiplier));
        MethodInfo? postfix = AccessTools.Method(typeof(ManaCostPatch), nameof(Postfix));
        if (target == null || postfix == null)
        {
            logger.LogWarning("MagicUtils.GetManaCostMultiplier target was not found.");
            return;
        }

        harmony.Patch(target, postfix: new HarmonyMethod(postfix));
        logger.LogInfo("Patched MagicUtils.GetManaCostMultiplier.");
    }

    private static void Postfix(ICharacter character, Item item, ref float __result)
    {
        if (!Plugin.Enabled || !ReferenceEquals(character, Hero.Current))
        {
            return;
        }

        __result *= Plugin.Multiplier;
    }
}
