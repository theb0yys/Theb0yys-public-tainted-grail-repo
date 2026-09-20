using Tainted.Abstractions.Runtime;

namespace TGTemplate.CarryWeightTweaks;

internal static class Feature
{
    internal const string SourceFamily = "carry-weight-tweaks";
    internal static readonly string[] Mechanisms =
    {
        "carry-weight limit tuning",
        "configurable stat override",
        "native stat initialization boundary"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "CarryWeightTweaks starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
