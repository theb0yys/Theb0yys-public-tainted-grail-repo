using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace TGTemplate.Il2CppHarmonyBasic;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BasePlugin
{
    public const string PluginGuid = "community.taintedgrail.template.il2cpp-harmony-basic";
    public const string PluginName = "TG IL2CPP Harmony Basic Template";
    public const string PluginVersion = "0.1.0";

    private Harmony? _harmony;

    public override void Load()
    {
        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Log.LogInfo($"{PluginName} loaded. Self-test={TemplateTarget.GetStatus()}");
    }

    public override bool Unload()
    {
        _harmony?.UnpatchSelf();
        _harmony = null;
        return true;
    }
}

internal static class TemplateTarget
{
    internal static string GetStatus() => "unpatched";
}

[HarmonyPatch(typeof(TemplateTarget), nameof(TemplateTarget.GetStatus))]
internal static class TemplatePatch
{
    private static void Postfix(ref string __result) => __result = "patched";
}
