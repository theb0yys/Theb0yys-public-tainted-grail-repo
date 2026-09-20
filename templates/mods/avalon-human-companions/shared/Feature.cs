using Tainted.Abstractions.Runtime;

namespace TGTemplate.AvalonHumanCompanions;

internal static class Feature
{
    internal const string SourceFamily = "avalon-human-companions";

    internal static bool CanAttachHumanCompanion(
        bool enabled,
        bool humanCandidateResolved,
        bool aiHostReady,
        bool commandOwnerReady,
        bool alreadyManaged)
        => enabled
           && humanCandidateResolved
           && aiHostReady
           && commandOwnerReady
           && !alreadyManaged;

    internal static bool ShouldReleaseHumanCompanion(
        bool noLongerManaged,
        bool ownedByMod)
        => noLongerManaged && ownedByMod;

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Avalon Human Companions starter initialized. runtime=" + runtimeKind +
           "; human-companion-owner-and-ai-host-gates-ready";
}
