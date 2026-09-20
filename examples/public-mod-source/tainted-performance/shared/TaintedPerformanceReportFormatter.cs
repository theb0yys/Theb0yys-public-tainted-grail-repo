using System;
using System.Globalization;
using System.Text;

namespace TaintedPerformance;

internal sealed class PerformanceReportContext
{
    public string ReportId { get; set; } = string.Empty;
    public DateTime GeneratedUtc { get; set; }
    public string MonitorPluginGuid { get; set; } = string.Empty;
    public string MonitorPluginName { get; set; } = string.Empty;
    public string MonitorPluginVersion { get; set; } = string.Empty;
    public string SceneName { get; set; } = string.Empty;
    public string GameVersion { get; set; } = string.Empty;
    public string UnityVersion { get; set; } = string.Empty;
    public string BepInExVersion { get; set; } = string.Empty;
    public string BranchAssumption { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
    public string ProcessorType { get; set; } = string.Empty;
    public int ProcessorCount { get; set; }
    public int SystemMemoryMegabytes { get; set; }
    public string GraphicsDeviceName { get; set; } = string.Empty;
    public string GraphicsDeviceType { get; set; } = string.Empty;
    public int GraphicsMemoryMegabytes { get; set; }
    public string ScreenDescription { get; set; } = string.Empty;
    public string QualityLevel { get; set; } = string.Empty;
    public int VSyncCount { get; set; }
    public int TargetFrameRate { get; set; }
    public string FrameTimingStatus { get; set; } = string.Empty;
    public string AllocationCounterStatus { get; set; } = string.Empty;
}

internal sealed class LoadedPluginContext
{
    public LoadedPluginContext(string guid, string name, string version)
    {
        Guid = guid;
        Name = name;
        Version = version;
    }

    public string Guid { get; }
    public string Name { get; }
    public string Version { get; }
}

internal static class PerformanceReportFormatter
{
    private const int MaximumMetadataLength = 512;

    public static string BuildJson(
        PerformanceReportContext context,
        PerformanceWindowSnapshot snapshot,
        PerformanceWindowSummary summary,
        PerformanceInvestigationAssessment assessment,
        LoadedPluginContext[] loadedPlugins)
    {
        ValidateArguments(context, snapshot, summary, assessment, loadedPlugins);

        StringBuilder builder = new StringBuilder(16384);
        builder.Append("{\n");
        builder.Append("  \"schema_version\": 2,\n");
        AppendStringProperty(builder, "  ", "report_id", context.ReportId, true);
        AppendStringProperty(builder, "  ", "generated_utc", context.GeneratedUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture), true);

        builder.Append("  \"monitor\": {\n");
        AppendStringProperty(builder, "    ", "plugin_guid", context.MonitorPluginGuid, true);
        AppendStringProperty(builder, "    ", "plugin_name", context.MonitorPluginName, true);
        AppendStringProperty(builder, "    ", "plugin_version", context.MonitorPluginVersion, true);
        builder.Append("    \"standalone_mod\": true,\n");
        builder.Append("    \"loaded_plugin_inventory_is_causal_evidence\": false\n");
        builder.Append("  },\n");

        builder.Append("  \"trigger\": {\n");
        AppendStringProperty(builder, "    ", "kind", snapshot.TriggerKind, true);
        AppendOptionalNumberProperty(builder, "    ", "threshold_fps", snapshot.ThresholdFramesPerSecond, true);
        AppendOptionalNumberProperty(builder, "    ", "sustained_seconds", snapshot.SustainedSeconds, true);
        AppendNumberProperty(builder, "    ", "observed_window_seconds", snapshot.WindowDurationSeconds, true);
        AppendNumberProperty(builder, "    ", "observed_average_fps", snapshot.AverageFramesPerSecond, true);
        builder.Append("    \"frame_count\": ").Append(snapshot.Samples.Length.ToString(CultureInfo.InvariantCulture)).Append(",\n");
        builder.Append("    \"report_index\": ").Append(snapshot.ReportIndex.ToString(CultureInfo.InvariantCulture)).Append(",\n");
        builder.Append("    \"max_reports_per_session\": ").Append(snapshot.MaxReportsPerSession.ToString(CultureInfo.InvariantCulture)).Append("\n");
        builder.Append("  },\n");

        builder.Append("  \"assessment\": {\n");
        AppendStringProperty(builder, "    ", "confidence", assessment.Confidence, true);
        AppendStringProperty(builder, "    ", "symptom_id", assessment.SymptomId, true);
        AppendStringProperty(builder, "    ", "evidence", assessment.Evidence, true);
        AppendStringProperty(builder, "    ", "does_not_prove", assessment.DoesNotProve, true);
        builder.Append("    \"causality_proven\": false,\n");
        AppendStringProperty(builder, "    ", "native_investigation", assessment.NativeInvestigation, true);
        AppendStringProperty(builder, "    ", "mod_stack_investigation", assessment.ModStackInvestigation, true);
        AppendStringProperty(builder, "    ", "best_next_check", assessment.BestNextCheck, false);
        builder.Append("  },\n");

        builder.Append("  \"telemetry\": {\n");
        builder.Append("    \"sample_count\": ").Append(summary.SampleCount.ToString(CultureInfo.InvariantCulture)).Append(",\n");
        AppendNumberProperty(builder, "    ", "average_frame_ms", summary.AverageFrameMilliseconds, true);
        AppendNumberProperty(builder, "    ", "median_frame_ms", summary.MedianFrameMilliseconds, true);
        AppendNumberProperty(builder, "    ", "p95_frame_ms", summary.P95FrameMilliseconds, true);
        AppendNumberProperty(builder, "    ", "p99_frame_ms", summary.P99FrameMilliseconds, true);
        AppendNumberProperty(builder, "    ", "maximum_frame_ms", summary.MaximumFrameMilliseconds, true);
        AppendNumberProperty(builder, "    ", "one_percent_low_fps", summary.OnePercentLowFramesPerSecond, true);
        AppendNumberProperty(builder, "    ", "reference_fps", summary.ReferenceFramesPerSecond, true);
        AppendNumberProperty(builder, "    ", "reference_frame_ms", summary.ReferenceFrameMilliseconds, true);
        builder.Append("    \"reference_budget_miss_count\": ").Append(summary.ReferenceBudgetMissCount.ToString(CultureInfo.InvariantCulture)).Append(",\n");
        AppendNumberProperty(builder, "    ", "reference_budget_miss_percent", summary.ReferenceBudgetMissPercentage, true);
        builder.Append("    \"stutter_50ms_count\": ").Append(summary.StutterCount.ToString(CultureInfo.InvariantCulture)).Append(",\n");
        AppendNumberProperty(builder, "    ", "stutter_50ms_percent", summary.StutterPercentage, true);
        builder.Append("    \"severe_stutter_100ms_count\": ").Append(summary.SevereStutterCount.ToString(CultureInfo.InvariantCulture)).Append(",\n");
        builder.Append("    \"cpu_timing_sample_count\": ").Append(summary.CpuTimingSampleCount.ToString(CultureInfo.InvariantCulture)).Append(",\n");
        AppendOptionalNumberProperty(builder, "    ", "average_cpu_frame_ms", summary.AverageCpuFrameMilliseconds, true);
        AppendOptionalNumberProperty(builder, "    ", "maximum_cpu_frame_ms", summary.CpuTimingSampleCount > 0 ? summary.MaximumCpuFrameMilliseconds : -1d, true);
        builder.Append("    \"gpu_timing_sample_count\": ").Append(summary.GpuTimingSampleCount.ToString(CultureInfo.InvariantCulture)).Append(",\n");
        AppendOptionalNumberProperty(builder, "    ", "average_gpu_frame_ms", summary.AverageGpuFrameMilliseconds, true);
        AppendOptionalNumberProperty(builder, "    ", "maximum_gpu_frame_ms", summary.GpuTimingSampleCount > 0 ? summary.MaximumGpuFrameMilliseconds : -1d, true);
        builder.Append("    \"allocation_sample_count\": ").Append(summary.AllocationSampleCount.ToString(CultureInfo.InvariantCulture)).Append(",\n");
        builder.Append("    \"managed_main_thread_allocation_total_bytes\": ").Append(summary.TotalManagedMainThreadAllocationBytes.ToString(CultureInfo.InvariantCulture)).Append(",\n");
        builder.Append("    \"managed_main_thread_allocation_maximum_frame_bytes\": ").Append(summary.MaximumManagedMainThreadAllocationBytes.ToString(CultureInfo.InvariantCulture)).Append(",\n");
        AppendNumberProperty(builder, "    ", "managed_main_thread_allocation_bytes_per_second", summary.ManagedMainThreadAllocationBytesPerSecond, true);
        builder.Append("    \"gc_gen0_collections\": ").Append(summary.Gen0Collections.ToString(CultureInfo.InvariantCulture)).Append(",\n");
        builder.Append("    \"gc_gen1_collections\": ").Append(summary.Gen1Collections.ToString(CultureInfo.InvariantCulture)).Append(",\n");
        builder.Append("    \"gc_gen2_collections\": ").Append(summary.Gen2Collections.ToString(CultureInfo.InvariantCulture)).Append("\n");
        builder.Append("  },\n");

        builder.Append("  \"environment\": {\n");
        AppendStringProperty(builder, "    ", "scene", context.SceneName, true);
        AppendStringProperty(builder, "    ", "game_version", context.GameVersion, true);
        AppendStringProperty(builder, "    ", "unity_version", context.UnityVersion, true);
        AppendStringProperty(builder, "    ", "bepinex_version", context.BepInExVersion, true);
        AppendStringProperty(builder, "    ", "branch_assumption", context.BranchAssumption, true);
        AppendStringProperty(builder, "    ", "operating_system", context.OperatingSystem, true);
        AppendStringProperty(builder, "    ", "processor", context.ProcessorType, true);
        builder.Append("    \"processor_count\": ").Append(context.ProcessorCount.ToString(CultureInfo.InvariantCulture)).Append(",\n");
        builder.Append("    \"system_memory_mb\": ").Append(context.SystemMemoryMegabytes.ToString(CultureInfo.InvariantCulture)).Append(",\n");
        AppendStringProperty(builder, "    ", "graphics_device", context.GraphicsDeviceName, true);
        AppendStringProperty(builder, "    ", "graphics_api", context.GraphicsDeviceType, true);
        builder.Append("    \"graphics_memory_mb\": ").Append(context.GraphicsMemoryMegabytes.ToString(CultureInfo.InvariantCulture)).Append(",\n");
        AppendStringProperty(builder, "    ", "screen", context.ScreenDescription, true);
        AppendStringProperty(builder, "    ", "quality_level", context.QualityLevel, true);
        builder.Append("    \"vsync_count\": ").Append(context.VSyncCount.ToString(CultureInfo.InvariantCulture)).Append(",\n");
        builder.Append("    \"target_frame_rate\": ").Append(context.TargetFrameRate.ToString(CultureInfo.InvariantCulture)).Append(",\n");
        AppendStringProperty(builder, "    ", "frame_timing_status", context.FrameTimingStatus, true);
        AppendStringProperty(builder, "    ", "allocation_counter_status", context.AllocationCounterStatus, false);
        builder.Append("  },\n");

        builder.Append("  \"loaded_plugins\": [\n");
        for (int i = 0; i < loadedPlugins.Length; i++)
        {
            LoadedPluginContext plugin = loadedPlugins[i];
            builder.Append("    {\n");
            AppendStringProperty(builder, "      ", "guid", plugin.Guid, true);
            AppendStringProperty(builder, "      ", "name", plugin.Name, true);
            AppendStringProperty(builder, "      ", "version", plugin.Version, true);
            builder.Append("      \"causal_evidence\": false\n");
            builder.Append("    }");
            builder.Append(i + 1 < loadedPlugins.Length ? ",\n" : "\n");
        }

        builder.Append("  ],\n");
        builder.Append("  \"frame_samples\": [\n");
        double remainingSeconds = snapshot.WindowDurationSeconds;
        for (int i = 0; i < snapshot.Samples.Length; i++)
        {
            PerformanceFrameSample sample = snapshot.Samples[i];
            remainingSeconds = Math.Max(0d, remainingSeconds - sample.FrameDurationSeconds);
            builder.Append("    {\n");
            builder.Append("      \"sample_index\": ").Append(i.ToString(CultureInfo.InvariantCulture)).Append(",\n");
            builder.Append("      \"frame_number\": ").Append(sample.FrameNumber.ToString(CultureInfo.InvariantCulture)).Append(",\n");
            AppendNumberProperty(builder, "      ", "seconds_before_report", -remainingSeconds, true);
            AppendNumberProperty(builder, "      ", "frame_ms", sample.FrameDurationSeconds * 1000d, true);
            AppendOptionalNumberProperty(builder, "      ", "cpu_frame_ms", sample.CpuFrameMilliseconds, true);
            AppendOptionalNumberProperty(builder, "      ", "gpu_frame_ms", sample.GpuFrameMilliseconds, true);
            AppendOptionalLongProperty(builder, "      ", "managed_main_thread_allocation_bytes", sample.ManagedMainThreadAllocationBytes, true);
            builder.Append("      \"gc_gen0_collections\": ").Append(Math.Max(0, sample.Gen0Collections).ToString(CultureInfo.InvariantCulture)).Append(",\n");
            builder.Append("      \"gc_gen1_collections\": ").Append(Math.Max(0, sample.Gen1Collections).ToString(CultureInfo.InvariantCulture)).Append(",\n");
            builder.Append("      \"gc_gen2_collections\": ").Append(Math.Max(0, sample.Gen2Collections).ToString(CultureInfo.InvariantCulture)).Append("\n");
            builder.Append("    }");
            builder.Append(i + 1 < snapshot.Samples.Length ? ",\n" : "\n");
        }

        builder.Append("  ]\n");
        builder.Append("}\n");
        return builder.ToString();
    }

    public static string BuildMarkdown(
        PerformanceReportContext context,
        PerformanceWindowSnapshot snapshot,
        PerformanceWindowSummary summary,
        PerformanceInvestigationAssessment assessment,
        LoadedPluginContext[] loadedPlugins)
    {
        ValidateArguments(context, snapshot, summary, assessment, loadedPlugins);

        StringBuilder builder = new StringBuilder(4096);
        builder.Append("# Tainted Performance report\n\n");
        builder.Append("Report: `").Append(MarkdownText(context.ReportId)).Append("`  \n");
        builder.Append("UTC: `").Append(context.GeneratedUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)).Append("`  \n");
        builder.Append("Scene: `").Append(MarkdownText(context.SceneName)).Append("`  \n");
        builder.Append("Trigger: `").Append(MarkdownText(snapshot.TriggerKind)).Append("`\n\n");

        builder.Append("## Assessment\n\n");
        builder.Append("- Confidence: `").Append(MarkdownText(assessment.Confidence)).Append("`\n");
        builder.Append("- Symptom ID: `").Append(MarkdownText(assessment.SymptomId)).Append("`\n");
        builder.Append("- Evidence: ").Append(MarkdownText(assessment.Evidence)).Append("\n");
        builder.Append("- Does not prove: ").Append(MarkdownText(assessment.DoesNotProve)).Append("\n");
        builder.Append("- Native investigation: ").Append(MarkdownText(assessment.NativeInvestigation)).Append("\n");
        builder.Append("- Mod-stack investigation: ").Append(MarkdownText(assessment.ModStackInvestigation)).Append("\n");
        builder.Append("- Best next check: ").Append(MarkdownText(assessment.BestNextCheck)).Append("\n\n");

        builder.Append("## Window\n\n");
        builder.Append("| Metric | Value |\n| --- | ---: |\n");
        AppendMarkdownMetric(builder, "Threshold", OptionalNumber(snapshot.ThresholdFramesPerSecond, " FPS"));
        AppendMarkdownMetric(builder, "Required duration", OptionalNumber(snapshot.SustainedSeconds, " s"));
        AppendMarkdownMetric(builder, "Observed duration", FormatNumber(snapshot.WindowDurationSeconds) + " s");
        AppendMarkdownMetric(builder, "Observed average", FormatNumber(snapshot.AverageFramesPerSecond) + " FPS");
        AppendMarkdownMetric(builder, "Frames", snapshot.Samples.Length.ToString(CultureInfo.InvariantCulture));
        AppendMarkdownMetric(builder, "1% low", FormatNumber(summary.OnePercentLowFramesPerSecond) + " FPS");
        AppendMarkdownMetric(builder, "Frame median", FormatNumber(summary.MedianFrameMilliseconds) + " ms");
        AppendMarkdownMetric(builder, "Frame p95", FormatNumber(summary.P95FrameMilliseconds) + " ms");
        AppendMarkdownMetric(builder, "Frame p99", FormatNumber(summary.P99FrameMilliseconds) + " ms");
        AppendMarkdownMetric(builder, "Maximum frame", FormatNumber(summary.MaximumFrameMilliseconds) + " ms");
        AppendMarkdownMetric(builder, "Reference budget", FormatNumber(summary.ReferenceFramesPerSecond) + " FPS / " + FormatNumber(summary.ReferenceFrameMilliseconds) + " ms");
        AppendMarkdownMetric(builder, "Budget misses", summary.ReferenceBudgetMissCount.ToString(CultureInfo.InvariantCulture) + " (" + FormatNumber(summary.ReferenceBudgetMissPercentage) + "%)");
        AppendMarkdownMetric(builder, "Stutters >=50 / >=100 ms", summary.StutterCount.ToString(CultureInfo.InvariantCulture) + " / " + summary.SevereStutterCount.ToString(CultureInfo.InvariantCulture));
        AppendMarkdownMetric(builder, "CPU timing average", OptionalNumber(summary.AverageCpuFrameMilliseconds, " ms"));
        AppendMarkdownMetric(builder, "GPU timing average", OptionalNumber(summary.AverageGpuFrameMilliseconds, " ms"));
        AppendMarkdownMetric(builder, "Main-thread allocation", summary.TotalManagedMainThreadAllocationBytes.ToString(CultureInfo.InvariantCulture) + " bytes");
        AppendMarkdownMetric(builder, "Main-thread allocation rate", FormatNumber(summary.ManagedMainThreadAllocationBytesPerSecond) + " bytes/s");
        AppendMarkdownMetric(builder, "GC collections (0/1/2)", string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}", summary.Gen0Collections, summary.Gen1Collections, summary.Gen2Collections));
        AppendMarkdownMetric(builder, "Automatic report", snapshot.ReportIndex > 0 ? string.Format(CultureInfo.InvariantCulture, "{0} of {1}", snapshot.ReportIndex, snapshot.MaxReportsPerSession) : "not applicable");

        builder.Append("\n## Environment\n\n");
        builder.Append("- Game / Unity: `").Append(MarkdownText(context.GameVersion)).Append("` / `").Append(MarkdownText(context.UnityVersion)).Append("`\n");
        builder.Append("- BepInEx: `").Append(MarkdownText(context.BepInExVersion)).Append("`\n");
        builder.Append("- Branch: ").Append(MarkdownText(context.BranchAssumption)).Append("\n");
        builder.Append("- CPU: ").Append(MarkdownText(context.ProcessorType)).Append(" (").Append(context.ProcessorCount.ToString(CultureInfo.InvariantCulture)).Append(" logical processors)\n");
        builder.Append("- GPU: ").Append(MarkdownText(context.GraphicsDeviceName)).Append(" / ").Append(MarkdownText(context.GraphicsDeviceType)).Append("\n");
        builder.Append("- Display / quality: ").Append(MarkdownText(context.ScreenDescription)).Append(" / ").Append(MarkdownText(context.QualityLevel)).Append("\n");
        builder.Append("- Loaded plugins recorded: ").Append(loadedPlugins.Length.ToString(CultureInfo.InvariantCulture)).Append(" (context only, not causal evidence)\n\n");

        builder.Append("## Artifact boundary\n\n");
        builder.Append("This report is local evidence from Tainted Performance only. It proves the sampled counter window, not native cause, mod cause, save safety, compatibility, or release readiness.\n");
        return builder.ToString();
    }

    public static string BuildFrameSamplesCsv(PerformanceWindowSnapshot snapshot)
    {
        if (snapshot == null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

        StringBuilder builder = new StringBuilder(Math.Max(1024, snapshot.Samples.Length * 96));
        builder.Append("sample_index,frame_number,seconds_before_report,frame_ms,cpu_frame_ms,gpu_frame_ms,managed_main_thread_allocation_bytes,gc_gen0_collections,gc_gen1_collections,gc_gen2_collections\n");
        double remainingSeconds = snapshot.WindowDurationSeconds;
        for (int i = 0; i < snapshot.Samples.Length; i++)
        {
            PerformanceFrameSample sample = snapshot.Samples[i];
            remainingSeconds = Math.Max(0d, remainingSeconds - sample.FrameDurationSeconds);
            builder.Append(i.ToString(CultureInfo.InvariantCulture)).Append(',');
            builder.Append(sample.FrameNumber.ToString(CultureInfo.InvariantCulture)).Append(',');
            builder.Append(FormatNumber(-remainingSeconds)).Append(',');
            builder.Append(FormatNumber(sample.FrameDurationSeconds * 1000d)).Append(',');
            builder.Append(OptionalNumber(sample.CpuFrameMilliseconds, string.Empty)).Append(',');
            builder.Append(OptionalNumber(sample.GpuFrameMilliseconds, string.Empty)).Append(',');
            if (sample.ManagedMainThreadAllocationBytes >= 0L)
            {
                builder.Append(sample.ManagedMainThreadAllocationBytes.ToString(CultureInfo.InvariantCulture));
            }

            builder.Append(',').Append(Math.Max(0, sample.Gen0Collections).ToString(CultureInfo.InvariantCulture));
            builder.Append(',').Append(Math.Max(0, sample.Gen1Collections).ToString(CultureInfo.InvariantCulture));
            builder.Append(',').Append(Math.Max(0, sample.Gen2Collections).ToString(CultureInfo.InvariantCulture));
            builder.Append('\n');
        }

        return builder.ToString();
    }

    public static string BuildLoadedPluginsCsv(LoadedPluginContext[] loadedPlugins)
    {
        if (loadedPlugins == null)
        {
            throw new ArgumentNullException(nameof(loadedPlugins));
        }

        StringBuilder builder = new StringBuilder(Math.Max(256, loadedPlugins.Length * 64));
        builder.Append("guid,name,version,causal_evidence\n");
        for (int i = 0; i < loadedPlugins.Length; i++)
        {
            LoadedPluginContext plugin = loadedPlugins[i];
            builder.Append(CsvText(plugin.Guid)).Append(',');
            builder.Append(CsvText(plugin.Name)).Append(',');
            builder.Append(CsvText(plugin.Version)).Append(",false\n");
        }

        return builder.ToString();
    }

    private static void ValidateArguments(
        PerformanceReportContext context,
        PerformanceWindowSnapshot snapshot,
        PerformanceWindowSummary summary,
        PerformanceInvestigationAssessment assessment,
        LoadedPluginContext[] loadedPlugins)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (snapshot == null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

        if (summary == null)
        {
            throw new ArgumentNullException(nameof(summary));
        }

        if (assessment == null)
        {
            throw new ArgumentNullException(nameof(assessment));
        }

        if (loadedPlugins == null)
        {
            throw new ArgumentNullException(nameof(loadedPlugins));
        }
    }

    private static void AppendStringProperty(
        StringBuilder builder,
        string indent,
        string name,
        string? value,
        bool comma)
    {
        builder.Append(indent).Append('"').Append(name).Append("\": ");
        AppendJsonString(builder, SafeMetadata(value));
        builder.Append(comma ? ",\n" : "\n");
    }

    private static void AppendNumberProperty(
        StringBuilder builder,
        string indent,
        string name,
        double value,
        bool comma)
    {
        builder.Append(indent).Append('"').Append(name).Append("\": ").Append(FormatNumber(value));
        builder.Append(comma ? ",\n" : "\n");
    }

    private static void AppendOptionalNumberProperty(
        StringBuilder builder,
        string indent,
        string name,
        double value,
        bool comma)
    {
        builder.Append(indent).Append('"').Append(name).Append("\": ");
        builder.Append(IsFinitePositive(value) ? FormatNumber(value) : "null");
        builder.Append(comma ? ",\n" : "\n");
    }

    private static void AppendOptionalLongProperty(
        StringBuilder builder,
        string indent,
        string name,
        long value,
        bool comma)
    {
        builder.Append(indent).Append('"').Append(name).Append("\": ");
        builder.Append(value >= 0L ? value.ToString(CultureInfo.InvariantCulture) : "null");
        builder.Append(comma ? ",\n" : "\n");
    }

    private static void AppendJsonString(StringBuilder builder, string value)
    {
        builder.Append('"');
        for (int i = 0; i < value.Length; i++)
        {
            char character = value[i];
            switch (character)
            {
                case '"':
                    builder.Append("\\\"");
                    break;
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '\b':
                    builder.Append("\\b");
                    break;
                case '\f':
                    builder.Append("\\f");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                default:
                    if (character < 0x20)
                    {
                        builder.Append("\\u").Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        builder.Append(character);
                    }

                    break;
            }
        }

        builder.Append('"');
    }

    private static void AppendMarkdownMetric(StringBuilder builder, string name, string value)
    {
        builder.Append("| ").Append(MarkdownText(name)).Append(" | ").Append(MarkdownText(value)).Append(" |\n");
    }

    private static string CsvText(string? value)
    {
        string safe = SafeMetadata(value);
        if (safe.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
        {
            return safe;
        }

        return "\"" + safe.Replace("\"", "\"\"") + "\"";
    }

    private static string MarkdownText(string? value)
    {
        return SafeMetadata(value)
            .Replace("|", "\\|")
            .Replace("\r", " ")
            .Replace("\n", " ");
    }

    private static string OptionalNumber(double value, string suffix)
    {
        return IsFinitePositive(value) ? FormatNumber(value) + suffix : "unavailable";
    }

    private static string FormatNumber(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return "0";
        }

        double normalized = Math.Abs(value) < 0.0000000005d ? 0d : value;
        return normalized.ToString("0.#########", CultureInfo.InvariantCulture);
    }

    private static bool IsFinitePositive(double value)
    {
        return value > 0d && !double.IsNaN(value) && !double.IsInfinity(value);
    }

    private static string SafeMetadata(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        string bounded = value.Length <= MaximumMetadataLength
            ? value
            : value.Substring(0, MaximumMetadataLength) + "...";

        if (LooksLikeSensitiveMetadata(bounded))
        {
            return "<redacted-metadata>";
        }

        return bounded;
    }

    private static bool LooksLikeSensitiveMetadata(string value)
    {
        return value.IndexOf(":\\", StringComparison.Ordinal) >= 0 ||
               value.IndexOf(":/", StringComparison.Ordinal) >= 0 ||
               value.StartsWith("\\\\", StringComparison.Ordinal) ||
               value.StartsWith("/", StringComparison.Ordinal) ||
               value.IndexOf("/Users/", StringComparison.OrdinalIgnoreCase) >= 0 ||
               value.IndexOf("/home/", StringComparison.OrdinalIgnoreCase) >= 0 ||
               value.IndexOf('@') >= 0 ||
               value.IndexOf("password=", StringComparison.OrdinalIgnoreCase) >= 0 ||
               value.IndexOf("secret=", StringComparison.OrdinalIgnoreCase) >= 0 ||
               value.IndexOf("token=", StringComparison.OrdinalIgnoreCase) >= 0 ||
               value.IndexOf("api_key", StringComparison.OrdinalIgnoreCase) >= 0 ||
               value.IndexOf("apikey", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
