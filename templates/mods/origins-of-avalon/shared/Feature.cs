using Tainted.Abstractions.Runtime;

namespace TGTemplate.OriginsOfAvalon;

internal static class Feature
{
    internal const string SourceFamily = "origins-of-avalon";

    internal static bool CanApplyOrigin(
        bool originSelected,
        bool prerequisitesMet,
        bool alreadyApplied)
        => originSelected && prerequisitesMet && !alreadyApplied;

    internal static bool ShouldRemoveOriginEffects(
        bool originNoLongerSelected,
        bool effectsOwnedByMod)
        => originNoLongerSelected && effectsOwnedByMod;

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Origins of Avalon starter initialized. runtime=" + runtimeKind +
           "; origin-effects-remain-explicit-and-owned";
}
