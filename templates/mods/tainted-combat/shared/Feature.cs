using Tainted.Abstractions.Runtime;

namespace TGTemplate.TaintedCombat;

internal static class Feature
{
    internal const string SourceFamily = "Tainted Combat";

    internal static float ScaleOutgoingDamage(float nativeDamage, float multiplier)
        => nativeDamage * NonNegative(multiplier);

    internal static float ScaleIncomingDamage(float nativeDamage, float multiplier)
        => nativeDamage * NonNegative(multiplier);

    internal static float ScaleWindow(float nativeSeconds, float multiplier)
        => nativeSeconds * NonNegative(multiplier);

    private static float NonNegative(float value) => value < 0f ? 0f : value;

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Tainted Combat starter initialized. runtime=" + runtimeKind +
           "; combat-concerns-remain-owner-scoped";
}
