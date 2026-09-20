using Tainted.Abstractions.Runtime;

namespace TGTemplate.AvalonStash;

internal static class Feature
{
    internal const string SourceFamily = "Avalon Stash";
    internal static readonly string[] Mechanisms =
    {
        "campfire storage entry",
        "stash-aware ingredient counts",
        "storage summary UI",
        "native stash ownership"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Avalon Stash starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
