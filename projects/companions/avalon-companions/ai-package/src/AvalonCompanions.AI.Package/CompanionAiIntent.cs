namespace AvalonCompanions.AI.Package;

public enum CompanionAiIntent
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
