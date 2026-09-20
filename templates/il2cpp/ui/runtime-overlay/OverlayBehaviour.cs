using System;
using UnityEngine;

namespace TGTemplate.Il2CppRuntimeOverlay;

internal sealed class OverlayBehaviour : MonoBehaviour
{
    public OverlayBehaviour(IntPtr pointer)
        : base(pointer)
    {
    }

    private void Update()
    {
        if (Input.GetKeyDown(Plugin.ToggleKey))
            Plugin.Visible = !Plugin.Visible;
    }

    private void OnGUI()
    {
        if (!Plugin.Visible)
            return;

        GUI.Box(
            new Rect(24f, 24f, 420f, 100f),
            Plugin.PluginName + "\nIL2CPP template-owned diagnostic surface\nFrame: " + Time.frameCount);
    }
}
