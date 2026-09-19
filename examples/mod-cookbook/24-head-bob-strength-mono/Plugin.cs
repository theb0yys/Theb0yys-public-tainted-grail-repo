using System;
using System.Reflection;
using Awaken.TG.Main.Settings.Accessibility;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGExample.HeadBob;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.head-bob";
    public const string PluginName = "TG Example - Head Bob";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<float> _strength = null!;
    private Harmony? _harmony;

    internal static float Strength => Clamp(s_instance?._strength.Value ?? 1f, 0f, 2f);

    private void Awake()
    {
        s_instance = this;
        _strength = Config.Bind("Camera", "HeadBobStrength", 0.5f,
            new ConfigDescription("Head-bob multiplier. 0 disables supported bob; 1 is vanilla.",
                new AcceptableValueRange<float>(0f, 2f)));

        _harmony = new Harmony(PluginGuid);
        MethodInfo? target = AccessTools.PropertyGetter(typeof(HeadBobbingSetting), nameof(HeadBobbingSetting.Intensity));
        MethodInfo? postfix = AccessTools.Method(typeof(Plugin), nameof(IntensityPostfix));
        if (target != null && postfix != null)
        {
            _harmony.Patch(target, postfix: new HarmonyMethod(postfix));
            Logger.LogInfo("Patched HeadBobbingSetting.Intensity.");
        }
        else
        {
            Logger.LogWarning("HeadBobbingSetting.Intensity target was not found.");
        }
    }

    private static void IntensityPostfix(ref float __result)
    {
        __result *= Strength;
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
