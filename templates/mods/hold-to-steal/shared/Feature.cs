using Tainted.Abstractions.Runtime;

namespace TGTemplate.HoldToSteal;

internal static class Feature
{
    internal const string SourceFamily = "hold-to-steal";
    internal static readonly string[] Mechanisms =
    {
        "illegal pickup guard",
        "theft action gating",
        "native inventory preservation",
        "shared UI bridge"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Hold to Steal starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
