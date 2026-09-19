using System;
using System.Reflection;
using Awaken.TG.Main.Cameras;
using Awaken.TG.Main.Heroes;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TGExample.FovTransition;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.fov-transition";
    public const string PluginName = "TG Example - FOV Transition";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<float> _durationMultiplier = null!;
    private Harmony? _harmony;

    internal static float DurationMultiplier => Clamp(s_instance?._durationMultiplier.Value ?? 1f, 0.25f, 4f);

    private void Awake()
    {
        s_instance = this;
        _durationMultiplier = Config.Bind("Camera", "TransitionDurationMultiplier", 1.35f,
            new ConfigDescription("FOV transition duration multiplier. 1 is vanilla.",
                new AcceptableValueRange<float>(0.25f, 4f)));

        _harmony = new Harmony(PluginGuid);
        FovTransitionPatch.Apply(_harmony, Logger);
        Logger.LogInfo($"{PluginName} loaded. Multiplier={DurationMultiplier:0.##}");
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

internal static class FovTransitionPatch
{
    internal static void Apply(Harmony harmony, BepInEx.Logging.ManualLogSource logger)
    {
        MethodInfo? target = AccessTools.Method(typeof(VHeroController), nameof(VHeroController.SetFoV));
        MethodInfo? prefix = AccessTools.Method(typeof(FovTransitionPatch), nameof(Prefix));
        if (target == null || prefix == null)
        {
            logger.LogWarning("VHeroController.SetFoV target was not found.");
            return;
        }

        harmony.Patch(target, prefix: new HarmonyMethod(prefix));
        logger.LogInfo("Patched VHeroController.SetFoV.");
    }

    private static void Prefix(ref HeroFoV.FoVChangeData data)
    {
        if (data.changeLength < 0.01f) return;
        data.changeLength = Mathf.Clamp(data.changeLength * Plugin.DurationMultiplier, 0.01f, 4f);
    }
}
