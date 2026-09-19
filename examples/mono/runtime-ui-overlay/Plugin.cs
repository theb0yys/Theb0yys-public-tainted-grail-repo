using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace TGTemplate.RuntimeUI;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.template.runtime-ui";
    public const string PluginName = "TG Runtime UI Template";
    public const string PluginVersion = "0.1.0";

    private ConfigEntry<KeyCode>? _toggleKey;
    private bool _visible;

    private void Awake()
    {
        _toggleKey = Config.Bind("UI", "ToggleKey", KeyCode.F6, "Toggle the small template panel.");
        Logger.LogInfo(PluginName + " " + PluginVersion + " loaded.");
    }

    private void Update()
    {
        if (_toggleKey != null && Input.GetKeyDown(_toggleKey.Value))
        {
            _visible = !_visible;
            Logger.LogInfo("Runtime UI Template visible=" + _visible);
        }
    }

    private void OnGUI()
    {
        if (!_visible)
        {
            return;
        }

        GUI.Box(
            new Rect(24f, 24f, 420f, 110f),
            PluginName + "\n" +
            "This panel is owned by the template only.\n" +
            "Frame: " + Time.frameCount);
    }
}
