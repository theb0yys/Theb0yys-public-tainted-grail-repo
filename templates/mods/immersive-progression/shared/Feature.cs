using Tainted.Abstractions.Runtime;

namespace TGTemplate.ImmersiveProgression;

internal static class Feature
{
    internal const string SourceFamily = "immersive-progression";
    internal static readonly string[] Mechanisms =
    {
        "progression planning",
        "campfire skill UI",
        "effect-intent apply/query",
        "provider bridge"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Immersive Progression starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
