namespace AvalonHumanCompanions.Framework;

public enum AvalonHumanAiRuntimeMode
{
    Follow,
    Hold,
    Defend
}

public enum AvalonHumanAiRuntimeCommand
{
    ApplyHoldMovementLock,
    ReleaseHoldMovementLock,
    EmergencyRecall,
    FollowRecall,
    CombatRecall,
    NativeDefendPrompt
}

public sealed class AvalonHumanAiRuntimeObservation
{
    internal AvalonHumanAiRuntimeObservation(
        long sequence,
        string actorRuntimeId,
        bool hasManagedHuman,
        bool? alive,
        bool? working,
        AvalonHumanAiRuntimeMode mode,
        bool holdMovementLockAttached,
        float? distanceToHero,
        float followRecallDistance,
        float combatRecallDistance,
        float emergencyRecallDistance,
        int heroLiveAttackers,
        bool nativeDefendAllowed,
        bool defendPromptReady,
        bool followCatchUpEnabled,
        bool emergencyRecallEnabled)
    {
        Sequence = sequence;
        ActorRuntimeId = actorRuntimeId ?? string.Empty;
        HasManagedHuman = hasManagedHuman;
        Alive = alive;
        Working = working;
        Mode = mode;
        HoldMovementLockAttached = holdMovementLockAttached;
        DistanceToHero = distanceToHero;
        FollowRecallDistance = followRecallDistance;
        CombatRecallDistance = combatRecallDistance;
        EmergencyRecallDistance = emergencyRecallDistance;
        HeroLiveAttackers = heroLiveAttackers;
        NativeDefendAllowed = nativeDefendAllowed;
        DefendPromptReady = defendPromptReady;
        FollowCatchUpEnabled = followCatchUpEnabled;
        EmergencyRecallEnabled = emergencyRecallEnabled;
    }

    public long Sequence { get; }
    public string ActorRuntimeId { get; }
    public bool HasManagedHuman { get; }
    public bool? Alive { get; }
    public bool? Working { get; }
    public AvalonHumanAiRuntimeMode Mode { get; }
    public bool HoldMovementLockAttached { get; }
    public float? DistanceToHero { get; }
    public float FollowRecallDistance { get; }
    public float CombatRecallDistance { get; }
    public float EmergencyRecallDistance { get; }
    public int HeroLiveAttackers { get; }
    public bool NativeDefendAllowed { get; }
    public bool DefendPromptReady { get; }
    public bool FollowCatchUpEnabled { get; }
    public bool EmergencyRecallEnabled { get; }
}

public static class AvalonHumanAiGameSystems
{
    public static string InspectReadiness(string ownerId)
    {
        return Plugin.InspectAiRuntimeReadiness(ownerId);
    }

    public static bool TryAcquireOwnership(string ownerId, out string reason)
    {
        return Plugin.TryAcquireAiRuntimeOwnership(ownerId, out reason);
    }

    public static bool TryReleaseOwnership(string ownerId)
    {
        return Plugin.TryReleaseAiRuntimeOwnership(ownerId);
    }

    public static bool TryCollectObservation(
        string ownerId,
        out AvalonHumanAiRuntimeObservation? observation)
    {
        return Plugin.TryCollectAiRuntimeObservation(ownerId, out observation);
    }

    public static bool TryDispatch(
        string ownerId,
        long observationSequence,
        AvalonHumanAiRuntimeCommand command)
    {
        return Plugin.TryDispatchAiRuntimeCommand(ownerId, observationSequence, command);
    }
}
