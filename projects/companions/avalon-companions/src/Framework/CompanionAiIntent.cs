namespace AvalonCompanions.Framework;

internal enum CompanionAiIntent
{
    NoActiveCompanion,
    IdleNativePatrol,
    StayPosition,
    FollowCatchUpCandidate,
    DefendWaitingForThreat,
    NativeCombatObserved,
    MovementBlocked,
    NoTargets,
    EvidenceInsufficient
}
