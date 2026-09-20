using BepInEx;
using BepInEx.Unity.IL2CPP;
using Tainted.Abstractions.Runtime;

namespace TGTemplate.MerchantStockTweaks;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency(TaintedFrameworkGuid, BepInDependency.DependencyFlags.HardDependency)]
public sealed class Plugin : BasePlugin
{
    public const string PluginGuid = "community.taintedgrail.template.merchant-stock-tweaks";
    public const string PluginName = "Merchant Stock Tweaks Template";
    public const string PluginVersion = "0.1.0";
    public const string TaintedFrameworkGuid = "kane.tgfoa.tainted-framework";

    public override void Load()
    {
        Log.LogInfo(Feature.Describe(TaintedRuntimeKind.Il2Cpp));
    }
}
