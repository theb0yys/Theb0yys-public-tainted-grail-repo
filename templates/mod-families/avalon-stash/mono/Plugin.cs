using BepInEx;
using Tainted.Abstractions.Runtime;

namespace TGTemplate.AvalonStash;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency(TaintedFrameworkGuid, BepInDependency.DependencyFlags.HardDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.template.avalon-stash";
    public const string PluginName = "Avalon Stash Template";
    public const string PluginVersion = "0.1.0";
    public const string TaintedFrameworkGuid = "kane.tgfoa.tainted-framework";

    private void Awake()
    {
        Logger.LogInfo(Feature.Describe(TaintedRuntimeKind.Mono));
    }
}
