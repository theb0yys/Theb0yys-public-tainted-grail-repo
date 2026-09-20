using Tainted.Abstractions.Runtime;

namespace TGTemplate.AvalonCompanions;

internal static class Feature
{
    internal const string SourceFamily = "avalon-companions";

    internal static bool CanAttachCompanion(
        bool enabled,
        bool candidateResolved,
        bool ownershipAvailable,
        bool alreadyAttached)
        => enabled && candidateResolved && ownershipAvailable && !alreadyAttached;

    internal static bool ShouldReleaseCompanion(
        bool noLongerManaged,
        bool ownedByMod)
        => noLongerManaged && ownedByMod;

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Avalon Companions starter initialized. runtime=" + runtimeKind +
           "; attach-and-release-ownership-gates-ready";
}
