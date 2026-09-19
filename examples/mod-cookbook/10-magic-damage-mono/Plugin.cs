using System;
using System.Reflection;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Fights.DamageInfo;
using Awaken.TG.Main.Heroes;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGExample.MagicDamage;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.magic-damage";
    public const string PluginName = "TG Example - Magic Damage";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<float> _multiplier = null!;
    private Harmony? _harmony;

    internal static float Multiplier => Clamp(s_instance?._multiplier.Value ?? 1f, 0f, 5f);

    private void Awake()
    {
        s_instance = this;
        _multiplier = Config.Bind("Magic", "DamageMultiplier", 1f,
            new ConfigDescription("Player magical-hit damage multiplier. 1 is vanilla.",
                new AcceptableValueRange<float>(0f, 5f)));

        _harmony = new Harmony(PluginGuid);
        MagicDamagePatch.Apply(_harmony, Logger);
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

internal static class MagicDamagePatch
{
    internal static void Apply(Harmony harmony, BepInEx.Logging.ManualLogSource logger)
    {
        MethodInfo? target = AccessTools.Method(typeof(HealthElement), "OnDamage");
        MethodInfo? prefix = AccessTools.Method(typeof(MagicDamagePatch), nameof(Prefix));
        if (target == null || prefix == null)
        {
            logger.LogWarning("HealthElement.OnDamage target was not found.");
            return;
        }

        harmony.Patch(target, prefix: new HarmonyMethod(prefix));
        logger.LogInfo("Patched HealthElement.OnDamage for player magic damage.");
    }

    private static void Prefix(Damage damage)
    {
        Hero? hero = Hero.Current;
        if (hero == null ||
            !ReferenceEquals(damage.DamageDealerPure, hero) ||
            damage.Type != DamageType.MagicalHitSource)
        {
            return;
        }

        float multiplier = Plugin.Multiplier;
        if (Math.Abs(multiplier - 1f) <= 0.0001f)
        {
            return;
        }

        damage.RawData.MultiplyMultModifier(multiplier);
    }
}
