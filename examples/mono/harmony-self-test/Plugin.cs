using BepInEx;
using HarmonyLib;

namespace TGCommunity.HarmonySelfTest;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.harmonyselftest";
    public const string PluginName = "TG Community Harmony Self-Test";
    public const string PluginVersion = "0.1.0";

    private Harmony? _harmony;

    private void Awake()
    {
        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll();

        Logger.LogInfo($"Harmony self-test result: {SelfTestTarget.GetStatus()}");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }
}

internal static class SelfTestTarget
{
    internal static string GetStatus() => "unpatched";
}

[HarmonyPatch(typeof(SelfTestTarget), nameof(SelfTestTarget.GetStatus))]
internal static class SelfTestPatch
{
    private static void Postfix(ref string __result)
    {
        __result = "patched";
    }
}
