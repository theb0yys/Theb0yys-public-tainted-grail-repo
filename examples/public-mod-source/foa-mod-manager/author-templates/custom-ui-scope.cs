using BepInEx;
using FoAModManager;
using UnityEngine;

namespace ExampleFoAMod;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "yourname.foa.example-mod";
    public const string PluginName = "Example Mod";
    public const string PluginVersion = "1.0.0";

    private const string UiOwnerId = PluginGuid + ".panel";
    private bool _panelOpen;
    private Rect _panelRect = new(120f, 120f, 420f, 260f);

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            if (_panelOpen)
            {
                ClosePanel();
            }
            else
            {
                OpenPanel(freezeWorld: true);
            }
        }
    }

    private void OpenPanel(bool freezeWorld)
    {
        _panelOpen = true;
        FoAModManagerApi.SetCustomUiScope(UiOwnerId, true, freezeWorld);
    }

    private void ClosePanel()
    {
        _panelOpen = false;
        FoAModManagerApi.SetCustomUiScope(UiOwnerId, false);
    }

    private void OnGUI()
    {
        if (!_panelOpen)
        {
            return;
        }

        _panelRect = GUI.Window(84001, _panelRect, DrawPanel, "Example Mod");
    }

    private void DrawPanel(int windowId)
    {
        GUILayout.Label("Custom UI is active.");
        GUILayout.Label("The manager owns input, cursor, and controller cursor while this panel is open.");

        if (GUILayout.Button("Close"))
        {
            ClosePanel();
        }

        GUI.DragWindow();
    }

    private void OnDisable()
    {
        ClosePanel();
    }

    private void OnDestroy()
    {
        ClosePanel();
    }
}
