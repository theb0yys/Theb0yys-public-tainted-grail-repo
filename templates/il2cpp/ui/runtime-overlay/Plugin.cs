using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace TGTemplate.Il2CppRuntimeOverlay;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BasePlugin
{
    public const string PluginGuid = "community.taintedgrail.template.il2cpp-runtime-overlay";
    public const string PluginName = "TG IL2CPP Runtime Overlay Template";
    public const string PluginVersion = "0.1.0";

    internal static KeyCode ToggleKey { get; private set; } = KeyCode.F6;
    internal static bool Visible { get; set; }

    private OverlayBehaviour? _behaviour;

    public override void Load()
    {
        ConfigEntry<KeyCode> toggle = Config.Bind("UI", "ToggleKey", KeyCode.F6, "Toggle the template panel.");
        ToggleKey = toggle.Value;

        ClassInjector.RegisterTypeInIl2Cpp<OverlayBehaviour>();
        _behaviour = AddComponent<OverlayBehaviour>();
        Log.LogInfo($"{PluginName} loaded. ToggleKey={ToggleKey}");
    }

    public override bool Unload()
    {
        if (_behaviour != null)
        {
            Object.Destroy(_behaviour);
            _behaviour = null;
        }

        Visible = false;
        return true;
    }
}
