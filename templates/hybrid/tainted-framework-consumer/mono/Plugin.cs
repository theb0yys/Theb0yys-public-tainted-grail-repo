using BepInEx;
using Tainted.Abstractions.Runtime;

namespace TGTemplate.TaintedFrameworkConsumer;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency(TaintedFrameworkGuid, BepInDependency.DependencyFlags.HardDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.template.framework-consumer";
    public const string PluginName = "TG Tainted Framework Consumer Template";
    public const string PluginVersion = "0.1.0";
    public const string TaintedFrameworkGuid = "kane.tgfoa.tainted-framework";

    private void Awake()
    {
        Logger.LogInfo(SharedFeature.Describe(TaintedRuntimeKind.Mono));
    }
}
