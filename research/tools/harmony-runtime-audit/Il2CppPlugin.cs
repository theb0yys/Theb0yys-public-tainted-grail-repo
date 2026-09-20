using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;

namespace TGCommunity.HarmonyRuntimeAudit;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BasePlugin
{
    public const string PluginGuid = "community.taintedgrail.harmony-runtime-audit-il2cpp";
    public const string PluginName = "TG Community Harmony Runtime Audit IL2CPP";
    public const string PluginVersion = "0.1.0";

    public override void Load()
    {
        ConfigEntry<bool> enabled = Config.Bind(
            "General",
            "Enabled",
            false,
            "Enable one read-only Harmony patch-table snapshot during plug-in load.");

        ConfigEntry<string> ownerFilter = Config.Bind(
            "Filter",
            "OwnerId",
            string.Empty,
            "Optional exact Harmony owner ID. Empty means include all owners.");

        ConfigEntry<string> expectedTargets = Config.Bind(
            "Filter",
            "ExpectedTargets",
            string.Empty,
            "Optional pipe-separated canonical target identities to compare with the live patch table.");

        if (!enabled.Value)
        {
            Log.LogInfo("Harmony runtime audit enabled=false.");
            return;
        }

        HarmonyRuntimeAuditEngine.Run(
            Log,
            ownerFilter.Value ?? string.Empty,
            expectedTargets.Value ?? string.Empty);
    }

    public override bool Unload()
    {
        return true;
    }
}
