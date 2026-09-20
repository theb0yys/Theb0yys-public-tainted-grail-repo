using BepInEx;
using BepInEx.Configuration;

namespace TGCommunity.MonoBasic;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.monobasic";
    public const string PluginName = "TG Community Mono Basic";
    public const string PluginVersion = "0.1.0";

    private ConfigEntry<bool>? _enabled;

    private void Awake()
    {
        _enabled = Config.Bind(
            "General",
            "Enabled",
            true,
            "Enable this starter plug-in.");

        Logger.LogInfo($"{PluginName} {PluginVersion} loaded. Enabled={_enabled.Value}");
    }
}
