using AvalonCore.Engines;
using BepInEx;

namespace TGCommunity.CoreReadOnly;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency(AvalonCore.Plugin.PluginGuid, BepInDependency.DependencyFlags.HardDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.infrastructure.core-readonly";
    public const string PluginName = "TG Community Avalon Core Read-Only";
    public const string PluginVersion = "0.1.0";

    private void Awake()
    {
        AvalonCore.HostTrustReportSnapshot trust = AvalonCore.Plugin.TrustReports;

        if (trust.WouldMutateRuntime)
        {
            Logger.LogWarning("Unexpected mutable trust snapshot; action=none.");
            return;
        }

        Logger.LogInfo(
            $"Core trust report: status={trust.Status}; " +
            $"candidates={trust.CandidatePackCount}; valid={trust.ValidPackCount}; " +
            $"blockers={trust.BlockingIssueCount}; action=read-only.");

        if (AvalonCore.Plugin.Registry == null ||
            !AvalonCore.Plugin.Registry.TryGet(
                "adapter-registry",
                out AdapterRegistryEngine? registry) ||
            registry == null)
        {
            Logger.LogWarning("adapter-registry unavailable; action=none.");
            return;
        }

        AvalonAdapterRegistryReport report = registry.CreateReport();

        Logger.LogInfo(
            $"Core registry: adapters={report.AdapterCount}; " +
            $"capabilities={report.CapabilityCount}; " +
            $"runtimeMutationCapabilities={report.RuntimeMutationCapabilityCount}; " +
            "action=read-only.");
    }
}
