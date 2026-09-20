using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace TGTemplate.Il2CppFrameSampler;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BasePlugin
{
    public const string PluginGuid = "community.taintedgrail.template.il2cpp-frame-sampler";
    public const string PluginName = "TG IL2CPP Frame Sampler Template";
    public const string PluginVersion = "0.1.0";

    internal static Plugin? Instance { get; private set; }
    internal static float LogIntervalSeconds { get; private set; } = 5f;

    private FrameSamplerBehaviour? _behaviour;

    public override void Load()
    {
        Instance = this;
        ConfigEntry<float> interval = Config.Bind("Sampler", "LogIntervalSeconds", 5f, "Seconds between aggregate frame reports.");
        LogIntervalSeconds = Mathf.Clamp(interval.Value, 1f, 60f);

        ClassInjector.RegisterTypeInIl2Cpp<FrameSamplerBehaviour>();
        _behaviour = AddComponent<FrameSamplerBehaviour>();
        Log.LogInfo($"{PluginName} loaded. Interval={LogIntervalSeconds:0.##}s");
    }

    internal void Report(float averageFrameMs, float approximateFps, int samples)
    {
        Log.LogInfo($"Frame sample: avgMs={averageFrameMs:0.00}; approxFps={approximateFps:0.0}; samples={samples}");
    }

    public override bool Unload()
    {
        if (_behaviour != null)
        {
            Object.Destroy(_behaviour);
            _behaviour = null;
        }

        Instance = null;
        return true;
    }
}
