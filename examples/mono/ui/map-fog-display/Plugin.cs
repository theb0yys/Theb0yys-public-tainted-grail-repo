using System.Reflection;
using Awaken.TG.Graphics.MapServices;
using Awaken.TG.Main.Heroes.CharacterSheet.Map;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TGCommunity.Example.MapFogDisplay;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.map-fog-display";
    public const string PluginName = "TG Example - Map Fog Display";
    public const string PluginVersion = "0.1.0";

    private static ConfigEntry<bool>? _disableMask;
    private static ConfigEntry<bool>? _revealMarkers;
    private static readonly FieldInfo? FogEnabled =
        AccessTools.Field(typeof(MapUI), "<FogOfWarEnabled>k__BackingField");

    private Harmony? _harmony;

    private void Awake()
    {
        _disableMask = Config.Bind("Map", "DisableFogMask", true, "Hide the map fog mask without writing discovery memory.");
        _revealMarkers = Config.Bind("Map", "RevealMarkers", true, "Treat marker reveal queries as visible without writing visited pixels.");

        if (_disableMask.Value)
        {
            ForceFogFlag(false);
        }

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }

    private static void ForceFogFlag(bool enabled)
    {
        if (FogEnabled != null)
        {
            FogEnabled.SetValue(null, enabled);
        }
        else
        {
            MapUI.ToggleFogOfWar(enabled);
        }
    }

    [HarmonyPatch(typeof(MapUI), nameof(MapUI.ToggleFogOfWar))]
    private static class ToggleFogPatch
    {
        private static bool Prefix(ref bool __result)
        {
            if (_disableMask?.Value != true)
            {
                return true;
            }

            ForceFogFlag(false);
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(FogOfWar), nameof(FogOfWar.CreateMaskTexture))]
    private static class CreateMaskPatch
    {
        private static bool Prefix(ref RenderTexture __result)
        {
            if (_disableMask?.Value != true)
            {
                return true;
            }

            ForceFogFlag(false);
            __result = null!;
            return false;
        }
    }

    [HarmonyPatch(typeof(FogOfWar), nameof(FogOfWar.IsPositionRevealed))]
    private static class MarkerRevealPatch
    {
        private static bool Prefix(ref bool __result)
        {
            if (_revealMarkers?.Value != true)
            {
                return true;
            }

            __result = true;
            return false;
        }
    }
}
