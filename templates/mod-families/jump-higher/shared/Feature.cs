using Tainted.Abstractions.Runtime;

namespace TGTemplate.JumpHigher;

internal static class Feature
{
    internal const string SourceFamily = "jump-higher";

    internal static bool CanUseExtraJump(
        bool enabled,
        bool grounded,
        bool swimming,
        int extraJumpsUsed,
        int extraJumpsAllowed)
        => enabled && !grounded && !swimming && extraJumpsUsed < extraJumpsAllowed;

    internal static float ResolveExtraJumpHeight(float nativeJumpHeight, float multiplier)
        => nativeJumpHeight * (multiplier < 0f ? 0f : multiplier);

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Jump Higher starter initialized. runtime=" + runtimeKind +
           "; session-only-extra-jump-gate";
}
