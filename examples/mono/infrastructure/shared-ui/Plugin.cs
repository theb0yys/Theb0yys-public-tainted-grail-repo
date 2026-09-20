using BepInEx;
using FoAModManager;
using TaintedInterface;
using UnityEngine;

namespace TGCommunity.SharedUiIntegration;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("kane.tgfoa.mod-manager", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("kane.tgfoa.tainted-interface", BepInDependency.DependencyFlags.HardDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.infrastructure.shared-ui";
    public const string PluginName = "TG Community Shared UI Integration";
    public const string PluginVersion = "0.1.0";

    private const string UiOwnerId = PluginGuid + ".panel";

    private bool _open;
    private Rect _window = new(100f, 100f, 380f, 180f);

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            if (_open)
            {
                ClosePanel();
            }
            else
            {
                OpenPanel();
            }
        }
    }

    private void OnGUI()
    {
        if (!_open)
        {
            return;
        }

        _window = GUI.Window(912731, _window, DrawWindow, "Shared UI Example");
    }

    private void OnDestroy()
    {
        ClosePanel();
    }

    private void OpenPanel()
    {
        _open = true;
        FoAModManagerApi.SetCustomUiScope(UiOwnerId, true, freezeWorld: true);
    }

    private void ClosePanel()
    {
        _open = false;
        FoAModManagerApi.SetCustomUiScope(UiOwnerId, false, freezeWorld: true);
    }

    private void DrawWindow(int id)
    {
        GUILayout.Label("FoA Mod Manager owns the shared modal scope.");
        GUILayout.Label("Tainted Interface supplies the shared button style.");

        if (TaintedInterfaceApi.Button("Close"))
        {
            ClosePanel();
        }

        GUI.DragWindow();
    }
}
