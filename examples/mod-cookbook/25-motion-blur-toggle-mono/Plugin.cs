using System.Reflection;
using Awaken.TG.Main.Settings.Graphics;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGExample.MotionBlur;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.motion-blur";
    public const string PluginName = "TG Example - Motion Blur";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<bool> _disable = null!;
    private Harmony? _harmony;

    internal static bool Disable => s_instance?._disable.Value ?? false;

    private void Awake()
    {
        s_instance = this;
        _disable = Config.Bind("Graphics", "DisableMotionBlur", false, "Disable FoA motion blur.");

        _harmony = new Harmony(PluginGuid);
        PatchGetter(nameof(MotionBlurSetting.Enabled), nameof(EnabledPostfix));
        PatchGetter(nameof(MotionBlurSetting.Intensity), nameof(IntensityPostfix));
    }

    private void PatchGetter(string propertyName, string postfixName)
    {
        MethodInfo? target = AccessTools.PropertyGetter(typeof(MotionBlurSetting), propertyName);
        MethodInfo? postfix = AccessTools.Method(typeof(Plugin), postfixName);
        if (target == null || postfix == null)
        {
            Logger.LogWarning($"MotionBlurSetting.{propertyName} target was not found.");
            return;
        }

        _harmony?.Patch(target, postfix: new HarmonyMethod(postfix));
        Logger.LogInfo($"Patched MotionBlurSetting.{propertyName}.");
    }

    private static void EnabledPostfix(ref bool __result)
    {
        if (Disable) __result = false;
    }

    private static void IntensityPostfix(ref float __result)
    {
        if (Disable) __result = 0f;
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        s_instance = null;
    }
}
