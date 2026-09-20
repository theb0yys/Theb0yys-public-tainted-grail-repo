using Tainted.Abstractions.Runtime;

namespace TGTemplate.StaminaControl;

internal static class Feature
{
    internal const string SourceFamily = "stamina-action-control";

    internal static float ResolveCost(float nativeCost, float multiplier, bool enabled)
    {
        if (!enabled)
            return nativeCost;

        float safeMultiplier = multiplier < 0f ? 0f : multiplier;
        return nativeCost * safeMultiplier;
    }

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "StaminaControl starter initialized. runtime=" + runtimeKind +
           "; native-action-execution-preserved";
}
