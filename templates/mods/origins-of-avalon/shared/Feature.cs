using Tainted.Abstractions.Runtime;

namespace TGTemplate.OriginsOfAvalon;

internal static class Feature
{
    internal const string SourceFamily = "origins-of-avalon";
    internal static readonly string[] Mechanisms =
    {
        "origin selection",
        "growth/progression provider",
        "journey state",
        "shared UI integration"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Origins of Avalon starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
