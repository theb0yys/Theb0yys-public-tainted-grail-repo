using System;
using Tainted.Abstractions.Runtime;

namespace TGTemplate.MultiPinMapNotes;

internal static class Feature
{
    internal const string SourceFamily = "multi-pin-map-notes";

    internal static bool CanAddPin(int currentPins, int maxPins)
        => maxPins > 0 && currentPins >= 0 && currentPins < maxPins;

    internal static string NormalizeLabel(string? label, int maxLength)
    {
        string value = (label ?? string.Empty).Trim();
        if (maxLength <= 0 || value.Length <= maxLength)
            return value;

        return value.Substring(0, maxLength);
    }

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Multi-Pin Map Notes starter initialized. runtime=" + runtimeKind +
           "; mod-owned-pins-do-not-create-world-discovery";
}
