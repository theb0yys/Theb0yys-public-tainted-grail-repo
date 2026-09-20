using BepInEx;
using BepInEx.Configuration;

namespace TGCommunity.HybridRuntime;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.hybridruntime";
    public const string PluginName = "TG Community Hybrid Runtime";
    public const string PluginVersion = "0.1.0";

    // Keep this value aligned with ../contract/README.md.
    public const string ContentContractId = "yourname.yourmod.content";

    private ConfigEntry<bool>? _enabled;

    private void Awake()
    {
        _enabled = Config.Bind("General", "Enabled", true, "Enable the runtime half of this hybrid mod.");

        Logger.LogInfo(
            $"{PluginName} {PluginVersion} loaded. " +
            $"Enabled={_enabled.Value}; ContentContractId={ContentContractId}. " +
            "Content presence/integration has not been proven by this log.");
    }
}
