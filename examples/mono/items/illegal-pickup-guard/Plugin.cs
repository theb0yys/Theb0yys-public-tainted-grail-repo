using Awaken.TG.Main.Locations.Pickables;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TGExample.IllegalPickupGuard;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.illegal-pickup-guard";
    public const string PluginName = "TG Example - Illegal Pickup Guard";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<KeyCode> _modifier = null!;
    private Harmony? _harmony;

    internal static bool Enabled => s_instance?._enabled.Value ?? false;
    internal static KeyCode Modifier => s_instance?._modifier.Value ?? KeyCode.LeftAlt;

    private void Awake()
    {
        s_instance = this;
        _enabled = Config.Bind("General", "Enabled", true, "Require a modifier for illegal world pickups.");
        _modifier = Config.Bind("General", "RequiredModifier", KeyCode.LeftAlt, "Modifier held to allow an illegal world pickup.");

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);

        Logger.LogInfo($"{PluginName} loaded. Modifier={Modifier}");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        s_instance = null;
    }
}

[HarmonyPatch(typeof(Pickable), nameof(Pickable.StartInteraction))]
internal static class IllegalPickablePatch
{
    private static bool Prefix(Pickable __instance, ref bool __result)
    {
        if (!Plugin.Enabled || !__instance.IsIllegal)
        {
            return true;
        }

        if (Input.GetKey(Plugin.Modifier))
        {
            return true;
        }

        __result = false;
        return false;
    }
}
