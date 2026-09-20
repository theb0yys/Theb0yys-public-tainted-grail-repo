using Tainted.Abstractions.Runtime;

namespace TGTemplate.TaintedCombat;

internal static class Feature
{
    internal const string SourceFamily = "Tainted Combat";
    internal static readonly string[] Mechanisms =
    {
        "block/parry tuning",
        "combat feel presets",
        "consumable pressure",
        "difficulty pressure"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Tainted Combat starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
