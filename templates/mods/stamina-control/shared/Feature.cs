using Tainted.Abstractions.Runtime;

namespace TGTemplate.StaminaControl;

internal static class Feature
{
    internal const string SourceFamily = "stamina-action-control";
    internal static readonly string[] Mechanisms =
    {
        "stamina action tuning",
        "character-stat initialization",
        "configurable sprint/combat split"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "StaminaControl starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
