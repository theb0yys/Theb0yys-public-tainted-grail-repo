using Tainted.Abstractions.Runtime;

namespace TGTemplate.NoFallDamage;

internal static class Feature
{
    internal const string SourceFamily = "no-fall-damage";

    internal static float ResolveFallDamage(
        float nativeFallDamage,
        bool enabled,
        float multiplier)
    {
        if (!enabled)
            return nativeFallDamage;

        float safeMultiplier = multiplier < 0f ? 0f : multiplier;
        return nativeFallDamage * safeMultiplier;
    }

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "No Fall Damage starter initialized. runtime=" + runtimeKind +
           "; only-verified-fall-damage-path-is-adjusted";
}
