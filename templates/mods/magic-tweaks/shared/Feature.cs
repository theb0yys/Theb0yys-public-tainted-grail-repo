using Tainted.Abstractions.Runtime;

namespace TGTemplate.MagicTweaks;

internal static class Feature
{
    internal const string SourceFamily = "magic-tweaks";
    internal static readonly string[] Mechanisms =
    {
        "cast speed",
        "projectile speed",
        "area/cooldown/cost tuning",
        "player-owned filtering"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Magic Tweaks starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
