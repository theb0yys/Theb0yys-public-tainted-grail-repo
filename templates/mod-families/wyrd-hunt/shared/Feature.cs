using System;
using Tainted.Abstractions.Runtime;

namespace TGTemplate.WyrdHunt;

internal static class Feature
{
    internal const string SourceFamily = "wyrd-hunt";

    internal static float Pressure(float secondsInWyrdness, float secondsToFullPressure)
    {
        if (secondsInWyrdness <= 0f || secondsToFullPressure <= 0f)
            return 0f;

        float value = secondsInWyrdness / secondsToFullPressure;
        return Math.Max(0f, Math.Min(1f, value));
    }

    internal static bool CanScheduleHunt(
        bool enabled,
        bool ownerReady,
        bool encounterAlreadyActive,
        float pressure,
        float threshold)
        => enabled && ownerReady && !encounterAlreadyActive && pressure >= threshold;

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Wyrd Hunt starter initialized. runtime=" + runtimeKind +
           "; pressure-and-encounter-gates-ready";
}
