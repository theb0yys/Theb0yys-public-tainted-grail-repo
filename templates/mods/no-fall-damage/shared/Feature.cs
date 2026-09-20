using Tainted.Abstractions.Runtime;

namespace TGTemplate.NoFallDamage;

internal static class Feature
{
    internal const string SourceFamily = "no-fall-damage";
    internal static readonly string[] Mechanisms =
    {
        "fall-damage guard",
        "result override",
        "narrow movement safety patch"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "No Fall Damage starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
