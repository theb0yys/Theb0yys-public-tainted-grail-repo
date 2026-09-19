using System;
using System.Reflection;
using Awaken.TG.Main.AI.Combat.Attachments;
using Awaken.TG.Main.AI.Combat.Behaviours.BaseBehaviours;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace TGExample.StaggerObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.stagger-observer";
    public const string PluginName = "TG Example - Stagger Observer";
    public const string PluginVersion = "0.1.0";

    internal static ManualLogSource? LogSource { get; private set; }
    private Harmony? _harmony;

    private void Awake()
    {
        LogSource = Logger;
        _harmony = new Harmony(PluginGuid);

        PatchPrefix(
            AccessTools.Method(typeof(EnemyBaseClass), "EnterStagger", new[] { typeof(float?) }),
            nameof(EnterStaggerPrefix),
            "EnemyBaseClass.EnterStagger");

        PatchPrefix(
            AccessTools.Method(typeof(StaggerBehaviour), nameof(StaggerBehaviour.UpdateStaggerDuration), new[] { typeof(float?) }),
            nameof(UpdateDurationPrefix),
            "StaggerBehaviour.UpdateStaggerDuration");

        Logger.LogInfo($"{PluginName} loaded. This example observes stagger entry and duration requests only.");
    }

    private void PatchPrefix(MethodInfo? target, string prefixName, string label)
    {
        MethodInfo? prefix = AccessTools.Method(typeof(Plugin), prefixName);
        if (target == null || prefix == null)
        {
            Logger.LogWarning($"{label} target was not found.");
            return;
        }

        _harmony!.Patch(target, prefix: new HarmonyMethod(prefix));
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        LogSource = null;
    }

    private static void EnterStaggerPrefix(EnemyBaseClass __instance, float? duration)
    {
        try
        {
            LogSource?.LogInfo(
                $"Stagger entry: requestedDuration={Format(duration)}; " +
                $"alreadyStaggered={__instance?.Staggered == true}; " +
                $"canBeStaggered={__instance?.CanBeStaggered == true}");
        }
        catch (Exception ex)
        {
            LogSource?.LogWarning($"Stagger-entry observation failed: {ex.Message}");
        }
    }

    private static void UpdateDurationPrefix(float? duration)
    {
        LogSource?.LogInfo($"Stagger duration request: duration={Format(duration)}");
    }

    private static string Format(float? value)
    {
        return value.HasValue ? value.Value.ToString("0.###") : "native-default";
    }
}
