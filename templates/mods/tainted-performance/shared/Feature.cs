using Tainted.Abstractions.Runtime;

namespace TGTemplate.TaintedPerformance;

internal sealed class FrameAccumulator
{
    private double _seconds;
    private int _samples;

    internal void AddSample(double frameSeconds)
    {
        if (frameSeconds <= 0d)
            return;

        _seconds += frameSeconds;
        _samples++;
    }

    internal int Samples => _samples;
    internal double AverageFrameMs => _samples == 0 ? 0d : (_seconds / _samples) * 1000d;
    internal double ApproximateFps => AverageFrameMs <= 0d ? 0d : 1000d / AverageFrameMs;

    internal void Reset()
    {
        _seconds = 0d;
        _samples = 0;
    }
}

internal static class Feature
{
    internal const string SourceFamily = "tainted-performance";

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Tainted Performance starter initialized. runtime=" + runtimeKind +
           "; read-only-frame-accumulator-ready";
}
