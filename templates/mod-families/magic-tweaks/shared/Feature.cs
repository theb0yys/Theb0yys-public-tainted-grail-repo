using Tainted.Abstractions.Runtime;

namespace TGTemplate.MagicTweaks;

internal static class Feature
{
    internal const string SourceFamily = "magic-tweaks";

    internal static float Scale(float nativeValue, float multiplier)
        => nativeValue * (multiplier < 0f ? 0f : multiplier);

    internal static bool AppliesToSource(
        bool sourceIsPlayer,
        bool sourceIsEnemy,
        bool affectPlayer,
        bool affectEnemy)
        => (sourceIsPlayer && affectPlayer) || (sourceIsEnemy && affectEnemy);

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Magic Tweaks starter initialized. runtime=" + runtimeKind +
           "; each tuning surface remains independently gated";
}
