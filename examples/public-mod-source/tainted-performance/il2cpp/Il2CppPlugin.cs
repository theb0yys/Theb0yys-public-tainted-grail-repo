using BepInEx;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using Tainted.Abstractions.Runtime;
using UnityEngine;

namespace TaintedPerformance.Il2Cpp;

[BepInPlugin(TaintedPerformance.Plugin.PluginGuid, TaintedPerformance.Plugin.PluginName, TaintedPerformance.Plugin.PluginVersion)]
[BepInDependency(TaintedPerformance.Plugin.TaintedFrameworkPluginGuid, TaintedPerformance.Plugin.TaintedFrameworkMinimumVersion)]
[BepInDependency(TaintedPerformance.Plugin.TaintedInterfacePluginGuid, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(TaintedPerformance.Plugin.FoAModManagerPluginGuid, BepInDependency.DependencyFlags.SoftDependency)]
public sealed class Il2CppPlugin : BasePlugin
{
    internal static TaintedPerformance.Plugin? Controller { get; private set; }

    private TaintedPerformanceBehaviour? _behaviour;

    public override void Load()
    {
        ClassInjector.RegisterTypeInIl2Cpp<TaintedPerformanceBehaviour>();
        Controller = new TaintedPerformance.Plugin();
        Controller.Initialize(Config, Log);
        _behaviour = AddComponent<TaintedPerformanceBehaviour>();
        Log.LogInfo(
            $"Tainted Performance IL2CPP host initialized through Tainted Framework. runtime={TaintedRuntimeKind.Il2Cpp}; frameworkMinimum={TaintedPerformance.Plugin.TaintedFrameworkMinimumVersion}.");
    }

    public override bool Unload()
    {
        Controller?.Shutdown();
        Controller = null;
        if (_behaviour != null)
        {
            Object.Destroy(_behaviour);
            _behaviour = null;
        }

        return true;
    }
}
