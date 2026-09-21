using AvalonCompanions.Patches;

namespace AvalonCompanions.Framework;

public enum AvalonCompanionAiRuntimeMode
{
    Follow,
    Stay,
    Defend
}

public enum AvalonCompanionAiRuntimeCommand
{
    CatchUpRecall,
    NativeDefendPrompt,
    RecoverStuck
}

public enum AvalonCompanionAiRuntimeFollowRange
{
    Close,
    Pace,
    Far
}

public enum AvalonCompanionAiRuntimeBondLevel
{
    Wary,
    Familiar,
    Trusted,
    Loyal
}

public enum AvalonCompanionAiRuntimeNativeState
{
    Idle,
    Alert,
    Combat,
    Flee,
    Unknown
}

public enum AvalonCompanionAiRuntimeActorLane
{
    ManagedAnimalCompanion,
    WildAnimal,
    Unmanaged
}

public readonly struct AvalonCompanionAiRuntimePoint
{
    internal AvalonCompanionAiRuntimePoint(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public double X { get; }

    public double Y { get; }

    public double Z { get; }
}

public sealed class AvalonCompanionAiRuntimeObservation
{
    internal AvalonCompanionAiRuntimeObservation(
        long sequence,
        string actorRuntimeId,
        bool hasActiveCompanion,
        AvalonCompanionAiRuntimeMode mode,
        bool hasNpcElement,
        bool? npcAlive,
        bool? npcWorking,
        bool? npcInCombat,
        bool? canMove,
        int? possibleTargetCount,
        int? possibleAttackerCount,
        int heroLiveAttackers,
        string movementState,
        bool followCatchUpCandidate,
        bool nativeDefendAssistAllowed,
        bool nativeDefendPromptReady)
    {
        Sequence = sequence;
        ActorRuntimeId = actorRuntimeId ?? string.Empty;
        HasActiveCompanion = hasActiveCompanion;
        Mode = mode;
        HasNpcElement = hasNpcElement;
        NpcAlive = npcAlive;
        NpcWorking = npcWorking;
        NpcInCombat = npcInCombat;
        CanMove = canMove;
        PossibleTargetCount = possibleTargetCount;
        PossibleAttackerCount = possibleAttackerCount;
        HeroLiveAttackers = heroLiveAttackers;
        MovementState = movementState;
        FollowCatchUpCandidate = followCatchUpCandidate;
        NativeDefendAssistAllowed = nativeDefendAssistAllowed;
        NativeDefendPromptReady = nativeDefendPromptReady;
    }

    public long Sequence { get; }

    public string ActorRuntimeId { get; }

    public bool HasActiveCompanion { get; }

    public AvalonCompanionAiRuntimeMode Mode { get; }

    public bool HasNpcElement { get; }

    public bool? NpcAlive { get; }

    public bool? NpcWorking { get; }

    public bool? NpcInCombat { get; }

    public bool? CanMove { get; }

    public int? PossibleTargetCount { get; }

    public int? PossibleAttackerCount { get; }

    public int HeroLiveAttackers { get; }

    public string MovementState { get; }

    public bool FollowCatchUpCandidate { get; }

    public bool NativeDefendAssistAllowed { get; }

    public bool NativeDefendPromptReady { get; }
}

public sealed class AvalonCompanionAdvancedAiRuntimeObservation
{
    internal AvalonCompanionAdvancedAiRuntimeObservation(
        long sequence,
        string actorRuntimeId,
        string templateGuid,
        string templateName,
        string scene,
        AvalonCompanionAiRuntimePoint actorPosition,
        AvalonCompanionAiRuntimePoint playerPosition,
        double distanceToPlayerMeters,
        double healthFraction,
        bool alive,
        bool unconscious,
        AvalonCompanionAiRuntimeMode mode,
        AvalonCompanionAiRuntimeFollowRange followRange,
        int trust,
        int loyalty,
        AvalonCompanionAiRuntimeBondLevel bondLevel,
        AvalonCompanionAiRuntimeNativeState nativeState,
        string currentTargetId,
        bool currentTargetThreatensPlayer,
        int visibleAttackersCount,
        bool movementBlocked,
        bool movementStuck,
        AvalonCompanionAiRuntimeActorLane actorLane,
        bool isManaged,
        bool isTameCandidate,
        bool isHostile,
        bool isUndeadOrBlocked,
        bool isPassiveOnly,
        bool defendCooldownReady,
        bool catchUpCooldownReady,
        bool recoverCooldownReady,
        bool tamePromptCooldownReady,
        bool ownershipValid,
        bool killSwitchActive)
    {
        Sequence = sequence;
        ActorRuntimeId = actorRuntimeId ?? string.Empty;
        TemplateGuid = templateGuid ?? string.Empty;
        TemplateName = templateName ?? string.Empty;
        Scene = scene ?? string.Empty;
        ActorPosition = actorPosition;
        PlayerPosition = playerPosition;
        DistanceToPlayerMeters = distanceToPlayerMeters < 0d ? 0d : distanceToPlayerMeters;
        HealthFraction = healthFraction < 0d ? 0d : healthFraction > 1d ? 1d : healthFraction;
        Alive = alive;
        Unconscious = unconscious;
        Mode = mode;
        FollowRange = followRange;
        Trust = trust < 0 ? 0 : trust > 100 ? 100 : trust;
        Loyalty = loyalty < 0 ? 0 : loyalty > 100 ? 100 : loyalty;
        BondLevel = bondLevel;
        NativeState = nativeState;
        CurrentTargetId = currentTargetId ?? string.Empty;
        CurrentTargetThreatensPlayer = currentTargetThreatensPlayer;
        VisibleAttackersCount = visibleAttackersCount < 0 ? 0 : visibleAttackersCount;
        MovementBlocked = movementBlocked;
        MovementStuck = movementStuck;
        ActorLane = actorLane;
        IsManaged = isManaged;
        IsTameCandidate = isTameCandidate;
        IsHostile = isHostile;
        IsUndeadOrBlocked = isUndeadOrBlocked;
        IsPassiveOnly = isPassiveOnly;
        DefendCooldownReady = defendCooldownReady;
        CatchUpCooldownReady = catchUpCooldownReady;
        RecoverCooldownReady = recoverCooldownReady;
        TamePromptCooldownReady = tamePromptCooldownReady;
        OwnershipValid = ownershipValid;
        KillSwitchActive = killSwitchActive;
    }

    public long Sequence { get; }

    public string ActorRuntimeId { get; }

    public string TemplateGuid { get; }

    public string TemplateName { get; }

    public string Scene { get; }

    public AvalonCompanionAiRuntimePoint ActorPosition { get; }

    public AvalonCompanionAiRuntimePoint PlayerPosition { get; }

    public double DistanceToPlayerMeters { get; }

    public double HealthFraction { get; }

    public bool Alive { get; }

    public bool Unconscious { get; }

    public AvalonCompanionAiRuntimeMode Mode { get; }

    public AvalonCompanionAiRuntimeFollowRange FollowRange { get; }

    public int Trust { get; }

    public int Loyalty { get; }

    public AvalonCompanionAiRuntimeBondLevel BondLevel { get; }

    public AvalonCompanionAiRuntimeNativeState NativeState { get; }

    public string CurrentTargetId { get; }

    public bool CurrentTargetThreatensPlayer { get; }

    public int VisibleAttackersCount { get; }

    public bool MovementBlocked { get; }

    public bool MovementStuck { get; }

    public AvalonCompanionAiRuntimeActorLane ActorLane { get; }

    public bool IsManaged { get; }

    public bool IsTameCandidate { get; }

    public bool IsHostile { get; }

    public bool IsUndeadOrBlocked { get; }

    public bool IsPassiveOnly { get; }

    public bool DefendCooldownReady { get; }

    public bool CatchUpCooldownReady { get; }

    public bool RecoverCooldownReady { get; }

    public bool TamePromptCooldownReady { get; }

    public bool OwnershipValid { get; }

    public bool KillSwitchActive { get; }
}

public static class AvalonCompanionAiGameSystems
{
    public static string InspectReadiness(string ownerId)
    {
        return PetCompanionController.InspectAiRuntimeReadiness(ownerId);
    }

    public static bool TryAcquireOwnership(string ownerId, out string reason)
    {
        return PetCompanionController.TryAcquireAiRuntimeOwnership(ownerId, out reason);
    }

    public static bool TryReleaseOwnership(string ownerId)
    {
        return PetCompanionController.TryReleaseAiRuntimeOwnership(ownerId);
    }

    public static bool TryInspectActiveActorId(string ownerId, out string actorRuntimeId)
    {
        return PetCompanionController.TryInspectAiRuntimeActorIdentity(ownerId, out actorRuntimeId);
    }

    public static bool TryCollectObservation(
        string ownerId,
        out AvalonCompanionAiRuntimeObservation? observation)
    {
        return PetCompanionController.TryCollectAiRuntimeObservation(ownerId, out observation);
    }

    public static bool TryCollectAdvancedObservation(
        string ownerId,
        bool killSwitchActive,
        out AvalonCompanionAdvancedAiRuntimeObservation? observation)
    {
        return PetCompanionController.TryCollectAdvancedAiRuntimeObservation(
            ownerId,
            killSwitchActive,
            out observation);
    }

    public static bool TryDispatch(
        string ownerId,
        long observationSequence,
        AvalonCompanionAiRuntimeCommand command)
    {
        return PetCompanionController.TryDispatchAiRuntimeCommand(
            ownerId,
            observationSequence,
            command);
    }

    public static bool TryAuditAdvancedProposal(
        string ownerId,
        long observationSequence,
        string proposalKind,
        string reason)
    {
        return PetCompanionController.TryAuditAdvancedAiRuntimeProposal(
            ownerId,
            observationSequence,
            proposalKind,
            reason);
    }
}
