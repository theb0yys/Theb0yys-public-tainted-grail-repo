using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using UnityEngine;

namespace TGCommunity.Example.PerformanceTelemetry;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.performance-telemetry";
    public const string PluginName = "TG Example - Performance Telemetry";
    public const string PluginVersion = "0.1.0";

    private const int Capacity = 4096;
    private readonly Sample[] _ring = new Sample[Capacity];
    private readonly FrameTiming[] _timing = new FrameTiming[1];

    private ConfigEntry<float> _lowFpsThreshold = null!;
    private ConfigEntry<float> _sustainedSeconds = null!;
    private int _next;
    private int _count;
    private float _lowDuration;
    private float _recoveryDuration;
    private bool _latched;
    private bool _frameTimingEnabled = true;
    private long _lastAllocated = -1;
    private int _lastGen0;
    private int _lastGen1;
    private int _lastGen2;
    private bool _gcReady;
    private string _outputRoot = string.Empty;

    private void Awake()
    {
        _lowFpsThreshold = Config.Bind("Trigger", "LowFpsThreshold", 45f, "Average/sustained low-FPS threshold.");
        _sustainedSeconds = Config.Bind("Trigger", "SustainedSeconds", 5f, "Seconds below threshold before one report is written.");
        _outputRoot = Path.Combine(Paths.ConfigPath, PluginGuid, "performance-reports");
        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void Update()
    {
        if (Time.timeScale <= 0.001f || Time.unscaledDeltaTime <= 0f)
        {
            ResetCounterBaselines();
            _lowDuration = 0f;
            _recoveryDuration = 0f;
            return;
        }

        Sample sample = CollectSample();
        Add(sample);

        float fps = 1f / sample.FrameSeconds;
        float threshold = Math.Max(1f, _lowFpsThreshold.Value);
        float sustained = Math.Max(1f, _sustainedSeconds.Value);

        if (fps <= threshold)
        {
            _lowDuration += sample.FrameSeconds;
            _recoveryDuration = 0f;

            if (!_latched && _lowDuration >= sustained)
            {
                _latched = true;
                WriteReport();
            }
        }
        else
        {
            _lowDuration = 0f;

            if (_latched)
            {
                _recoveryDuration += sample.FrameSeconds;
                if (_recoveryDuration >= 3f)
                {
                    _latched = false;
                    _recoveryDuration = 0f;
                }
            }
        }
    }

    private Sample CollectSample()
    {
        double cpuMs = -1;
        double gpuMs = -1;

        if (_frameTimingEnabled)
        {
            try
            {
                FrameTimingManager.CaptureFrameTimings();
                uint count = FrameTimingManager.GetLatestTimings(1u, _timing);
                if (count > 0)
                {
                    cpuMs = _timing[0].cpuFrameTime;
                    gpuMs = _timing[0].gpuFrameTime;
                }
            }
            catch
            {
                _frameTimingEnabled = false;
            }
        }

        long allocation = -1;
        try
        {
            long current = GC.GetAllocatedBytesForCurrentThread();
            allocation = _lastAllocated < 0 || current < _lastAllocated ? 0 : current - _lastAllocated;
            _lastAllocated = current;
        }
        catch
        {
            _lastAllocated = -1;
        }

        int gen0 = GC.CollectionCount(0);
        int gen1 = GC.CollectionCount(1);
        int gen2 = GC.CollectionCount(2);

        int d0 = _gcReady ? Math.Max(0, gen0 - _lastGen0) : 0;
        int d1 = _gcReady ? Math.Max(0, gen1 - _lastGen1) : 0;
        int d2 = _gcReady ? Math.Max(0, gen2 - _lastGen2) : 0;

        _gcReady = true;
        _lastGen0 = gen0;
        _lastGen1 = gen1;
        _lastGen2 = gen2;

        return new Sample(
            Time.frameCount,
            Time.unscaledDeltaTime,
            cpuMs,
            gpuMs,
            allocation,
            d0,
            d1,
            d2);
    }

    private void Add(Sample sample)
    {
        _ring[_next] = sample;
        _next = (_next + 1) % Capacity;
        _count = Math.Min(Capacity, _count + 1);
    }

    private void WriteReport()
    {
        try
        {
            Directory.CreateDirectory(_outputRoot);
            string stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff");
            string folder = Path.Combine(_outputRoot, "incident-" + stamp);
            Directory.CreateDirectory(folder);

            var csv = new StringBuilder();
            csv.AppendLine("frame,frame_ms,cpu_ms,gpu_ms,main_thread_alloc_bytes,gc0,gc1,gc2");

            foreach (Sample sample in Snapshot())
            {
                csv.Append(sample.Frame).Append(',')
                    .Append((sample.FrameSeconds * 1000f).ToString("0.###", CultureInfo.InvariantCulture)).Append(',')
                    .Append(sample.CpuMs.ToString("0.###", CultureInfo.InvariantCulture)).Append(',')
                    .Append(sample.GpuMs.ToString("0.###", CultureInfo.InvariantCulture)).Append(',')
                    .Append(sample.AllocationBytes).Append(',')
                    .Append(sample.Gen0).Append(',')
                    .Append(sample.Gen1).Append(',')
                    .Append(sample.Gen2).AppendLine();
            }

            File.WriteAllText(Path.Combine(folder, "frame_samples.csv"), csv.ToString(), Encoding.UTF8);

            var plugins = new StringBuilder();
            plugins.AppendLine("guid,name,version");
            foreach (PluginInfo info in Chainloader.PluginInfos.Values)
            {
                plugins.Append(Escape(info.Metadata.GUID)).Append(',')
                    .Append(Escape(info.Metadata.Name)).Append(',')
                    .Append(Escape(info.Metadata.Version?.ToString() ?? string.Empty))
                    .AppendLine();
            }

            File.WriteAllText(Path.Combine(folder, "loaded_plugins.csv"), plugins.ToString(), Encoding.UTF8);
            File.WriteAllText(
                Path.Combine(folder, "README.txt"),
                "Counters are correlation data. Loaded plug-ins are environment context and are not attribution.\n",
                Encoding.UTF8);

            Logger.LogWarning($"Performance incident written: {folder}");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Performance report failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private IEnumerable<Sample> Snapshot()
    {
        int start = (_next - _count + Capacity) % Capacity;
        for (int i = 0; i < _count; i++)
        {
            yield return _ring[(start + i) % Capacity];
        }
    }

    private void ResetCounterBaselines()
    {
        _lastAllocated = -1;
        _gcReady = false;
    }

    private static string Escape(string value)
    {
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    private readonly struct Sample
    {
        internal Sample(int frame, float frameSeconds, double cpuMs, double gpuMs, long allocationBytes, int gen0, int gen1, int gen2)
        {
            Frame = frame;
            FrameSeconds = frameSeconds;
            CpuMs = cpuMs;
            GpuMs = gpuMs;
            AllocationBytes = allocationBytes;
            Gen0 = gen0;
            Gen1 = gen1;
            Gen2 = gen2;
        }

        internal int Frame { get; }
        internal float FrameSeconds { get; }
        internal double CpuMs { get; }
        internal double GpuMs { get; }
        internal long AllocationBytes { get; }
        internal int Gen0 { get; }
        internal int Gen1 { get; }
        internal int Gen2 { get; }
    }
}
