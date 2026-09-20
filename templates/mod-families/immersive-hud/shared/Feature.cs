using Tainted.Abstractions.Runtime;

namespace TGTemplate.ImmersiveHud;

internal static class Feature
{
    internal const string SourceFamily = "always-show-hud";

    internal static bool ResolveVisibility(
        bool enabled,
        bool targetHudElement,
        bool nativeVisible,
        bool forceVisible)
        => targetHudElement && enabled && forceVisible ? true : nativeVisible;

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Immersive HUD starter initialized. runtime=" + runtimeKind +
           "; selected-hud-presentation-only";
}
