using System.Reflection;
using Awaken.TG.Main.Heroes;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGExample.ForceHeroHud;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.force-hero-hud";
    public const string PluginName = "TG Example - Force Hero HUD";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<bool> _enabled = null!;
    private Harmony? _harmony;

    internal static bool Enabled => s_instance?._enabled.Value ?? false;

    private void Awake()
    {
        s_instance = this;
        _enabled = Config.Bind("General", "Enabled", true, "Force VHeroHUD.ShowBars to true.");

        _harmony = new Harmony(PluginGuid);

        MethodInfo? target = AccessTools.PropertyGetter(typeof(VHeroHUD), "ShowBars");
        MethodInfo? postfix = AccessTools.Method(typeof(HudPatch), nameof(HudPatch.Postfix));

        if (target == null || postfix == null)
        {
            Logger.LogWarning("VHeroHUD.ShowBars was not found.");
            return;
        }

        _harmony.Patch(target, postfix: new HarmonyMethod(postfix));
        Logger.LogInfo($"{PluginName} loaded. Patched VHeroHUD.ShowBars.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        s_instance = null;
    }
}

internal static class HudPatch
{
    internal static void Postfix(ref bool? __result)
    {
        if (Plugin.Enabled)
        {
            __result = true;
        }
    }
}
