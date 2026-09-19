using System;
using System.Reflection;
using Awaken.TG.Main.Cameras;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGExample.CameraShake;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.camera-shake";
    public const string PluginName = "TG Example - Camera Shake";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<float> _strength = null!;
    private Harmony? _harmony;

    internal static float Strength => Clamp(s_instance?._strength.Value ?? 1f, 0f, 2f);

    private void Awake()
    {
        s_instance = this;
        _strength = Config.Bind("Camera", "ShakeStrength", 0.5f,
            new ConfigDescription("0 suppresses native camera shake; 1 is vanilla.",
                new AcceptableValueRange<float>(0f, 2f)));

        _harmony = new Harmony(PluginGuid);
        CameraShakePatch.Apply(_harmony, Logger);
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

internal static class CameraShakePatch
{
    internal static void Apply(Harmony harmony, BepInEx.Logging.ManualLogSource logger)
    {
        MethodInfo? target = AccessTools.Method(typeof(GameCamera), nameof(GameCamera.Shake));
        MethodInfo? prefix = AccessTools.Method(typeof(CameraShakePatch), nameof(Prefix));
        if (target == null || prefix == null)
        {
            logger.LogWarning("GameCamera.Shake target was not found.");
            return;
        }

        harmony.Patch(target, prefix: new HarmonyMethod(prefix));
        logger.LogInfo("Patched GameCamera.Shake.");
    }

    private static void Prefix(ref float amplitude, ref float frequency, ref float time)
    {
        float strength = Plugin.Strength;
        amplitude *= strength;
        frequency *= strength;
        if (strength <= 0.001f) time = 0f;
    }
}
