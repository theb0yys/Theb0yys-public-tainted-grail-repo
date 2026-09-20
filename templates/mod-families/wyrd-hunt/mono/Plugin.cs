using BepInEx;
using Tainted.Abstractions.Runtime;

namespace TGTemplate.WyrdHunt;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency(TaintedFrameworkGuid, BepInDependency.DependencyFlags.HardDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.template.wyrd-hunt";
    public const string PluginName = "Wyrd Hunt Template";
    public const string PluginVersion = "0.1.0";
    public const string TaintedFrameworkGuid = "kane.tgfoa.tainted-framework";

    private void Awake()
    {
        Logger.LogInfo(Feature.Describe(TaintedRuntimeKind.Mono));
    }
}
