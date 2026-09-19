using System;
using System.Reflection;
using Awaken.TG.Main.Cameras;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGExample.FovKick;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.fov-kick";
    public const string PluginName = "TG Example - FOV Kick";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<float> _strength = null!;
    private Harmony? _harmony;

    internal static float Strength => Clamp(s_instance?._strength.Value ?? 1f, 0f, 2f);

    private void Awake()
    {
        s_instance = this;
        _strength = Config.Bind("Camera", "MovementFovKickStrength", 0.5f,
            new ConfigDescription("0 removes movement FOV kick; 1 is vanilla.",
                new AcceptableValueRange<float>(0f, 2f)));

        _harmony = new Harmony(PluginGuid);
        FovPatch.Apply(_harmony, Logger);
        Logger.LogInfo($"{PluginName} loaded. Strength={Strength:0.##}");
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

internal static class FovPatch
{
    internal static void Apply(Harmony harmony, BepInEx.Logging.ManualLogSource logger)
    {
        MethodInfo? target = AccessTools.Method(typeof(HeroFoV), "GetMovementFoVMultiplier");
        MethodInfo? postfix = AccessTools.Method(typeof(FovPatch), nameof(Postfix));
        if (target == null || postfix == null)
        {
            logger.LogWarning("HeroFoV.GetMovementFoVMultiplier target was not found.");
            return;
        }

        harmony.Patch(target, postfix: new HarmonyMethod(postfix));
        logger.LogInfo("Patched HeroFoV.GetMovementFoVMultiplier.");
    }

    private static void Postfix(ref float __result)
    {
        __result = 1f + ((__result - 1f) * Plugin.Strength);
    }
}
