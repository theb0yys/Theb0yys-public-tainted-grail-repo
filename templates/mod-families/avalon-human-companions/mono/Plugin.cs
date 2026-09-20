using BepInEx;
using Tainted.Abstractions.Runtime;

namespace TGTemplate.AvalonHumanCompanions;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency(TaintedFrameworkGuid, BepInDependency.DependencyFlags.HardDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.template.avalon-human-companions";
    public const string PluginName = "Avalon Human Companions Template";
    public const string PluginVersion = "0.1.0";
    public const string TaintedFrameworkGuid = "kane.tgfoa.tainted-framework";

    private void Awake()
    {
        Logger.LogInfo(Feature.Describe(TaintedRuntimeKind.Mono));
    }
}
