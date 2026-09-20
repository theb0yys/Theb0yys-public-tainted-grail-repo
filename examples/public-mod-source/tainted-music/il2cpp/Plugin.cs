using BepInEx;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using Tainted.Abstractions.Runtime;
using UnityEngine;

namespace TaintedMusic.Il2Cpp;

[BepInPlugin(TaintedMusic.Plugin.PluginGuid, TaintedMusic.Plugin.PluginName, TaintedMusic.Plugin.PluginVersion)]
[BepInDependency(TaintedFrameworkGuid, TaintedFrameworkMinimumVersion)]
[BepInDependency(TaintedInterfaceGuid)]
public sealed class Il2CppPlugin : BasePlugin
{
    public const string TaintedFrameworkGuid = "kane.tgfoa.tainted-framework";
    public const string TaintedFrameworkMinimumVersion = "0.1.38";
    public const string TaintedInterfaceGuid = "kane.tgfoa.tainted-interface";

    private TaintedMusic.Plugin? _controller;
    private TaintedMusicBehaviour? _behaviour;

    public override void Load()
    {
        ClassInjector.RegisterTypeInIl2Cpp<TaintedMusicBehaviour>();

        _controller = new TaintedMusic.Plugin();
        _controller.Initialize(Config, Log);
        _behaviour = AddComponent<TaintedMusicBehaviour>();

        Log.LogInfo(
            $"{TaintedMusic.Plugin.PluginName} IL2CPP host ready. Runtime={TaintedRuntimeKind.Il2Cpp}; " +
            "Framework=Tainted.Abstractions; NativeOwner=AudioCore/FMOD; EmbeddedOwnedResources=144; " +
            "SeparateMenu=Tainted Music; Renderer=Tainted Interface.");
    }

    public override bool Unload()
    {
        _controller?.Shutdown();
        _controller = null;

        if (_behaviour != null)
        {
            Object.Destroy(_behaviour);
            _behaviour = null;
        }

        return true;
    }
}
