using BepInEx;
using BepInEx.Unity.IL2CPP;
using Tainted.Abstractions.Runtime;
using UnityEngine;

namespace AvalonExceptions.Il2Cpp;

[BepInPlugin(Plugin.PluginGuid, Plugin.PluginName, Plugin.PluginVersion)]
[BepInDependency(Plugin.TaintedFrameworkGuid, Plugin.TaintedFrameworkMinimumVersion)]
[BepInDependency("kane.tgfoa.tainted-interface", BepInDependency.DependencyFlags.SoftDependency)]
public sealed class Il2CppPlugin : BasePlugin
{
    private Plugin? _behaviour;

    public override void Load()
    {
        _behaviour = AddComponent<Plugin>();
        _behaviour.Initialize(Config, Log);
        Log.LogInfo(
            $"{Plugin.PluginName} IL2CPP host ready. Runtime={TaintedRuntimeKind.Il2Cpp}; Framework=Tainted.Abstractions; Surface=local-crash-watchdog-reporting.");
    }

    public override bool Unload()
    {
        if (_behaviour != null)
        {
            Object.Destroy(_behaviour);
            _behaviour = null;
        }

        return true;
    }
}
