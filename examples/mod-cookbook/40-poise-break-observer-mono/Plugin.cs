using System;
using System.Reflection;
using Awaken.TG.Main.AI.Combat.Attachments;
using Awaken.TG.Main.Animations.FSM.Npc.Base;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace TGExample.PoiseBreakObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.poise-break-observer";
    public const string PluginName = "TG Example - Poise-Break Observer";
    public const string PluginVersion = "0.1.0";

    internal static ManualLogSource? LogSource { get; private set; }
    private Harmony? _harmony;

    private void Awake()
    {
        LogSource = Logger;
        _harmony = new Harmony(PluginGuid);

        MethodInfo? target = AccessTools.Method(
            typeof(EnemyBaseClass),
            nameof(EnemyBaseClass.EnterPoise),
            new[] { typeof(NpcStateType), typeof(bool) });
        MethodInfo? prefix = AccessTools.Method(typeof(Plugin), nameof(EnterPoisePrefix));

        if (target == null || prefix == null)
        {
            Logger.LogWarning("EnemyBaseClass.EnterPoise target was not found.");
            return;
        }

        _harmony.Patch(target, prefix: new HarmonyMethod(prefix));
        Logger.LogInfo($"{PluginName} loaded. This example observes native poise-break entry only.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        LogSource = null;
    }

    private static void EnterPoisePrefix(
        EnemyBaseClass __instance,
        NpcStateType poiseBreakDirection,
        bool isInCombat)
    {
        try
        {
            if (__instance == null)
            {
                return;
            }

            LogSource?.LogInfo(
                $"Poise break: direction={poiseBreakDirection}; inCombat={isInCombat}; " +
                $"staggered={__instance.Staggered}; canBeStaggered={__instance.CanBeStaggered}");
        }
        catch (Exception ex)
        {
            LogSource?.LogWarning($"Poise-break observation failed: {ex.Message}");
        }
    }
}
