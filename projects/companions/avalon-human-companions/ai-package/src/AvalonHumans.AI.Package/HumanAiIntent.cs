namespace AvalonHumans.AI.Package;

public enum HumanAiIntent
{
    NoManagedHuman,
    ApplyHoldMovementLockCandidate,
    ReleaseHoldMovementLockCandidate,
    EmergencyRecallCandidate,
    CombatRecallCandidate,
    FollowRecallCandidate,
    NativeDefendCandidate,
    HoldPosition,
    ThreatObserved,
    DefendWaitingForThreat,
    KeepingClose,
    EvidenceInsufficient
}
