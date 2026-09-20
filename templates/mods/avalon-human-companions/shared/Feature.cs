using Tainted.Abstractions.Runtime;

namespace TGTemplate.AvalonHumanCompanions;

internal static class Feature
{
    internal const string SourceFamily = "avalon-human-companions";
    internal static readonly string[] Mechanisms =
    {
        "human companion roster",
        "AI package",
        "companion commands",
        "shared UI/control panel"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Avalon Human Companions starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
