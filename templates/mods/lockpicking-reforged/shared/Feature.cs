using Tainted.Abstractions.Runtime;

namespace TGTemplate.LockpickingReforged;

internal static class Feature
{
    internal const string SourceFamily = "lockpicking-reforged";
    internal static readonly string[] Mechanisms =
    {
        "lock entry",
        "tolerance tuning",
        "pick durability",
        "auto-unlock guard"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Lockpicking Reforged starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
