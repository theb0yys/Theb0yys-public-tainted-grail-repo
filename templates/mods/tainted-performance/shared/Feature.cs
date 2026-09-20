using Tainted.Abstractions.Runtime;

namespace TGTemplate.TaintedPerformance;

internal static class Feature
{
    internal const string SourceFamily = "tainted-performance";
    internal static readonly string[] Mechanisms =
    {
        "frame sampling",
        "incident reports",
        "plugin inventory",
        "performance overview UI"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Tainted Performance starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
