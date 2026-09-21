namespace AvalonCompanions.Framework;

internal readonly struct CompanionAiProfile
{
    internal CompanionAiProfile(
        CompanionAiIntent intent,
        string intentReason,
        string scene,
        CompanionMode mode,
        FollowRangeProfile followRange,
        int activeRosterCount,
        int trackedCreatureCount,
        string templateGuid,
        string templateName,
        string displayName,
        string locationId,
        string debugName,
        string coords,
        float? distanceToHero,
        float? catchUpThreshold,
        int? heroLiveAttackers,
        int? possibleTargetCount,
        int? possibleAttackerCount,
        string movementState,
        bool? canMove,
        bool? canOverrideDestination,
        bool? npcAlive,
        bool? npcWorking,
        bool? npcInCombat,
        bool? npcInIdle,
        bool hasNpcElement,
        bool? hasTargetOverride,
        bool? hasHeroSummonTargetOverride)
    {
        Intent = intent;
        IntentReason = intentReason;
        Scene = scene;
        Mode = mode;
        FollowRange = followRange;
        ActiveRosterCount = activeRosterCount;
        TrackedCreatureCount = trackedCreatureCount;
        TemplateGuid = templateGuid;
        TemplateName = templateName;
        DisplayName = displayName;
        LocationId = locationId;
        DebugName = debugName;
        Coords = coords;
        DistanceToHero = distanceToHero;
        CatchUpThreshold = catchUpThreshold;
        HeroLiveAttackers = heroLiveAttackers;
        PossibleTargetCount = possibleTargetCount;
        PossibleAttackerCount = possibleAttackerCount;
        MovementState = movementState;
        CanMove = canMove;
        CanOverrideDestination = canOverrideDestination;
        NpcAlive = npcAlive;
        NpcWorking = npcWorking;
        NpcInCombat = npcInCombat;
        NpcInIdle = npcInIdle;
        HasNpcElement = hasNpcElement;
        HasTargetOverride = hasTargetOverride;
        HasHeroSummonTargetOverride = hasHeroSummonTargetOverride;
    }

    internal CompanionAiIntent Intent { get; }

    internal string IntentReason { get; }

    internal string Scene { get; }

    internal CompanionMode Mode { get; }

    internal FollowRangeProfile FollowRange { get; }

    internal int ActiveRosterCount { get; }

    internal int TrackedCreatureCount { get; }

    internal string TemplateGuid { get; }

    internal string TemplateName { get; }

    internal string DisplayName { get; }

    internal string LocationId { get; }

    internal string DebugName { get; }

    internal string Coords { get; }

    internal float? DistanceToHero { get; }

    internal float? CatchUpThreshold { get; }

    internal int? HeroLiveAttackers { get; }

    internal int? PossibleTargetCount { get; }

    internal int? PossibleAttackerCount { get; }

    internal string MovementState { get; }

    internal bool? CanMove { get; }

    internal bool? CanOverrideDestination { get; }

    internal bool? NpcAlive { get; }

    internal bool? NpcWorking { get; }

    internal bool? NpcInCombat { get; }

    internal bool? NpcInIdle { get; }

    internal bool HasNpcElement { get; }

    internal bool? HasTargetOverride { get; }

    internal bool? HasHeroSummonTargetOverride { get; }
}
