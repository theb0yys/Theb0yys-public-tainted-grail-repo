using System.Reflection;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace TGCommunity.Il2CppPatchHealth;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BasePlugin
{
    public const string PluginGuid = "community.taintedgrail.il2cpp-patch-health";
    public const string PluginName = "TG Community IL2CPP Patch Health";
    public const string PluginVersion = "0.1.0";

    private Harmony? _harmony;

    internal static bool FeatureEnabled { get; private set; }

    public override void Load()
    {
        _harmony = new Harmony(PluginGuid);

        MethodInfo? target = AccessTools.Method(
            typeof(DemoFeatureTarget),
            nameof(DemoFeatureTarget.Compute),
            new[] { typeof(int) });

        MethodInfo? postfix = AccessTools.Method(
            typeof(DemoFeaturePatch),
            nameof(DemoFeaturePatch.Postfix));

        PatchHealthResult health = HarmonyPatchHealth.TryInstallPostfix(
            _harmony,
            PluginGuid,
            target,
            postfix);

        FeatureEnabled = health.Ready;

        Log.LogInfo(
            "Patch health feature=demo " +
            "enabled=" + FeatureEnabled +
            " reason=" + health.Reason +
            " target=" + (health.Target?.ToString() ?? "<unresolved>"));

        int observed = DemoFeatureTarget.Compute(1);
        Log.LogInfo(
            "Patch health demo originalInput=1 observedResult=" + observed +
            " expectedWhenEnabled=" + (FeatureEnabled ? "11" : "1"));
    }

    public override bool Unload()
    {
        FeatureEnabled = false;
        _harmony?.UnpatchSelf();
        _harmony = null;
        return true;
    }
}

internal static class DemoFeatureTarget
{
    internal static int Compute(int value)
    {
        return value;
    }
}

internal static class DemoFeaturePatch
{
    internal static void Postfix(ref int __result)
    {
        if (!Plugin.FeatureEnabled)
            return;

        __result += 10;
    }
}
