using Tainted.Abstractions.Runtime;

namespace TGTemplate.ImmersiveHud;

internal static class Feature
{
    internal const string SourceFamily = "always-show-hud";
    internal static readonly string[] Mechanisms =
    {
        "HUD visibility context",
        "damage-number presentation",
        "hero bar visibility",
        "shared UI integration"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Immersive HUD starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
