# Build Performance Telemetry Without Blaming a Plugin

Collect frame, CPU/GPU, allocation and GC counters into bounded windows. Treat the loaded mod list as context, not attribution.

Working lineage: [Telemetry Without Blame](../../../research/case-studies/performance/telemetry-without-blame.md).

## Sample once per eligible Update

The maintained Tainted Performance sampler builds one PerformanceFrameSample from:

~~~csharp
Time.frameCount
Time.unscaledDeltaTime
CPU frame milliseconds
GPU frame milliseconds
main-thread allocation delta
Gen0 collection delta
Gen1 collection delta
Gen2 collection delta
~~~

Skip paused/loading/menu periods according to your eligibility policy so those frames do not contaminate the gameplay window.

## CPU/GPU timing

When enabled:

~~~csharp
FrameTimingManager.CaptureFrameTimings();

uint count =
    FrameTimingManager.GetLatestTimings(
        1u,
        frameTimingBuffer);
~~~

Read:

- FrameTiming.cpuFrameTime;
- FrameTiming.gpuFrameTime.

If FrameTimingManager throws or returns no valid samples, mark those counters unavailable and continue with the remaining telemetry.

Do not crash the monitor because a graphics path does not expose timings.

## Main-thread allocation

Use:

~~~csharp
long current =
    GC.GetAllocatedBytesForCurrentThread();
~~~

Record the positive delta from the previous eligible sample.

This is **main-thread allocation correlation**, not whole-process allocation attribution.

## Collection activity

Read:

~~~csharp
GC.CollectionCount(0)
GC.CollectionCount(1)
GC.CollectionCount(2)
~~~

Store deltas from the previous baseline.

Advance/reset baselines carefully around ineligible frames so menu/loading allocations are not charged to the first gameplay sample.

## Use a bounded ring

Keep recent frame samples in a fixed-capacity ring.

Normal Update should do only:

~~~text
eligibility check
counter reads
ring insert/evict
threshold/recovery arithmetic
~~~

No reflection, plugin enumeration, LINQ, string formatting, logging, or file I/O on the ordinary hot path.

## Incident trigger

A useful automatic trigger is:

~~~text
average/rolling FPS at or below threshold
for configured sustained duration
→ latch one incident request
~~~

Once latched, do not emit repeated reports until a complete above-threshold recovery window rearms the detector.

Cap reports per session.

## Gather expensive context only after a trigger

After the incident has been captured:

- copy the bounded sample window;
- read scene/application/system info;
- enumerate loaded BepInEx plugins once;
- summarize timing/allocation/GC data;
- write the report.

For Mono, loaded plug-ins come from Chainloader.PluginInfos.Values. For IL2CPP, use the IL2CPP chainloader's plugin collection.

Record GUID/name/version only as environment context.

## Report files

The maintained implementation writes a new incident directory containing data such as:

~~~text
performance_report.json
report.md
frame_samples.csv
loaded_plugins.csv
~~~

Use create-new semantics for report files so one incident never silently overwrites another.

## Interpretation

Keep these statements distinct:

~~~text
plugin loaded
≠ spike occurred while plugin loaded
≠ spike correlates with plugin activity
≠ plugin isolated as contributor
≠ plugin proven cause
~~~

To move from telemetry to attribution, reproduce the problem and change one variable at a time or use a profiler capable of identifying the expensive code path.
