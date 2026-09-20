using BepInEx;
using FoAModManager;

namespace ExampleFoAMod;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "yourname.foa.example-mod";
    public const string PluginName = "Example Mod";
    public const string PluginVersion = "1.0.0";

    private const string TogglePanelActionId = PluginGuid + ".toggle-panel";
    private bool _panelOpen;

    private void Awake()
    {
        FoAModManagerApi.RegisterControllerAction(
            TogglePanelActionId,
            "Toggle Example Panel",
            "Example Mod",
            "Open or close the Example Mod panel.",
            TogglePanel);
    }

    private void TogglePanel()
    {
        _panelOpen = !_panelOpen;
        Logger.LogInfo($"Panel open={_panelOpen}");
    }

    private void OnDestroy()
    {
        FoAModManagerApi.UnregisterControllerAction(TogglePanelActionId);
    }
}
