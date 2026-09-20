# Performance Telemetry

A bounded telemetry example that keeps the hot path small.

Every eligible Update records:

- Time.unscaledDeltaTime;
- optional FrameTimingManager CPU/GPU timing;
- GC.GetAllocatedBytesForCurrentThread delta;
- GC.CollectionCount deltas.

When low FPS persists for the configured duration, the example copies the bounded ring and only then enumerates loaded BepInEx plug-ins and writes CSV files.

Loaded plug-ins are context, not blame.

## Build

~~~powershell
dotnet build .\PerformanceTelemetry.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Guide: [Build non-causal performance telemetry](../../../../guides/tasks/diagnostics/build-non-causal-performance-telemetry.md)
