using Tainted.Abstractions.Runtime;

namespace TGTemplate.AvalonCompanions;

internal static class Feature
{
    internal const string SourceFamily = "avalon-companions";
    internal static readonly string[] Mechanisms =
    {
        "companion lifecycle",
        "AI owner/package",
        "follow/recall",
        "HUD/control panel"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Avalon Companions starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
