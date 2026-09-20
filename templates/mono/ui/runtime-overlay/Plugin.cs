using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace TGCommunity.RuntimeUiBasic;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.runtimeuibasic";
    public const string PluginName = "TG Community Runtime UI Basic";
    public const string PluginVersion = "0.1.0";

    private ConfigEntry<KeyCode>? _toggleKey;
    private bool _visible;

    private void Awake()
    {
        _toggleKey = Config.Bind("UI", "ToggleKey", KeyCode.F6, "Toggle the template panel.");
        Logger.LogInfo($"{PluginName} {PluginVersion} loaded.");
    }

    private void Update()
    {
        if (_toggleKey != null && Input.GetKeyDown(_toggleKey.Value))
        {
            _visible = !_visible;
        }
    }

    private void OnGUI()
    {
        if (!_visible)
        {
            return;
        }

        GUI.Box(
            new Rect(24f, 24f, 420f, 100f),
            $"{PluginName}\nTemplate-owned diagnostic surface\nFrame: {Time.frameCount}");
    }
}
