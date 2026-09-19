using System.Reflection;
using Awaken.TG.Main.AudioSystem;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGExample.NativeMusicMute;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.native-music-mute";
    public const string PluginName = "TG Example - Native Music Mute";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<bool> _muteNativeMusic = null!;
    private Harmony? _harmony;

    internal static bool MuteNativeMusic => s_instance?._muteNativeMusic.Value ?? false;

    private void Awake()
    {
        s_instance = this;
        _muteNativeMusic = Config.Bind("Music", "MuteNativeMusic", false,
            "When true, skip new native exploration, alert and combat music starts.");

        _harmony = new Harmony(PluginGuid);
        NativeMusicPatch.Apply(_harmony, Logger);
        Logger.LogInfo($"{PluginName} loaded. MuteNativeMusic={MuteNativeMusic}");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        s_instance = null;
    }
}

internal static class NativeMusicPatch
{
    internal static void Apply(Harmony harmony, BepInEx.Logging.ManualLogSource logger)
    {
        Patch(harmony, logger, "PlayExplorationMusic", nameof(ExplorationPrefix));
        Patch(harmony, logger, "PlayAlertMusic", nameof(AlertPrefix));
        Patch(harmony, logger, "PlayCombatMusic", nameof(CombatPrefix));
    }

    private static void Patch(Harmony harmony, BepInEx.Logging.ManualLogSource logger, string targetName, string prefixName)
    {
        MethodInfo? target = AccessTools.Method(typeof(AudioCore), targetName);
        MethodInfo? prefix = AccessTools.Method(typeof(NativeMusicPatch), prefixName);
        if (target == null || prefix == null)
        {
            logger.LogWarning($"AudioCore.{targetName} target was not found.");
            return;
        }

        harmony.Patch(target, prefix: new HarmonyMethod(prefix));
        logger.LogInfo($"Patched AudioCore.{targetName}.");
    }

    private static bool ExplorationPrefix(ref bool __result) => ShouldRunOriginal(ref __result);
    private static bool AlertPrefix(ref bool __result) => ShouldRunOriginal(ref __result);
    private static bool CombatPrefix(ref bool __result) => ShouldRunOriginal(ref __result);

    private static bool ShouldRunOriginal(ref bool result)
    {
        if (!Plugin.MuteNativeMusic) return true;
        result = false;
        return false;
    }
}
