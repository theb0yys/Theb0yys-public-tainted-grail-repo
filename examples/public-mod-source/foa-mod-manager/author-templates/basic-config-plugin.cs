using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace ExampleFoAMod;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "yourname.foa.example-mod";
    public const string PluginName = "Example Mod";
    public const string PluginVersion = "1.0.0";

    private ConfigEntry<bool>? _enabled;
    private ConfigEntry<KeyCode>? _openPanelKey;
    private ConfigEntry<float>? _searchRadius;

    private void Awake()
    {
        _enabled = Config.Bind(
            "General",
            "Enabled",
            true,
            "Enable Example Mod.");

        _openPanelKey = Config.Bind(
            "Controls",
            "OpenPanelKey",
            KeyCode.F8,
            "Open or close the Example Mod panel.");

        _searchRadius = Config.Bind(
            "Detection",
            "SearchRadius",
            30f,
            new ConfigDescription(
                "Search radius in meters.",
                new AcceptableValueRange<float>(5f, 100f)));
    }

    private void Update()
    {
        if (_enabled?.Value != true)
        {
            return;
        }

        if (_openPanelKey != null && Input.GetKeyDown(_openPanelKey.Value))
        {
            Logger.LogInfo($"Open panel key pressed. Radius={_searchRadius?.Value}");
        }
    }
}
