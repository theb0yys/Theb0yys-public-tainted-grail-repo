using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace TGTemplate.Il2CppResultPostfix;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BasePlugin
{
    public const string PluginGuid = "community.taintedgrail.template.il2cpp-result-postfix";
    public const string PluginName = "TG IL2CPP Result Postfix Template";
    public const string PluginVersion = "0.1.0";

    internal static bool ForceAllow { get; private set; }
    private Harmony? _harmony;

    public override void Load()
    {
        ConfigEntry<bool> forceAllow = Config.Bind("Demo", "ForceAllow", true, "Override the self-owned demo result.");
        ForceAllow = forceAllow.Value;

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Log.LogInfo($"{PluginName} loaded. Result={TemplateDecision.ShouldAllow()}");
    }

    public override bool Unload()
    {
        _harmony?.UnpatchSelf();
        _harmony = null;
        return true;
    }
}

internal static class TemplateDecision
{
    internal static bool ShouldAllow() => false;
}

[HarmonyPatch(typeof(TemplateDecision), nameof(TemplateDecision.ShouldAllow))]
internal static class ResultPatch
{
    private static void Postfix(ref bool __result)
    {
        if (Plugin.ForceAllow)
            __result = true;
    }
}
