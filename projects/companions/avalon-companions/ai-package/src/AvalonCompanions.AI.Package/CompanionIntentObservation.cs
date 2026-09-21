using AvalonAI.Contracts;

namespace AvalonCompanions.AI.Package;

public sealed class CompanionIntentObservation : IAvalonAiObservation
{
    public CompanionIntentObservation(
        bool hasActiveCompanion,
        CompanionAiMode mode,
        bool hasNpcElement,
        bool? npcAlive,
        bool? npcWorking,
        bool? npcInCombat,
        bool? canMove,
        int? possibleTargetCount,
        int? possibleAttackerCount,
        int heroLiveAttackers,
        string? movementState,
        bool followCatchUpCandidate)
        : this(
            hasActiveCompanion,
            mode,
            hasNpcElement,
            npcAlive,
            npcWorking,
            npcInCombat,
            canMove,
            possibleTargetCount,
            possibleAttackerCount,
            heroLiveAttackers,
            movementState,
            followCatchUpCandidate,
            false,
            true)
    {
    }

    public CompanionIntentObservation(
        bool hasActiveCompanion,
        CompanionAiMode mode,
        bool hasNpcElement,
        bool? npcAlive,
        bool? npcWorking,
        bool? npcInCombat,
        bool? canMove,
        int? possibleTargetCount,
        int? possibleAttackerCount,
        int heroLiveAttackers,
        string? movementState,
        bool followCatchUpCandidate,
        bool nativeDefendAssistAllowed)
        : this(
            hasActiveCompanion,
            mode,
            hasNpcElement,
            npcAlive,
            npcWorking,
            npcInCombat,
            canMove,
            possibleTargetCount,
            possibleAttackerCount,
            heroLiveAttackers,
            movementState,
            followCatchUpCandidate,
            nativeDefendAssistAllowed,
            true)
    {
    }

    public CompanionIntentObservation(
        bool hasActiveCompanion,
        CompanionAiMode mode,
        bool hasNpcElement,
        bool? npcAlive,
        bool? npcWorking,
        bool? npcInCombat,
        bool? canMove,
        int? possibleTargetCount,
        int? possibleAttackerCount,
        int heroLiveAttackers,
        string? movementState,
        bool followCatchUpCandidate,
        bool nativeDefendAssistAllowed,
        bool nativeDefendPromptReady)
    {
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
        MovementState = movementState ?? string.Empty;
        FollowCatchUpCandidate = followCatchUpCandidate;
        NativeDefendAssistAllowed = nativeDefendAssistAllowed;
        NativeDefendPromptReady = nativeDefendPromptReady;
    }

    public bool HasActiveCompanion { get; }

    public CompanionAiMode Mode { get; }

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
