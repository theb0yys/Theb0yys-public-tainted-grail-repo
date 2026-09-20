using Tainted.Abstractions.Runtime;

namespace TGTemplate.EasyAvalon;

internal static class Feature
{
    internal const string SourceFamily = "easy-avalon";

    internal static float ScalePlayerDamageTaken(float nativeAmount, float multiplier)
        => nativeAmount * ClampNonNegative(multiplier);

    internal static float ScalePlayerDamageDealt(float nativeAmount, float multiplier)
        => nativeAmount * ClampNonNegative(multiplier);

    private static float ClampNonNegative(float value) => value < 0f ? 0f : value;

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Easy Avalon starter initialized. runtime=" + runtimeKind +
           "; bounded-player-combat-multipliers";
}
