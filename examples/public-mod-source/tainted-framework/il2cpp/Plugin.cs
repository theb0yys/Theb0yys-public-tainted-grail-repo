using System.Diagnostics;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Tainted.Abstractions.Capabilities;
using Tainted.Abstractions.Reporting;
using Tainted.Abstractions.Runtime;
using Tainted.Contracts.Runtime;
using Tainted.Core.Capabilities;
using Tainted.Core.Runtime;
using UnityEngine;

namespace Tainted.Host.IL2CPP;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BasePlugin
{
    public const string PluginGuid = "kane.tgfoa.tainted-framework";
    public const string PluginName = "Tainted Framework";
    public const string PluginVersion = "0.1.38";

    public override void Load()
    {
        var processName = GetProcessName();
        var runtimeFingerprint = CreateRuntimeFingerprint(processName);
        var runtimeInfo = CreateRuntimeInfo(processName, runtimeFingerprint);
        var runtimeReport = CreateRuntimeReport(runtimeInfo);

        Log.LogInfo(
            $"{PluginName} {PluginVersion} loaded. " +
            $"runtime={runtimeInfo.RuntimeKind}; " +
            $"support={runtimeInfo.SupportState}; " +
            "reportOnly=True; " +
            "dryRunOnly=True; " +
            $"mutatesRuntime={runtimeInfo.WouldMutateRuntime}; " +
            "hostLiveLoadEvidence=True; " +
            "hostPackageEvidence=True.");

        LogRuntimeReport(runtimeReport);
        LogRuntimeFingerprint(runtimeFingerprint);
        LogCapabilitySnapshot(runtimeReport);
    }

    private static string GetProcessName()
    {
        using (var process = Process.GetCurrentProcess())
        {
            return process.ProcessName;
        }
    }

    private static TaintedRuntimeFingerprint CreateRuntimeFingerprint(string processName)
    {
        return TaintedScaffoldRuntimeFingerprintFactory.CreateIl2Cpp(
            "il2cpp-startup-runtime-fingerprint",
            processName,
            Paths.GameRootPath,
            System.IO.Path.Combine(Paths.GameRootPath, "BepInEx"),
            TaintedSupportState.Verified.ToString());
    }

    private static ITaintedRuntimeInfo CreateRuntimeInfo(
        string processName,
        TaintedRuntimeFingerprint runtimeFingerprint)
    {
        return new StaticRuntimeInfo(
            processName,
            TaintedRuntimeKind.Il2Cpp,
            gameVersion: Application.version ?? "unknown",
            loaderVersion: "BepInEx IL2CPP " + typeof(BasePlugin).Assembly.GetName().Version,
            unityVersion: Application.unityVersion ?? "unknown",
            buildFingerprint: runtimeFingerprint.BuildFingerprint,
            supportState: TaintedSupportState.Verified,
            isVerifiedBuild: true,
            isInteropFresh: false,
            wouldMutateRuntime: false);
    }

    private static TaintedRuntimeReport CreateRuntimeReport(ITaintedRuntimeInfo runtimeInfo)
    {
        var capabilities = TaintedScaffoldCapabilityPolicy.CreateDefaultDecisions(
            enabled: true,
            reportOnlyMode: true,
            dryRunOnly: true,
            il2CppHostLiveLoadValidated: true);

        return new TaintedRuntimeReport(PluginVersion, runtimeInfo, capabilities);
    }

    private void LogRuntimeReport(TaintedRuntimeReport report)
    {
        Log.LogInfo(
            $"{PluginName} runtime report. " +
            $"schema={report.SchemaVersion}; " +
            $"frameworkVersion={report.FrameworkVersion}; " +
            $"process={report.RuntimeInfo.ProcessName}; " +
            $"gameVersion={report.RuntimeInfo.GameVersion}; " +
            $"loader={report.RuntimeInfo.LoaderVersion}; " +
            $"unity={report.RuntimeInfo.UnityVersion}; " +
            $"support={report.RuntimeInfo.SupportState}; " +
            $"verified={report.RuntimeInfo.IsVerifiedBuild}; " +
            $"interopFresh={report.RuntimeInfo.IsInteropFresh}; " +
            $"capabilities={report.CapabilityCount}; " +
            $"allowed={report.AllowedCapabilityCount}; " +
            $"denied={report.DeniedCapabilityCount}.");
    }

    private void LogRuntimeFingerprint(TaintedRuntimeFingerprint fingerprint)
    {
        Log.LogInfo(
            $"{PluginName} runtime fingerprint. " +
            $"schema={fingerprint.SchemaVersion}; " +
            $"fingerprintId={fingerprint.FingerprintId}; " +
            $"runtimeTrack={fingerprint.RuntimeTrack}; " +
            $"gameRootKnown={fingerprint.GameRootKnown}; " +
            $"files={fingerprint.FileCount}; " +
            $"presentFiles={fingerprint.PresentFileCount}; " +
            $"monoManagedAssembly={fingerprint.MonoManagedAssemblyPresent}; " +
            $"il2cppMarkers={fingerprint.Il2CppMarkersPresent}; " +
            $"interopHash={fingerprint.InteropHashPresent}; " +
            $"buildFingerprintAvailable={fingerprint.BuildFingerprintAvailable}; " +
            $"support={fingerprint.SupportState}.");
    }

    private void LogCapabilitySnapshot(TaintedRuntimeReport report)
    {
        Log.LogInfo($"{PluginName} capability snapshot: {FormatCapabilities(report)}");
    }

    private static string FormatCapabilities(TaintedRuntimeReport report)
    {
        var values = new string[report.Capabilities.Count];

        for (var index = 0; index < report.Capabilities.Count; index++)
        {
            var capability = report.Capabilities[index];
            values[index] =
                capability.CapabilityId +
                "=" +
                (capability.Allowed ? "allowed" : "denied") +
                ":" +
                capability.ReasonCode;
        }

        return string.Join("; ", values);
    }

    private sealed class StaticRuntimeInfo : ITaintedRuntimeInfo
    {
        public StaticRuntimeInfo(
            string processName,
            TaintedRuntimeKind runtimeKind,
            string gameVersion,
            string loaderVersion,
            string unityVersion,
            string buildFingerprint,
            TaintedSupportState supportState,
            bool isVerifiedBuild,
            bool isInteropFresh,
            bool wouldMutateRuntime)
        {
            ProcessName = processName;
            RuntimeKind = runtimeKind;
            GameVersion = gameVersion;
            LoaderVersion = loaderVersion;
            UnityVersion = unityVersion;
            BuildFingerprint = buildFingerprint;
            SupportState = supportState;
            IsVerifiedBuild = isVerifiedBuild;
            IsInteropFresh = isInteropFresh;
            WouldMutateRuntime = wouldMutateRuntime;
        }

        public string ProcessName { get; }

        public TaintedRuntimeKind RuntimeKind { get; }

        public string GameVersion { get; }

        public string LoaderVersion { get; }

        public string UnityVersion { get; }

        public string BuildFingerprint { get; }

        public TaintedSupportState SupportState { get; }

        public bool IsVerifiedBuild { get; }

        public bool IsInteropFresh { get; }

        public bool WouldMutateRuntime { get; }
    }
}
