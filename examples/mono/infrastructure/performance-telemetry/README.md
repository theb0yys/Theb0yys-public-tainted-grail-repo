# Record Performance Problems Without Guessing the Cause

This example records a small rolling window of frame-performance information.

If low frame rate lasts long enough, it writes a report you can inspect later.

It does **not** say that a loaded mod caused the slowdown.

## Build it

~~~powershell
dotnet build .\PerformanceTelemetry.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Try it in game

Use a low-FPS threshold that is easy to trigger for testing.

Play until the threshold is sustained long enough for the example to create a report.

The report includes things such as:

- frame time;
- FPS;
- CPU/GPU frame timing when Unity provides it;
- memory allocated on the main thread;
- garbage-collection activity;
- the plug-ins that were loaded at the time.

## What to change first

Change the FPS threshold or the number of seconds required before an incident is captured.

Do not begin by adding more counters to every frame.

## How it works

Every normal sample does only a small amount of work.

The example reads:

~~~text
Time.unscaledDeltaTime
FrameTimingManager
GC allocation counter
GC collection counters
~~~

and puts the result into a fixed-size recent-history buffer.

Only after a performance incident is detected does it do the more expensive work of copying the history, listing loaded plug-ins, and writing report files.

A loaded plug-in appearing in the report means only that it was loaded. It is not proof that the plug-in caused the slowdown.

## Next

[Read the performance telemetry guide](../../../../guides/tasks/diagnostics/build-non-causal-performance-telemetry.md)
