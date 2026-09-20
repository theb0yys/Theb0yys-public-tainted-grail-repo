using System;
using System.Globalization;

namespace TaintedPerformance;

internal readonly struct PerformanceFrameSample
{
    public PerformanceFrameSample(
        long frameNumber,
        double frameDurationSeconds,
        double cpuFrameMilliseconds,
        double gpuFrameMilliseconds,
        long managedMainThreadAllocationBytes,
        int gen0Collections,
        int gen1Collections,
        int gen2Collections)
    {
        FrameNumber = frameNumber;
        FrameDurationSeconds = frameDurationSeconds;
        CpuFrameMilliseconds = cpuFrameMilliseconds;
        GpuFrameMilliseconds = gpuFrameMilliseconds;
        ManagedMainThreadAllocationBytes = managedMainThreadAllocationBytes;
        Gen0Collections = gen0Collections;
        Gen1Collections = gen1Collections;
        Gen2Collections = gen2Collections;
    }

    public long FrameNumber { get; }
    public double FrameDurationSeconds { get; }
    public double CpuFrameMilliseconds { get; }
    public double GpuFrameMilliseconds { get; }
    public long ManagedMainThreadAllocationBytes { get; }
    public int Gen0Collections { get; }
    public int Gen1Collections { get; }
    public int Gen2Collections { get; }
}

internal sealed class PerformanceSampleWindow
{
    private readonly PerformanceFrameSample[] _samples;

    private int _head;
    private int _count;
    private double _durationSeconds;

    public PerformanceSampleWindow(int sampleCapacity)
    {
        if (sampleCapacity < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleCapacity));
        }

        _samples = new PerformanceFrameSample[sampleCapacity];
    }

    public int Count => _count;
    public double DurationSeconds => _durationSeconds;
    public PerformanceFrameSample Oldest => _count == 0
        ? throw new InvalidOperationException("The sample window is empty.")
        : _samples[_head];

    public void Add(PerformanceFrameSample sample)
    {
        if (!IsFinitePositive(sample.FrameDurationSeconds))
        {
            Clear();
            return;
        }

        if (_count == _samples.Length)
        {
            PerformanceFrameSample oldest = _samples[_head];
            _durationSeconds = Math.Max(0d, _durationSeconds - oldest.FrameDurationSeconds);
            _head = (_head + 1) % _samples.Length;
            _count--;
        }

        int index = (_head + _count) % _samples.Length;
        _samples[index] = sample;
        _count++;
        _durationSeconds += sample.FrameDurationSeconds;
    }

    public PerformanceFrameSample[] CopyChronologically()
    {
        PerformanceFrameSample[] copy = new PerformanceFrameSample[_count];
        for (int i = 0; i < _count; i++)
        {
            copy[i] = _samples[(_head + i) % _samples.Length];
        }

        return copy;
    }

    public void RemoveOldest()
    {
        if (_count == 0)
        {
            return;
        }

        PerformanceFrameSample oldest = _samples[_head];
        _durationSeconds = Math.Max(0d, _durationSeconds - oldest.FrameDurationSeconds);
        _head = (_head + 1) % _samples.Length;
        _count--;
    }

    public void Clear()
    {
        _head = 0;
        _count = 0;
        _durationSeconds = 0d;
    }

    private static bool IsFinitePositive(double value)
    {
        return value > 0d && !double.IsNaN(value) && !double.IsInfinity(value);
    }
}

internal sealed class PerformanceWindowSnapshot
{
    public PerformanceWindowSnapshot(
        string triggerKind,
        PerformanceFrameSample[] samples,
        double windowDurationSeconds,
        double averageFramesPerSecond,
        double thresholdFramesPerSecond,
        double sustainedSeconds,
        int reportIndex,
        int maxReportsPerSession)
    {
        TriggerKind = triggerKind ?? throw new ArgumentNullException(nameof(triggerKind));
        Samples = samples ?? throw new ArgumentNullException(nameof(samples));
        WindowDurationSeconds = windowDurationSeconds;
        AverageFramesPerSecond = averageFramesPerSecond;
        ThresholdFramesPerSecond = thresholdFramesPerSecond;
        SustainedSeconds = sustainedSeconds;
        ReportIndex = reportIndex;
        MaxReportsPerSession = maxReportsPerSession;
    }

    public string TriggerKind { get; }
    public PerformanceFrameSample[] Samples { get; }
    public double WindowDurationSeconds { get; }
    public double AverageFramesPerSecond { get; }
    public double ThresholdFramesPerSecond { get; }
    public double SustainedSeconds { get; }
    public int ReportIndex { get; }
    public int MaxReportsPerSession { get; }

    public static PerformanceWindowSnapshot Manual(PerformanceFrameSample[] samples)
    {
        return FromSamples(
            "manual",
            samples,
            0d,
            0d,
            0,
            0);
    }

    public static PerformanceWindowSnapshot FromSamples(
        string triggerKind,
        PerformanceFrameSample[] samples,
        double thresholdFramesPerSecond,
        double sustainedSeconds,
        int reportIndex,
        int maxReportsPerSession)
    {
        if (samples == null)
        {
            throw new ArgumentNullException(nameof(samples));
        }

        double duration = 0d;
        for (int i = 0; i < samples.Length; i++)
        {
            if (samples[i].FrameDurationSeconds > 0d)
            {
                duration += samples[i].FrameDurationSeconds;
            }
        }

        double averageFps = duration > 0d ? samples.Length / duration : 0d;
        return new PerformanceWindowSnapshot(
            triggerKind,
            samples,
            duration,
            averageFps,
            thresholdFramesPerSecond,
            sustainedSeconds,
            reportIndex,
            maxReportsPerSession);
    }
}

internal sealed class LowPerformanceIncidentDetector
{
    private const double DurationEpsilonSeconds = 0.000000001d;

    private readonly double _thresholdFramesPerSecond;
    private readonly double _sustainedSeconds;
    private readonly int _maxReportsPerSession;
    private readonly PerformanceSampleWindow _window;

    private bool _latched;
    private int _reportsRequested;
    private long _recoveryFrameCount;
    private double _recoveryDurationSeconds;

    public LowPerformanceIncidentDetector(
        double thresholdFramesPerSecond,
        double sustainedSeconds,
        int maxReportsPerSession,
        int sampleCapacity)
    {
        if (maxReportsPerSession < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxReportsPerSession));
        }

        _thresholdFramesPerSecond = thresholdFramesPerSecond;
        _sustainedSeconds = sustainedSeconds;
        _maxReportsPerSession = maxReportsPerSession;
        _window = new PerformanceSampleWindow(sampleCapacity);
    }

    public bool IsConfigured => IsConfiguredThreshold(_thresholdFramesPerSecond, _sustainedSeconds);
    public bool IsLatched => _latched;
    public int ReportsRequested => _reportsRequested;

    public static bool IsConfiguredThreshold(double thresholdFramesPerSecond, double sustainedSeconds)
    {
        return IsFinitePositive(thresholdFramesPerSecond) && IsFinitePositive(sustainedSeconds);
    }

    public static int CalculateSampleCapacity(
        double thresholdFramesPerSecond,
        double sustainedSeconds,
        int minimumCapacity)
    {
        if (minimumCapacity < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumCapacity));
        }

        if (!IsConfiguredThreshold(thresholdFramesPerSecond, sustainedSeconds))
        {
            return minimumCapacity;
        }

        double requiredCapacity = Math.Ceiling(thresholdFramesPerSecond * sustainedSeconds) + 2d;
        if (requiredCapacity > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(sustainedSeconds));
        }

        return Math.Max(minimumCapacity, (int)requiredCapacity);
    }

    public PerformanceWindowSnapshot? Observe(bool eligible, PerformanceFrameSample sample)
    {
        if (!IsConfigured || !eligible || !IsFinitePositive(sample.FrameDurationSeconds))
        {
            ClearWindow();
            return null;
        }

        _window.Add(sample);
        TrimOldSamples();

        if (_latched)
        {
            ObserveRecovery(sample);
            return null;
        }

        if (_window.DurationSeconds + DurationEpsilonSeconds < _sustainedSeconds)
        {
            return null;
        }

        bool aboveThreshold = IsAboveThreshold(_window.Count, _window.DurationSeconds);
        if (aboveThreshold)
        {
            return null;
        }

        _latched = true;
        ClearRecoveryWindow();
        if (_reportsRequested >= _maxReportsPerSession)
        {
            return null;
        }

        _reportsRequested++;
        PerformanceFrameSample[] samples = _window.CopyChronologically();
        return new PerformanceWindowSnapshot(
            "automatic-low-fps",
            samples,
            _window.DurationSeconds,
            samples.Length / _window.DurationSeconds,
            _thresholdFramesPerSecond,
            _sustainedSeconds,
            _reportsRequested,
            _maxReportsPerSession);
    }

    private void ObserveRecovery(PerformanceFrameSample sample)
    {
        _recoveryFrameCount++;
        _recoveryDurationSeconds += sample.FrameDurationSeconds;
        if (_recoveryDurationSeconds + DurationEpsilonSeconds < _sustainedSeconds)
        {
            return;
        }

        bool recovered = IsAboveThreshold(_recoveryFrameCount, _recoveryDurationSeconds);
        if (recovered)
        {
            _latched = false;
        }

        ClearRecoveryWindow();
    }

    private void TrimOldSamples()
    {
        while (_window.Count > 1)
        {
            PerformanceFrameSample oldest = _window.Oldest;
            double durationWithoutOldest = _window.DurationSeconds - oldest.FrameDurationSeconds;
            if (durationWithoutOldest + DurationEpsilonSeconds < _sustainedSeconds)
            {
                return;
            }

            _window.RemoveOldest();
        }
    }

    private void ClearWindow()
    {
        _window.Clear();
        ClearRecoveryWindow();
    }

    private void ClearRecoveryWindow()
    {
        _recoveryFrameCount = 0L;
        _recoveryDurationSeconds = 0d;
    }

    private bool IsAboveThreshold(long frameCount, double durationSeconds)
    {
        double thresholdFrameCount = _thresholdFramesPerSecond * durationSeconds;
        double floatingPointMargin = Math.Max(1d, thresholdFrameCount) * 0.000000001d;
        return frameCount > thresholdFrameCount + floatingPointMargin;
    }

    private static bool IsFinitePositive(double value)
    {
        return value > 0d && !double.IsNaN(value) && !double.IsInfinity(value);
    }
}

internal sealed class PerformanceWindowSummary
{
    private PerformanceWindowSummary()
    {
    }

    public int SampleCount { get; private set; }
    public double WindowDurationSeconds { get; private set; }
    public double AverageFramesPerSecond { get; private set; }
    public double AverageFrameMilliseconds { get; private set; }
    public double MedianFrameMilliseconds { get; private set; }
    public double P95FrameMilliseconds { get; private set; }
    public double P99FrameMilliseconds { get; private set; }
    public double MaximumFrameMilliseconds { get; private set; }
    public double OnePercentLowFramesPerSecond { get; private set; }
    public double ReferenceFramesPerSecond { get; private set; }
    public double ReferenceFrameMilliseconds { get; private set; }
    public int ReferenceBudgetMissCount { get; private set; }
    public double ReferenceBudgetMissPercentage { get; private set; }
    public int StutterCount { get; private set; }
    public double StutterPercentage { get; private set; }
    public int SevereStutterCount { get; private set; }
    public int CpuTimingSampleCount { get; private set; }
    public double AverageCpuFrameMilliseconds { get; private set; }
    public double MaximumCpuFrameMilliseconds { get; private set; }
    public int GpuTimingSampleCount { get; private set; }
    public double AverageGpuFrameMilliseconds { get; private set; }
    public double MaximumGpuFrameMilliseconds { get; private set; }
    public int AllocationSampleCount { get; private set; }
    public long TotalManagedMainThreadAllocationBytes { get; private set; }
    public long MaximumManagedMainThreadAllocationBytes { get; private set; }
    public double ManagedMainThreadAllocationBytesPerSecond { get; private set; }
    public int Gen0Collections { get; private set; }
    public int Gen1Collections { get; private set; }
    public int Gen2Collections { get; private set; }

    public static PerformanceWindowSummary Create(
        PerformanceWindowSnapshot snapshot,
        double referenceFramesPerSecond = 0d)
    {
        if (snapshot == null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

        PerformanceWindowSummary summary = new PerformanceWindowSummary
        {
            SampleCount = snapshot.Samples.Length,
            WindowDurationSeconds = snapshot.WindowDurationSeconds,
            AverageFramesPerSecond = snapshot.AverageFramesPerSecond,
            ReferenceFramesPerSecond = IsFinitePositive(referenceFramesPerSecond)
                ? referenceFramesPerSecond
                : 0d
        };
        summary.ReferenceFrameMilliseconds = summary.ReferenceFramesPerSecond > 0d
            ? 1000d / summary.ReferenceFramesPerSecond
            : 0d;

        if (snapshot.Samples.Length == 0)
        {
            summary.AverageCpuFrameMilliseconds = -1d;
            summary.AverageGpuFrameMilliseconds = -1d;
            return summary;
        }

        double[] sortedFrameMilliseconds = new double[snapshot.Samples.Length];
        double cpuTotal = 0d;
        double gpuTotal = 0d;

        for (int i = 0; i < snapshot.Samples.Length; i++)
        {
            PerformanceFrameSample sample = snapshot.Samples[i];
            double frameMilliseconds = sample.FrameDurationSeconds * 1000d;
            sortedFrameMilliseconds[i] = frameMilliseconds;
            if (frameMilliseconds > summary.MaximumFrameMilliseconds)
            {
                summary.MaximumFrameMilliseconds = frameMilliseconds;
            }

            if (summary.ReferenceFrameMilliseconds > 0d &&
                frameMilliseconds > summary.ReferenceFrameMilliseconds + 0.000000001d)
            {
                summary.ReferenceBudgetMissCount++;
            }

            if (frameMilliseconds >= 50d)
            {
                summary.StutterCount++;
            }

            if (frameMilliseconds >= 100d)
            {
                summary.SevereStutterCount++;
            }

            if (IsFinitePositive(sample.CpuFrameMilliseconds))
            {
                summary.CpuTimingSampleCount++;
                cpuTotal += sample.CpuFrameMilliseconds;
                if (sample.CpuFrameMilliseconds > summary.MaximumCpuFrameMilliseconds)
                {
                    summary.MaximumCpuFrameMilliseconds = sample.CpuFrameMilliseconds;
                }
            }

            if (IsFinitePositive(sample.GpuFrameMilliseconds))
            {
                summary.GpuTimingSampleCount++;
                gpuTotal += sample.GpuFrameMilliseconds;
                if (sample.GpuFrameMilliseconds > summary.MaximumGpuFrameMilliseconds)
                {
                    summary.MaximumGpuFrameMilliseconds = sample.GpuFrameMilliseconds;
                }
            }

            if (sample.ManagedMainThreadAllocationBytes >= 0L)
            {
                summary.AllocationSampleCount++;
                summary.TotalManagedMainThreadAllocationBytes = SaturatingAdd(
                    summary.TotalManagedMainThreadAllocationBytes,
                    sample.ManagedMainThreadAllocationBytes);
                if (sample.ManagedMainThreadAllocationBytes > summary.MaximumManagedMainThreadAllocationBytes)
                {
                    summary.MaximumManagedMainThreadAllocationBytes = sample.ManagedMainThreadAllocationBytes;
                }
            }

            summary.Gen0Collections = SaturatingAdd(summary.Gen0Collections, sample.Gen0Collections);
            summary.Gen1Collections = SaturatingAdd(summary.Gen1Collections, sample.Gen1Collections);
            summary.Gen2Collections = SaturatingAdd(summary.Gen2Collections, sample.Gen2Collections);
        }

        Array.Sort(sortedFrameMilliseconds);
        summary.MedianFrameMilliseconds = NearestRankPercentile(sortedFrameMilliseconds, 0.50d);
        summary.P95FrameMilliseconds = NearestRankPercentile(sortedFrameMilliseconds, 0.95d);
        summary.P99FrameMilliseconds = NearestRankPercentile(sortedFrameMilliseconds, 0.99d);
        summary.AverageFrameMilliseconds = snapshot.WindowDurationSeconds > 0d
            ? snapshot.WindowDurationSeconds * 1000d / snapshot.Samples.Length
            : 0d;
        int onePercentFrameCount = Math.Max(1, (int)Math.Ceiling(sortedFrameMilliseconds.Length * 0.01d));
        double onePercentFrameMilliseconds = 0d;
        for (int i = sortedFrameMilliseconds.Length - onePercentFrameCount;
             i < sortedFrameMilliseconds.Length;
             i++)
        {
            onePercentFrameMilliseconds += sortedFrameMilliseconds[i];
        }

        double onePercentAverageMilliseconds = onePercentFrameMilliseconds / onePercentFrameCount;
        summary.OnePercentLowFramesPerSecond = onePercentAverageMilliseconds > 0d
            ? 1000d / onePercentAverageMilliseconds
            : 0d;
        summary.ReferenceBudgetMissPercentage = summary.SampleCount > 0
            ? summary.ReferenceBudgetMissCount * 100d / summary.SampleCount
            : 0d;
        summary.StutterPercentage = summary.SampleCount > 0
            ? summary.StutterCount * 100d / summary.SampleCount
            : 0d;
        summary.ManagedMainThreadAllocationBytesPerSecond = summary.WindowDurationSeconds > 0d
            ? summary.TotalManagedMainThreadAllocationBytes / summary.WindowDurationSeconds
            : 0d;
        summary.AverageCpuFrameMilliseconds = summary.CpuTimingSampleCount == 0
            ? -1d
            : cpuTotal / summary.CpuTimingSampleCount;
        summary.AverageGpuFrameMilliseconds = summary.GpuTimingSampleCount == 0
            ? -1d
            : gpuTotal / summary.GpuTimingSampleCount;
        return summary;
    }

    private static double NearestRankPercentile(double[] sortedValues, double percentile)
    {
        if (sortedValues.Length == 0)
        {
            return 0d;
        }

        int index = Math.Max(0, (int)Math.Ceiling(sortedValues.Length * percentile) - 1);
        return sortedValues[Math.Min(sortedValues.Length - 1, index)];
    }

    private static bool IsFinitePositive(double value)
    {
        return value > 0d && !double.IsNaN(value) && !double.IsInfinity(value);
    }

    private static long SaturatingAdd(long left, long right)
    {
        if (right <= 0L)
        {
            return left;
        }

        return left > long.MaxValue - right ? long.MaxValue : left + right;
    }

    private static int SaturatingAdd(int left, int right)
    {
        if (right <= 0)
        {
            return left;
        }

        return left > int.MaxValue - right ? int.MaxValue : left + right;
    }
}

internal sealed class PerformanceInvestigationAssessment
{
    public PerformanceInvestigationAssessment(
        string confidence,
        string symptomId,
        string evidence,
        string doesNotProve,
        string nativeInvestigation,
        string modStackInvestigation,
        string bestNextCheck)
    {
        Confidence = confidence;
        SymptomId = symptomId;
        Evidence = evidence;
        DoesNotProve = doesNotProve;
        NativeInvestigation = nativeInvestigation;
        ModStackInvestigation = modStackInvestigation;
        BestNextCheck = bestNextCheck;
    }

    public string Confidence { get; }
    public string SymptomId { get; }
    public string Evidence { get; }
    public string DoesNotProve { get; }
    public string NativeInvestigation { get; }
    public string ModStackInvestigation { get; }
    public string BestNextCheck { get; }
    public bool CausalityProven => false;
}

internal static class PerformanceInvestigationAnalyzer
{
    private const string CounterBoundary =
        "Counters identify broad correlated pressure only; they do not identify an exact FoA native subsystem, method, scene object, asset, or loaded plugin.";

    public static PerformanceInvestigationAssessment Classify(
        PerformanceWindowSnapshot snapshot,
        PerformanceWindowSummary summary,
        int loadedPluginCount,
        int otherLoadedPluginCount)
    {
        if (snapshot == null || summary == null)
        {
            return Build(
                "unattributed",
                "invalid-report-input",
                "The snapshot or summary input was missing.",
                loadedPluginCount,
                otherLoadedPluginCount);
        }

        if (!LowPerformanceIncidentDetector.IsConfiguredThreshold(
                snapshot.ThresholdFramesPerSecond,
                snapshot.SustainedSeconds))
        {
            return Build(
                "snapshot-only",
                "manual-performance-snapshot",
                "Manual report captured the bounded sample window without an automatic incident threshold.",
                loadedPluginCount,
                otherLoadedPluginCount);
        }

        double frameBudgetMilliseconds = 1000d / snapshot.ThresholdFramesPerSecond;
        bool cpuWindowComplete = summary.SampleCount > 0 &&
                                 summary.CpuTimingSampleCount == summary.SampleCount;
        bool gpuWindowComplete = summary.SampleCount > 0 &&
                                 summary.GpuTimingSampleCount == summary.SampleCount;
        bool cpuPressure = cpuWindowComplete &&
                           summary.AverageCpuFrameMilliseconds >= frameBudgetMilliseconds;
        bool gpuPressure = gpuWindowComplete &&
                           summary.AverageGpuFrameMilliseconds >= frameBudgetMilliseconds;

        if (cpuPressure && gpuPressure)
        {
            return Build(
                "likely-counter-pressure",
                "mixed-cpu-gpu-frame-pressure",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Average CPU timing was {0:0.###} ms and average GPU timing was {1:0.###} ms across the complete incident window; both met or exceeded the configured {2:0.###} ms frame budget.",
                    summary.AverageCpuFrameMilliseconds,
                    summary.AverageGpuFrameMilliseconds,
                    frameBudgetMilliseconds),
                loadedPluginCount,
                otherLoadedPluginCount);
        }

        if (cpuPressure)
        {
            return Build(
                "likely-counter-pressure",
                "cpu-frame-pressure",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Average CPU timing was {0:0.###} ms across the complete incident window, meeting or exceeding the configured {1:0.###} ms frame budget.",
                    summary.AverageCpuFrameMilliseconds,
                    frameBudgetMilliseconds),
                loadedPluginCount,
                otherLoadedPluginCount);
        }

        if (gpuPressure)
        {
            return Build(
                "likely-counter-pressure",
                "gpu-frame-pressure",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Average GPU timing was {0:0.###} ms across the complete incident window, meeting or exceeding the configured {1:0.###} ms frame budget.",
                    summary.AverageGpuFrameMilliseconds,
                    frameBudgetMilliseconds),
                loadedPluginCount,
                otherLoadedPluginCount);
        }

        long totalCollections = (long)summary.Gen0Collections + summary.Gen1Collections + summary.Gen2Collections;
        if (totalCollections > 0L)
        {
            return Build(
                "likely-counter-correlation",
                "managed-gc-activity-correlated",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} managed collection event(s) overlapped the configured low-FPS window while complete CPU/GPU timing evidence did not independently cross the configured frame budget.",
                    totalCollections),
                loadedPluginCount,
                otherLoadedPluginCount);
        }

        bool partialFrameTiming =
            (summary.CpuTimingSampleCount > 0 && !cpuWindowComplete) ||
            (summary.GpuTimingSampleCount > 0 && !gpuWindowComplete);
        return Build(
            "unattributed",
            "low-fps-unattributed",
            partialFrameTiming
                ? "The configured low-FPS trigger was observed, but CPU/GPU timing coverage was incomplete for the window and GC counters did not support a bounded classification."
                : "The configured low-FPS trigger was observed, but available CPU/GPU/GC counters did not support a bounded classification.",
            loadedPluginCount,
            otherLoadedPluginCount);
    }

    private static PerformanceInvestigationAssessment Build(
        string confidence,
        string symptomId,
        string evidence,
        int loadedPluginCount,
        int otherLoadedPluginCount)
    {
        string nativeLane =
            "Open investigation lane: reproduce the same save, scene, settings, and route with only BepInEx plus Tainted Performance enabled. If the symptom persists there, capture a native/engine-capable profiler timeline before naming a FoA subsystem.";
        string modLane = otherLoadedPluginCount > 0
            ? string.Format(
                CultureInfo.InvariantCulture,
                "Open investigation lane: {0} other BepInEx plugin(s) were loaded. This is inventory context only; compare against a Tainted-Performance-only baseline, then isolate the mod stack before naming a mod.",
                otherLoadedPluginCount)
            : string.Format(
                CultureInfo.InvariantCulture,
                "Open investigation lane: loaded plugin inventory contained {0} plugin(s), with no additional plugin beyond the monitor counted here. This still does not prove a native cause without a controlled baseline.",
                loadedPluginCount);

        return new PerformanceInvestigationAssessment(
            confidence,
            symptomId,
            evidence,
            CounterBoundary + " Loaded plugin inventory is context only and is never causal proof.",
            nativeLane,
            modLane,
            "Keep the report with the save/settings/scenario notes, then run controlled baseline and profiler checks for the lane that remains reproducible.");
    }
}
