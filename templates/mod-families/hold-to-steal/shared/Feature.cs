using Tainted.Abstractions.Runtime;

namespace TGTemplate.HoldToSteal;

internal static class Feature
{
    internal const string SourceFamily = "hold-to-steal";

    internal static bool ShouldAllowAction(
        bool enabled,
        bool isIllegalAction,
        bool requiredModifierHeld)
        => !enabled || !isIllegalAction || requiredModifierHeld;

    internal static bool ShouldSuppressNativeAction(
        bool enabled,
        bool isIllegalAction,
        bool requiredModifierHeld)
        => enabled && isIllegalAction && !requiredModifierHeld;

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Hold to Steal starter initialized. runtime=" + runtimeKind +
           "; legal-actions-remain-native";
}
