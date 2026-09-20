using BepInEx;
using BepInEx.Unity.IL2CPP;
using Tainted.Abstractions.Runtime;

namespace ImmersiveBackgrounds.Il2Cpp;

[BepInPlugin(ImmersiveBackgrounds.Plugin.PluginGuid, ImmersiveBackgrounds.Plugin.PluginName, ImmersiveBackgrounds.Plugin.PluginVersion)]
[BepInDependency(TaintedFrameworkGuid, TaintedFrameworkMinimumVersion)]
public sealed class Il2CppPlugin : BasePlugin
{
    public const string TaintedFrameworkGuid = "kane.tgfoa.tainted-framework";
    public const string TaintedFrameworkMinimumVersion = "0.1.38";

    private ImmersiveBackgrounds.Plugin? _controller;

    public override void Load()
    {
        _controller = new ImmersiveBackgrounds.Plugin();
        _controller.Initialize(Config, Log);
        Log.LogInfo(
            $"{ImmersiveBackgrounds.Plugin.PluginName} IL2CPP host ready. Runtime={TaintedRuntimeKind.Il2Cpp}; " +
            "Framework=Tainted.Abstractions; NativeOwner=StoryChoices/ItemSet; " +
            "SelectionHook=VChoice.Select; PreviewHook=Choice..ctor; EmbeddedOwnedResources=0.");
    }

    public override bool Unload()
    {
        _controller?.Shutdown();
        _controller = null;
        return true;
    }
}