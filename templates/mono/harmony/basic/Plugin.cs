using BepInEx;
using HarmonyLib;

namespace TGCommunity.HarmonyBasic;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.harmonybasic";
    public const string PluginName = "TG Community Harmony Basic";
    public const string PluginVersion = "0.1.0";

    private Harmony? _harmony;

    private void Awake()
    {
        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll();

        Logger.LogInfo($"{PluginName} {PluginVersion} loaded.");
        Logger.LogInfo($"Harmony wiring self-test: {TemplateTarget.GetStatus()}");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }
}

internal static class TemplateTarget
{
    internal static string GetStatus() => "unpatched";
}

[HarmonyPatch(typeof(TemplateTarget), nameof(TemplateTarget.GetStatus))]
internal static class TemplateSelfTestPatch
{
    private static void Postfix(ref string __result)
    {
        __result = "patched";
    }
}
