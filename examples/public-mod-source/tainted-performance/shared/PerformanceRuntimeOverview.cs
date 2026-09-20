using System;

namespace TaintedPerformance;

internal sealed class PerformanceRuntimeOverview
{
    public DateTime UpdatedUtc { get; set; }
    public bool Enabled { get; set; }
    public bool AutomaticIncidentsEnabled { get; set; }
    public bool AutomaticThresholdConfigured { get; set; }
    public bool CurrentLowFpsWarning { get; set; }
    public string StatusLevel { get; set; } = "Info";
    public string Summary { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public int SampleCount { get; set; }
    public double WindowDurationSeconds { get; set; }
    public double AverageFramesPerSecond { get; set; }
    public double AverageFrameMilliseconds { get; set; }
    public double MedianFrameMilliseconds { get; set; }
    public double P95FrameMilliseconds { get; set; }
    public double P99FrameMilliseconds { get; set; }
    public double MaximumFrameMilliseconds { get; set; }
    public double OnePercentLowFramesPerSecond { get; set; }
    public double ReferenceFramesPerSecond { get; set; }
    public double ReferenceFrameMilliseconds { get; set; }
    public int ReferenceBudgetMissCount { get; set; }
    public double ReferenceBudgetMissPercentage { get; set; }
    public int StutterCount { get; set; }
    public double StutterPercentage { get; set; }
    public int SevereStutterCount { get; set; }
    public double[] FrameTimelineMilliseconds { get; set; } = Array.Empty<double>();
    public int CpuTimingSampleCount { get; set; }
    public double AverageCpuFrameMilliseconds { get; set; }
    public double MaximumCpuFrameMilliseconds { get; set; }
    public int GpuTimingSampleCount { get; set; }
    public double AverageGpuFrameMilliseconds { get; set; }
    public double MaximumGpuFrameMilliseconds { get; set; }
    public int AllocationSampleCount { get; set; }
    public long TotalManagedMainThreadAllocationBytes { get; set; }
    public long MaximumManagedMainThreadAllocationBytes { get; set; }
    public double ManagedMainThreadAllocationBytesPerSecond { get; set; }
    public int Gen0Collections { get; set; }
    public int Gen1Collections { get; set; }
    public int Gen2Collections { get; set; }
    public double ThresholdFramesPerSecond { get; set; }
    public double SustainedSeconds { get; set; }
    public string AssessmentConfidence { get; set; } = string.Empty;
    public string SymptomId { get; set; } = string.Empty;
    public string NativeInvestigation { get; set; } = string.Empty;
    public string ModStackInvestigation { get; set; } = string.Empty;
    public string BestNextCheck { get; set; } = string.Empty;
    public int LoadedPluginCount { get; set; }
    public int OtherLoadedPluginCount { get; set; }
    public string SceneName { get; set; } = string.Empty;
    public string GameVersion { get; set; } = string.Empty;
    public string UnityVersion { get; set; } = string.Empty;
    public string ScreenDescription { get; set; } = string.Empty;
    public string QualityLevel { get; set; } = string.Empty;
    public int VSyncCount { get; set; }
    public int TargetFrameRate { get; set; }
    public string FrameTimingStatus { get; set; } = string.Empty;
    public string AllocationCounterStatus { get; set; } = string.Empty;
    public string LastReportId { get; set; } = string.Empty;
    public string LastReportFolder { get; set; } = string.Empty;
    public string LastReportTriggerKind { get; set; } = string.Empty;
    public string LastReportStatus { get; set; } = string.Empty;
    public DateTime LastReportUtc { get; set; }
    public string GameSettingsRecommendation { get; set; } = string.Empty;
    public string ModManagerRecommendation { get; set; } = string.Empty;
    public string[] ManagerLines { get; set; } = Array.Empty<string>();
}
