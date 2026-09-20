using System.Diagnostics;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using Tainted.Abstractions.Capabilities;
using Tainted.Abstractions.Consumers;
using Tainted.Abstractions.Integration;
using Tainted.Abstractions.Plugins;
using Tainted.Abstractions.Readiness;
using Tainted.Abstractions.Release;
using Tainted.Abstractions.Reporting;
using Tainted.Abstractions.Runtime;
using Tainted.Abstractions.Safety;
using Tainted.Abstractions.Services;
using Tainted.Abstractions.Surfaces;
using Tainted.Core.Capabilities;
using Tainted.Core.Contracts;
using Tainted.Core.Plugins;
using Tainted.Core.Runtime;
using Tainted.Core.Services;
using Tainted.Contracts.Manifests;
using Tainted.Contracts.Plugins;
using Tainted.Contracts.Runtime;
using UnityEngine;

namespace Tainted.Host.Mono;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "kane.tgfoa.tainted-framework";
    public const string PluginName = "Tainted Framework";
    public const string PluginVersion = "0.1.33";

    private const string FirstConsumerId = "mods/Tainted-Diagnostic Tool";
    private const string FirstConsumerApiSurface = "runtime-report";
    private const string FirstConsumerMigrationScope = "diagnostics-only";
    private const string FirstConsumerDecisionRecordPath =
        "mods/tainted-framework/docs/decisions/0002-first-consumer-tainted-diagnostic-runtime-report.md";
    private const string FirstConsumerCompileValidationCommand =
        "dotnet build \"mods/Tainted-Diagnostic Tool/src/TemplateDiagnostics.csproj\" -c Release -p:FoAGameRoot=\"A:\\SteamLibrary\\steamapps\\common\\Tainted Grail FoA\" -p:UseSharedCompilation=false -p:NuGetAudit=false -nr:false";

    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<bool> _reportOnlyMode = null!;
    private ConfigEntry<bool> _dryRunOnly = null!;
    private TaintedRuntimeFingerprint? _runtimeFingerprint;
    private ITaintedRuntimeInfo? _runtimeInfo;
    private TaintedRuntimeReport? _runtimeReport;
    private TaintedRuntimeCompatibilitySnapshot? _runtimeCompatibility;
    private TaintedReleaseReadinessStatus? _releaseReadiness;
    private TaintedReportBundle? _reportBundle;
    private TaintedReportExportDestinationPolicy? _reportExportDestinationPolicy;
    private TaintedReportExportRetentionPolicy? _reportExportRetentionPolicy;
    private TaintedReportExportLiveFileValidationPolicy? _reportExportLiveFileValidationPolicy;
    private TaintedReportExportPreflight? _reportExportPreflight;
    private TaintedReportExportPlan? _reportExportPlan;
    private TaintedReportExportReadiness? _reportExportReadiness;
    private TaintedApiSurfaceSnapshot? _apiSurfaceSnapshot;
    private TaintedApiSurfaceVersionPolicy? _apiSurfaceVersionPolicy;
    private TaintedApiSurfaceCompatibilityPolicy? _apiSurfaceCompatibilityPolicy;
    private TaintedApiSurfaceDocumentationStatus? _apiSurfaceDocumentationStatus;
    private TaintedApiSurfacePackageImpactReview? _apiSurfacePackageImpactReview;
    private TaintedFirstConsumerMigrationStatus? _firstConsumerMigration;
    private TaintedApiSurfaceReadiness? _apiSurfaceReadiness;
    private TaintedConsumerReadiness? _consumerReadiness;
    private TaintedHostSafetySnapshot? _hostSafetySnapshot;
    private TaintedContractsManifest? _contractsManifest;
    private TaintedPluginCatalog? _pluginCatalog;
    private TaintedPluginRegistrationResult? _pluginRegistration;
    private TaintedServiceRegistrySnapshot? _serviceRegistry;
    private TaintedServiceActivationReadiness? _serviceActivationReadiness;
    private TaintedModManagerIntegrationStatus? _modManagerIntegration;
    private TaintedExternalFrameworkRelationshipStatus? _externalFrameworkRelationship;

    private void Awake()
    {
        _enabled = Config.Bind(
            "General",
            "Enabled",
            true,
            CreateConfigDescription(
                "Enable the framework host stub.",
                "Framework Host",
                "Enabled",
                sectionOrder: 0,
                order: 0));
        _reportOnlyMode = Config.Bind(
            "Safety",
            "ReportOnlyMode",
            true,
            CreateConfigDescription(
                "Keep framework behavior report-only.",
                "Safety",
                "Report-Only Mode",
                sectionOrder: 10,
                order: 0));
        _dryRunOnly = Config.Bind(
            "Safety",
            "DryRunOnly",
            true,
            CreateConfigDescription(
                "Block mutation-capable behavior.",
                "Safety",
                "Dry-Run Only",
                sectionOrder: 10,
                order: 1));

        var processName = GetProcessName();
        _runtimeFingerprint = CreateRuntimeFingerprint(processName);
        _runtimeInfo = CreateRuntimeInfo(processName, _runtimeFingerprint);
        _runtimeReport = CreateRuntimeReport(
            _runtimeInfo,
            _enabled.Value,
            _reportOnlyMode.Value,
            _dryRunOnly.Value);
        _runtimeCompatibility = CreateRuntimeCompatibility();
        _releaseReadiness = CreateReleaseReadiness();
        _reportBundle = CreateReportBundle(_runtimeReport);
        _reportExportDestinationPolicy = CreateReportExportDestinationPolicy();
        _reportExportRetentionPolicy = CreateReportExportRetentionPolicy();
        _reportExportLiveFileValidationPolicy = CreateReportExportLiveFileValidationPolicy();
        _reportExportPreflight = CreateReportExportPreflight(
            _reportExportDestinationPolicy,
            _reportExportRetentionPolicy,
            _reportExportLiveFileValidationPolicy);
        _reportExportPlan = CreateReportExportPlan(_reportExportPreflight);
        _reportExportReadiness = CreateReportExportReadiness(
            _reportExportDestinationPolicy,
            _reportExportRetentionPolicy,
            _reportExportLiveFileValidationPolicy,
            _reportExportPreflight,
            _reportExportPlan);
        _apiSurfaceSnapshot = CreateApiSurfaceSnapshot();
        _apiSurfaceVersionPolicy = CreateApiSurfaceVersionPolicy();
        _apiSurfaceCompatibilityPolicy = CreateApiSurfaceCompatibilityPolicy();
        _apiSurfaceDocumentationStatus = CreateApiSurfaceDocumentationStatus(_apiSurfaceSnapshot);
        _apiSurfacePackageImpactReview = CreateApiSurfacePackageImpactReview();
        _firstConsumerMigration = CreateFirstConsumerMigration();
        _apiSurfaceReadiness = CreateApiSurfaceReadiness(
            _apiSurfaceSnapshot,
            _apiSurfaceVersionPolicy,
            _apiSurfaceCompatibilityPolicy,
            _apiSurfaceDocumentationStatus,
            _apiSurfacePackageImpactReview,
            _firstConsumerMigration);
        _consumerReadiness = CreateConsumerReadiness(_firstConsumerMigration);
        _hostSafetySnapshot = CreateHostSafetySnapshot(
            _runtimeReport,
            _reportExportReadiness,
            _apiSurfaceReadiness,
            _consumerReadiness);
        _contractsManifest = CreateContractsManifest();
        _pluginCatalog = CreatePluginCatalog();
        _pluginRegistration = CreatePluginRegistration();
        _serviceRegistry = CreateServiceRegistry();
        _serviceActivationReadiness = CreateServiceActivationReadiness(
            _serviceRegistry,
            _reportBundle.ReadinessSnapshot,
            _apiSurfaceReadiness,
            _consumerReadiness);
        _modManagerIntegration = CreateModManagerIntegration();
        _externalFrameworkRelationship = CreateExternalFrameworkRelationship();

        Logger.LogInfo(
            $"{PluginName} {PluginVersion} loaded. " +
            $"runtime={_runtimeInfo.RuntimeKind}; " +
            $"support={_runtimeInfo.SupportState}; " +
            $"reportOnly={_reportOnlyMode.Value}; " +
            $"dryRunOnly={_dryRunOnly.Value}; " +
            $"mutatesRuntime={_runtimeInfo.WouldMutateRuntime}.");

        LogRuntimeReport(_runtimeReport);
        LogRuntimeFingerprint(_runtimeFingerprint);
        LogRuntimeCompatibility(_runtimeCompatibility);
        LogReleaseReadiness(_releaseReadiness);
        LogReportBundle(_reportBundle);
        LogReportExportDestinationPolicy(_reportExportDestinationPolicy);
        LogReportExportRetentionPolicy(_reportExportRetentionPolicy);
        LogReportExportLiveFileValidationPolicy(_reportExportLiveFileValidationPolicy);
        LogReportExportPreflight(_reportExportPreflight);
        LogReportExportPlan(_reportExportPlan);
        LogReportExportReadiness(_reportExportReadiness);
        LogApiSurfaceSnapshot(_apiSurfaceSnapshot);
        LogApiSurfaceVersionPolicy(_apiSurfaceVersionPolicy);
        LogApiSurfaceCompatibilityPolicy(_apiSurfaceCompatibilityPolicy);
        LogApiSurfaceDocumentationStatus(_apiSurfaceDocumentationStatus);
        LogApiSurfacePackageImpactReview(_apiSurfacePackageImpactReview);
        LogFirstConsumerMigration(_firstConsumerMigration);
        LogApiSurfaceReadiness(_apiSurfaceReadiness);
        LogConsumerReadiness(_consumerReadiness);
        LogHostSafetySnapshot(_hostSafetySnapshot);
        LogContractsManifest(_contractsManifest);
        LogPluginCatalog(_pluginCatalog);
        LogPluginRegistration(_pluginRegistration);
        LogServiceRegistry(_serviceRegistry);
        LogServiceActivationReadiness(_serviceActivationReadiness);
        LogModManagerIntegration(_modManagerIntegration);
        LogExternalFrameworkRelationship(_externalFrameworkRelationship);

        if (!_enabled.Value)
        {
            Logger.LogWarning($"{PluginName} is disabled by config; no framework services are active.");
        }
    }

    private static string GetProcessName()
    {
        using (Process process = Process.GetCurrentProcess())
        {
            return process.ProcessName;
        }
    }

    private static ConfigDescription CreateConfigDescription(
        string description,
        string displaySection,
        string displayName,
        int sectionOrder,
        int order)
    {
        return new ConfigDescription(
            description,
            acceptableValues: null,
            new TaintedConfigUiMetadata(displaySection, displayName, sectionOrder, order));
    }

    private static TaintedRuntimeFingerprint CreateRuntimeFingerprint(string processName)
    {
        return TaintedScaffoldRuntimeFingerprintFactory.CreateMono(
            "mono-startup-runtime-fingerprint",
            processName,
            Paths.GameRootPath,
            TaintedSupportState.Provisional.ToString());
    }

    private static ITaintedRuntimeInfo CreateRuntimeInfo(
        string processName,
        TaintedRuntimeFingerprint runtimeFingerprint)
    {
        return new StaticRuntimeInfo(
            processName,
            TaintedRuntimeKind.Mono,
            gameVersion: Application.version ?? "unknown",
            loaderVersion: "BepInEx " + typeof(BaseUnityPlugin).Assembly.GetName().Version,
            unityVersion: Application.unityVersion ?? "unknown",
            buildFingerprint: runtimeFingerprint.BuildFingerprint,
            supportState: TaintedSupportState.Provisional,
            isVerifiedBuild: false,
            isInteropFresh: false,
            wouldMutateRuntime: false);
    }

    private static TaintedRuntimeReport CreateRuntimeReport(
        ITaintedRuntimeInfo runtimeInfo,
        bool enabled,
        bool reportOnly,
        bool dryRunOnly)
    {
        var capabilities = TaintedScaffoldCapabilityPolicy.CreateDefaultDecisions(
            enabled,
            reportOnly,
            dryRunOnly);

        return new TaintedRuntimeReport(PluginVersion, runtimeInfo, capabilities);
    }

    private static TaintedRuntimeCompatibilitySnapshot CreateRuntimeCompatibility()
    {
        return TaintedRuntimeCompatibilityEvaluator.Evaluate(
            "mono-startup-runtime-compatibility",
            PluginVersion,
            "Mono",
            "mono-loader-v5",
            "build-package-validated",
            buildValidationPassed: true,
            packageValidationPassed: true,
            liveLoadValidationPassed: false,
            latestLiveLoadEvidenceVersion: "0.1.30",
            il2CppHostImplemented: false,
            crossBranchBinaryCompatible: false,
            consumerBindingAllowed: false);
    }

    private static TaintedReleaseReadinessStatus CreateReleaseReadiness()
    {
        return TaintedReleaseReadinessEvaluator.Evaluate(
            "mono-package-public-release-readiness",
            PluginVersion,
            "TaintedFramework-" + PluginVersion,
            "mono-loader-v5",
            latestLiveLoadEvidenceVersion: "0.1.30",
            sourceMetadataAligned: true,
            packageLayoutValidated: true,
            packageMetadataAligned: true,
            currentVersionLiveLoadValidated: false,
            il2CppValidationPassed: false,
            publicArchiveCreated: false,
            releaseDecisionRecorded: false,
            protectedFileReviewPassed: true);
    }

    private static TaintedReportBundle CreateReportBundle(TaintedRuntimeReport report)
    {
        var readiness = TaintedRuntimeReadinessEvaluator.Evaluate(report);

        return new TaintedReportBundle(
            "mono-startup",
            report,
            readiness,
            fileExportEnabled: false,
            fileExportReasonCode: "blocked-no-writable-destination");
    }

    private static TaintedReportExportDestinationPolicy CreateReportExportDestinationPolicy()
    {
        return TaintedReportExportDestinationPolicyEvaluator.Evaluate(
            "mono-startup-report-bundle-destination",
            destinationKind: "unnamed",
            destinationPath: string.Empty,
            writableValidationPassed: false,
            fileSystemProbeAllowed: false,
            fileWriteAttempted: false,
            liveGamePathAllowed: false,
            isLiveGamePath: false);
    }

    private static TaintedReportExportRetentionPolicy CreateReportExportRetentionPolicy()
    {
        return TaintedReportExportRetentionPolicyEvaluator.Evaluate(
            "mono-startup-report-bundle-retention",
            retentionPolicy: "undefined",
            retentionValidationPassed: false,
            fileDeletionAllowed: false,
            fileDeleteAttempted: false);
    }

    private static TaintedReportExportLiveFileValidationPolicy CreateReportExportLiveFileValidationPolicy()
    {
        return TaintedReportExportLiveFileValidationPolicyEvaluator.Evaluate(
            "mono-startup-report-bundle-live-file-validation",
            validationScope: "undefined",
            liveFileValidationPassed: false,
            fileSystemProbeAllowed: false,
            fileWriteAttempted: false,
            fileDeleteAttempted: false);
    }

    private static TaintedReportExportPreflight CreateReportExportPreflight(
        TaintedReportExportDestinationPolicy destinationPolicy,
        TaintedReportExportRetentionPolicy retentionPolicy,
        TaintedReportExportLiveFileValidationPolicy liveFileValidationPolicy)
    {
        return TaintedReportExportPreflightEvaluator.Evaluate(
            "mono-startup-report-bundle-preflight",
            "mono-startup-report-bundle",
            dryRunOnly: true,
            destinationPolicy,
            retentionPolicy,
            liveFileValidationPolicy,
            fileWriteAttempted: false);
    }

    private static TaintedReportExportPlan CreateReportExportPlan(
        TaintedReportExportPreflight preflight)
    {
        return new TaintedReportExportPlan(
            preflight.PlanId,
            fileExportEnabled: preflight.CanEnableFileExport,
            destinationKind: preflight.DestinationKind,
            destinationPath: preflight.DestinationPath,
            retentionPolicy: preflight.RetentionPolicy,
            liveFileValidationPassed: preflight.LiveFileValidationPassed,
            blockers: preflight.Blockers);
    }

    private static TaintedReportExportReadiness CreateReportExportReadiness(
        TaintedReportExportDestinationPolicy destinationPolicy,
        TaintedReportExportRetentionPolicy retentionPolicy,
        TaintedReportExportLiveFileValidationPolicy liveFileValidationPolicy,
        TaintedReportExportPreflight preflight,
        TaintedReportExportPlan plan)
    {
        return TaintedReportExportReadinessEvaluator.Evaluate(
            "mono-startup-report-bundle-readiness",
            destinationPolicy,
            retentionPolicy,
            liveFileValidationPolicy,
            preflight,
            plan);
    }

    private static TaintedFirstConsumerMigrationStatus CreateFirstConsumerMigration()
    {
        return TaintedFirstConsumerMigrationEvaluator.Evaluate(
            "mono-startup-first-consumer-migration",
            consumerId: FirstConsumerId,
            apiSurface: FirstConsumerApiSurface,
            migrationScope: FirstConsumerMigrationScope,
            decisionRecordPath: FirstConsumerDecisionRecordPath,
            compileValidationCommand: FirstConsumerCompileValidationCommand,
            loaderDependencyPlanRecorded: true,
            loadOrderPlanRecorded: true,
            packageImpactPlanRecorded: true,
            liveLoadScopeDefined: true,
            protectedFileReviewPassed: true);
    }

    private static TaintedConsumerReadiness CreateConsumerReadiness(
        TaintedFirstConsumerMigrationStatus firstConsumerMigration)
    {
        return TaintedConsumerReadinessEvaluator.Evaluate(
            "mono-startup-consumer-readiness",
            consumerId: firstConsumerMigration.ConsumerId,
            apiSurface: firstConsumerMigration.ApiSurface,
            migrationDecisionRecorded: firstConsumerMigration.IsMigrationDecisionReady,
            compileValidationPassed: true,
            loadOrderValidationPassed: true,
            packageValidationPassed: true,
            liveLoadScopeDefined: firstConsumerMigration.LiveLoadScopeDefined,
            liveLoadValidationPassed: true);
    }

    private static TaintedApiSurfaceSnapshot CreateApiSurfaceSnapshot()
    {
        return TaintedApiSurfaceSnapshotFactory.CreateDefault("mono-startup-api-surface");
    }

    private static TaintedApiSurfaceVersionPolicy CreateApiSurfaceVersionPolicy()
    {
        return TaintedApiSurfaceVersionPolicyFactory.CreateDefault(
            "mono-startup-api-surface-version-policy");
    }

    private static TaintedApiSurfaceCompatibilityPolicy CreateApiSurfaceCompatibilityPolicy()
    {
        return TaintedApiSurfaceCompatibilityPolicyFactory.CreateDefault(
            "mono-startup-api-surface-compatibility-policy");
    }

    private static TaintedApiSurfaceDocumentationStatus CreateApiSurfaceDocumentationStatus(
        TaintedApiSurfaceSnapshot snapshot)
    {
        return TaintedApiSurfaceDocumentationStatusFactory.CreateDefault(
            "mono-startup-api-surface-documentation",
            snapshot);
    }

    private static TaintedApiSurfacePackageImpactReview CreateApiSurfacePackageImpactReview()
    {
        return TaintedApiSurfacePackageImpactReviewFactory.CreateDefault(
            "mono-startup-api-surface-package-impact");
    }

    private static TaintedContractsManifest CreateContractsManifest()
    {
        return TaintedScaffoldContractsManifestFactory.CreateDefault(
            "mono-startup-contracts-manifest",
            PluginVersion);
    }

    private static TaintedPluginCatalog CreatePluginCatalog()
    {
        return TaintedScaffoldPluginCatalogFactory.CreateDefault(
            "mono-startup-plugin-catalog",
            PluginVersion);
    }

    private static TaintedPluginRegistrationResult CreatePluginRegistration()
    {
        var request = new TaintedPluginRegistrationRequest(
            PluginGuid,
            PluginName,
            PluginVersion,
            "mono-loader-v5",
            "plugin-registration",
            frameworkSelfEntry: true);

        return TaintedPluginRegistrationEvaluator.Evaluate(
            "mono-startup-plugin-registration",
            request,
            "bepinex-hosted-registration-api",
            discoveryEnabled: false,
            downstreamLoadEnabled: false,
            consumerBindingAllowed: false,
            firstConsumerDecisionRecorded: true,
            runtimeMutationAllowed: false,
            fileWriteAllowed: false,
            gameApiAllowed: false);
    }

    private static TaintedServiceRegistrySnapshot CreateServiceRegistry()
    {
        return TaintedScaffoldServiceRegistryFactory.CreateDefault(
            "mono-startup-service-registry");
    }

    private static TaintedServiceActivationReadiness CreateServiceActivationReadiness(
        TaintedServiceRegistrySnapshot registry,
        TaintedRuntimeReadinessSnapshot runtimeReadiness,
        TaintedApiSurfaceReadiness apiSurfaceReadiness,
        TaintedConsumerReadiness consumerReadiness)
    {
        return TaintedServiceActivationReadinessEvaluator.Evaluate(
            "mono-startup-service-activation",
            registry,
            runtimeReadiness,
            apiSurfaceReadiness,
            consumerReadiness);
    }

    private static TaintedModManagerIntegrationStatus CreateModManagerIntegration()
    {
        return TaintedModManagerIntegrationEvaluator.Evaluate(
            "mono-startup-mod-manager-integration",
            "FoA Mod Manager",
            apiDecisionRecorded: true,
            uiSurfaceNamed: true,
            reportSurfaceNamed: false,
            validationPlanRecorded: true,
            buildReferenceAllowed: true,
            apiCallAllowed: false,
            consumerBindingAllowed: false,
            runtimeBehaviorAllowed: false,
            liveLoadValidationPassed: false);
    }

    private static TaintedExternalFrameworkRelationshipStatus CreateExternalFrameworkRelationship()
    {
        return TaintedExternalFrameworkRelationshipEvaluator.Evaluate(
            "mono-startup-avalon-core-relationship",
            "Avalon Core",
            relationshipDecisionRecorded: true,
            relationshipTypeNamed: true,
            targetSurfaceNamed: true,
            frameworkSurfaceNamed: true,
            dependencyDirectionNamed: true,
            firstConsumerImpactReviewed: true,
            validationPlanRecorded: true,
            buildReferenceAllowed: true,
            implementationAllowed: false,
            packageImpactReviewed: true,
            consumerBindingAllowed: false,
            liveLoadValidationPassed: false);
    }

    private static TaintedApiSurfaceReadiness CreateApiSurfaceReadiness(
        TaintedApiSurfaceSnapshot snapshot,
        TaintedApiSurfaceVersionPolicy versionPolicy,
        TaintedApiSurfaceCompatibilityPolicy compatibilityPolicy,
        TaintedApiSurfaceDocumentationStatus documentationStatus,
        TaintedApiSurfacePackageImpactReview packageImpactReview,
        TaintedFirstConsumerMigrationStatus firstConsumerMigration)
    {
        return TaintedApiSurfaceReadinessEvaluator.Evaluate(
            "mono-startup-api-surface-readiness",
            snapshot,
            versionPolicyDefined: versionPolicy.IsVersionPolicyDefined,
            compatibilityPolicyDefined: compatibilityPolicy.IsCompatibilityPolicyDefined,
            documentationComplete: documentationStatus.IsDocumentationComplete,
            migrationDecisionRecorded: firstConsumerMigration.IsMigrationDecisionReady,
            consumerCompileValidationPassed: true,
            consumerLoadOrderValidationPassed: true,
            packageImpactReviewed: packageImpactReview.IsPackageImpactReviewed);
    }

    private TaintedHostSafetySnapshot CreateHostSafetySnapshot(
        TaintedRuntimeReport runtimeReport,
        TaintedReportExportReadiness reportExportReadiness,
        TaintedApiSurfaceReadiness apiSurfaceReadiness,
        TaintedConsumerReadiness consumerReadiness)
    {
        return TaintedHostSafetyEvaluator.Evaluate(
            "mono-startup-host-safety",
            _enabled.Value,
            _reportOnlyMode.Value,
            _dryRunOnly.Value,
            runtimeReport,
            reportExportReadiness,
            apiSurfaceReadiness,
            consumerReadiness);
    }

    private void LogRuntimeReport(TaintedRuntimeReport report)
    {
        Logger.LogInfo(
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

        Logger.LogInfo($"{PluginName} capability snapshot: {FormatCapabilities(report)}");
    }

    private void LogRuntimeFingerprint(TaintedRuntimeFingerprint fingerprint)
    {
        Logger.LogInfo(
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

    private void LogRuntimeCompatibility(TaintedRuntimeCompatibilitySnapshot snapshot)
    {
        Logger.LogInfo(
            $"{PluginName} runtime compatibility. " +
            $"schema={snapshot.SchemaVersion}; " +
            $"snapshotId={snapshot.SnapshotId}; " +
            $"frameworkVersion={snapshot.FrameworkVersion}; " +
            $"runtime={snapshot.RuntimeKind}; " +
            $"runtimeTrack={snapshot.RuntimeTrack}; " +
            $"scope={snapshot.CompatibilityScope}; " +
            $"buildValidation={snapshot.BuildValidationPassed}; " +
            $"packageValidation={snapshot.PackageValidationPassed}; " +
            $"liveLoadValidation={snapshot.LiveLoadValidationPassed}; " +
            $"latestLiveLoadEvidence={snapshot.LatestLiveLoadEvidenceVersion}; " +
            $"il2cppHost={snapshot.Il2CppHostImplemented}; " +
            $"crossBranchBinaryCompatible={snapshot.CrossBranchBinaryCompatible}; " +
            $"consumerBindingAllowed={snapshot.ConsumerBindingAllowed}; " +
            $"compatibilityReady={snapshot.IsCompatibilityReady}; " +
            $"blockers={snapshot.BlockerCount}.");
    }

    private void LogReleaseReadiness(TaintedReleaseReadinessStatus status)
    {
        Logger.LogInfo(
            $"{PluginName} release readiness. " +
            $"schema={status.SchemaVersion}; " +
            $"releaseId={status.ReleaseId}; " +
            $"frameworkVersion={status.FrameworkVersion}; " +
            $"packageId={status.PackageId}; " +
            $"runtimeTrack={status.RuntimeTrack}; " +
            $"latestLiveLoadEvidence={status.LatestLiveLoadEvidenceVersion}; " +
            $"sourceMetadata={status.SourceMetadataAligned}; " +
            $"packageLayout={status.PackageLayoutValidated}; " +
            $"packageMetadata={status.PackageMetadataAligned}; " +
            $"currentLiveLoad={status.CurrentVersionLiveLoadValidated}; " +
            $"il2cppValidation={status.Il2CppValidationPassed}; " +
            $"publicArchive={status.PublicArchiveCreated}; " +
            $"releaseDecision={status.ReleaseDecisionRecorded}; " +
            $"protectedFileReview={status.ProtectedFileReviewPassed}; " +
            $"releaseReady={status.IsReleaseReady}; " +
            $"blockers={status.BlockerCount}.");
    }

    private void LogReportBundle(TaintedReportBundle bundle)
    {
        Logger.LogInfo(
            $"{PluginName} report bundle. " +
            $"schema={bundle.SchemaVersion}; " +
            $"bundleId={bundle.BundleId}; " +
            $"fileExportEnabled={bundle.FileExportEnabled}; " +
            $"fileExportReason={bundle.FileExportReasonCode}; " +
            $"readiness={bundle.ReadinessSnapshot.Status}; " +
            $"blockers={bundle.ReadinessSnapshot.BlockerCount}; " +
            $"capabilities={bundle.RuntimeReport.CapabilityCount}.");
    }

    private void LogReportExportDestinationPolicy(TaintedReportExportDestinationPolicy policy)
    {
        Logger.LogInfo(
            $"{PluginName} report export destination policy. " +
            $"schema={policy.SchemaVersion}; " +
            $"policyId={policy.PolicyId}; " +
            $"destination={policy.DestinationKind}; " +
            $"destinationNamed={policy.DestinationNamed}; " +
            $"writableValidation={policy.WritableValidationPassed}; " +
            $"fileSystemProbeAllowed={policy.FileSystemProbeAllowed}; " +
            $"fileWriteAttempted={policy.FileWriteAttempted}; " +
            $"liveGamePathAllowed={policy.LiveGamePathAllowed}; " +
            $"isLiveGamePath={policy.IsLiveGamePath}; " +
            $"eligible={policy.IsDestinationEligible}; " +
            $"blockers={policy.BlockerCount}.");
    }

    private void LogReportExportRetentionPolicy(TaintedReportExportRetentionPolicy policy)
    {
        Logger.LogInfo(
            $"{PluginName} report export retention policy. " +
            $"schema={policy.SchemaVersion}; " +
            $"policyId={policy.PolicyId}; " +
            $"retention={policy.RetentionPolicy}; " +
            $"retentionDefined={policy.RetentionPolicyDefined}; " +
            $"retentionValidation={policy.RetentionValidationPassed}; " +
            $"fileDeletionAllowed={policy.FileDeletionAllowed}; " +
            $"fileDeleteAttempted={policy.FileDeleteAttempted}; " +
            $"eligible={policy.IsRetentionEligible}; " +
            $"blockers={policy.BlockerCount}.");
    }

    private void LogReportExportLiveFileValidationPolicy(TaintedReportExportLiveFileValidationPolicy policy)
    {
        Logger.LogInfo(
            $"{PluginName} report export live-file validation policy. " +
            $"schema={policy.SchemaVersion}; " +
            $"policyId={policy.PolicyId}; " +
            $"validationScope={policy.ValidationScope}; " +
            $"validationScopeDefined={policy.ValidationScopeDefined}; " +
            $"liveFileValidation={policy.LiveFileValidationPassed}; " +
            $"fileSystemProbeAllowed={policy.FileSystemProbeAllowed}; " +
            $"fileWriteAttempted={policy.FileWriteAttempted}; " +
            $"fileDeleteAttempted={policy.FileDeleteAttempted}; " +
            $"eligible={policy.IsLiveFileValidationEligible}; " +
            $"blockers={policy.BlockerCount}.");
    }

    private void LogReportExportPreflight(TaintedReportExportPreflight preflight)
    {
        Logger.LogInfo(
            $"{PluginName} report export preflight. " +
            $"schema={preflight.SchemaVersion}; " +
            $"preflightId={preflight.PreflightId}; " +
            $"planId={preflight.PlanId}; " +
            $"dryRunOnly={preflight.DryRunOnly}; " +
            $"fileWriteAttempted={preflight.FileWriteAttempted}; " +
            $"destinationNamed={preflight.DestinationNamed}; " +
            $"retentionDefined={preflight.RetentionPolicyDefined}; " +
            $"liveFileValidation={preflight.LiveFileValidationPassed}; " +
            $"canEnableFileExport={preflight.CanEnableFileExport}; " +
            $"blockers={preflight.BlockerCount}.");
    }

    private void LogReportExportPlan(TaintedReportExportPlan plan)
    {
        Logger.LogInfo(
            $"{PluginName} report export plan. " +
            $"schema={plan.SchemaVersion}; " +
            $"planId={plan.PlanId}; " +
            $"fileExportEnabled={plan.FileExportEnabled}; " +
            $"destination={plan.DestinationKind}; " +
            $"retention={plan.RetentionPolicy}; " +
            $"liveFileValidation={plan.LiveFileValidationPassed}; " +
            $"blockers={plan.BlockerCount}.");
    }

    private void LogReportExportReadiness(TaintedReportExportReadiness readiness)
    {
        Logger.LogInfo(
            $"{PluginName} report export readiness. " +
            $"schema={readiness.SchemaVersion}; " +
            $"readinessId={readiness.ReadinessId}; " +
            $"planId={readiness.PlanId}; " +
            $"destinationEligible={readiness.DestinationEligible}; " +
            $"retentionEligible={readiness.RetentionEligible}; " +
            $"liveFileValidationEligible={readiness.LiveFileValidationEligible}; " +
            $"preflightPassed={readiness.PreflightPassed}; " +
            $"planAllowsExport={readiness.PlanAllowsExport}; " +
            $"dryRunOnly={readiness.DryRunOnly}; " +
            $"fileWriteAttempted={readiness.FileWriteAttempted}; " +
            $"fileExportReady={readiness.FileExportReady}; " +
            $"blockers={readiness.BlockerCount}.");
    }

    private void LogApiSurfaceSnapshot(TaintedApiSurfaceSnapshot snapshot)
    {
        Logger.LogInfo(
            $"{PluginName} API surface snapshot. " +
            $"schema={snapshot.SchemaVersion}; " +
            $"snapshotId={snapshot.SnapshotId}; " +
            $"surfaces={snapshot.SurfaceCount}; " +
            $"consumerReady={snapshot.ConsumerReadySurfaceCount}; " +
            $"notConsumerReady={snapshot.NotConsumerReadySurfaceCount}; " +
            $"hasConsumerReadySurface={snapshot.HasConsumerReadySurface}.");
    }

    private void LogApiSurfaceVersionPolicy(TaintedApiSurfaceVersionPolicy policy)
    {
        Logger.LogInfo(
            $"{PluginName} API surface version policy. " +
            $"schema={policy.SchemaVersion}; " +
            $"policyId={policy.PolicyId}; " +
            $"surfaceVersion={policy.SurfaceVersion}; " +
            $"scheme={policy.VersioningScheme}; " +
            $"stability={policy.Stability}; " +
            $"breakingChangePolicy={policy.BreakingChangePolicy}; " +
            $"consumerBindingAllowed={policy.ConsumerBindingAllowed}; " +
            $"versionPolicyDefined={policy.IsVersionPolicyDefined}.");
    }

    private void LogApiSurfaceCompatibilityPolicy(TaintedApiSurfaceCompatibilityPolicy policy)
    {
        Logger.LogInfo(
            $"{PluginName} API surface compatibility policy. " +
            $"schema={policy.SchemaVersion}; " +
            $"policyId={policy.PolicyId}; " +
            $"monoLoader={policy.MonoLoaderTrack}; " +
            $"il2cppLoader={policy.Il2CppLoaderTrack}; " +
            $"monoScope={policy.MonoCompatibilityScope}; " +
            $"il2cppScope={policy.Il2CppCompatibilityScope}; " +
            $"crossBranchBinaryCompatible={policy.CrossBranchBinaryCompatible}; " +
            $"consumerBindingAllowed={policy.ConsumerBindingAllowed}; " +
            $"compatibilityPolicyDefined={policy.IsCompatibilityPolicyDefined}.");
    }

    private void LogApiSurfaceDocumentationStatus(TaintedApiSurfaceDocumentationStatus status)
    {
        Logger.LogInfo(
            $"{PluginName} API surface documentation. " +
            $"schema={status.SchemaVersion}; " +
            $"documentationId={status.DocumentationId}; " +
            $"documentationSet={status.DocumentationSetVersion}; " +
            $"documentationPath={status.DocumentationPath}; " +
            $"surfaces={status.SurfaceCount}; " +
            $"documentedSurfaces={status.DocumentedSurfaceCount}; " +
            $"consumerBindingAllowed={status.ConsumerBindingAllowed}; " +
            $"documentationComplete={status.IsDocumentationComplete}.");
    }

    private void LogApiSurfacePackageImpactReview(TaintedApiSurfacePackageImpactReview review)
    {
        Logger.LogInfo(
            $"{PluginName} API surface package impact. " +
            $"schema={review.SchemaVersion}; " +
            $"reviewId={review.ReviewId}; " +
            $"packageId={review.PackageId}; " +
            $"layout={review.PackageLayout}; " +
            $"files={review.PackageFileCount}; " +
            $"manifest={review.ManifestIncluded}; " +
            $"changelog={review.ChangelogIncluded}; " +
            $"dllOnlyRuntimePayload={review.DllOnlyRuntimePayload}; " +
            $"downstreamPackageChangeRequired={review.DownstreamPackageChangeRequired}; " +
            $"consumerBindingAllowed={review.ConsumerBindingAllowed}; " +
            $"packageImpactReviewed={review.IsPackageImpactReviewed}.");
    }

    private void LogFirstConsumerMigration(TaintedFirstConsumerMigrationStatus status)
    {
        Logger.LogInfo(
            $"{PluginName} first consumer migration. " +
            $"schema={status.SchemaVersion}; " +
            $"statusId={status.StatusId}; " +
            $"consumer={status.ConsumerId}; " +
            $"consumerNamed={status.ConsumerNamed}; " +
            $"apiSurface={status.ApiSurface}; " +
            $"apiSurfaceNamed={status.ApiSurfaceNamed}; " +
            $"migrationScope={status.MigrationScope}; " +
            $"migrationScopeNamed={status.MigrationScopeNamed}; " +
            $"decisionRecord={status.DecisionRecordPathNamed}; " +
            $"compileCommand={status.CompileValidationCommandNamed}; " +
            $"loaderDependencyPlan={status.LoaderDependencyPlanRecorded}; " +
            $"loadOrderPlan={status.LoadOrderPlanRecorded}; " +
            $"packageImpactPlan={status.PackageImpactPlanRecorded}; " +
            $"liveLoadScope={status.LiveLoadScopeDefined}; " +
            $"protectedFileReview={status.ProtectedFileReviewPassed}; " +
            $"migrationDecisionReady={status.IsMigrationDecisionReady}; " +
            $"blockers={status.BlockerCount}.");
    }

    private void LogApiSurfaceReadiness(TaintedApiSurfaceReadiness readiness)
    {
        Logger.LogInfo(
            $"{PluginName} API surface readiness. " +
            $"schema={readiness.SchemaVersion}; " +
            $"readinessId={readiness.ReadinessId}; " +
            $"snapshotId={readiness.SnapshotId}; " +
            $"surfaces={readiness.SurfaceCount}; " +
            $"consumerReadySurfaces={readiness.ConsumerReadySurfaceCount}; " +
            $"versionPolicy={readiness.VersionPolicyDefined}; " +
            $"compatibilityPolicy={readiness.CompatibilityPolicyDefined}; " +
            $"documentation={readiness.DocumentationComplete}; " +
            $"migrationDecision={readiness.MigrationDecisionRecorded}; " +
            $"consumerCompileValidation={readiness.ConsumerCompileValidationPassed}; " +
            $"consumerLoadOrderValidation={readiness.ConsumerLoadOrderValidationPassed}; " +
            $"packageImpact={readiness.PackageImpactReviewed}; " +
            $"apiSurfaceReady={readiness.IsApiSurfaceReady}; " +
            $"blockers={readiness.BlockerCount}.");
    }

    private void LogConsumerReadiness(TaintedConsumerReadiness readiness)
    {
        Logger.LogInfo(
            $"{PluginName} consumer readiness. " +
            $"schema={readiness.SchemaVersion}; " +
            $"readinessId={readiness.ReadinessId}; " +
            $"consumer={readiness.ConsumerId}; " +
            $"consumerNamed={readiness.ConsumerNamed}; " +
            $"apiSurface={readiness.ApiSurface}; " +
            $"apiSurfaceNamed={readiness.ApiSurfaceNamed}; " +
            $"migrationDecision={readiness.MigrationDecisionRecorded}; " +
            $"compileValidation={readiness.CompileValidationPassed}; " +
            $"loadOrderValidation={readiness.LoadOrderValidationPassed}; " +
            $"packageValidation={readiness.PackageValidationPassed}; " +
            $"liveLoadScope={readiness.LiveLoadScopeDefined}; " +
            $"liveLoadValidation={readiness.LiveLoadValidationPassed}; " +
            $"consumerReady={readiness.IsConsumerReady}; " +
            $"blockers={readiness.BlockerCount}.");
    }

    private void LogHostSafetySnapshot(TaintedHostSafetySnapshot snapshot)
    {
        Logger.LogInfo(
            $"{PluginName} host safety. " +
            $"schema={snapshot.SchemaVersion}; " +
            $"snapshotId={snapshot.SnapshotId}; " +
            $"enabled={snapshot.Enabled}; " +
            $"reportOnly={snapshot.ReportOnlyMode}; " +
            $"dryRunOnly={snapshot.DryRunOnly}; " +
            $"runtimeMutationAllowed={snapshot.RuntimeMutationAllowed}; " +
            $"fileExportReady={snapshot.FileExportReady}; " +
            $"fileWriteAttempted={snapshot.FileWriteAttempted}; " +
            $"apiSurfaceReady={snapshot.ApiSurfaceReady}; " +
            $"consumerReady={snapshot.ConsumerReady}; " +
            $"consumerBindingAllowed={snapshot.ConsumerBindingAllowed}; " +
            $"nonMutating={snapshot.IsNonMutating}; " +
            $"safeMode={snapshot.IsSafeMode}; " +
            $"violations={snapshot.ViolationCount}.");
    }

    private void LogContractsManifest(TaintedContractsManifest manifest)
    {
        Logger.LogInfo(
            $"{PluginName} contracts manifest. " +
            $"schema={manifest.SchemaVersion}; " +
            $"manifestId={manifest.ManifestId}; " +
            $"frameworkVersion={manifest.FrameworkVersion}; " +
            $"surfaceVersion={manifest.SurfaceVersion}; " +
            $"runtimeTrack={manifest.RuntimeTrack}; " +
            $"status={manifest.ContractStatus}; " +
            $"assemblies={manifest.AssemblyCount}; " +
            $"requiredAssemblies={manifest.RequiredAssemblyCount}; " +
            $"consumerBindingAllowed={manifest.ConsumerBindingAllowed}; " +
            $"runtimeMutationAllowed={manifest.RuntimeMutationAllowed}; " +
            $"fileExportAllowed={manifest.FileExportAllowed}; " +
            $"publicReleaseReady={manifest.PublicReleaseReady}.");
    }

    private void LogPluginCatalog(TaintedPluginCatalog catalog)
    {
        Logger.LogInfo(
            $"{PluginName} plugin catalog. " +
            $"schema={catalog.SchemaVersion}; " +
            $"catalogId={catalog.CatalogId}; " +
            $"frameworkVersion={catalog.FrameworkVersion}; " +
            $"registrationMode={catalog.RegistrationMode}; " +
            $"discoveryEnabled={catalog.DiscoveryEnabled}; " +
            $"downstreamLoadEnabled={catalog.DownstreamLoadEnabled}; " +
            $"consumerBindingAllowed={catalog.ConsumerBindingAllowed}; " +
            $"plugins={catalog.PluginCount}; " +
            $"enabledPlugins={catalog.EnabledPluginCount}; " +
            $"consumerReadyPlugins={catalog.ConsumerReadyPluginCount}.");
    }

    private void LogPluginRegistration(TaintedPluginRegistrationResult registration)
    {
        Logger.LogInfo(
            $"{PluginName} plugin registration. " +
            $"schema={registration.SchemaVersion}; " +
            $"registrationId={registration.RegistrationId}; " +
            $"plugin={registration.Request.PluginId}; " +
            $"mode={registration.RegistrationMode}; " +
            $"identityValid={registration.IdentityValid}; " +
            $"registrationRecorded={registration.RegistrationRecorded}; " +
            $"discoveryEnabled={registration.DiscoveryEnabled}; " +
            $"downstreamLoadEnabled={registration.DownstreamLoadEnabled}; " +
            $"consumerBindingAllowed={registration.ConsumerBindingAllowed}; " +
            $"firstConsumerDecision={registration.FirstConsumerDecisionRecorded}; " +
            $"downstreamRegistrationAllowed={registration.DownstreamRegistrationAllowed}; " +
            $"registrationReady={registration.IsRegistrationReady}; " +
            $"blockers={registration.BlockerCount}.");
    }

    private void LogServiceRegistry(TaintedServiceRegistrySnapshot registry)
    {
        Logger.LogInfo(
            $"{PluginName} service registry. " +
            $"schema={registry.SchemaVersion}; " +
            $"registryId={registry.RegistryId}; " +
            $"mode={registry.RegistrationMode}; " +
            $"services={registry.ServiceCount}; " +
            $"consumerVisible={registry.ConsumerVisibleServiceCount}; " +
            $"activationAllowed={registry.ActivationAllowedServiceCount}; " +
            $"blocked={registry.BlockedServiceCount}; " +
            $"consumerBindingAllowed={registry.ConsumerBindingAllowed}; " +
            $"runtimeMutationAllowed={registry.RuntimeMutationAllowed}; " +
            $"fileWriteAllowed={registry.FileWriteAllowed}; " +
            $"gameApiAllowed={registry.GameApiAllowed}; " +
            $"consumerUsable={registry.IsRegistryUsableByConsumers}.");
    }

    private void LogServiceActivationReadiness(TaintedServiceActivationReadiness readiness)
    {
        Logger.LogInfo(
            $"{PluginName} service activation readiness. " +
            $"schema={readiness.SchemaVersion}; " +
            $"readinessId={readiness.ReadinessId}; " +
            $"registryId={readiness.RegistryId}; " +
            $"services={readiness.ServiceCount}; " +
            $"activationAllowed={readiness.ActivationAllowedServiceCount}; " +
            $"consumerVisible={readiness.ConsumerVisibleServiceCount}; " +
            $"runtimeReady={readiness.RuntimeReady}; " +
            $"apiSurfaceReady={readiness.ApiSurfaceReady}; " +
            $"consumerReady={readiness.ConsumerReady}; " +
            $"consumerBindingAllowed={readiness.ConsumerBindingAllowed}; " +
            $"runtimeMutationAllowed={readiness.RuntimeMutationAllowed}; " +
            $"fileWriteAllowed={readiness.FileWriteAllowed}; " +
            $"gameApiAllowed={readiness.GameApiAllowed}; " +
            $"serviceActivationReady={readiness.IsServiceActivationReady}; " +
            $"blockers={readiness.BlockerCount}.");
    }

    private void LogModManagerIntegration(TaintedModManagerIntegrationStatus status)
    {
        Logger.LogInfo(
            $"{PluginName} Mod Manager integration. " +
            $"schema={status.SchemaVersion}; " +
            $"integrationId={status.IntegrationId}; " +
            $"target={status.TargetModName}; " +
            $"apiDecision={status.ApiDecisionRecorded}; " +
            $"uiSurface={status.UiSurfaceNamed}; " +
            $"reportSurface={status.ReportSurfaceNamed}; " +
            $"validationPlan={status.ValidationPlanRecorded}; " +
            $"buildReferenceAllowed={status.BuildReferenceAllowed}; " +
            $"apiCallAllowed={status.ApiCallAllowed}; " +
            $"consumerBindingAllowed={status.ConsumerBindingAllowed}; " +
            $"runtimeBehaviorAllowed={status.RuntimeBehaviorAllowed}; " +
            $"liveLoadValidation={status.LiveLoadValidationPassed}; " +
            $"integrationReady={status.IsIntegrationReady}; " +
            $"blockers={status.BlockerCount}.");
    }

    private void LogExternalFrameworkRelationship(TaintedExternalFrameworkRelationshipStatus status)
    {
        Logger.LogInfo(
            $"{PluginName} external framework relationship. " +
            $"schema={status.SchemaVersion}; " +
            $"relationshipId={status.RelationshipId}; " +
            $"target={status.TargetFrameworkName}; " +
            $"relationshipDecision={status.RelationshipDecisionRecorded}; " +
            $"relationshipType={status.RelationshipTypeNamed}; " +
            $"targetSurface={status.TargetSurfaceNamed}; " +
            $"frameworkSurface={status.FrameworkSurfaceNamed}; " +
            $"dependencyDirection={status.DependencyDirectionNamed}; " +
            $"firstConsumerImpact={status.FirstConsumerImpactReviewed}; " +
            $"validationPlan={status.ValidationPlanRecorded}; " +
            $"buildReferenceAllowed={status.BuildReferenceAllowed}; " +
            $"implementationAllowed={status.ImplementationAllowed}; " +
            $"packageImpact={status.PackageImpactReviewed}; " +
            $"consumerBindingAllowed={status.ConsumerBindingAllowed}; " +
            $"liveLoadValidation={status.LiveLoadValidationPassed}; " +
            $"relationshipReady={status.IsRelationshipReady}; " +
            $"blockers={status.BlockerCount}.");
    }

    private static string FormatCapabilities(TaintedRuntimeReport report)
    {
        var builder = new StringBuilder();

        for (var index = 0; index < report.Capabilities.Count; index++)
        {
            var capability = report.Capabilities[index];

            if (index > 0)
            {
                builder.Append("; ");
            }

            builder
                .Append(capability.CapabilityId)
                .Append('=')
                .Append(capability.Allowed ? "allowed" : "denied")
                .Append(':')
                .Append(capability.ReasonCode);
        }

        return builder.ToString();
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

    private sealed class TaintedConfigUiMetadata
    {
        internal TaintedConfigUiMetadata(
            string displaySection,
            string displayName,
            int sectionOrder,
            int order)
        {
            DisplaySection = displaySection;
            DisplayName = displayName;
            SectionOrder = sectionOrder;
            Order = order;
        }

        public string DisplaySection { get; }

        public string DisplayName { get; }

        public int SectionOrder { get; }

        public int Order { get; }
    }
}
