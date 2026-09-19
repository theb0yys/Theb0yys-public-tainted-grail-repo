using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGTemplate.ResultPostfix;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.template.result-postfix";
    public const string PluginName = "TG Result Postfix Template";
    public const string PluginVersion = "0.1.0";

    private Harmony? _harmony;
    private ConfigEntry<bool>? _forceAllow;

    internal static bool ForceAllow => Instance?._forceAllow?.Value ?? false;
    internal static Plugin? Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        _forceAllow = Config.Bind("Demo", "ForceAllow", true, "Override the demo result to true.");

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);

        bool original = DemoDecision.UnpatchedValue;
        bool final = DemoDecision.ShouldAllow();
        Logger.LogInfo("Result Postfix Template demo: original=" + original + " final=" + final);
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        Instance = null;
    }
}

internal static class DemoDecision
{
    internal const bool UnpatchedValue = false;

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static bool ShouldAllow() => UnpatchedValue;
}

[HarmonyPatch(typeof(DemoDecision), nameof(DemoDecision.ShouldAllow))]
internal static class DemoDecisionPatch
{
    private static void Postfix(ref bool __result)
    {
        if (Plugin.ForceAllow)
        {
            __result = true;
        }
    }
}
