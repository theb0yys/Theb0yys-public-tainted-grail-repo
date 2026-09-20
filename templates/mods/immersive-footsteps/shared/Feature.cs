using Tainted.Abstractions.Runtime;

namespace TGTemplate.ImmersiveFootsteps;

internal static class Feature
{
    internal const string SourceFamily = "immersive-footsteps";

    internal static bool ShouldSuppressNativeFootstep(
        bool enabled,
        bool heroFootstep,
        bool replacementStarted)
        => enabled && heroFootstep && replacementStarted;

    internal static bool ShouldRunNativeFootstep(
        bool enabled,
        bool heroFootstep,
        bool replacementStarted)
        => !ShouldSuppressNativeFootstep(enabled, heroFootstep, replacementStarted);

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Immersive Footsteps starter initialized. runtime=" + runtimeKind +
           "; suppress-native-only-after-replacement-starts";
}
