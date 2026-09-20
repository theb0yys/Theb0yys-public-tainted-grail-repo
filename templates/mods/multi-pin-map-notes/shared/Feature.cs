using Tainted.Abstractions.Runtime;

namespace TGTemplate.MultiPinMapNotes;

internal static class Feature
{
    internal const string SourceFamily = "multi-pin-map-notes";
    internal static readonly string[] Mechanisms =
    {
        "custom map pins",
        "note state",
        "map input integration",
        "shared UI bridge"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Multi-Pin Map Notes starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
