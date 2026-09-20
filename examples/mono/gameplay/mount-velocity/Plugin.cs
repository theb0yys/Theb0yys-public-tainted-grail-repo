using System;
using System.Reflection;
using Awaken.TG.Main.Fights.Mounts;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGCommunity.Example.MountVelocity;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.mount-velocity";
    public const string PluginName = "TG Example - Mount Velocity";
    public const string PluginVersion = "0.1.0";

    private static ConfigEntry<float>? _runningMultiplier;
    private static ConfigEntry<float>? _turningMultiplier;
    private static BepInEx.Logging.ManualLogSource? _log;
    private static bool _loggedRunning;
    private static bool _loggedTurning;
    private Harmony? _harmony;

    private void Awake()
    {
        _log = Logger;
        _runningMultiplier = Config.Bind("Mount", "RunningMultiplier", 1.25f, "Scale VMount.RunningVelocity. 1.0 is vanilla.");
        _turningMultiplier = Config.Bind("Mount", "TurningMultiplier", 1.25f, "Scale VMount.TurningVelocity. 1.0 is vanilla.");

        MethodInfo? running = AccessTools.PropertyGetter(typeof(VMount), nameof(VMount.RunningVelocity));
        MethodInfo? turning = AccessTools.PropertyGetter(typeof(VMount), nameof(VMount.TurningVelocity));
        MethodInfo? runningPostfix = AccessTools.Method(typeof(Plugin), nameof(RunningPostfix));
        MethodInfo? turningPostfix = AccessTools.Method(typeof(Plugin), nameof(TurningPostfix));

        if (running == null || turning == null || runningPostfix == null || turningPostfix == null)
        {
            Logger.LogError("VMount velocity getters were not found. No patches were installed.");
            return;
        }

        _harmony = new Harmony(PluginGuid);
        _harmony.Patch(running, postfix: new HarmonyMethod(runningPostfix));
        _harmony.Patch(turning, postfix: new HarmonyMethod(turningPostfix));

        Logger.LogInfo($"{PluginName} loaded. Running={RunningMultiplier:0.###} Turning={TurningMultiplier:0.###}.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        _log = null;
    }

    private static float RunningMultiplier => ClampMultiplier(_runningMultiplier?.Value ?? 1f);
    private static float TurningMultiplier => ClampMultiplier(_turningMultiplier?.Value ?? 1f);

    private static void RunningPostfix(ref float __result)
    {
        float multiplier = RunningMultiplier;
        if (Math.Abs(multiplier - 1f) < 0.001f)
        {
            return;
        }

        __result *= multiplier;
        if (!_loggedRunning)
        {
            _loggedRunning = true;
            _log?.LogInfo($"RunningVelocity postfix active. Multiplier={multiplier:0.###}.");
        }
    }

    private static void TurningPostfix(ref float __result)
    {
        float multiplier = TurningMultiplier;
        if (Math.Abs(multiplier - 1f) < 0.001f)
        {
            return;
        }

        __result *= multiplier;
        if (!_loggedTurning)
        {
            _loggedTurning = true;
            _log?.LogInfo($"TurningVelocity postfix active. Multiplier={multiplier:0.###}.");
        }
    }

    private static float ClampMultiplier(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
        {
            return 1f;
        }

        return Math.Max(0.5f, Math.Min(2f, value));
    }
}
