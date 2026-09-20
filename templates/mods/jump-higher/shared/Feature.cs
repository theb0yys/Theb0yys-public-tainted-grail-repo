using Tainted.Abstractions.Runtime;

namespace TGTemplate.JumpHigher;

internal static class Feature
{
    internal const string SourceFamily = "jump-higher";
    internal static readonly string[] Mechanisms =
    {
        "movement jump scaling",
        "bounded movement patch",
        "configurable multiplier"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Jump Higher starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
