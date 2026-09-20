using Tainted.Abstractions.Runtime;

namespace TGTemplate.BetterBonfireMenu;

internal static class Feature
{
    internal const string SourceFamily = "better-bonfire-menu";

    internal static bool ShouldAddEntry(
        bool enabled,
        bool nativeMenuReady,
        bool duplicateEntryExists,
        bool featureOwnerReady)
        => enabled && nativeMenuReady && !duplicateEntryExists && featureOwnerReady;

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Better Bonfire Menu starter initialized. runtime=" + runtimeKind +
           "; native-menu-extension-gate-ready";
}
