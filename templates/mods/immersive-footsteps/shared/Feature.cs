using Tainted.Abstractions.Runtime;

namespace TGTemplate.ImmersiveFootsteps;

internal static class Feature
{
    internal const string SourceFamily = "immersive-footsteps";
    internal static readonly string[] Mechanisms =
    {
        "FMOD footstep interception",
        "surface/context replacement",
        "replacement-first suppression",
        "runtime audio decode"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Immersive Footsteps starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
