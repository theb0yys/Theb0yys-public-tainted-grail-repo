using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
#if IL2CPP
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TaintedPerformance;

#if !IL2CPP
[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency(TaintedFrameworkPluginGuid, TaintedFrameworkMinimumVersion)]
[BepInDependency(TaintedInterfacePluginGuid, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(FoAModManagerPluginGuid, BepInDependency.DependencyFlags.SoftDependency)]
public sealed class Plugin : BaseUnityPlugin
#else
public sealed class Plugin
#endif
{
    public const string PluginGuid = "kane.tgfoa.tainted-performance";
    public const string PluginName = "Tainted Performance";
    public const string PluginVersion = "0.2.0";
    public const string TaintedFrameworkPluginGuid = "kane.tgfoa.tainted-framework";
    public const string TaintedFrameworkMinimumVersion = "0.1.38";
    public const string TaintedInterfacePluginGuid = "kane.tgfoa.tainted-interface";
    public const string FoAModManagerPluginGuid = "kane.tgfoa.mod-manager";

    private TaintedPerformanceConfig? _config;
    private RuntimePerformanceWatcher? _watcher;
    private PerformanceOverviewScreen? _overviewScreen;
    private ManualLogSource? _logger;
    private bool _initialized;

#if !IL2CPP
    private void Awake()
    {
        Initialize(Config, Logger);
    }

    private void Update()
    {
        Tick();
    }

    private void OnGUI()
    {
        Draw();
    }

    private void OnDestroy()
    {
        Shutdown();
    }
#endif

    internal void Initialize(ConfigFile configFile, ManualLogSource logger)
    {
        if (_initialized)
        {
            return;
        }

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = TaintedPerformanceConfig.Bind(configFile);
        _watcher = new RuntimePerformanceWatcher(_config, _logger);
        _overviewScreen = new PerformanceOverviewScreen(_config, _watcher, _logger);
        TaintedInterfaceBridge.SetLogger(_logger);
        if (_config.RegisterWithFoAModManager.Value)
        {
            FoAModManagerBridge.RegisterIntegrations(_logger, _watcher.GetOverview, _overviewScreen.Show);
        }

        _initialized = true;
        _logger.LogInfo(
            $"{PluginName} {PluginVersion} loaded. Runtime={RuntimeCompatibility.RuntimeName}; TaintedFramework>={TaintedFrameworkMinimumVersion}; Standalone=true; TaintedInterface=soft; FoAModManager=soft; OverviewPanel={_config.OverviewPanelEnabled.Value}; OverviewHotkey={_config.OverviewPanelHotkey.Value}; AutomaticIncidents={_config.AutomaticIncidentsEnabled.Value}; ThresholdFps={_config.LowFpsThreshold.Value}; SustainedSeconds={_config.SustainedSeconds.Value}; ManualReport={_config.ManualReportEnabled.Value}; ReportHotkey={_config.ManualReportHotkey.Value}.");
    }

    internal void Tick()
    {
        if (!_initialized)
        {
            return;
        }

        _watcher?.Update();
        _overviewScreen?.Update();
    }

    internal void Draw()
    {
        if (_initialized)
        {
            _overviewScreen?.OnGUI();
        }
    }

    internal void Shutdown()
    {
        if (!_initialized)
        {
            return;
        }

        _initialized = false;
        _overviewScreen?.Close();
        FoAModManagerBridge.UnregisterIntegrations();
        _watcher?.Suspend();
        _overviewScreen = null;
        _watcher = null;
        _config = null;
        _logger = null;
    }
}

internal sealed class TaintedPerformanceConfig
{
    private TaintedPerformanceConfig(
        ConfigEntry<bool> enabled,
        ConfigEntry<bool> manualReportEnabled,
        ConfigEntry<KeyCode> manualReportHotkey,
        ConfigEntry<int> sampleCapacity,
        ConfigEntry<float> referenceFramesPerSecond,
        ConfigEntry<bool> automaticIncidentsEnabled,
        ConfigEntry<float> lowFpsThreshold,
        ConfigEntry<float> sustainedSeconds,
        ConfigEntry<int> maxReportsPerSession,
        ConfigEntry<bool> useFrameTimingManager,
        ConfigEntry<bool> ignorePausedTimeScale,
        ConfigEntry<bool> overviewPanelEnabled,
        ConfigEntry<KeyCode> overviewPanelHotkey,
        ConfigEntry<bool> freezeWorldWhileOverviewOpen,
        ConfigEntry<bool> registerWithFoAModManager)
    {
        Enabled = enabled;
        ManualReportEnabled = manualReportEnabled;
        ManualReportHotkey = manualReportHotkey;
        SampleCapacity = sampleCapacity;
        ReferenceFramesPerSecond = referenceFramesPerSecond;
        AutomaticIncidentsEnabled = automaticIncidentsEnabled;
        LowFpsThreshold = lowFpsThreshold;
        SustainedSeconds = sustainedSeconds;
        MaxReportsPerSession = maxReportsPerSession;
        UseFrameTimingManager = useFrameTimingManager;
        IgnorePausedTimeScale = ignorePausedTimeScale;
        OverviewPanelEnabled = overviewPanelEnabled;
        OverviewPanelHotkey = overviewPanelHotkey;
        FreezeWorldWhileOverviewOpen = freezeWorldWhileOverviewOpen;
        RegisterWithFoAModManager = registerWithFoAModManager;
    }

    public ConfigEntry<bool> Enabled { get; }
    public ConfigEntry<bool> ManualReportEnabled { get; }
    public ConfigEntry<KeyCode> ManualReportHotkey { get; }
    public ConfigEntry<int> SampleCapacity { get; }
    public ConfigEntry<float> ReferenceFramesPerSecond { get; }
    public ConfigEntry<bool> AutomaticIncidentsEnabled { get; }
    public ConfigEntry<float> LowFpsThreshold { get; }
    public ConfigEntry<float> SustainedSeconds { get; }
    public ConfigEntry<int> MaxReportsPerSession { get; }
    public ConfigEntry<bool> UseFrameTimingManager { get; }
    public ConfigEntry<bool> IgnorePausedTimeScale { get; }
    public ConfigEntry<bool> OverviewPanelEnabled { get; }
    public ConfigEntry<KeyCode> OverviewPanelHotkey { get; }
    public ConfigEntry<bool> FreezeWorldWhileOverviewOpen { get; }
    public ConfigEntry<bool> RegisterWithFoAModManager { get; }

    public static TaintedPerformanceConfig Bind(ConfigFile config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        return new TaintedPerformanceConfig(
            config.Bind("General", "Enabled", true, PublicConfig("Enable Tainted Performance.", "General", "Enabled", 0, 0)),
            config.Bind("Reports", "ManualReportEnabled", true, PublicConfig("Enable the manual performance report hotkey.", "Reports", "Manual Report Hotkey", 10, 0)),
            config.Bind("Reports", "ManualReportHotkey", KeyCode.F9, PublicConfig("Hotkey that writes a local performance report from the current bounded sample window.", "Reports", "Report Hotkey", 10, 10)),
            config.Bind(
                "Reports",
                "SampleCapacity",
                4096,
                PublicConfig(
                    "Maximum frame samples retained for manual reports and configured incident windows.",
                    "Reports",
                    "Sample Capacity",
                    10,
                    20,
                    new AcceptableValueRange<int>(128, 20000))),
            config.Bind(
                "Monitor",
                "ReferenceFramesPerSecond",
                60f,
                PublicConfig(
                    "Reference FPS used for dashboard budget-miss and frame-pacing metrics. This does not enable automatic reports.",
                    "Monitor",
                    "Reference FPS",
                    15,
                    0,
                    new AcceptableValueRange<float>(15f, 240f))),
            config.Bind("AutomaticIncidents", "Enabled", false, PublicConfig("Enable automatic low-FPS incident reports. Set LowFpsThreshold and SustainedSeconds above zero before enabling.", "Warnings", "Automatic Incident Reports", 20, 0)),
            config.Bind(
                "AutomaticIncidents",
                "LowFpsThreshold",
                0f,
                PublicConfig(
                    "Configured rolling-average FPS threshold. Zero disables automatic incident thresholds.",
                    "Warnings",
                    "Low FPS Threshold",
                    20,
                    10,
                    new AcceptableValueRange<float>(0f, 240f))),
            config.Bind(
                "AutomaticIncidents",
                "SustainedSeconds",
                0f,
                PublicConfig(
                    "Configured continuous seconds at or below LowFpsThreshold before an automatic report. Zero disables automatic incident thresholds.",
                    "Warnings",
                    "Sustained Seconds",
                    20,
                    20,
                    new AcceptableValueRange<float>(0f, 120f))),
            config.Bind(
                "AutomaticIncidents",
                "MaxReportsPerSession",
                3,
                PublicConfig(
                    "Maximum automatic incident requests per game session. Failed writes still consume a request to prevent retry storms.",
                    "Warnings",
                    "Max Reports Per Session",
                    20,
                    30,
                    new AcceptableValueRange<int>(1, 10))),
            config.Bind("Counters", "UseUnityFrameTimingManager", true, PublicConfig("Sample Unity FrameTimingManager when available.", "Counters", "Unity Frame Timing", 30, 0)),
            config.Bind("Counters", "IgnorePausedTimeScale", true, PublicConfig("Do not add samples while Time.timeScale is effectively paused.", "Counters", "Ignore Paused Time", 30, 10)),
            config.Bind("Interface", "OverviewPanelEnabled", true, PublicConfig("Enable the in-game Tainted Performance overview screen.", "Interface", "Overview Screen", 40, 0)),
            config.Bind("Interface", "OverviewPanelHotkey", KeyCode.F8, PublicConfig("Hotkey that opens or closes the in-game overview screen.", "Interface", "Overview Hotkey", 40, 10)),
            config.Bind("Interface", "FreezeWorldWhileOverviewOpen", true, PublicConfig("Freeze game world time while the overview screen is open through Tainted Interface or FoA Mod Manager UI scope.", "Interface", "Freeze World In Overview", 40, 20)),
            config.Bind("Interface", "RegisterWithFoAModManager", true, PublicConfig("Register Tainted Performance status and open-panel actions with FoA Mod Manager when that optional mod is loaded.", "Interface", "FoA Mod Manager Integration", 40, 30)));
    }

    private static ConfigDescription PublicConfig(
        string description,
        string displaySection,
        string displayName,
        int sectionOrder,
        int order,
        AcceptableValueBase? acceptableValues = null)
    {
        bool hidden = !(
            string.Equals(displaySection, "General", StringComparison.Ordinal) &&
            string.Equals(displayName, "Enabled", StringComparison.Ordinal)) &&
            !string.Equals(displayName, "Overview Hotkey", StringComparison.Ordinal) &&
            !string.Equals(displayName, "Report Hotkey", StringComparison.Ordinal);

        return new ConfigDescription(
            description,
            acceptableValues,
            new TaintedPerformanceConfigUiMetadata(displaySection, displayName, sectionOrder, order, hidden));
    }

    private sealed class TaintedPerformanceConfigUiMetadata
    {
        public TaintedPerformanceConfigUiMetadata(
            string displaySection,
            string displayName,
            int sectionOrder,
            int order,
            bool hidden)
        {
            DisplaySection = displaySection;
            DisplayName = displayName;
            SectionOrder = sectionOrder;
            Order = order;
            Hidden = hidden;
        }

        public string DisplaySection { get; }
        public string DisplayName { get; }
        public int SectionOrder { get; }
        public int Order { get; }
        public bool Hidden { get; }
    }
}

internal sealed class RuntimePerformanceWatcher
{
    private const float PauseTimeScaleThreshold = 0.0001f;

    private static readonly UTF8Encoding Utf8WithoutBom = new UTF8Encoding(false);

    private readonly TaintedPerformanceConfig _config;
    private readonly ManualLogSource _logger;
    private readonly PerformanceSampleWindow _manualWindow;
    private readonly LowPerformanceIncidentDetector _incidentDetector;
#if IL2CPP
    private readonly Il2CppStructArray<FrameTiming> _frameTimingBuffer = new(1);
#else
    private readonly FrameTiming[] _frameTimingBuffer = new FrameTiming[1];
#endif
    private readonly string _outputRoot;

    private bool _frameTimingEnabled;
    private bool _frameTimingReturnedData;
    private bool _allocationCounterEnabled = true;
    private long _lastAllocatedBytes = -1L;
    private bool _gcBaselineReady;
    private int _lastGen0Collections;
    private int _lastGen1Collections;
    private int _lastGen2Collections;
    private PerformanceRuntimeOverview? _cachedOverview;
    private float _nextOverviewRefreshTime;
    private string _lastReportId = string.Empty;
    private string _lastReportFolder = string.Empty;
    private string _lastReportTriggerKind = string.Empty;
    private string _lastReportStatus = "No report written yet.";
    private DateTime _lastReportUtc;

    public RuntimePerformanceWatcher(TaintedPerformanceConfig config, ManualLogSource logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        int minimumCapacity = Math.Max(128, _config.SampleCapacity.Value);
        double threshold = _config.AutomaticIncidentsEnabled.Value ? _config.LowFpsThreshold.Value : 0d;
        double sustainedSeconds = _config.AutomaticIncidentsEnabled.Value ? _config.SustainedSeconds.Value : 0d;
        int incidentCapacity = LowPerformanceIncidentDetector.CalculateSampleCapacity(
            threshold,
            sustainedSeconds,
            minimumCapacity);

        _manualWindow = new PerformanceSampleWindow(incidentCapacity);
        _incidentDetector = new LowPerformanceIncidentDetector(
            threshold,
            sustainedSeconds,
            Math.Max(1, _config.MaxReportsPerSession.Value),
            incidentCapacity);
        _frameTimingEnabled = _config.UseFrameTimingManager.Value;
        _outputRoot = Path.Combine(Paths.ConfigPath, Plugin.PluginGuid, "performance-reports");

        if (!_incidentDetector.IsConfigured && _config.AutomaticIncidentsEnabled.Value)
        {
            _logger.LogWarning("Tainted Performance automatic incidents are enabled but LowFpsThreshold or SustainedSeconds is zero; automatic incident detection is disabled until both values are configured.");
        }
    }

    public void Update()
    {
        if (!_config.Enabled.Value)
        {
            Suspend();
            return;
        }

        if (_config.ManualReportEnabled.Value && Input.GetKeyDown(_config.ManualReportHotkey.Value))
        {
            WriteManualReport();
        }

        bool eligible = IsEligibleForSampling();
        if (!eligible)
        {
            _incidentDetector.Observe(false, default);
            ResetCounterBaselines();
            return;
        }

        PerformanceFrameSample sample = CollectSample();
        _manualWindow.Add(sample);
        PerformanceWindowSnapshot? incident = _incidentDetector.Observe(true, sample);
        if (incident != null)
        {
            TryWriteReport(incident);
        }
    }

    public void Suspend()
    {
        _manualWindow.Clear();
        _incidentDetector.Observe(false, default);
        ResetCounterBaselines();
        _cachedOverview = null;
    }

    public bool WriteManualReport()
    {
        return TryWriteReport(PerformanceWindowSnapshot.Manual(_manualWindow.CopyChronologically()));
    }

    public PerformanceRuntimeOverview GetOverview()
    {
        float now = Time.realtimeSinceStartup;
        if (_cachedOverview != null && now < _nextOverviewRefreshTime)
        {
            return _cachedOverview;
        }

        _cachedOverview = BuildOverview();
        _nextOverviewRefreshTime = now + 0.5f;
        return _cachedOverview;
    }

    private bool IsEligibleForSampling()
    {
        return !_config.IgnorePausedTimeScale.Value || Time.timeScale > PauseTimeScaleThreshold;
    }

    private PerformanceFrameSample CollectSample()
    {
        ReadFrameTiming(out double cpuFrameMilliseconds, out double gpuFrameMilliseconds);
        long allocationDelta = ReadAllocationDelta();
        ReadCollectionDeltas(out int gen0Collections, out int gen1Collections, out int gen2Collections);

        return new PerformanceFrameSample(
            Time.frameCount,
            Time.unscaledDeltaTime,
            cpuFrameMilliseconds,
            gpuFrameMilliseconds,
            allocationDelta,
            gen0Collections,
            gen1Collections,
            gen2Collections);
    }

    private void ReadFrameTiming(out double cpuFrameMilliseconds, out double gpuFrameMilliseconds)
    {
        cpuFrameMilliseconds = -1d;
        gpuFrameMilliseconds = -1d;
        if (!_frameTimingEnabled)
        {
            return;
        }

        try
        {
            FrameTimingManager.CaptureFrameTimings();
            uint count = FrameTimingManager.GetLatestTimings(1u, _frameTimingBuffer);
            if (count == 0u)
            {
                return;
            }

            FrameTiming timing = _frameTimingBuffer[0];
            cpuFrameMilliseconds = NormalizeTiming(timing.cpuFrameTime);
            gpuFrameMilliseconds = NormalizeTiming(timing.gpuFrameTime);
            if (cpuFrameMilliseconds > 0d || gpuFrameMilliseconds > 0d)
            {
                _frameTimingReturnedData = true;
            }
        }
        catch (Exception ex)
        {
            _frameTimingEnabled = false;
            _logger.LogWarning($"Tainted Performance disabled Unity frame timing after {ex.GetType().Name}; reports will use remaining counters.");
        }
    }

    private long ReadAllocationDelta()
    {
        if (!_allocationCounterEnabled)
        {
            return -1L;
        }

        try
        {
            long current = GC.GetAllocatedBytesForCurrentThread();
            long delta = _lastAllocatedBytes < 0L || current < _lastAllocatedBytes
                ? 0L
                : current - _lastAllocatedBytes;
            _lastAllocatedBytes = current;
            return delta;
        }
        catch (Exception ex)
        {
            _allocationCounterEnabled = false;
            _lastAllocatedBytes = -1L;
            _logger.LogWarning($"Tainted Performance disabled the main-thread allocation counter after {ex.GetType().Name}; reports will mark allocation samples unavailable.");
            return -1L;
        }
    }

    private void ReadCollectionDeltas(
        out int gen0Collections,
        out int gen1Collections,
        out int gen2Collections)
    {
        int currentGen0 = GC.CollectionCount(0);
        int currentGen1 = GC.CollectionCount(1);
        int currentGen2 = GC.CollectionCount(2);

        if (!_gcBaselineReady)
        {
            gen0Collections = 0;
            gen1Collections = 0;
            gen2Collections = 0;
            _gcBaselineReady = true;
        }
        else
        {
            gen0Collections = Math.Max(0, currentGen0 - _lastGen0Collections);
            gen1Collections = Math.Max(0, currentGen1 - _lastGen1Collections);
            gen2Collections = Math.Max(0, currentGen2 - _lastGen2Collections);
        }

        _lastGen0Collections = currentGen0;
        _lastGen1Collections = currentGen1;
        _lastGen2Collections = currentGen2;
    }

    private void ResetCounterBaselines()
    {
        _lastAllocatedBytes = -1L;
        _gcBaselineReady = false;
    }

    private bool TryWriteReport(PerformanceWindowSnapshot snapshot)
    {
        try
        {
            DateTime generatedUtc = DateTime.UtcNow;
            string baseReportId = string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "tainted-performance-{0:yyyyMMdd-HHmmss-fff}-{1}",
                generatedUtc,
                snapshot.TriggerKind);
            string reportId = baseReportId;
            string reportFolder = Path.Combine(_outputRoot, reportId);
            int collision = 1;
            while (Directory.Exists(reportFolder))
            {
                collision++;
                reportId = baseReportId + "-" + collision.ToString(System.Globalization.CultureInfo.InvariantCulture);
                reportFolder = Path.Combine(_outputRoot, reportId);
            }

            Directory.CreateDirectory(reportFolder);

            LoadedPluginContext[] loadedPlugins = CollectLoadedPlugins();
            int otherPluginCount = CountOtherPlugins(loadedPlugins);
            PerformanceWindowSummary summary = PerformanceWindowSummary.Create(
                snapshot,
                _config.ReferenceFramesPerSecond.Value);
            PerformanceInvestigationAssessment assessment = PerformanceInvestigationAnalyzer.Classify(
                snapshot,
                summary,
                loadedPlugins.Length,
                otherPluginCount);
            PerformanceReportContext context = BuildContext(reportId, generatedUtc);

            WriteNewTextFile(
                Path.Combine(reportFolder, "performance_report.json"),
                PerformanceReportFormatter.BuildJson(context, snapshot, summary, assessment, loadedPlugins));
            WriteNewTextFile(
                Path.Combine(reportFolder, "report.md"),
                PerformanceReportFormatter.BuildMarkdown(context, snapshot, summary, assessment, loadedPlugins));
            WriteNewTextFile(
                Path.Combine(reportFolder, "frame_samples.csv"),
                PerformanceReportFormatter.BuildFrameSamplesCsv(snapshot));
            WriteNewTextFile(
                Path.Combine(reportFolder, "loaded_plugins.csv"),
                PerformanceReportFormatter.BuildLoadedPluginsCsv(loadedPlugins));

            _logger.LogWarning(
                $"Tainted Performance wrote {snapshot.TriggerKind} report {reportId}; samples={snapshot.Samples.Length}; average={snapshot.AverageFramesPerSecond:0.###} FPS; symptom={assessment.SymptomId}; folder={reportFolder}");
            _lastReportId = reportId;
            _lastReportFolder = reportFolder;
            _lastReportTriggerKind = snapshot.TriggerKind;
            _lastReportStatus = "Report written.";
            _lastReportUtc = generatedUtc;
            _cachedOverview = null;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Tainted Performance failed to write a {snapshot.TriggerKind} report after {ex.GetType().Name}. Automatic incident requests remain consumed by the detector.");
            _lastReportStatus = "Report write failed: " + ex.GetType().Name;
            _cachedOverview = null;
            return false;
        }
    }

    private PerformanceRuntimeOverview BuildOverview()
    {
        DateTime generatedUtc = DateTime.UtcNow;
        PerformanceFrameSample[] samples = _manualWindow.CopyChronologically();
        double threshold = _config.AutomaticIncidentsEnabled.Value ? _config.LowFpsThreshold.Value : 0d;
        double sustainedSeconds = _config.AutomaticIncidentsEnabled.Value ? _config.SustainedSeconds.Value : 0d;
        bool thresholdConfigured = LowPerformanceIncidentDetector.IsConfiguredThreshold(threshold, sustainedSeconds);
        PerformanceWindowSnapshot currentWindow = PerformanceWindowSnapshot.FromSamples(
            "overview",
            samples,
            0d,
            0d,
            0,
            Math.Max(1, _config.MaxReportsPerSession.Value));
        PerformanceWindowSummary summary = PerformanceWindowSummary.Create(
            currentWindow,
            _config.ReferenceFramesPerSecond.Value);
        bool lowFpsWarning = thresholdConfigured &&
                             summary.SampleCount > 0 &&
                             summary.WindowDurationSeconds + 0.000000001d >= sustainedSeconds &&
                             summary.AverageFramesPerSecond <= threshold + 0.000000001d;
        PerformanceWindowSnapshot assessmentWindow = PerformanceWindowSnapshot.FromSamples(
            lowFpsWarning ? "overview-low-fps" : "manual",
            samples,
            lowFpsWarning ? threshold : 0d,
            lowFpsWarning ? sustainedSeconds : 0d,
            0,
            Math.Max(1, _config.MaxReportsPerSession.Value));
        LoadedPluginContext[] loadedPlugins = CollectLoadedPlugins();
        int otherPluginCount = CountOtherPlugins(loadedPlugins);
        PerformanceInvestigationAssessment assessment = PerformanceInvestigationAnalyzer.Classify(
            assessmentWindow,
            summary,
            loadedPlugins.Length,
            otherPluginCount);
        PerformanceReportContext context = BuildContext("runtime-overview", generatedUtc);
        string statusLevel = BuildStatusLevel(lowFpsWarning, summary.SampleCount);
        string summaryText = BuildSummaryText(lowFpsWarning, thresholdConfigured, summary);
        string gameSettingsRecommendation = BuildGameSettingsRecommendation(lowFpsWarning, assessment);
        string modManagerRecommendation = BuildModManagerRecommendation(lowFpsWarning, otherPluginCount);

        return new PerformanceRuntimeOverview
        {
            UpdatedUtc = generatedUtc,
            Enabled = _config.Enabled.Value,
            AutomaticIncidentsEnabled = _config.AutomaticIncidentsEnabled.Value,
            AutomaticThresholdConfigured = thresholdConfigured,
            CurrentLowFpsWarning = lowFpsWarning,
            StatusLevel = statusLevel,
            Summary = summaryText,
            Detail = assessment.Evidence,
            SampleCount = summary.SampleCount,
            WindowDurationSeconds = summary.WindowDurationSeconds,
            AverageFramesPerSecond = summary.AverageFramesPerSecond,
            AverageFrameMilliseconds = summary.AverageFrameMilliseconds,
            MedianFrameMilliseconds = summary.MedianFrameMilliseconds,
            P95FrameMilliseconds = summary.P95FrameMilliseconds,
            P99FrameMilliseconds = summary.P99FrameMilliseconds,
            MaximumFrameMilliseconds = summary.MaximumFrameMilliseconds,
            OnePercentLowFramesPerSecond = summary.OnePercentLowFramesPerSecond,
            ReferenceFramesPerSecond = summary.ReferenceFramesPerSecond,
            ReferenceFrameMilliseconds = summary.ReferenceFrameMilliseconds,
            ReferenceBudgetMissCount = summary.ReferenceBudgetMissCount,
            ReferenceBudgetMissPercentage = summary.ReferenceBudgetMissPercentage,
            StutterCount = summary.StutterCount,
            StutterPercentage = summary.StutterPercentage,
            SevereStutterCount = summary.SevereStutterCount,
            FrameTimelineMilliseconds = BuildFrameTimeline(samples, 180),
            CpuTimingSampleCount = summary.CpuTimingSampleCount,
            AverageCpuFrameMilliseconds = summary.AverageCpuFrameMilliseconds,
            MaximumCpuFrameMilliseconds = summary.MaximumCpuFrameMilliseconds,
            GpuTimingSampleCount = summary.GpuTimingSampleCount,
            AverageGpuFrameMilliseconds = summary.AverageGpuFrameMilliseconds,
            MaximumGpuFrameMilliseconds = summary.MaximumGpuFrameMilliseconds,
            AllocationSampleCount = summary.AllocationSampleCount,
            TotalManagedMainThreadAllocationBytes = summary.TotalManagedMainThreadAllocationBytes,
            MaximumManagedMainThreadAllocationBytes = summary.MaximumManagedMainThreadAllocationBytes,
            ManagedMainThreadAllocationBytesPerSecond = summary.ManagedMainThreadAllocationBytesPerSecond,
            Gen0Collections = summary.Gen0Collections,
            Gen1Collections = summary.Gen1Collections,
            Gen2Collections = summary.Gen2Collections,
            ThresholdFramesPerSecond = threshold,
            SustainedSeconds = sustainedSeconds,
            AssessmentConfidence = assessment.Confidence,
            SymptomId = assessment.SymptomId,
            NativeInvestigation = assessment.NativeInvestigation,
            ModStackInvestigation = assessment.ModStackInvestigation,
            BestNextCheck = assessment.BestNextCheck,
            LoadedPluginCount = loadedPlugins.Length,
            OtherLoadedPluginCount = otherPluginCount,
            SceneName = context.SceneName,
            GameVersion = context.GameVersion,
            UnityVersion = context.UnityVersion,
            ScreenDescription = context.ScreenDescription,
            QualityLevel = context.QualityLevel,
            VSyncCount = context.VSyncCount,
            TargetFrameRate = context.TargetFrameRate,
            FrameTimingStatus = context.FrameTimingStatus,
            AllocationCounterStatus = context.AllocationCounterStatus,
            LastReportId = _lastReportId,
            LastReportFolder = _lastReportFolder,
            LastReportTriggerKind = _lastReportTriggerKind,
            LastReportStatus = _lastReportStatus,
            LastReportUtc = _lastReportUtc,
            GameSettingsRecommendation = gameSettingsRecommendation,
            ModManagerRecommendation = modManagerRecommendation,
            ManagerLines = BuildManagerLines(summary, thresholdConfigured, loadedPlugins.Length, otherPluginCount, gameSettingsRecommendation, modManagerRecommendation)
        };
    }

    private static double[] BuildFrameTimeline(PerformanceFrameSample[] samples, int maximumPoints)
    {
        if (samples.Length == 0 || maximumPoints < 1)
        {
            return Array.Empty<double>();
        }

        int stride = Math.Max(1, (int)Math.Ceiling(samples.Length / (double)maximumPoints));
        int pointCount = (samples.Length + stride - 1) / stride;
        double[] timeline = new double[pointCount];
        int point = 0;
        for (int start = 0; start < samples.Length; start += stride)
        {
            double maximum = 0d;
            int end = Math.Min(samples.Length, start + stride);
            for (int i = start; i < end; i++)
            {
                maximum = Math.Max(maximum, samples[i].FrameDurationSeconds * 1000d);
            }

            timeline[point++] = maximum;
        }

        return timeline;
    }

    private string BuildStatusLevel(bool lowFpsWarning, int sampleCount)
    {
        if (!_config.Enabled.Value)
        {
            return "Info";
        }

        if (lowFpsWarning)
        {
            return "Warning";
        }

        return sampleCount > 0 ? "Ok" : "Info";
    }

    private string BuildSummaryText(bool lowFpsWarning, bool thresholdConfigured, PerformanceWindowSummary summary)
    {
        if (!_config.Enabled.Value)
        {
            return "Tainted Performance is disabled.";
        }

        if (summary.SampleCount == 0)
        {
            return "Collecting the first performance samples.";
        }

        if (lowFpsWarning)
        {
            return "Configured low-FPS warning is active in the current sample window.";
        }

        return thresholdConfigured
            ? "Current sample window does not cross the configured low-FPS warning."
            : "Manual overview only; configure a threshold and duration to enable warnings.";
    }

    private static string BuildGameSettingsRecommendation(bool lowFpsWarning, PerformanceInvestigationAssessment assessment)
    {
        if (!lowFpsWarning)
        {
            return "No current low-FPS warning. Use the overview after reproducing the issue.";
        }

        if (string.Equals(assessment.SymptomId, "gpu-frame-pressure", StringComparison.OrdinalIgnoreCase))
        {
            return "GPU-side pressure is indicated by counters. Lower resolution scale, shadows, view distance, effects, or post-processing, then compare another report.";
        }

        if (string.Equals(assessment.SymptomId, "mixed-cpu-gpu-frame-pressure", StringComparison.OrdinalIgnoreCase))
        {
            return "Both CPU and GPU counters are over budget. Lower graphics settings first, then compare against a Tainted-Performance-only mod baseline.";
        }

        return "If the warning reproduces with only Tainted Performance loaded, adjust FoA graphics/frame-rate settings and capture a native-capable profiler run.";
    }

    private static string BuildModManagerRecommendation(bool lowFpsWarning, int otherPluginCount)
    {
        if (otherPluginCount <= 0)
        {
            return "No other BepInEx plugin is currently counted in the loaded-plugin inventory.";
        }

        if (lowFpsWarning)
        {
            return "Open FoA Mod Manager and disable or tune loaded mods one at a time, then compare this overview against a fresh report.";
        }

        return "FoA Mod Manager can adjust loaded mod settings. Plugin inventory is context only until a controlled comparison reproduces the issue.";
    }

    private string[] BuildManagerLines(
        PerformanceWindowSummary summary,
        bool thresholdConfigured,
        int loadedPluginCount,
        int otherPluginCount,
        string gameSettingsRecommendation,
        string modManagerRecommendation)
    {
        return new[]
        {
            FormatLine("Average FPS", FormatNumber(summary.AverageFramesPerSecond) + " over " + FormatNumber(summary.WindowDurationSeconds) + "s"),
            FormatLine("1% low FPS", FormatNumber(summary.OnePercentLowFramesPerSecond)),
            FormatLine("Frame p95/max", FormatNumber(summary.P95FrameMilliseconds) + " ms / " + FormatNumber(summary.MaximumFrameMilliseconds) + " ms"),
            FormatLine("Reference budget", FormatNumber(summary.ReferenceFramesPerSecond) + " FPS; " + summary.ReferenceBudgetMissCount + " misses (" + FormatNumber(summary.ReferenceBudgetMissPercentage) + "%)"),
            FormatLine("Stutters", summary.StutterCount + " >=50 ms; " + summary.SevereStutterCount + " >=100 ms"),
            FormatLine("CPU/GPU avg", FormatOptionalMilliseconds(summary.AverageCpuFrameMilliseconds) + " / " + FormatOptionalMilliseconds(summary.AverageGpuFrameMilliseconds)),
            FormatLine("Main-thread allocation", FormatNumber(summary.ManagedMainThreadAllocationBytesPerSecond) + " B/s"),
            FormatLine("GC collections", summary.Gen0Collections + "/" + summary.Gen1Collections + "/" + summary.Gen2Collections),
            FormatLine("Loaded plugins", loadedPluginCount + " total, " + otherPluginCount + " other"),
            FormatLine("Warning threshold", thresholdConfigured ? FormatNumber(_config.LowFpsThreshold.Value) + " FPS for " + FormatNumber(_config.SustainedSeconds.Value) + "s" : "not configured"),
            FormatLine("Game settings", gameSettingsRecommendation),
            FormatLine("Mod settings", modManagerRecommendation)
        };
    }

    private static string FormatLine(string name, string value)
    {
        return name + ": " + value;
    }

    internal static string FormatNumber(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return "0";
        }

        double normalized = Math.Abs(value) < 0.0000000005d ? 0d : value;
        return normalized.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
    }

    internal static string FormatOptionalMilliseconds(double value)
    {
        return value > 0d && !double.IsNaN(value) && !double.IsInfinity(value)
            ? FormatNumber(value) + " ms"
            : "unavailable";
    }

    private PerformanceReportContext BuildContext(string reportId, DateTime generatedUtc)
    {
        Scene scene = SceneManager.GetActiveScene();
        return new PerformanceReportContext
        {
            ReportId = reportId,
            GeneratedUtc = generatedUtc,
            MonitorPluginGuid = Plugin.PluginGuid,
            MonitorPluginName = Plugin.PluginName,
            MonitorPluginVersion = Plugin.PluginVersion,
            SceneName = scene.IsValid() ? scene.name ?? string.Empty : "unavailable",
            GameVersion = Application.version,
            UnityVersion = Application.unityVersion,
            BepInExVersion = RuntimeCompatibility.BepInExVersion,
            BranchAssumption = RuntimeCompatibility.BranchDescription,
            OperatingSystem = SystemInfo.operatingSystem,
            ProcessorType = SystemInfo.processorType,
            ProcessorCount = SystemInfo.processorCount,
            SystemMemoryMegabytes = SystemInfo.systemMemorySize,
            GraphicsDeviceName = SystemInfo.graphicsDeviceName,
            GraphicsDeviceType = SystemInfo.graphicsDeviceType.ToString() ?? "unknown",
            GraphicsMemoryMegabytes = SystemInfo.graphicsMemorySize,
            ScreenDescription = string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{0}x{1} {2}",
                Screen.width,
                Screen.height,
                Screen.fullScreenMode),
            QualityLevel = ReadQualityLevel(),
            VSyncCount = QualitySettings.vSyncCount,
            TargetFrameRate = Application.targetFrameRate,
            FrameTimingStatus = !_frameTimingEnabled
                ? "unavailable"
                : _frameTimingReturnedData ? "available" : "no-valid-samples-yet",
            AllocationCounterStatus = _allocationCounterEnabled ? "available" : "unavailable"
        };
    }

    private static LoadedPluginContext[] CollectLoadedPlugins()
    {
        List<LoadedPluginContext> plugins = new List<LoadedPluginContext>();
        try
        {
            foreach (var pluginInfo in GetLoadedPluginInfos())
            {
                if (pluginInfo.Metadata == null)
                {
                    plugins.Add(new LoadedPluginContext("unknown", "unknown", "unknown"));
                    continue;
                }

                plugins.Add(new LoadedPluginContext(
                    pluginInfo.Metadata.GUID ?? "unknown",
                    pluginInfo.Metadata.Name ?? "unknown",
                    pluginInfo.Metadata.Version?.ToString() ?? "unknown"));
            }
        }
        catch
        {
            plugins.Add(new LoadedPluginContext(
                Plugin.PluginGuid,
                Plugin.PluginName,
                Plugin.PluginVersion));
        }

        plugins.Sort((left, right) => string.Compare(left.Guid, right.Guid, StringComparison.OrdinalIgnoreCase));
        return plugins.ToArray();
    }

#if IL2CPP
    private static IEnumerable<BepInEx.PluginInfo> GetLoadedPluginInfos()
#else
    private static IEnumerable<PluginInfo> GetLoadedPluginInfos()
#endif
    {
#if IL2CPP
        var plugins = IL2CPPChainloader.Instance?.Plugins;
        return plugins == null
            ? Array.Empty<BepInEx.PluginInfo>()
            : plugins.Values;
#else
        return Chainloader.PluginInfos.Values;
#endif
    }

    private static int CountOtherPlugins(LoadedPluginContext[] loadedPlugins)
    {
        int count = 0;
        for (int i = 0; i < loadedPlugins.Length; i++)
        {
            if (!string.Equals(loadedPlugins[i].Guid, Plugin.PluginGuid, StringComparison.OrdinalIgnoreCase))
            {
                count++;
            }
        }

        return count;
    }

    private static string ReadQualityLevel()
    {
        try
        {
            int qualityLevel = QualitySettings.GetQualityLevel();
            var qualityNames = QualitySettings.names;
            if (qualityLevel >= 0 && qualityLevel < qualityNames.Length)
            {
                return qualityNames[qualityLevel];
            }

            return qualityLevel.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        catch
        {
            return "unavailable";
        }
    }

    private static void WriteNewTextFile(string path, string content)
    {
        using FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
        using StreamWriter writer = new StreamWriter(stream, Utf8WithoutBom);
        writer.Write(content);
    }

    private static double NormalizeTiming(double milliseconds)
    {
        return milliseconds > 0d && !double.IsNaN(milliseconds) && !double.IsInfinity(milliseconds)
            ? milliseconds
            : -1d;
    }
}
