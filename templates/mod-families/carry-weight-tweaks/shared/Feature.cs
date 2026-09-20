using Tainted.Abstractions.Runtime;

namespace TGTemplate.CarryWeightTweaks;

internal static class Feature
{
    internal const string SourceFamily = "carry-weight-tweaks";

    internal static float ResolveCarryCapacity(float vanillaCapacity, float configuredCapacity)
        => configuredCapacity > 0f ? configuredCapacity : vanillaCapacity;

    internal static bool ShouldApplyOverride(float configuredCapacity)
        => configuredCapacity > 0f;

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "CarryWeightTweaks starter initialized. runtime=" + runtimeKind +
           "; configured-capacity-zero=vanilla";
}
