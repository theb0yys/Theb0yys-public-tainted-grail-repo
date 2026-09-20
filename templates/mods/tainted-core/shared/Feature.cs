using Tainted.Abstractions.Runtime;

namespace TGTemplate.TaintedCore;

internal static class Feature
{
    internal const string SourceFamily = "avalon-core";
    internal static readonly string[] Mechanisms =
    {
        "adapter registry",
        "trust reports",
        "capability contracts",
        "read-only consumer discovery"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Tainted Core's starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
